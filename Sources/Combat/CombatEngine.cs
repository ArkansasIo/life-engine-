/**
 * LifeEngine is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * LifeEngine is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 *
 * Copyright © 2026 ArkansasIo
 */

using System;
using System.Collections.Generic;

namespace LifeEngine.Combat
{
    /// <summary>How a battle should be resolved.</summary>
    public sealed class BattleRules
    {
        private int m_maxRounds;
        private bool m_allowFleeing;
        private double m_fleeThreshold;
        private double m_damageVariance;
        private bool m_effectsTickEachRound;
        private double m_roundsBetweenEffectTicks;

        /// <summary>The most rounds a fight can run before it is called a draw.</summary>
        public int MaxRounds
        {
            get { return m_maxRounds; }
            set { m_maxRounds = value < 1 ? 1 : value; }
        }

        /// <summary>Whether a losing fighter may run.</summary>
        public bool AllowFleeing
        {
            get { return m_allowFleeing; }
            set { m_allowFleeing = value; }
        }

        /// <summary>
        /// The morale below which a fighter considers running. Morale falls as
        /// health does, so this is really "how badly hurt before it quits".
        /// </summary>
        public double FleeThreshold
        {
            get { return m_fleeThreshold; }
            set { m_fleeThreshold = value; }
        }

        /// <summary>
        /// How much a blow can vary, as a fraction. Zero makes every blow land
        /// for exactly the same damage, which is useful for testing.
        /// </summary>
        public double DamageVariance
        {
            get { return m_damageVariance; }
            set { m_damageVariance = value < 0.0 ? 0.0 : value; }
        }

        /// <summary>Whether bleeding and poison tick.</summary>
        public bool EffectsTickEachRound
        {
            get { return m_effectsTickEachRound; }
            set { m_effectsTickEachRound = value; }
        }

        /// <summary>How often effects wear off and tick, in rounds.</summary>
        public double RoundsBetweenEffectTicks
        {
            get { return m_roundsBetweenEffectTicks; }
            set { m_roundsBetweenEffectTicks = value <= 0.0 ? 1.0 : value; }
        }

        public BattleRules()
        {
            m_maxRounds = 40;
            m_allowFleeing = true;
            m_fleeThreshold = 0.2;
            m_damageVariance = 0.25;
            m_effectsTickEachRound = true;
            m_roundsBetweenEffectTicks = 1.0;
        }

        /// <summary>A fight to the death, with no running.</summary>
        public static BattleRules ToTheDeath()
        {
            BattleRules rules = new BattleRules();

            rules.AllowFleeing = false;
            rules.MaxRounds = 60;

            return rules;
        }

        /// <summary>A quick skirmish: few rounds, and the loser runs early.</summary>
        public static BattleRules Skirmish()
        {
            BattleRules rules = new BattleRules();

            rules.MaxRounds = 12;
            rules.FleeThreshold = 0.45;

            return rules;
        }
    }

    /// <summary>
    /// Resolves a fight between two sides.
    ///
    /// The engine is turn based: every fighter acts once per round in order of
    /// initiative, and the round ends with recovery. It holds no state between
    /// battles, so the same fighters can be run through it twice with the same
    /// random stream and give the same result.
    ///
    /// Every number the engine reads comes through <see cref="Combatant.GetStat"/>,
    /// so an effect cannot be skipped by accident.
    /// </summary>
    public sealed class CombatEngine
    {
        private BattleRules m_rules;
        private Random m_random;
        private BattleLog m_log;
        private List<Combatant> m_fighters;

        /// <summary>The rules this engine fights by.</summary>
        public BattleRules Rules
        {
            get { return m_rules; }
            set { m_rules = value ?? new BattleRules(); }
        }

        public CombatEngine(Random random)
        {
            m_random = random ?? new Random(20261007);
            m_rules = new BattleRules();
            m_log = new BattleLog();
            m_fighters = new List<Combatant>();
        }

        // ------------------------------------------------------------------
        // Setting up
        // ------------------------------------------------------------------

        /// <summary>Adds a fighter to the coming battle.</summary>
        public Combatant Add(Combatant fighter)
        {
            if (fighter == null)
                return null;

            m_fighters.Add(fighter);

            return fighter;
        }

        /// <summary>
        /// Adds every wolf of a pack as a fighter on the given side. Only the
        /// living ones: the dead do not fight twice.
        /// </summary>
        public List<Combatant> AddPack(Biology.WolfPack pack, Side side)
        {
            List<Combatant> added = new List<Combatant>();

            if (pack == null)
                return added;

            for (int i = 0; i < pack.Members.Count; i++)
            {
                Biology.Wolf wolf = pack.Members[i];

                if (!wolf.IsAlive)
                    continue;

                Combatant fighter = Combatant.FromWolf(wolf, side);

                m_fighters.Add(fighter);
                added.Add(fighter);
            }

            return added;
        }

        public void Clear()
        {
            m_fighters.Clear();
        }

        public IList<Combatant> Fighters
        {
            get { return m_fighters; }
        }

        // ------------------------------------------------------------------
        // The fight
        // ------------------------------------------------------------------

        /// <summary>
        /// Runs the battle to a finish: one side down, one side fled, or the
        /// round limit reached.
        /// </summary>
        public BattleResult Fight(string name)
        {
            BattleResult result = new BattleResult(name ?? "Battle");

            for (int i = 0; i < m_fighters.Count; i++)
                result.Combatants.Add(m_fighters[i]);

            if (m_fighters.Count < 2)
            {
                result.Winner = Side.Neutral;
                result.Narrative = "There is nobody to fight.";
                result.Log.Note("The battle was called off: fewer than two fighters.");

                return result;
            }

            m_log = new BattleLog();

            m_log.Start(OpeningLine());

            int round = 0;

            while (round < m_rules.MaxRounds)
            {
                round++;

                m_log.BeginRound(round);

                if (!RunRound())
                    break;
            }

            result.Rounds = round;

            Finish(result);

            return result;
        }

        /// <summary>Runs the fight for a set number of rounds, for a step-through view.</summary>
        public BattleResult FightRounds(string name, int rounds)
        {
            BattleResult result = new BattleResult(name ?? "Battle");

            for (int i = 0; i < m_fighters.Count; i++)
                result.Combatants.Add(m_fighters[i]);

            m_log = new BattleLog();
            m_log.Start(OpeningLine());

            int round = 0;

            while (round < rounds && round < m_rules.MaxRounds)
            {
                round++;

                m_log.BeginRound(round);

                if (!RunRound())
                    break;
            }

            result.Rounds = round;
            result.Log.Add(new LogEntry(round, LogKind.Note, "(stopped early)"));

            Finish(result);

            return result;
        }

        private string OpeningLine()
        {
            int pack = 0;
            int rival = 0;
            int prey = 0;

            for (int i = 0; i < m_fighters.Count; i++)
            {
                switch (m_fighters[i].Side)
                {
                    case Side.Pack: pack++; break;
                    case Side.Rival: rival++; break;
                    case Side.Prey: prey++; break;
                }
            }

            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.Append("The fight begins: ");

            if (pack > 0)
                builder.Append(pack + " of the pack");

            if (rival > 0)
                builder.Append((pack > 0 ? ", " : string.Empty) + rival + " rival");

            if (prey > 0)
                builder.Append((pack > 0 || rival > 0 ? ", " : string.Empty) + prey + " prey");

            builder.Append(".");

            return builder.ToString();
        }

        /// <summary>
        /// One round. Returns false when the fight is over, so the caller stops.
        /// </summary>
        private bool RunRound()
        {
            List<Combatant> order = TurnOrder();

            for (int i = 0; i < order.Count; i++)
            {
                Combatant actor = order[i];

                if (!actor.CanFight)
                    continue;

                if (IsOver())
                    return false;

                TakeTurn(actor);
            }

            // End of round: effects wear off, wounds bleed, stamina returns.
            EndOfRound();

            return !IsOver();
        }

        /// <summary>
        /// Everyone still fighting, fastest first. A tie is broken by the roll,
        /// so two equally quick wolves do not always act in the same order.
        /// </summary>
        private List<Combatant> TurnOrder()
        {
            List<Combatant> order = new List<Combatant>();

            for (int i = 0; i < m_fighters.Count; i++)
            {
                if (m_fighters[i].CanFight)
                    order.Add(m_fighters[i]);
            }

            order.Sort(delegate (Combatant left, Combatant right)
            {
                double leftSpeed = left.GetStat(EffectTarget.Initiative) + m_random.NextDouble() * 0.2;
                double rightSpeed = right.GetStat(EffectTarget.Initiative) + m_random.NextDouble() * 0.2;

                return rightSpeed.CompareTo(leftSpeed);
            });

            return order;
        }

        /// <summary>
        /// One fighter's turn: pick a target, then attack it. A fighter with no
        /// stamina left catches its breath instead.
        /// </summary>
        private void TakeTurn(Combatant actor)
        {
            // Out of breath: recover instead of attacking.
            if (actor.Stamina < 5.0)
            {
                actor.Recover();

                m_log.Note(actor.Name + " is blown and circles, catching its breath.");

                return;
            }

            Combatant target = ChooseTarget(actor);

            if (target == null)
                return;

            Attack(actor, target);
        }

        /// <summary>
        /// Who to hit. A fighter prefers whoever it can hurt most easily, which
        /// means the weakest enemy still standing, with a bias towards whoever
        /// hurt it last.
        /// </summary>
        private Combatant ChooseTarget(Combatant actor)
        {
            List<Combatant> enemies = new List<Combatant>();

            for (int i = 0; i < m_fighters.Count; i++)
            {
                Combatant other = m_fighters[i];

                if (other == actor || !other.CanFight)
                    continue;

                if (IsEnemy(actor.Side, other.Side))
                    enemies.Add(other);
            }

            if (enemies.Count == 0)
                return null;

            // The weakest first, with a little noise so it is not mechanical.
            Combatant best = enemies[0];
            double bestScore = double.MaxValue;

            for (int i = 0; i < enemies.Count; i++)
            {
                double score = enemies[i].HealthFraction + m_random.NextDouble() * 0.25;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = enemies[i];
                }
            }

            return best;
        }

        /// <summary>Which sides fight which.</summary>
        public static bool IsEnemy(Side left, Side right)
        {
            if (left == right)
                return false;

            if (left == Side.Neutral || right == Side.Neutral)
                return false;

            // Prey fights back against a hunter, but prey does not fight prey.
            if (left == Side.Prey && right == Side.Prey)
                return false;

            return true;
        }

        /// <summary>
        /// One attack. The attacker rolls to land it, the defender rolls to
        /// avoid it, and if it lands the damage is worked out from the
        /// attacker's damage and the defender's defence.
        /// </summary>
        private void Attack(Combatant attacker, Combatant defender)
        {
            attacker.SpendStamina(8.0);

            double accuracy = attacker.GetStat(EffectTarget.Accuracy);
            double evasion = defender.GetStat(EffectTarget.Evasion);

            double hitChance = accuracy - evasion * 0.6;

            if (hitChance < 0.1)
                hitChance = 0.1;

            if (m_random.NextDouble() > hitChance)
            {
                // A miss, or a dodge, depending on how evasive the defender is.
                if (m_random.NextDouble() < evasion)
                    m_log.Dodge(attacker.Name, defender.Name);
                else
                    m_log.Miss(attacker.Name, defender.Name);

                return;
            }

            int damage = RollDamage(attacker, defender);
            int taken = defender.TakeDamage(damage);

            attacker.RecordBlow(taken);

            m_log.Blow(attacker.Name, defender.Name, taken);

            // A hard blow can leave a mark worth remembering.
            if (taken > defender.MaxHealth * 0.3)
            {
                string wound = PickWound();

                defender.AddWound(wound);
                m_log.Wound(defender.Name, wound);
            }

            // Being hurt shakes morale, and a shaken fighter may run.
            ConsiderFleeing(defender);

            if (defender.IsDead)
            {
                attacker.RecordKill();

                m_log.Death(defender.Name, "goes down and does not rise");
            }
        }

        /// <summary>
        /// The damage of one blow: the attacker's damage, less the defender's
        /// defence, scaled by mass, with the configured variance on top.
        /// </summary>
        private int RollDamage(Combatant attacker, Combatant defender)
        {
            double damage = attacker.GetStat(EffectTarget.Damage) * 20.0;

            // A heavy attacker hits harder.
            damage += attacker.Mass * 0.15;

            // Defence subtracts a proportion, not a flat amount, so a small
            // defender is not made immune by a big defence score.
            double defence = defender.GetStat(EffectTarget.Defence);
            damage *= 1.0 - Math.Min(0.6, defence * 0.4);

            if (m_rules.DamageVariance > 0.0)
            {
                double swing = (m_random.NextDouble() * 2.0 - 1.0) * m_rules.DamageVariance;

                damage *= 1.0 + swing;
            }

            int result = (int)Math.Round(damage);

            if (result < 1)
                result = 1;

            return result;
        }

        private static readonly string[] WoundNames =
        {
            "a torn shoulder", "a gash along the flank", "a bitten foreleg",
            "a ragged ear", "a deep bite to the neck", "a raked muzzle",
            "a torn haunch", "a split paw"
        };

        private string PickWound()
        {
            return WoundNames[m_random.Next(WoundNames.Length)];
        }

        /// <summary>
        /// Whether a fighter has had enough. Morale is health plus a bonus for
        /// aggression, so a bold wolf stays longer than a timid one.
        /// </summary>
        private void ConsiderFleeing(Combatant fighter)
        {
            if (!m_rules.AllowFleeing || fighter.HasFled || fighter.IsDead)
                return;

            double morale = fighter.HealthFraction +
                            fighter.GetStat(EffectTarget.Morale) * 0.2 +
                            fighter.GetStat(EffectTarget.Aggression) * 0.2 -
                            fighter.GetStat(EffectTarget.Fear) * 0.5;

            if (morale > m_rules.FleeThreshold)
                return;

            // A cub runs sooner than an adult, and a cornered one does not run
            // at all.
            if (fighter.CanFight && m_random.NextDouble() < 0.5)
            {
                fighter.Flee();
                m_log.Flee(fighter.Name);
            }
        }

        /// <summary>Effects tick down, wounds bleed, stamina comes back.</summary>
        private void EndOfRound()
        {
            double elapsed = m_rules.RoundsBetweenEffectTicks;

            for (int i = 0; i < m_fighters.Count; i++)
            {
                Combatant fighter = m_fighters[i];

                if (fighter.IsDead)
                    continue;

                if (m_rules.EffectsTickEachRound)
                {
                    List<string> expired = fighter.Effects.Tick(elapsed);

                    for (int e = 0; e < expired.Count; e++)
                        m_log.Expired(fighter.Name, expired[e]);
                }

                fighter.Recover();

                if (fighter.IsDead)
                    m_log.Death(fighter.Name, "succumbs to its wounds");
            }
        }

        /// <summary>
        /// Whether the fight is over: one side has nobody left standing, or
        /// everybody still upright is on the same side.
        /// </summary>
        private bool IsOver()
        {
            bool packStanding = false;
            bool rivalStanding = false;
            bool preyStanding = false;

            for (int i = 0; i < m_fighters.Count; i++)
            {
                Combatant fighter = m_fighters[i];

                if (!fighter.CanFight)
                    continue;

                switch (fighter.Side)
                {
                    case Side.Pack: packStanding = true; break;
                    case Side.Rival: rivalStanding = true; break;
                    case Side.Prey: preyStanding = true; break;
                }
            }

            int sidesStanding = 0;

            if (packStanding) sidesStanding++;
            if (rivalStanding) sidesStanding++;
            if (preyStanding) sidesStanding++;

            return sidesStanding < 2;
        }

        /// <summary>
        /// Works out who won, writes the result back into the wolves, and
        /// writes the closing line.
        /// </summary>
        private void Finish(BattleResult result)
        {
            result.Log = m_log;

            for (int i = 0; i < m_fighters.Count; i++)
            {
                Combatant fighter = m_fighters[i];

                if (fighter.IsDead)
                    result.Dead.Add(fighter);
                else if (fighter.HasFled)
                    result.Fled.Add(fighter);
            }

            int pack = result.Standing(Side.Pack);
            int rival = result.Standing(Side.Rival);
            int prey = result.Standing(Side.Prey);

            if (pack > 0 && rival == 0 && prey == 0)
                result.Winner = Side.Pack;
            else if (rival > 0 && pack == 0 && prey == 0)
                result.Winner = Side.Rival;
            else if (prey > 0 && pack == 0 && rival == 0)
                result.Winner = Side.Prey;
            else
                result.Winner = Side.Neutral;

            // Write the outcome back into the wolves.
            for (int i = 0; i < m_fighters.Count; i++)
            {
                Combatant fighter = m_fighters[i];

                if (fighter.Wolf == null)
                    continue;

                fighter.ApplyToWolf(fighter.Side == result.Winner);
            }

            result.Narrative = ClosingLine(result);

            m_log.End(result.Narrative);
        }

        private string ClosingLine(BattleResult result)
        {
            if (result.Winner == Side.Neutral)
                return "The fight ends with nobody the clearer.";

            string winner = result.Winner == Side.Pack ? "The pack"
                          : result.Winner == Side.Rival ? "The rivals"
                          : "The prey";

            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.Append(winner + " holds the field after " + result.Rounds + " rounds");

            if (result.Dead.Count > 0)
                builder.Append("; " + result.Dead.Count + " dead");

            if (result.Fled.Count > 0)
                builder.Append(", " + result.Fled.Count + " fled");

            builder.Append(".");

            return builder.ToString();
        }

        public override string ToString()
        {
            return "CombatEngine (" + m_fighters.Count + " fighters)";
        }
    }
}

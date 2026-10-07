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

using LifeEngine.Biology;

namespace LifeEngine.Combat
{
    /// <summary>Which side a fighter is on.</summary>
    public enum Side
    {
        /// <summary>The pack the player is following.</summary>
        Pack,
        /// <summary>Another pack, a rival, an intruder.</summary>
        Rival,
        /// <summary>An animal being hunted.</summary>
        Prey,
        /// <summary>Neither side: for a duel with no stakes.</summary>
        Neutral
    }

    /// <summary>How a fighter is doing.</summary>
    public enum CombatState
    {
        Ready,
        Fighting,
        Wounded,
        Fleeing,
        Down,
        Dead
    }

    /// <summary>
    /// A creature in a fight.
    ///
    /// It is built from a wolf's traits and then owns its own derived numbers,
    /// so effects can move them without writing back into the wolf until the
    /// battle is over. That separation is what lets the same wolf fight several
    /// battles in a session without its permanent stats drifting.
    /// </summary>
    public sealed class Combatant
    {
        private string m_name;
        private Side m_side;
        private CombatState m_state;
        private Wolf m_wolf;

        // Base numbers, taken from the wolf at construction.
        private double m_strength;
        private double m_speed;
        private double m_aggression;
        private double m_defence;
        private double m_accuracy;
        private double m_evasion;
        private double m_initiative;
        private double m_damage;
        private double m_damageReduction;
        private double m_healing;
        private double m_morale;

        // Pools.
        private double m_health;
        private double m_maxHealth;
        private double m_stamina;
        private double m_maxStamina;
        private double m_staminaRegeneration;

        private double m_mass;
        private double m_age;
        private bool m_isCub;
        private bool m_isElder;
        private bool m_isCursed;
        private bool m_isShapeshifter;
        private double m_humanRetention;

        private EffectCollection m_effects;
        private List<string> m_wounds;

        // What it has done in this fight.
        private int m_blowsStruck;
        private int m_blowsTaken;
        private int m_damageDealt;
        private int m_damageTaken;
        private int m_kills;
        private bool m_hasFled;

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public Side Side
        {
            get { return m_side; }
            set { m_side = value; }
        }

        public CombatState State
        {
            get { return m_state; }
            set { m_state = value; }
        }

        /// <summary>The wolf behind it, or null for a creature with no model.</summary>
        public Wolf Wolf
        {
            get { return m_wolf; }
        }

        /// <summary>Everything currently affecting it.</summary>
        public EffectCollection Effects
        {
            get { return m_effects; }
        }

        /// <summary>The injuries it has taken, as text.</summary>
        public IList<string> Wounds
        {
            get { return m_wounds; }
        }

        public double Health
        {
            get { return m_health; }
        }

        public double MaxHealth
        {
            get { return m_maxHealth; }
        }

        public double Stamina
        {
            get { return m_stamina; }
        }

        public double MaxStamina
        {
            get { return m_maxStamina; }
        }

        /// <summary>Health as a fraction, for a bar.</summary>
        public double HealthFraction
        {
            get { return m_maxHealth <= 0.0 ? 0.0 : m_health / m_maxHealth; }
        }

        public double StaminaFraction
        {
            get { return m_maxStamina <= 0.0 ? 0.0 : m_stamina / m_maxStamina; }
        }

        /// <summary>True while it still has health and has not fled.</summary>
        public bool CanFight
        {
            get { return m_health > 0.0 && !m_hasFled && m_state != CombatState.Dead; }
        }

        public bool IsDead
        {
            get { return m_health <= 0.0 || m_state == CombatState.Dead; }
        }

        public bool HasFled
        {
            get { return m_hasFled; }
        }

        public int BlowsStruck
        {
            get { return m_blowsStruck; }
        }

        public int BlowsTaken
        {
            get { return m_blowsTaken; }
        }

        public int DamageDealt
        {
            get { return m_damageDealt; }
        }

        public int DamageTaken
        {
            get { return m_damageTaken; }
        }

        public int Kills
        {
            get { return m_kills; }
        }

        public double Mass
        {
            get { return m_mass; }
        }

        public bool IsCursed
        {
            get { return m_isCursed; }
        }

        public bool IsShapeshifter
        {
            get { return m_isShapeshifter; }
        }

        public double HumanRetention
        {
            get { return m_humanRetention; }
        }

        public Combatant(string name)
        {
            m_name = name;
            m_side = Side.Neutral;
            m_state = CombatState.Ready;

            m_strength = 0.5;
            m_speed = 0.5;
            m_aggression = 0.5;
            m_defence = 0.5;
            m_accuracy = 0.6;
            m_evasion = 0.3;
            m_initiative = 0.5;
            m_damage = 0.5;
            m_damageReduction = 0.0;
            m_healing = 0.5;
            m_morale = 1.0;

            m_maxHealth = 100.0;
            m_health = 100.0;
            m_maxStamina = 100.0;
            m_stamina = 100.0;
            m_staminaRegeneration = 0.1;

            m_mass = 40.0;
            m_age = 3.0;

            m_effects = new EffectCollection();
            m_wounds = new List<string>();
        }

        // ------------------------------------------------------------------
        // Building a fighter from a wolf
        // ------------------------------------------------------------------

        /// <summary>
        /// Derives every combat number from a wolf's traits.
        ///
        /// The mapping is deliberately legible rather than clever: strength sets
        /// damage, mass sets health, healing sets regeneration, and the traits
        /// that have no combat meaning are simply not read.
        /// </summary>
        public static Combatant FromWolf(Wolf wolf, Side side)
        {
            if (wolf == null)
                return null;

            Combatant fighter = new Combatant(wolf.Name);

            fighter.m_wolf = wolf;
            fighter.m_side = side;
            fighter.m_state = wolf.IsAlive ? CombatState.Ready : CombatState.Dead;

            WerewolfTraits traits = wolf.Traits;

            fighter.m_strength = traits.Strength;
            fighter.m_speed = traits.Speed;
            fighter.m_aggression = traits.Aggression;
            fighter.m_healing = traits.Healing;
            fighter.m_isCursed = traits.IsCursed;
            fighter.m_isShapeshifter = traits.IsShapeshifter;
            fighter.m_humanRetention = traits.HumanRetention;

            // A big wolf is harder to move and harder to hurt.
            fighter.m_mass = wolf.MassKilograms;
            fighter.m_age = wolf.AgeYears;
            fighter.m_isCub = wolf.IsCub();
            fighter.m_isElder = wolf.IsElder();

            fighter.m_defence = 0.3 + traits.Strength * 0.4;
            fighter.m_damage = 0.4 + traits.Strength * 0.8;
            fighter.m_accuracy = 0.5 + traits.Scent * 0.3;
            fighter.m_evasion = 0.2 + traits.Speed * 0.4;
            fighter.m_initiative = 0.3 + traits.Speed * 0.5 + traits.NightSight * 0.2;

            // Scars are experience: a scarred wolf has learnt where to stand.
            fighter.m_damageReduction = Math.Min(0.4, traits.ScarCount * 0.02);

            // Health scales with mass, so a cub is fragile and a large wolf is
            // not. Forty kilograms is the baseline.
            fighter.m_maxHealth = 60.0 + wolf.MassKilograms * 1.6;
            fighter.m_maxStamina = 50.0 + traits.Speed * 80.0;
            fighter.m_staminaRegeneration = 0.05 + traits.Healing * 0.15;

            // Morale starts at full, less whatever the wolf is carrying.
            fighter.m_morale = 0.5 + wolf.Health * 0.5;

            if (wolf.IsCub())
            {
                fighter.m_maxHealth *= 0.4;
                fighter.m_maxStamina *= 0.5;
                fighter.m_damage *= 0.3;
                fighter.m_morale *= 0.7;
            }
            else if (wolf.IsElder())
            {
                fighter.m_maxHealth *= 0.85;
                fighter.m_maxStamina *= 0.7;
                fighter.m_initiative *= 0.7;
                fighter.m_evasion *= 0.7;
            }

            // The moon's grip: a wolf with no humanity left is stronger and
            // harder to reason with, which in a fight is an advantage.
            if (traits.IsCursed && traits.HumanRetention <= 0.1)
            {
                fighter.m_strength += 0.05;
                fighter.m_aggression += 0.1;
                fighter.m_morale += 0.1;
            }

            fighter.m_health = fighter.m_maxHealth;
            fighter.m_stamina = fighter.m_maxStamina;

            // Wounds the wolf is already carrying carry into the fight.
            if (wolf.Health < 1.0)
                fighter.m_health = fighter.m_maxHealth * wolf.Health;

            return fighter;
        }

        /// <summary>A plain fighter with numbers set by hand, for a beast or a rival.</summary>
        public static Combatant Plain(string name, Side side, double strength,
                                      double speed, double mass)
        {
            Combatant fighter = new Combatant(name);

            fighter.m_side = side;
            fighter.m_strength = strength;
            fighter.m_speed = speed;
            fighter.m_mass = mass;
            fighter.m_defence = 0.3 + strength * 0.4;
            fighter.m_damage = 0.4 + strength * 0.8;
            fighter.m_accuracy = 0.6;
            fighter.m_evasion = 0.2 + speed * 0.4;
            fighter.m_initiative = 0.3 + speed * 0.5;
            fighter.m_maxHealth = 60.0 + mass * 1.6;
            fighter.m_maxStamina = 50.0 + speed * 80.0;
            fighter.m_staminaRegeneration = 0.1;
            fighter.m_health = fighter.m_maxHealth;
            fighter.m_stamina = fighter.m_maxStamina;

            return fighter;
        }

        // ------------------------------------------------------------------
        // Derived numbers
        // ------------------------------------------------------------------

        /// <summary>
        /// One stat, with every effect on it applied. This is the only way the
        /// engine reads a stat, so an effect cannot be forgotten somewhere.
        ///
        /// The result is clamped to sane limits: an evasion above 0.95 would make
        /// a creature untouchable and a damage reduction above 0.8 would make it
        /// immortal, and neither makes for a fight.
        /// </summary>
        public double GetStat(EffectTarget target)
        {
            double bonus = m_effects.TotalFor(target);
            double value;

            switch (target)
            {
                case EffectTarget.Strength: value = m_strength + bonus; break;
                case EffectTarget.Speed: value = m_speed + bonus; break;
                case EffectTarget.Aggression: value = m_aggression + bonus; break;
                case EffectTarget.Defence: value = m_defence + bonus; break;
                case EffectTarget.Accuracy: value = m_accuracy + bonus; break;
                case EffectTarget.Evasion: value = m_evasion + bonus; break;
                case EffectTarget.Initiative: value = m_initiative + bonus; break;
                case EffectTarget.Damage: value = m_damage + bonus; break;
                case EffectTarget.DamageReduction: value = m_damageReduction + bonus; break;
                case EffectTarget.Healing: value = m_healing + bonus; break;
                case EffectTarget.MaxHealth: value = m_maxHealth + bonus; break;
                case EffectTarget.MaxStamina: value = m_maxStamina + bonus; break;
                case EffectTarget.StaminaRegeneration: value = m_staminaRegeneration + bonus; break;
                case EffectTarget.Morale: value = m_morale + bonus; break;
                case EffectTarget.Fear: value = bonus; break;
                case EffectTarget.Bleed: value = bonus; break;
                case EffectTarget.Poison: value = bonus; break;
                default: value = bonus; break;
            }

            return Clamp(value, target);
        }

        private static double Clamp(double value, EffectTarget target)
        {
            switch (target)
            {
                case EffectTarget.Evasion:
                    return Limit(value, 0.0, 0.95);

                case EffectTarget.Accuracy:
                    return Limit(value, 0.05, 1.0);

                case EffectTarget.DamageReduction:
                    return Limit(value, 0.0, 0.8);

                case EffectTarget.Defence:
                    return Limit(value, 0.0, 1.5);

                case EffectTarget.Morale:
                    return Limit(value, 0.0, 2.0);

                case EffectTarget.Fear:
                    return Limit(value, 0.0, 1.0);

                default:
                    return Limit(value, 0.0, 5.0);
            }
        }

        private static double Limit(double value, double minimum, double maximum)
        {
            if (value < minimum)
                return minimum;

            if (value > maximum)
                return maximum;

            return value;
        }

        // ------------------------------------------------------------------
        // Damage and healing
        // ------------------------------------------------------------------

        /// <summary>
        /// Takes damage. The reduction from armour, scars and effects is applied
        /// first, and any effect that a hit shakes off is removed after.
        /// Returns the damage actually taken.
        /// </summary>
        public int TakeDamage(int raw)
        {
            if (raw <= 0)
                return 0;

            double reduction = GetStat(EffectTarget.DamageReduction);
            int taken = (int)Math.Round(raw * (1.0 - reduction));

            if (taken < 1)
                taken = 1;

            m_health -= taken;

            if (m_health < 0.0)
                m_health = 0.0;

            m_blowsTaken++;
            m_damageTaken += taken;

            // A hit steadies a wavering fighter and shakes off a trance.
            m_effects.BreakOnDamage();

            if (m_health <= 0.0)
                m_state = CombatState.Dead;
            else if (m_health < m_maxHealth * 0.3)
                m_state = CombatState.Wounded;

            return taken;
        }

        /// <summary>Puts health back, up to the maximum.</summary>
        public int Heal(int amount)
        {
            if (amount <= 0 || IsDead)
                return 0;

            double before = m_health;

            m_health += amount;

            if (m_health > m_maxHealth)
                m_health = m_maxHealth;

            return (int)Math.Round(m_health - before);
        }

        /// <summary>
        /// The end-of-round recovery: regeneration from healing, minus whatever
        /// is bleeding or poisoned, and stamina back.
        /// </summary>
        public void Recover()
        {
            if (IsDead)
                return;

            int regenerated = Heal((int)Math.Round(GetStat(EffectTarget.Healing) * 3.0));

            double drain = m_effects.DamagePerRound();

            if (drain > 0.0)
            {
                int lost = (int)Math.Round(drain);

                if (lost < 1)
                    lost = 1;

                m_health -= lost;

                if (m_health < 0.0)
                    m_health = 0.0;

                m_damageTaken += lost;

                if (m_health <= 0.0)
                    m_state = CombatState.Dead;
            }

            double staminaBack = GetStat(EffectTarget.StaminaRegeneration) * 10.0;

            m_stamina += staminaBack;

            if (m_stamina > GetStat(EffectTarget.MaxStamina))
                m_stamina = GetStat(EffectTarget.MaxStamina);
        }

        /// <summary>Spends stamina. Returns false if there was not enough.</summary>
        public bool SpendStamina(double amount)
        {
            if (m_stamina < amount)
                return false;

            m_stamina -= amount;

            return true;
        }

        /// <summary>Records a blow this fighter struck.</summary>
        public void RecordBlow(int damage)
        {
            m_blowsStruck++;
            m_damageDealt += damage;
        }

        public void RecordKill()
        {
            m_kills++;
        }

        /// <summary>Notes an injury, for the report.</summary>
        public void AddWound(string wound)
        {
            if (!string.IsNullOrEmpty(wound))
                m_wounds.Add(wound);
        }

        /// <summary>Gives up: out of the fight but alive.</summary>
        public void Flee()
        {
            m_hasFled = true;
            m_state = CombatState.Fleeing;
        }

        /// <summary>
        /// Adds to the health pool, for a creature that is simply tougher than
        /// its mass suggests — a bear, say. The pool is topped up as well, so
        /// the bonus is not wasted on a fighter that has already been built.
        /// </summary>
        public void HealthBonus(int bonus)
        {
            if (bonus == 0)
                return;

            m_maxHealth += bonus;
            m_health += bonus;

            if (m_health > m_maxHealth)
                m_health = m_maxHealth;

            if (m_health < 1.0)
                m_health = 1.0;
        }

        /// <summary>
        /// Writes the outcome back into the wolf: health, energy, the record and
        /// a scar if it lost.
        /// </summary>
        public void ApplyToWolf(bool won)
        {
            if (m_wolf == null)
                return;

            // Health comes back as a fraction, so a mauling leaves the wolf in
            // poor condition after the fight as well as during it.
            m_wolf.Health = HealthFraction;

            // A fight burns reserves.
            m_wolf.Energy = Math.Max(0.0, m_wolf.Energy - 0.2);

            if (IsDead)
            {
                m_wolf.IsAlive = false;
                m_wolf.Health = 0.0;
                return;
            }

            m_wolf.RecordFight(won);

            for (int i = 0; i < m_kills; i++)
                m_wolf.RecordKill();
        }

        /// <summary>A one-line summary, for a roster.</summary>
        public string Summary()
        {
            return m_name + " (" + m_side + ", " + Math.Round(HealthFraction * 100.0) + "% health, " +
                   m_state + ")";
        }

        public override string ToString()
        {
            return Summary();
        }
    }
}

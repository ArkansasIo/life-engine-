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

namespace LifeEngine.Biology
{
    /// <summary>
    /// The rank an individual holds in its pack. Rank is not a property of the
    /// animal: it is a position in a dominance order that shifts when an
    /// individual dies or is beaten.
    /// </summary>
    public enum PackRank
    {
        /// <summary>Too young to hold rank.</summary>
        Cub,
        /// <summary>Bottom of the adult order.</summary>
        Omega,
        /// <summary>A middling member.</summary>
        Beta,
        /// <summary>Second in command, and the next mate.</summary>
        Second,
        /// <summary>The wolf that leads the hunt and takes the first share.</summary>
        Alpha
    }

    /// <summary>
    /// How a wolf's body and senses are doing. A wolf that cannot hear a
    /// retreat or will not back off from a losing fight is a wolf that dies,
    /// so these are not decoration: the fight and hunt logic reads them.
    /// </summary>
    public sealed class WerewolfTraits
    {
        private double m_strength;
        private double m_speed;
        private double m_aggression;
        private double m_hearing;
        private double m_scent;
        private double m_nightSight;
        private double m_healing;
        private int m_scarCount;
        private bool m_isCursed;
        private bool m_isShapeshifter;
        private double m_humanRetention;

        /// <summary>Raw physical power, 0 to 1. Decides fights and the first share.</summary>
        public double Strength
        {
            get { return m_strength; }
            set { m_strength = Clamp01(value); }
        }

        /// <summary>Running speed, 0 to 1. Decides who catches whom.</summary>
        public double Speed
        {
            get { return m_speed; }
            set { m_speed = Clamp01(value); }
        }

        /// <summary>Willingness to start and to continue a fight, 0 to 1.</summary>
        public double Aggression
        {
            get { return m_aggression; }
            set { m_aggression = Clamp01(value); }
        }

        /// <summary>How far a howl carries and how well a challenge is heard.</summary>
        public double Hearing
        {
            get { return m_hearing; }
            set { m_hearing = Clamp01(value); }
        }

        /// <summary>Ability to find prey and to track a wounded animal.</summary>
        public double Scent
        {
            get { return m_scent; }
            set { m_scent = Clamp01(value); }
        }

        /// <summary>How well it hunts on a dark tile.</summary>
        public double NightSight
        {
            get { return m_nightSight; }
            set { m_nightSight = Clamp01(value); }
        }

        /// <summary>How quickly wounds close between ticks.</summary>
        public double Healing
        {
            get { return m_healing; }
            set { m_healing = Clamp01(value); }
        }

        /// <summary>How many fights this wolf has survived. Scars carry status.</summary>
        public int ScarCount
        {
            get { return m_scarCount; }
            set { m_scarCount = value < 0 ? 0 : value; }
        }

        /// <summary>Whether the moon binds it.</summary>
        public bool IsCursed
        {
            get { return m_isCursed; }
            set { m_isCursed = value; }
        }

        /// <summary>Whether it can hold a man's shape at all.</summary>
        public bool IsShapeshifter
        {
            get { return m_isShapeshifter; }
            set { m_isShapeshifter = value; }
        }

        /// <summary>
        /// How much of the human mind survives the change, 0 to 1. High is a
        /// wolf that still knows its name; low is a wolf that does not.
        /// </summary>
        public double HumanRetention
        {
            get { return m_humanRetention; }
            set { m_humanRetention = Clamp01(value); }
        }

        public WerewolfTraits()
        {
            m_strength = 0.5;
            m_speed = 0.5;
            m_aggression = 0.5;
            m_hearing = 0.7;
            m_scent = 0.8;
            m_nightSight = 0.8;
            m_healing = 0.5;
            m_isCursed = true;
            m_isShapeshifter = true;
            m_humanRetention = 0.5;
        }

        /// <summary>
        /// A single number for "how dangerous is this wolf", used to seed the
        /// dominance order when a pack is founded.
        /// </summary>
        public double Dominance()
        {
            return (m_strength * 0.5) +
                   (m_aggression * 0.25) +
                   (m_speed * 0.15) +
                   (m_scarCount * 0.01);
        }

        private static double Clamp01(double value)
        {
            if (value < 0.0)
                return 0.0;

            if (value > 1.0)
                return 1.0;

            return value;
        }

        public override string ToString()
        {
            return "str " + Math.Round(m_strength, 2) +
                   ", aggr " + Math.Round(m_aggression, 2) +
                   ", speed " + Math.Round(m_speed, 2);
        }
    }

    /// <summary>
    /// One wolf. An individual, not a species: it has a name, an age, a body,
    /// wounds and a place in a pack.
    ///
    /// The wolf does not decide anything itself. <see cref="WolfPack"/> and
    /// <see cref="LifeEngine.Wolves.WolfBehaviour"/> hold the decisions, because a
    /// dominance order and a hunt are properties of the group, not of one animal.
    /// </summary>
    public sealed class Wolf
    {
        private string m_name;
        private int m_id;
        private Wolf m_mother;
        private Wolf m_father;
        private PackRank m_rank;
        private double m_ageYears;
        private double m_massKilograms;
        private double m_health;
        private double m_energy;
        private bool m_isAlive;
        private bool m_isPregnant;
        private double m_gestationYears;
        private int m_kills;
        private int m_dominanceWins;
        private int m_dominanceLosses;
        private WerewolfTraits m_traits;
        private double m_humanFormYears;

        /// <summary>The name the wolf answers to, or is remembered by.</summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>A number unique within a world, for tracking.</summary>
        public int Id
        {
            get { return m_id; }
        }

        public Wolf Mother
        {
            get { return m_mother; }
        }

        public Wolf Father
        {
            get { return m_father; }
        }

        public PackRank Rank
        {
            get { return m_rank; }
            set { m_rank = value; }
        }

        public double AgeYears
        {
            get { return m_ageYears; }
            set { m_ageYears = value < 0.0 ? 0.0 : value; }
        }

        public double MassKilograms
        {
            get { return m_massKilograms; }
            set { m_massKilograms = value < 0.0 ? 0.0 : value; }
        }

        /// <summary>Body condition, 0 (dying) to 1 (unhurt).</summary>
        public double Health
        {
            get { return m_health; }
            set { m_health = value < 0.0 ? 0.0 : (value > 1.0 ? 1.0 : value); }
        }

        /// <summary>Reserves, 0 (starving) to 1 (fed). Spent by hunting and healing.</summary>
        public double Energy
        {
            get { return m_energy; }
            set { m_energy = value < 0.0 ? 0.0 : (value > 1.0 ? 1.0 : value); }
        }

        public bool IsAlive
        {
            get { return m_isAlive; }
            set { m_isAlive = value; }
        }

        public bool IsPregnant
        {
            get { return m_isPregnant; }
            set { m_isPregnant = value; }
        }

        public double GestationYears
        {
            get { return m_gestationYears; }
            set { m_gestationYears = value < 0.0 ? 0.0 : value; }
        }

        /// <summary>Prey brought down by this wolf, ever.</summary>
        public int Kills
        {
            get { return m_kills; }
        }

        /// <summary>Dominance fights this wolf has won.</summary>
        public int DominanceWins
        {
            get { return m_dominanceWins; }
        }

        /// <summary>Dominance fights this wolf has lost.</summary>
        public int DominanceLosses
        {
            get { return m_dominanceLosses; }
        }

        public WerewolfTraits Traits
        {
            get { return m_traits; }
        }

        /// <summary>Years spent in human form, if it can take one.</summary>
        public double HumanFormYears
        {
            get { return m_humanFormYears; }
            set { m_humanFormYears = value < 0.0 ? 0.0 : value; }
        }

        public Wolf(string name, int id)
        {
            m_name = name;
            m_id = id;
            m_rank = PackRank.Omega;
            m_ageYears = 0.0;
            m_massKilograms = 40.0;
            m_health = 1.0;
            m_energy = 1.0;
            m_isAlive = true;
            m_traits = new WerewolfTraits();
        }

        /// <summary>A cub of two parents, inheriting the middle of both bodies.</summary>
        public static Wolf Breed(string name, int id, Wolf mother, Wolf father, Random random)
        {
            Wolf cub = new Wolf(name, id);

            cub.m_mother = mother;
            cub.m_father = father;
            cub.m_rank = PackRank.Cub;
            cub.m_massKilograms = 4.0;

            if (mother != null && father != null && random != null)
            {
                cub.m_traits.Strength = Blend(mother.Traits.Strength, father.Traits.Strength, random);
                cub.m_traits.Speed = Blend(mother.Traits.Speed, father.Traits.Speed, random);
                cub.m_traits.Aggression = Blend(mother.Traits.Aggression, father.Traits.Aggression, random);
                cub.m_traits.Hearing = Blend(mother.Traits.Hearing, father.Traits.Hearing, random);
                cub.m_traits.Scent = Blend(mother.Traits.Scent, father.Traits.Scent, random);
                cub.m_traits.NightSight = Blend(mother.Traits.NightSight, father.Traits.NightSight, random);
                cub.m_traits.Healing = Blend(mother.Traits.Healing, father.Traits.Healing, random);
                cub.m_traits.HumanRetention = Blend(mother.Traits.HumanRetention, father.Traits.HumanRetention, random);
            }

            return cub;
        }

        /// <summary>The average of two values, nudged a little either way.</summary>
        private static double Blend(double left, double right, Random random)
        {
            double middle = (left + right) / 2.0;
            double noise = (random.NextDouble() - 0.5) * 0.1;

            double result = middle + noise;

            if (result < 0.0)
                return 0.0;

            if (result > 1.0)
                return 1.0;

            return result;
        }

        /// <summary>True if this wolf is old enough to hold a rank above cub.</summary>
        public bool IsAdult()
        {
            return m_ageYears >= 1.0;
        }

        public bool IsCub()
        {
            return m_rank == PackRank.Cub || m_ageYears < 1.0;
        }

        /// <summary>A wolf past its prime: slower, and loses fights it would have won.</summary>
        public bool IsElder()
        {
            return m_ageYears >= 8.0;
        }

        /// <summary>
        /// How likely this wolf is to win a fight, before luck. Health, energy,
        /// age and scars all count.
        /// </summary>
        public double FightingPower()
        {
            double power = m_traits.Dominance();

            power *= 0.4 + m_health * 0.4 + m_energy * 0.2;

            if (IsElder())
                power *= 0.75;

            return power;
        }

        /// <summary>Records the outcome of a fight and the scar of losing one.</summary>
        public void RecordFight(bool won)
        {
            if (won)
            {
                m_dominanceWins++;
                return;
            }

            m_dominanceLosses++;
            m_traits.ScarCount = m_traits.ScarCount + 1;
            m_health = m_health - 0.1;
        }

        public void RecordKill()
        {
            m_kills++;
        }

        /// <summary>
        /// Heals, feeds and ages the wolf by a slice of a year. A wolf with no
        /// energy loses health; a wolf with health and energy recovers.
        /// </summary>
        public void Advance(double years, double foodShare)
        {
            m_ageYears = m_ageYears + years;

            m_energy = m_energy + (foodShare - 0.25);

            if (m_energy > 1.0)
                m_energy = 1.0;

            if (m_energy < 0.0)
                m_energy = 0.0;

            if (m_energy <= 0.0)
                m_health = m_health - 0.15;
            else
                m_health = m_health + m_traits.Healing * 0.05;

            if (m_health > 1.0)
                m_health = 1.0;

            if (m_health <= 0.0)
            {
                m_health = 0.0;
                m_isAlive = false;
            }

            // A grown wolf puts on mass with age, up to a limit.
            if (m_ageYears < 2.0)
                m_massKilograms = 4.0 + (m_ageYears / 2.0) * 36.0;
            else
                m_massKilograms = 40.0 + m_traits.Strength * 12.0;

            if (m_isPregnant)
            {
                m_gestationYears = m_gestationYears + years;

                if (m_gestationYears >= 0.25)
                {
                    m_isPregnant = false;
                    m_gestationYears = 0.0;
                }
            }
        }

        /// <summary>How many cubs a pregnancy is worth: healthier mothers, more cubs.</summary>
        public int LitterSize(Random random)
        {
            if (random == null)
                return 1;

            int baseCubs = 2 + (int)(m_health * 4.0);
            int born = baseCubs - 1 + random.Next(2);

            if (born < 1)
                born = 1;

            return born;
        }

        public override string ToString()
        {
            return m_name + " (" + m_rank + ", " + Math.Round(m_ageYears, 1) + "y)";
        }
    }

    /// <summary>
    /// A pack of wolves, with an alpha at the head of it.
    ///
    /// The pack owns the dominance order. Ranks are rebuilt rather than
    /// assigned: any wolf that wins enough fights climbs, any wolf that loses
    /// falls, and the alpha is whoever is on top when the order is read.
    /// </summary>
    public sealed class WolfPack
    {
        private string m_name;
        private string m_territory;
        private List<Wolf> m_members;
        private List<Wolf> m_dead;
        private Wolf m_alpha;
        private Wolf m_alphaMate;
        private int m_nextId;
        private double m_cohesion;
        private int m_generationNumber;

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>Where the pack hunts, e.g. "Boreal Forest" or a tile name.</summary>
        public string Territory
        {
            get { return m_territory; }
            set { m_territory = value; }
        }

        /// <summary>The living members.</summary>
        public IList<Wolf> Members
        {
            get { return m_members; }
        }

        /// <summary>The wolves this pack has lost, for a history view.</summary>
        public IList<Wolf> Dead
        {
            get { return m_dead; }
        }

        /// <summary>The wolf at the head of the order, or null if there is none.</summary>
        public Wolf Alpha
        {
            get { return m_alpha; }
        }

        /// <summary>The alpha's mate, the only wolf that breeds with it.</summary>
        public Wolf AlphaMate
        {
            get { return m_alphaMate; }
        }

        /// <summary>
        /// How tightly the pack holds together, 0 to 1. A broken pack hunts
        /// badly and splinters into rivals.
        /// </summary>
        public double Cohesion
        {
            get { return m_cohesion; }
            set { m_cohesion = value < 0.0 ? 0.0 : (value > 1.0 ? 1.0 : value); }
        }

        /// <summary>How many times the alpha has changed. A high number is a troubled pack.</summary>
        public int GenerationNumber
        {
            get { return m_generationNumber; }
        }

        public WolfPack(string name, string territory)
        {
            m_name = name;
            m_territory = territory;
            m_members = new List<Wolf>();
            m_dead = new List<Wolf>();
            m_nextId = 1;

            // A pack is steadier when its wolves know each other: cohesion is
            // the share of the order every wolf accepts. It drifts with fights,
            // births and losses, so it starts near the middle.
            m_cohesion = 0.5;
        }

        /// <summary>
        /// Founds a pack around a single wolf, which becomes alpha by default
        /// and takes a name for the pack.
        /// </summary>
        public Wolf Found(string alphaName)
        {
            Wolf alpha = new Wolf(alphaName, NextId());

            alpha.Rank = PackRank.Alpha;
            alpha.AgeYears = 4.0;
            alpha.Traits.Strength = 0.75;
            alpha.Traits.Aggression = 0.7;

            m_members.Add(alpha);
            m_alpha = alpha;

            return alpha;
        }

        /// <summary>Adds a wolf, letting the dominance order settle afterwards.</summary>
        public Wolf Recruit(string name)
        {
            Wolf wolf = new Wolf(name, NextId());

            return Recruit(wolf);
        }

        public Wolf Recruit(Wolf wolf)
        {
            if (wolf == null || m_members.Contains(wolf))
                return null;

            wolf.IsAlive = true;

            if (wolf.Rank == PackRank.Alpha && m_alpha != null)
                wolf.Rank = PackRank.Omega;

            m_members.Add(wolf);
            SettleRanks();

            return wolf;
        }

        /// <summary>Every living wolf of a given rank.</summary>
        public List<Wolf> GetByRank(PackRank rank)
        {
            List<Wolf> result = new List<Wolf>();

            for (int i = 0; i < m_members.Count; i++)
            {
                if (m_members[i].Rank == rank && m_members[i].IsAlive)
                    result.Add(m_members[i]);
            }

            return result;
        }

        /// <summary>Every living wolf that is not a cub.</summary>
        public List<Wolf> GetAdults()
        {
            List<Wolf> adults = new List<Wolf>();

            for (int i = 0; i < m_members.Count; i++)
            {
                if (m_members[i].IsAlive && m_members[i].IsAdult())
                    adults.Add(m_members[i]);
            }

            return adults;
        }

        public int Count
        {
            get
            {
                int alive = 0;

                for (int i = 0; i < m_members.Count; i++)
                {
                    if (m_members[i].IsAlive)
                        alive++;
                }

                return alive;
            }
        }

        // ------------------------------------------------------------------
        // Dominance
        // ------------------------------------------------------------------

        /// <summary>
        /// Rebuilds the dominance order from the wolves' bodies and records. The
        /// strongest wolf present is alpha; the weakest is omega.
        /// </summary>
        public void SettleRanks()
        {
            List<Wolf> order = GetAdults();

            order.Sort(delegate (Wolf left, Wolf right)
            {
                double leftPower = left.FightingPower();
                double rightPower = right.FightingPower();

                return rightPower.CompareTo(leftPower);
            });

            if (order.Count == 0)
            {
                m_alpha = null;
                m_alphaMate = null;
                return;
            }

            Wolf previous = m_alpha;

            for (int i = 0; i < order.Count; i++)
            {
                PackRank rank;

                if (i == 0)
                    rank = PackRank.Alpha;
                else if (i == 1)
                    rank = PackRank.Second;
                else if (i == order.Count - 1 && order.Count > 2)
                    rank = PackRank.Omega;
                else
                    rank = PackRank.Beta;

                if (order[i].Rank == PackRank.Alpha && i != 0)
                    m_generationNumber++;

                order[i].Rank = rank;
            }

            m_alpha = order[0];

            if (previous != null && previous != m_alpha)
                m_generationNumber++;

            m_alphaMate = order.Count > 1 ? order[1] : null;
        }

        /// <summary>
        /// Runs one challenge between two wolves. The winner takes the higher
        /// rank; the loser takes a scar.
        ///
        /// The chance is not the raw power: it is the power plus a push from
        /// aggression, plus a share of the pack's cohesion for the wolf that is
        /// already higher, because a pack backs its own order.
        /// </summary>
        public Wolf Challenge(Wolf challenger, Wolf defender, Random random)
        {
            if (challenger == null || defender == null || random == null)
                return null;

            if (!m_members.Contains(challenger) || !m_members.Contains(defender))
                return null;

            double challengerPower = challenger.FightingPower();
            challengerPower += challenger.Traits.Aggression * 0.15;
            challengerPower += m_cohesion * 0.05;

            double defenderPower = defender.FightingPower();
            defenderPower += defender.Traits.Aggression * 0.10;
            defenderPower += (defender.Rank == PackRank.Alpha ? 0.2 : 0.0);
            defenderPower += m_cohesion * 0.10;

            double total = challengerPower + defenderPower;

            double challengerChance = total <= 0.0 ? 0.5 : challengerPower / total;

            bool challengerWins = random.NextDouble() < challengerChance;

            Wolf winner = challengerWins ? challenger : defender;
            Wolf loser = challengerWins ? defender : challenger;

            winner.RecordFight(true);
            loser.RecordFight(false);

            if (challengerWins)
            {
                PackRank taken = defender.Rank;

                if (taken == PackRank.Alpha)
                    m_generationNumber++;

                defender.Rank = challenger.Rank == PackRank.Cub ? PackRank.Omega : challenger.Rank;
                challenger.Rank = taken;

                // A pack whose alpha was beaten is unsettled for a while.
                m_cohesion = m_cohesion - 0.1;
            }
            else
            {
                m_cohesion = m_cohesion + 0.05;
            }

            SettleRanks();

            return winner;
        }

        /// <summary>
        /// Runs a round of challenges among the wolves that are willing to try.
        /// Only a wolf with the aggression for it challenges, and only upward.
        /// </summary>
        public List<Wolf> HoldDominanceRound(Random random)
        {
            List<Wolf> winners = new List<Wolf>();

            if (random == null)
                return winners;

            List<Wolf> adults = GetAdults();

            for (int i = adults.Count - 1; i >= 0; i--)
            {
                Wolf challenger = adults[i];

                if (challenger.Rank == PackRank.Alpha)
                    continue;

                if (random.NextDouble() > challenger.Traits.Aggression * 0.25)
                    continue;

                // Challenge the wolf directly above, not the alpha straight off.
                Wolf defender = GetNextAbove(challenger);

                if (defender == null)
                    continue;

                Wolf winner = Challenge(challenger, defender, random);

                if (winner != null)
                    winners.Add(winner);
            }

            return winners;
        }

        private Wolf GetNextAbove(Wolf wolf)
        {
            List<Wolf> adults = GetAdults();

            adults.Sort(delegate (Wolf left, Wolf right)
            {
                double leftPower = left.FightingPower();
                double rightPower = right.FightingPower();

                return rightPower.CompareTo(leftPower);
            });

            int index = adults.IndexOf(wolf);

            if (index <= 0)
                return null;

            return adults[index - 1];
        }

        // ------------------------------------------------------------------
        // Reproduction
        // ------------------------------------------------------------------

        /// <summary>
        /// The alpha pair breeds, and no one else. Returns the cubs that were
        /// born, which are already members of the pack.
        /// </summary>
        public List<Wolf> BreedSeason(Random random)
        {
            List<Wolf> cubs = new List<Wolf>();

            if (random == null || m_alpha == null)
                return cubs;

            if (!m_alpha.IsAlive || m_alpha.IsCub())
                return cubs;

            Wolf mate = m_alphaMate;

            if (mate == null || !mate.IsAlive)
            {
                // No second wolf: the alpha breeds alone or not at all.
                mate = PickMateFor(m_alpha);
            }

            if (mate == null)
                return cubs;

            if (m_alpha.Health < 0.4 || m_alpha.Energy < 0.4)
                return cubs;

            int litter = m_alpha.LitterSize(random);

            for (int i = 0; i < litter; i++)
            {
                Wolf cub = Wolf.Breed("Cub " + NextId(), PeekId(), m_alpha, mate, random);

                cub.Rank = PackRank.Cub;
                m_members.Add(cub);
                cubs.Add(cub);
            }

            m_alpha.Energy = m_alpha.Energy - 0.3;
            mate.Energy = mate.Energy - 0.3;

            return cubs;
        }

        private Wolf PickMateFor(Wolf wolf)
        {
            List<Wolf> adults = GetAdults();

            for (int i = 0; i < adults.Count; i++)
            {
                if (adults[i] != wolf)
                    return adults[i];
            }

            return null;
        }

        /// <summary>
        /// Ages every wolf by a slice of a year. Kills the ones that run out of
        /// health and moves them to the dead list.
        /// </summary>
        public List<Wolf> Advance(double years, double foodShare, Random random)
        {
            List<Wolf> lost = new List<Wolf>();

            for (int i = 0; i < m_members.Count; i++)
            {
                Wolf wolf = m_members[i];

                if (!wolf.IsAlive)
                    continue;

                wolf.Advance(years, foodShare);

                // Old age: past the limit, every year is a coin toss.
                if (wolf.IsElder())
                {
                    double chance = (wolf.AgeYears - 8.0) * 0.15;

                    if (random != null && random.NextDouble() < chance)
                    {
                        wolf.Health = 0.0;
                        wolf.IsAlive = false;
                    }
                }

                if (!wolf.IsAlive)
                    lost.Add(wolf);
            }

            for (int i = 0; i < lost.Count; i++)
            {
                m_members.Remove(lost[i]);
                m_dead.Add(lost[i]);
            }

            if (lost.Count > 0)
            {
                // Losing wolves breaks a pack's order, so it has to settle again.
                m_cohesion = m_cohesion - 0.05 * lost.Count;

                if (m_cohesion < 0.0)
                    m_cohesion = 0.0;

                SettleRanks();
            }

            return lost;
        }

        /// <summary>
        /// Splits the pack when it grows past what the territory feeds, or when
        /// cohesion has fallen away. The wolves that leave are returned.
        /// </summary>
        public List<Wolf> Disperse(int capacity, Random random)
        {
            List<Wolf> leaving = new List<Wolf>();

            if (random == null || m_alpha == null)
                return leaving;

            if (Count <= capacity && m_cohesion > 0.2)
                return leaving;

            List<Wolf> adults = GetAdults();

            for (int i = 0; i < adults.Count; i++)
            {
                Wolf wolf = adults[i];

                if (wolf == m_alpha || wolf == m_alphaMate || wolf == m_alpha)
                    continue;

                // Low cohesion sends away more wolves than a full belly does.
                double chance = 0.15 + (1.0 - m_cohesion) * 0.4;

                if (Count > capacity)
                    chance += 0.3;

                if (random.NextDouble() < chance)
                    leaving.Add(wolf);
            }

            for (int i = 0; i < leaving.Count; i++)
                m_members.Remove(leaving[i]);

            if (leaving.Count > 0)
            {
                m_cohesion = m_cohesion + 0.1;

                if (m_cohesion > 1.0)
                    m_cohesion = 1.0;

                SettleRanks();
            }

            return leaving;
        }

        /// <summary>Every living pack member, for rendering or a report.</summary>
        public string Describe()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.AppendLine("Pack:      " + m_name + " (" + m_territory + ")");
            builder.AppendLine("Members:   " + Count);
            builder.AppendLine("Cohesion:  " + Math.Round(m_cohesion, 2));
            builder.AppendLine("Alphas changed: " + m_generationNumber);
            builder.AppendLine();
            builder.AppendLine("  Rank        Name              Age  Health  Energy  Kills  Wins/Losses");

            List<Wolf> members = new List<Wolf>(m_members);

            members.Sort(delegate (Wolf left, Wolf right)
            {
                return right.FightingPower().CompareTo(left.FightingPower());
            });

            for (int i = 0; i < members.Count; i++)
            {
                Wolf wolf = members[i];

                builder.AppendLine("  " +
                                   Pad(wolf.Rank.ToString(), 12) +
                                   Pad(wolf.Name, 18) +
                                   Pad(Math.Round(wolf.AgeYears, 1).ToString(), 5) +
                                   Pad(Math.Round(wolf.Health, 2).ToString(), 8) +
                                   Pad(Math.Round(wolf.Energy, 2).ToString(), 8) +
                                   Pad(wolf.Kills.ToString(), 7) +
                                   wolf.DominanceWins + "/" + wolf.DominanceLosses);
            }

            return builder.ToString();
        }

        private static string Pad(string text, int width)
        {
            if (text == null)
                text = string.Empty;

            if (text.Length >= width)
                return text + " ";

            return text + new string(' ', width - text.Length);
        }

        private int NextId()
        {
            int id = m_nextId;
            m_nextId++;
            return id;
        }

        private int PeekId()
        {
            return m_nextId;
        }

        public override string ToString()
        {
            return m_name + " (" + Count + " wolves, alpha " +
                   (m_alpha == null ? "none" : m_alpha.Name) + ")";
        }
    }
}

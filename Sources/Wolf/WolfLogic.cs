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

namespace LifeEngine.WolfLogic
{
    /// <summary>
    /// Something a pack can hunt. Prey is a population on a tile rather than an
    /// individual: a hunt reduces the headcount, it does not track one deer.
    /// </summary>
    public sealed class Prey
    {
        private string m_name;
        private double m_massKilograms;
        private double m_speedKph;
        private double m_defence;
        private int m_population;
        private int m_maxPopulation;
        private double m_growthRate;
        private Biology.BiomeType m_preferredBiome;

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>How much meat one animal is worth.</summary>
        public double MassKilograms
        {
            get { return m_massKilograms; }
            set { m_massKilograms = value < 0.0 ? 0.0 : value; }
        }

        /// <summary>How fast it runs. A pack that is slower catches nothing.</summary>
        public double SpeedKph
        {
            get { return m_speedKph; }
            set { m_speedKph = value < 0.0 ? 0.0 : value; }
        }

        /// <summary>How hard it is to bring down, 0 to 1. Horns and hooves count.</summary>
        public double Defence
        {
            get { return m_defence; }
            set { m_defence = value < 0.0 ? 0.0 : (value > 1.0 ? 1.0 : value); }
        }

        public int Population
        {
            get { return m_population; }
            set { m_population = value < 0 ? 0 : value; }
        }

        public int MaxPopulation
        {
            get { return m_maxPopulation; }
            set { m_maxPopulation = value < 1 ? 1 : value; }
        }

        /// <summary>Fraction the herd grows by each season, when there is room.</summary>
        public double GrowthRate
        {
            get { return m_growthRate; }
            set { m_growthRate = value < 0.0 ? 0.0 : value; }
        }

        public Biology.BiomeType PreferredBiome
        {
            get { return m_preferredBiome; }
            set { m_preferredBiome = value; }
        }

        public Prey(string name, double massKilograms, double speedKph, double defence, int population)
        {
            m_name = name;
            m_massKilograms = massKilograms;
            m_speedKph = speedKph;
            m_defence = defence;
            m_population = population;
            m_maxPopulation = population * 2;
            m_growthRate = 0.08;
            m_preferredBiome = Biology.BiomeType.TemperateDeciduousForest;
        }

        /// <summary>The herd grows toward what the land feeds, and stops there.</summary>
        public int Grow(int capacity)
        {
            if (capacity < 1)
                capacity = 1;

            if (m_population >= capacity)
            {
                // Over the capacity, the herd loses animals.
                int lost = (m_population - capacity) / 4 + 1;
                m_population -= lost;

                if (m_population < 0)
                    m_population = 0;

                return -lost;
            }

            int gained = (int)Math.Round((capacity - m_population) * m_growthRate);

            if (gained < 1)
                gained = 1;

            m_population += gained;

            return gained;
        }

        /// <summary>Takes animals out of the herd. Returns how many were actually taken.</summary>
        public int Take(int count)
        {
            if (count < 0)
                count = 0;

            if (count > m_population)
                count = m_population;

            m_population -= count;

            return count;
        }

        public override string ToString()
        {
            return m_name + " (" + m_population + ")";
        }
    }

    /// <summary>
    /// The result of one hunt: who ate, how much, and what it cost.
    /// </summary>
    public sealed class HuntResult
    {
        private bool m_success;
        private string m_preyName;
        private int m_taken;
        private double m_meatKilograms;
        private List<Wolf> m_hunters;
        private List<Wolf> m_wounded;
        private string m_narrative;

        public bool Success
        {
            get { return m_success; }
        }

        public string PreyName
        {
            get { return m_preyName; }
        }

        /// <summary>Animals taken out of the herd.</summary>
        public int Taken
        {
            get { return m_taken; }
        }

        public double MeatKilograms
        {
            get { return m_meatKilograms; }
        }

        /// <summary>The wolves that took part.</summary>
        public IList<Wolf> Hunters
        {
            get { return m_hunters; }
        }

        /// <summary>The wolves that were hurt doing it.</summary>
        public IList<Wolf> Wounded
        {
            get { return m_wounded; }
        }

        /// <summary>A sentence describing what happened, for a log or a panel.</summary>
        public string Narrative
        {
            get { return m_narrative; }
            set { m_narrative = value; }
        }

        public HuntResult()
        {
            m_hunters = new List<Wolf>();
            m_wounded = new List<Wolf>();
            m_narrative = string.Empty;
            m_preyName = string.Empty;
        }

        internal void Set(bool success, string preyName, int taken, double meat)
        {
            m_success = success;
            m_preyName = preyName;
            m_taken = taken;
            m_meatKilograms = meat;
        }

        public override string ToString()
        {
            return (m_success ? "Hunt succeeded" : "Hunt failed") +
                   " on " + m_preyName + ": " + m_taken + " taken";
        }
    }

    /// <summary>
    /// The decisions. A hunt, a challenge, a howl, a season: each is a method
    /// here that reads the wolves and the land and reports what happened.
    ///
    /// Nothing in this class keeps state. Give it the same wolves and the same
    /// random stream and it does the same thing, which is what makes the whole
    /// system reproducible from a seed.
    /// </summary>
    public static class WolfLogic
    {
        /// <summary>
        /// Runs a hunt against a herd.
        ///
        /// The pack's chance comes from its strongest hunters, the alpha's
        /// leadership, the cohesion of the pack and how dark it is. The herd's
        /// chance comes from the slower of the two: a wolf that cannot outrun the
        /// prey has to catch it tired, and defence subtracts from the kill.
        /// </summary>
        public static HuntResult Hunt(Biology.WolfPack pack, Prey prey, bool isNight, Random random)
        {
            HuntResult result = new HuntResult();

            if (pack == null || prey == null || random == null)
                return result;

            if (prey.Population <= 0)
            {
                result.Narrative = "There is nothing of " + prey.Name + " left to hunt.";
                return result;
            }

            List<Wolf> hunters = pack.GetAdults();

            if (hunters.Count == 0)
            {
                result.Narrative = "The pack has no hunters.";
                return result;
            }

            for (int i = 0; i < hunters.Count; i++)
                result.Hunters.Add(hunters[i]);

            // Best three hunters do the work; the rest mostly watch.
            double packSpeed = 0.0;
            double packPower = 0.0;
            int counted = 0;

            for (int i = 0; i < hunters.Count && counted < 3; i++)
            {
                Wolf hunter = hunters[i];

                if (!hunter.IsAlive || hunter.IsCub())
                    continue;

                packSpeed += hunter.Traits.Speed * SpeedOf(hunter);
                packPower += hunter.FightingPower();
                counted++;
            }

            if (counted == 0)
            {
                result.Narrative = "Only cubs are left; they cannot hunt.";
                return result;
            }

            packSpeed = (packSpeed / counted) * 60.0;
            packPower = packPower / counted;

            if (isNight)
            {
                double nightBonus = 0.0;

                for (int i = 0; i < hunters.Count; i++)
                    nightBonus += hunters[i].Traits.NightSight;

                packSpeed += (nightBonus / hunters.Count) * 12.0;
            }

            // The alpha's condition sets the tone of the hunt.
            Wolf alpha = pack.Alpha;

            if (alpha != null && alpha.IsAlive)
                packPower += alpha.FightingPower() * 0.3 + pack.Cohesion * 0.2;
            else
                packPower *= 0.7;

            // Can the pack catch it at all?
            double speedRatio = packSpeed / Math.Max(1.0, prey.SpeedKph);

            if (speedRatio < 0.8)
            {
                result.Narrative = "A " + prey.Name + " outran the pack.";
                return result;
            }

            double catchChance = Math.Min(0.95, 0.35 + (speedRatio - 0.8) * 0.8);
            double powerChance = Math.Min(0.95, packPower);
            double chance = catchChance * (0.5 + powerChance * 0.5) * (1.0 - prey.Defence * 0.4);

            if (chance < 0.05)
                chance = 0.05;

            if (random.NextDouble() > chance)
            {
                // A failed hunt costs the pack energy and may hurt someone.
                for (int i = 0; i < hunters.Count; i++)
                    hunters[i].Energy = hunters[i].Energy - 0.12;

                if (random.NextDouble() < prey.Defence * 0.4)
                {
                    Wolf unlucky = hunters[random.Next(hunters.Count)];

                    unlucky.Health = unlucky.Health - 0.2;
                    result.Wounded.Add(unlucky);
                }

                result.Narrative = "The pack chased a " + prey.Name + " and lost it.";
                return result;
            }

            // A kill feeds the pack: enough animals to go round, within the herd.
            int wanted = Math.Max(1, hunters.Count / 3);
            int taken = prey.Take(wanted);

            double meat = taken * prey.MassKilograms;

            result.Set(true, prey.Name, taken, meat);

            // Sharing follows rank: the alpha eats first and the omega last.
            double totalShare = 0.0;

            List<Wolf> order = new List<Wolf>(hunters);

            order.Sort(delegate (Wolf left, Wolf right)
            {
                return RankWeight(right.Rank).CompareTo(RankWeight(left.Rank));
            });

            double remaining = 1.0;

            for (int i = 0; i < order.Count; i++)
            {
                Wolf wolf = order[i];
                double share;

                if (i == 0)
                    share = 0.45;
                else if (i == 1)
                    share = 0.25;
                else
                    share = Math.Max(0.05, remaining / (order.Count - i));

                if (share > remaining)
                    share = remaining;

                remaining -= share;

                wolf.Energy = wolf.Energy + share;
                wolf.RecordKill();

                totalShare += share;
            }

            // The hunt costs something even when it works.
            for (int i = 0; i < hunters.Count; i++)
                hunters[i].Energy = hunters[i].Energy - 0.05;

            if (random.NextDouble() < prey.Defence * 0.25)
            {
                Wolf hurt = hunters[random.Next(hunters.Count)];
                hurt.Health = hurt.Health - 0.15;
                result.Wounded.Add(hurt);
            }

            result.Narrative = "The pack brought down " + taken + " " + prey.Name +
                               " (" + Math.Round(meat) + " kg).";
            return result;
        }

        /// <summary>How well a wolf turns its legs to running, from its build.</summary>
        private static double SpeedOf(Wolf wolf)
        {
            if (wolf.IsElder())
                return 0.7;

            return 1.0;
        }

        /// <summary>How much of a kill a rank is entitled to.</summary>
        private static double RankWeight(Biology.PackRank rank)
        {
            switch (rank)
            {
                case Biology.PackRank.Alpha: return 5.0;
                case Biology.PackRank.Second: return 4.0;
                case Biology.PackRank.Beta: return 3.0;
                case Biology.PackRank.Omega: return 1.0;
                default: return 0.0;
            }
        }

        /// <summary>
        /// One wolf howls. Every wolf within earshot answers, and a wolf that
        /// hears its own pack holds together better than one that hears nothing.
        ///
        /// Returns the wolves that answered.
        /// </summary>
        public static List<Wolf> Howl(Biology.WolfPack pack, double carryingDistanceMetres, Random random)
        {
            List<Wolf> answered = new List<Wolf>();

            if (pack == null || random == null)
                return answered;

            Wolf caller = pack.Alpha;

            if (caller == null)
                return answered;

            for (int i = 0; i < pack.Members.Count; i++)
            {
                Wolf wolf = pack.Members[i];

                if (wolf == caller || !wolf.IsAlive)
                    continue;

                // Distance is abstract here: only the callers hearing matters.
                double chance = wolf.Traits.Hearing * (carryingDistanceMetres / 5000.0);

                if (chance > 0.95)
                    chance = 0.95;

                if (random.NextDouble() < chance)
                    answered.Add(wolf);
            }

            // A call that is answered steadies the pack; a call into silence
            // unsettles it.
            if (answered.Count > 0)
            {
                pack.Cohesion = pack.Cohesion + 0.02 * answered.Count;
                caller.Energy = caller.Energy + 0.02;
            }
            else
            {
                pack.Cohesion = pack.Cohesion - 0.05;
            }

            return answered;
        }

        /// <summary>
        /// Runs one night on the land: the herd grows, the pack hunts, and the
        /// dominance order shifts.
        ///
        /// <paramref name="isFullMoon"/> makes any cursed wolf in the pack lose
        /// its hold on a human mind for the night.
        /// </summary>
        public static HuntResult Night(
            Biology.WolfPack pack,
            Prey prey,
            bool isFullMoon,
            Random random)
        {
            if (pack == null || random == null)
                return new HuntResult();

            if (isFullMoon)
                Moonshift(pack, random);

            // Herd growth at the start of the night, so the pack hunts what is
            // actually on the land.
            if (prey != null)
            {
                int capacity = prey.MaxPopulation;
                prey.Grow(capacity);
            }

            Howl(pack, 3000.0, random);

            HuntResult result = Hunt(pack, prey, true, random);

            pack.HoldDominanceRound(random);

            return result;
        }

        /// <summary>
        /// The moon. Every cursed wolf loses some grip on its human mind; a
        /// shapeshifter that loses all of it does not come back in the morning
        /// as anything but a wolf.
        /// </summary>
        public static void Moonshift(Biology.WolfPack pack, Random random)
        {
            if (pack == null || random == null)
                return;

            for (int i = 0; i < pack.Members.Count; i++)
            {
                Wolf wolf = pack.Members[i];

                if (!wolf.Traits.IsCursed)
                    continue;

                double loss = 0.15 + random.NextDouble() * 0.25;

                wolf.Traits.HumanRetention = wolf.Traits.HumanRetention - loss;

                // A wolf with no humanity left is stronger, and will not be reasoned with.
                if (wolf.Traits.HumanRetention <= 0.1)
                {
                    wolf.Traits.Strength = wolf.Traits.Strength + 0.05;
                    wolf.Traits.Aggression = wolf.Traits.Aggression + 0.1;
                }
            }

            // The moon unsettles any order, and pulls the pack tighter.
            pack.Cohesion = pack.Cohesion + 0.1;

            if (pack.Cohesion > 1.0)
                pack.Cohesion = 1.0;
        }

        /// <summary>
        /// A season passes: the pack ages, breeds if it can, and splinters if it
        /// has grown past what the land feeds. Returns the wolves that left.
        /// </summary>
        public static List<Wolf> Season(
            Biology.WolfPack pack,
            double years,
            double foodShare,
            int territoryCapacity,
            Random random)
        {
            List<Wolf> dispersed = new List<Wolf>();

            if (pack == null || random == null)
                return dispersed;

            pack.Advance(years, foodShare, random);

            // Cubs become adults once a year has turned.
            for (int i = 0; i < pack.Members.Count; i++)
            {
                Wolf wolf = pack.Members[i];

                if (wolf.Rank == Biology.PackRank.Cub && wolf.IsAdult())
                    wolf.Rank = Biology.PackRank.Beta;
            }

            pack.SettleRanks();
            pack.BreedSeason(random);

            dispersed = pack.Disperse(territoryCapacity, random);

            return dispersed;
        }

        /// <summary>
        /// Founds a new pack from wolves that left another one, led by the
        /// strongest of them. This is how the land fills up.
        /// </summary>
        public static Biology.WolfPack FindNewPack(string name, string territory, List<Wolf> founders, Random random)
        {
            if (founders == null || founders.Count == 0)
                return null;

            Biology.WolfPack pack = new Biology.WolfPack(name, territory);

            Wolf leader = founders[0];

            for (int i = 1; i < founders.Count; i++)
            {
                if (founders[i].FightingPower() > leader.FightingPower())
                    leader = founders[i];
            }

            pack.Recruit(leader);
            leader.Rank = Biology.PackRank.Alpha;

            for (int i = 0; i < founders.Count; i++)
            {
                if (founders[i] != leader)
                    pack.Recruit(founders[i]);
            }

            pack.SettleRanks();

            if (random != null)
                pack.Cohesion = 0.4 + random.NextDouble() * 0.3;

            return pack;
        }

        /// <summary>
        /// Whether a wolf can hold a man's shape tonight. A wolf with too little
        /// humanity left cannot change back.
        /// </summary>
        public static bool CanTakeHumanForm(Wolf wolf)
        {
            if (wolf == null)
                return false;

            if (!wolf.Traits.IsShapeshifter || !wolf.Traits.IsCursed)
                return false;

            return wolf.Traits.HumanRetention > 0.3 && wolf.Health > 0.4;
        }

        /// <summary>
        /// Puts a wolf back into human shape for a time. It keeps its memories
        /// and its scars; it trades its strength for hands.
        /// </summary>
        public static bool TakeHumanForm(Wolf wolf, double years)
        {
            if (!CanTakeHumanForm(wolf))
                return false;

            wolf.HumanFormYears = wolf.HumanFormYears + years;
            wolf.Traits.Strength = wolf.Traits.Strength - 0.1;
            wolf.Health = wolf.Health - 0.05;

            return true;
        }
    }
}

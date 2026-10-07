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
    /// <summary>What a preset creature is, for grouping in a list.</summary>
    public enum BeastKind
    {
        /// <summary>Something the pack hunts.</summary>
        Prey,
        /// <summary>A wolf from another pack.</summary>
        Rival,
        /// <summary>Something that hunts the pack.</summary>
        Predator,
        /// <summary>Neither: a rival of a different species.</summary>
        Other
    }

    /// <summary>
    /// One entry in the bestiary: what it is called, what it fights like, and
    /// which effects it carries.
    ///
    /// The numbers are on the same scale as a wolf's traits, so 0.5 is an
    /// average wolf and 0.8 is a large, dangerous one.
    /// </summary>
    public sealed class Beast
    {
        private string m_name;
        private BeastKind m_kind;
        private Side m_side;
        private string m_description;
        private double m_strength;
        private double m_speed;
        private double m_mass;
        private double m_defence;
        private double m_aggression;
        private int m_healthBonus;
        private List<Effect> m_effects;
        private char m_symbol;

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public BeastKind Kind
        {
            get { return m_kind; }
            set { m_kind = value; }
        }

        /// <summary>Which side it fights on when added to an engine.</summary>
        public Side Side
        {
            get { return m_side; }
            set { m_side = value; }
        }

        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        public double Strength
        {
            get { return m_strength; }
            set { m_strength = value; }
        }

        public double Speed
        {
            get { return m_speed; }
            set { m_speed = value; }
        }

        public double Mass
        {
            get { return m_mass; }
            set { m_mass = value; }
        }

        public double Defence
        {
            get { return m_defence; }
            set { m_defence = value; }
        }

        public double Aggression
        {
            get { return m_aggression; }
            set { m_aggression = value; }
        }

        /// <summary>Extra health, on top of what the mass gives it.</summary>
        public int HealthBonus
        {
            get { return m_healthBonus; }
            set { m_healthBonus = value; }
        }

        /// <summary>The effects it starts a fight with.</summary>
        public IList<Effect> Effects
        {
            get { return m_effects; }
        }

        /// <summary>One character, for a map or a list.</summary>
        public char Symbol
        {
            get { return m_symbol; }
            set { m_symbol = value; }
        }

        public Beast(string name, BeastKind kind)
        {
            m_name = name;
            m_kind = kind;
            m_side = kind == BeastKind.Rival ? Side.Rival
                   : kind == BeastKind.Prey ? Side.Prey
                   : Side.Rival;

            m_strength = 0.5;
            m_speed = 0.5;
            m_mass = 40.0;
            m_defence = 0.5;
            m_aggression = 0.5;
            m_effects = new List<Effect>();
            m_symbol = '?';
            m_description = string.Empty;
        }

        /// <summary>Sets the fighting numbers in one call.</summary>
        public Beast Stats(double strength, double speed, double mass)
        {
            m_strength = strength;
            m_speed = speed;
            m_mass = mass;
            m_defence = 0.3 + strength * 0.4;
            m_aggression = strength;

            return this;
        }

        /// <summary>Gives it an effect to start with.</summary>
        public Beast With(Effect effect)
        {
            if (effect != null)
                m_effects.Add(effect);

            return this;
        }

        public Beast Described(string description)
        {
            m_description = description;
            return this;
        }

        public Beast Marked(char symbol)
        {
            m_symbol = symbol;
            return this;
        }

        /// <summary>
        /// Turns the entry into a fighter ready for the engine. The effects are
        /// cloned, so two of the same beast do not share a stack count.
        /// </summary>
        public Combatant Spawn()
        {
            Combatant fighter = Combatant.Plain(m_name, m_side, m_strength, m_speed, m_mass);

            fighter.HealthBonus(m_healthBonus);

            for (int i = 0; i < m_effects.Count; i++)
                fighter.Effects.Apply(m_effects[i]);

            return fighter;
        }

        /// <summary>How dangerous it is, as a single number, for sorting a list.</summary>
        public double Threat()
        {
            double threat = m_strength * 0.5 + m_speed * 0.2 + (m_mass / 100.0) * 0.3;

            for (int i = 0; i < m_effects.Count; i++)
            {
                if (m_effects[i].Kind == EffectKind.Buff)
                    threat += 0.05;
            }

            return threat;
        }

        public override string ToString()
        {
            return m_name + " (" + m_kind + ")";
        }
    }

    /// <summary>
    /// The bestiary: the creatures a pack can meet.
    ///
    /// This is what makes a battle possible without a whole species model for
    /// every animal: an entry carries just enough to fight with, and enough
    /// description to be worth reading.
    /// </summary>
    public static class Bestiary
    {
        /// <summary>Every creature in the bestiary, in a sensible order.</summary>
        public static List<Beast> All()
        {
            List<Beast> beasts = new List<Beast>();

            beasts.AddRange(Prey());
            beasts.AddRange(Rivals());
            beasts.AddRange(Predators());
            beasts.AddRange(Others());

            return beasts;
        }

        // ------------------------------------------------------------------
        // What the pack hunts
        // ------------------------------------------------------------------

        public static List<Beast> Prey()
        {
            List<Beast> prey = new List<Beast>();

            Beast deer = new Beast("Red Deer", BeastKind.Prey);
            deer.Stats(0.45, 0.75, 120.0).Marked('d');
            deer.Described("Fast, and it kicks. A stag will turn on a lone wolf.");
            prey.Add(deer);

            Beast stag = new Beast("Stag", BeastKind.Prey);
            stag.Stats(0.6, 0.7, 160.0).Marked('D');
            stag.Described("Heavier than a hind, and armed.");
            stag.With(EffectsCatalogue.Braced());
            prey.Add(stag);

            Beast hare = new Beast("Hare", BeastKind.Prey);
            hare.Stats(0.1, 0.95, 4.0).Marked('h');
            hare.Described("Not worth chasing unless the pack is desperate.");
            prey.Add(hare);

            Beast boar = new Beast("Wild Boar", BeastKind.Prey);
            boar.Stats(0.7, 0.55, 100.0).Marked('b');
            boar.Described("Dangerous when cornered. It does not run far.");
            boar.With(EffectsCatalogue.Cornered());
            prey.Add(boar);

            Beast salmon = new Beast("Salmon", BeastKind.Prey);
            salmon.Stats(0.1, 0.8, 5.0).Marked('s');
            salmon.Described("Taken from the shallows, in the running season.");
            prey.Add(salmon);

            return prey;
        }

        // ------------------------------------------------------------------
        // Other wolves
        // ------------------------------------------------------------------

        public static List<Beast> Rivals()
        {
            List<Beast> rivals = new List<Beast>();

            Beast scout = new Beast("Rival Scout", BeastKind.Rival);
            scout.Stats(0.5, 0.65, 40.0).Marked('r');
            scout.Described("Young, quick, and on the wrong side of the boundary.");
            rivals.Add(scout);

            Beast intruder = new Beast("Pack Intruder", BeastKind.Rival);
            intruder.Stats(0.65, 0.6, 46.0).Marked('i');
            intruder.Described("An adult from another pack, and it knows it is trespassing.");
            intruder.With(EffectsCatalogue.HomeGround());
            rivals.Add(intruder);

            Beast rivalAlpha = new Beast("Rival Alpha", BeastKind.Rival);
            rivalAlpha.Stats(0.8, 0.65, 52.0).Marked('A');
            rivalAlpha.Described("It leads its own pack. Beating it ends the fight.");
            rivalAlpha.With(EffectsCatalogue.Rallied());
            rivalAlpha.With(EffectsCatalogue.Calloused());
            rivals.Add(rivalAlpha);

            Beast lone = new Beast("Lone Wolf", BeastKind.Rival);
            lone.Stats(0.6, 0.55, 42.0).Marked('l');
            lone.Described("Driven out of its own pack, and it has nothing to lose.");
            lone.With(EffectsCatalogue.Cornered());
            lone.With(EffectsCatalogue.Starving());
            rivals.Add(lone);

            Beast moonMad = new Beast("Moon-Mad Wolf", BeastKind.Rival);
            moonMad.Stats(0.75, 0.7, 48.0).Marked('M');
            moonMad.Described("No humanity left in it. It will not break off.");
            moonMad.With(EffectsCatalogue.MoonMad());
            moonMad.With(EffectsCatalogue.MoonBound());
            rivals.Add(moonMad);

            return rivals;
        }

        // ------------------------------------------------------------------
        // What hunts the pack
        // ------------------------------------------------------------------

        public static List<Beast> Predators()
        {
            List<Beast> predators = new List<Beast>();

            Beast bear = new Beast("Brown Bear", BeastKind.Predator);
            bear.Stats(0.95, 0.55, 300.0).Marked('B');
            bear.Described("Far heavier than a wolf. It does not need to win quickly.");
            bear.HealthBonus = 120;
            bear.With(EffectsCatalogue.Braced());
            predators.Add(bear);

            Beast lynx = new Beast("Lynx", BeastKind.Predator);
            lynx.Stats(0.6, 0.85, 25.0).Marked('L');
            lynx.Described("Quick, and it goes for the eyes. Fights on its terms or not at all.");
            predators.Add(lynx);

            Beast wolverine = new Beast("Wolverine", BeastKind.Predator);
            wolverine.Stats(0.7, 0.6, 20.0).Marked('W');
            wolverine.Described("Smaller than a wolf and entirely unbothered by that.");
            wolverine.With(EffectsCatalogue.Cornered());
            predators.Add(wolverine);

            Beast eagle = new Beast("Golden Eagle", BeastKind.Predator);
            eagle.Stats(0.4, 0.9, 5.0).Marked('E');
            eagle.Described("Takes cubs, and anything else small enough to carry.");
            eagle.With(EffectsCatalogue.NightEyes());
            predators.Add(eagle);

            return predators;
        }

        public static List<Beast> Others()
        {
            List<Beast> others = new List<Beast>();

            Beast stray = new Beast("Stray Dog", BeastKind.Other);
            stray.Stats(0.45, 0.6, 30.0).Marked('y');
            stray.Described("Bold around a kill, and gone the moment it is challenged.");
            others.Add(stray);

            Beast rabid = new Beast("Rabid Animal", BeastKind.Other);
            rabid.Stats(0.55, 0.5, 35.0).Marked('R');
            rabid.Described("It fights without sense and bites without letting go.");
            rabid.With(EffectsCatalogue.Poisoned());
            others.Add(rabid);

            return others;
        }

        /// <summary>Finds a creature by name, or null.</summary>
        public static Beast ByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            List<Beast> all = All();

            for (int i = 0; i < all.Count; i++)
            {
                if (string.Equals(all[i].Name, name, StringComparison.OrdinalIgnoreCase))
                    return all[i];
            }

            return null;
        }

        /// <summary>Everything of one kind.</summary>
        public static List<Beast> OfKind(BeastKind kind)
        {
            switch (kind)
            {
                case BeastKind.Prey: return Prey();
                case BeastKind.Rival: return Rivals();
                case BeastKind.Predator: return Predators();
                default: return Others();
            }
        }

        public static string Describe()
        {
            List<Beast> all = All();

            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.AppendLine("The bestiary (" + all.Count + " creatures)");
            builder.AppendLine();

            for (int i = 0; i < all.Count; i++)
            {
                Beast beast = all[i];

                builder.AppendLine("  " + beast.Symbol + "  " + beast.Name +
                                   "  (" + beast.Kind + ")");

                builder.AppendLine("     " + beast.Description);
                builder.AppendLine("     strength " + Math.Round(beast.Strength, 2) +
                                   ", speed " + Math.Round(beast.Speed, 2) +
                                   ", mass " + Math.Round(beast.Mass) + " kg" +
                                   ", threat " + Math.Round(beast.Threat(), 2));

                if (beast.Effects.Count > 0)
                {
                    builder.Append("     carries: ");

                    for (int e = 0; e < beast.Effects.Count; e++)
                    {
                        if (e > 0)
                            builder.Append(", ");

                        builder.Append(beast.Effects[e].Name);
                    }

                    builder.AppendLine();
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}

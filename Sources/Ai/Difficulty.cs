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

namespace LifeEngine.Ai
{
    /// <summary>
    /// How hard the machine plays, on a scale of 1 to 9.
    ///
    /// Each level is a named bundle of decisions rather than a single "skill"
    /// number, because the interesting differences between an easy opponent and
    /// a hard one are not just that it plays better: it also thinks further
    /// ahead, forgives less, and is more willing to commit.
    /// </summary>
    public enum Difficulty
    {
        /// <summary>Level 1. Reacts to what is next to it and nothing else.</summary>
        Idiot = 1,
        /// <summary>Level 2. Barely better than random.</summary>
        Feeble = 2,
        /// <summary>Level 3. Takes the obvious move.</summary>
        Weak = 3,
        /// <summary>Level 4. Looks one step ahead.</summary>
        Cautious = 4,
        /// <summary>Level 5. The middle of the scale: a fair fight.</summary>
        Even = 5,
        /// <summary>Level 6. Looks two steps ahead and defends.</summary>
        Sharp = 6,
        /// <summary>Level 7. Coordinates with its neighbours.</summary>
        Cunning = 7,
        /// <summary>Level 8. Plans, baits and cuts losses.</summary>
        Ruthless = 8,
        /// <summary>Level 9. Plays the long game and does not make mistakes.</summary>
        Alpha = 9
    }

    /// <summary>
    /// What one difficulty level actually changes.
    ///
    /// Every field here is read by <see cref="CellAi"/> at decision time. The
    /// table below is the whole of the difficulty system: there is no hidden
    /// cheating, no extra resources and no dice loaded in the machine's favour,
    /// because an opponent that wins by cheating is not interesting to play.
    /// </summary>
    public sealed class DifficultyProfile
    {
        private Difficulty m_level;
        private string m_name;
        private string m_description;

        /// <summary>How many moves ahead it considers.</summary>
        private int m_lookahead;

        /// <summary>How much it values its own survival, 0 to 1.</summary>
        private double m_selfPreservation;

        /// <summary>How much it values hurting the other side, 0 to 1.</summary>
        private double m_aggression;

        /// <summary>How much it values holding ground, 0 to 1.</summary>
        private double m_territoriality;

        /// <summary>How willing it is to take a risk for a bigger gain, 0 to 1.</summary>
        private double m_riskAppetite;

        /// <summary>How much it cooperates with its own kind, 0 to 1.</summary>
        private double m_cooperation;

        /// <summary>How far it looks for company, in cells.</summary>
        private int m_visionRange;

        /// <summary>How often it changes its mind, 0 (never) to 1 (always).</summary>
        private double m_hesitation;

        /// <summary>How much it remembers about where the enemy has been, 0 to 1.</summary>
        private double m_memory;

        /// <summary>Chance of simply picking a legal move at random.</summary>
        private double m_blunderChance;

        /// <summary>Whether it retreats when badly outnumbered.</summary>
        private bool m_canRetreat;

        /// <summary>Whether it deliberately baits the opponent into a trap.</summary>
        private bool m_canBait;

        /// <summary>Whether it coordinates with neighbours of the same kind.</summary>
        private bool m_canCoordinate;

        public Difficulty Level
        {
            get { return m_level; }
        }

        public string Name
        {
            get { return m_name; }
        }

        public string Description
        {
            get { return m_description; }
        }

        /// <summary>How many moves ahead it thinks. Zero means it does not.</summary>
        public int Lookahead
        {
            get { return m_lookahead; }
        }

        public double SelfPreservation
        {
            get { return m_selfPreservation; }
        }

        public double Aggression
        {
            get { return m_aggression; }
        }

        public double Territoriality
        {
            get { return m_territoriality; }
        }

        public double RiskAppetite
        {
            get { return m_riskAppetite; }
        }

        public double Cooperation
        {
            get { return m_cooperation; }
        }

        /// <summary>How far it can see. Level 1 sees only its eight neighbours.</summary>
        public int VisionRange
        {
            get { return m_visionRange; }
        }

        public double Hesitation
        {
            get { return m_hesitation; }
        }

        public double Memory
        {
            get { return m_memory; }
        }

        /// <summary>The chance per turn of a random move.</summary>
        public double BlunderChance
        {
            get { return m_blunderChance; }
        }

        public bool CanRetreat
        {
            get { return m_canRetreat; }
        }

        public bool CanBait
        {
            get { return m_canBait; }
        }

        public bool CanCoordinate
        {
            get { return m_canCoordinate; }
        }

        /// <summary>A one-line summary of the level, for a menu.</summary>
        public string Summary()
        {
            return ((int)m_level) + ". " + m_name + " - " + m_description;
        }

        private DifficultyProfile(Difficulty level, string name, string description)
        {
            m_level = level;
            m_name = name;
            m_description = description;
        }

        /// <summary>
        /// The nine levels.
        ///
        /// The numbers move in one direction as the level rises, with two
        /// deliberate exceptions: hesitation and blunder chance fall, which is
        /// what makes a high level feel decisive rather than merely stronger.
        /// </summary>
        public static DifficultyProfile For(Difficulty level)
        {
            DifficultyProfile profile;

            switch (level)
            {
                case Difficulty.Idiot:
                    profile = new DifficultyProfile(level, "Idiot",
                        "Moves at random. Sees only what touches it.");
                    profile.m_lookahead = 0;
                    profile.m_selfPreservation = 0.1;
                    profile.m_aggression = 0.1;
                    profile.m_territoriality = 0.0;
                    profile.m_riskAppetite = 0.9;
                    profile.m_cooperation = 0.0;
                    profile.m_visionRange = 1;
                    profile.m_hesitation = 0.8;
                    profile.m_memory = 0.0;
                    profile.m_blunderChance = 0.7;
                    break;

                case Difficulty.Feeble:
                    profile = new DifficultyProfile(level, "Feeble",
                        "Rarely does anything sensible.");
                    profile.m_lookahead = 0;
                    profile.m_selfPreservation = 0.2;
                    profile.m_aggression = 0.2;
                    profile.m_territoriality = 0.1;
                    profile.m_riskAppetite = 0.8;
                    profile.m_cooperation = 0.1;
                    profile.m_visionRange = 1;
                    profile.m_hesitation = 0.6;
                    profile.m_memory = 0.1;
                    profile.m_blunderChance = 0.5;
                    break;

                case Difficulty.Weak:
                    profile = new DifficultyProfile(level, "Weak",
                        "Takes the obvious move and no other.");
                    profile.m_lookahead = 1;
                    profile.m_selfPreservation = 0.3;
                    profile.m_aggression = 0.35;
                    profile.m_territoriality = 0.2;
                    profile.m_riskAppetite = 0.65;
                    profile.m_cooperation = 0.2;
                    profile.m_visionRange = 2;
                    profile.m_hesitation = 0.45;
                    profile.m_memory = 0.2;
                    profile.m_blunderChance = 0.3;
                    break;

                case Difficulty.Cautious:
                    profile = new DifficultyProfile(level, "Cautious",
                        "Looks one step ahead and prefers to hold ground.");
                    profile.m_lookahead = 1;
                    profile.m_selfPreservation = 0.5;
                    profile.m_aggression = 0.4;
                    profile.m_territoriality = 0.45;
                    profile.m_riskAppetite = 0.5;
                    profile.m_cooperation = 0.35;
                    profile.m_visionRange = 2;
                    profile.m_hesitation = 0.35;
                    profile.m_memory = 0.35;
                    profile.m_blunderChance = 0.18;
                    break;

                case Difficulty.Even:
                    profile = new DifficultyProfile(level, "Even",
                        "A fair fight: looks ahead, defends, and commits.");
                    profile.m_lookahead = 2;
                    profile.m_selfPreservation = 0.6;
                    profile.m_aggression = 0.55;
                    profile.m_territoriality = 0.5;
                    profile.m_riskAppetite = 0.45;
                    profile.m_cooperation = 0.5;
                    profile.m_visionRange = 3;
                    profile.m_hesitation = 0.25;
                    profile.m_memory = 0.5;
                    profile.m_blunderChance = 0.1;
                    break;

                case Difficulty.Sharp:
                    profile = new DifficultyProfile(level, "Sharp",
                        "Looks two moves ahead and knows when to withdraw.");
                    profile.m_lookahead = 2;
                    profile.m_selfPreservation = 0.7;
                    profile.m_aggression = 0.65;
                    profile.m_territoriality = 0.55;
                    profile.m_riskAppetite = 0.4;
                    profile.m_cooperation = 0.6;
                    profile.m_visionRange = 3;
                    profile.m_hesitation = 0.18;
                    profile.m_memory = 0.65;
                    profile.m_blunderChance = 0.05;
                    profile.m_canRetreat = true;
                    break;

                case Difficulty.Cunning:
                    profile = new DifficultyProfile(level, "Cunning",
                        "Coordinates with its neighbours and cuts its losses.");
                    profile.m_lookahead = 3;
                    profile.m_selfPreservation = 0.75;
                    profile.m_aggression = 0.7;
                    profile.m_territoriality = 0.6;
                    profile.m_riskAppetite = 0.4;
                    profile.m_cooperation = 0.75;
                    profile.m_visionRange = 4;
                    profile.m_hesitation = 0.12;
                    profile.m_memory = 0.75;
                    profile.m_blunderChance = 0.02;
                    profile.m_canRetreat = true;
                    profile.m_canCoordinate = true;
                    break;

                case Difficulty.Ruthless:
                    profile = new DifficultyProfile(level, "Ruthless",
                        "Plans, baits, and does not forgive a mistake.");
                    profile.m_lookahead = 3;
                    profile.m_selfPreservation = 0.8;
                    profile.m_aggression = 0.85;
                    profile.m_territoriality = 0.6;
                    profile.m_riskAppetite = 0.45;
                    profile.m_cooperation = 0.8;
                    profile.m_visionRange = 4;
                    profile.m_hesitation = 0.06;
                    profile.m_memory = 0.85;
                    profile.m_blunderChance = 0.0;
                    profile.m_canRetreat = true;
                    profile.m_canBait = true;
                    profile.m_canCoordinate = true;
                    break;

                default:
                    profile = new DifficultyProfile(Difficulty.Alpha, "Alpha",
                        "Plays the long game. Makes no mistakes.");
                    profile.m_lookahead = 4;
                    profile.m_selfPreservation = 0.85;
                    profile.m_aggression = 0.9;
                    profile.m_territoriality = 0.65;
                    profile.m_riskAppetite = 0.5;
                    profile.m_cooperation = 0.9;
                    profile.m_visionRange = 5;
                    profile.m_hesitation = 0.0;
                    profile.m_memory = 1.0;
                    profile.m_blunderChance = 0.0;
                    profile.m_canRetreat = true;
                    profile.m_canBait = true;
                    profile.m_canCoordinate = true;
                    break;
            }

            return profile;
        }

        /// <summary>All nine levels, in order.</summary>
        public static List<DifficultyProfile> All()
        {
            List<DifficultyProfile> profiles = new List<DifficultyProfile>();

            for (int level = 1; level <= 9; level++)
                profiles.Add(For((Difficulty)level));

            return profiles;
        }

        /// <summary>Clamps a number to a valid level.</summary>
        public static Difficulty Clamp(int level)
        {
            if (level < 1)
                return Difficulty.Idiot;

            if (level > 9)
                return Difficulty.Alpha;

            return (Difficulty)level;
        }

        /// <summary>Reads a level from a name or a number.</summary>
        public static Difficulty Parse(string text)
        {
            int number;

            if (int.TryParse(text, out number))
                return Clamp(number);

            if (string.IsNullOrEmpty(text))
                return Difficulty.Even;

            string trimmed = text.Trim();

            for (int level = 1; level <= 9; level++)
            {
                Difficulty candidate = (Difficulty)level;

                if (string.Equals(candidate.ToString(), trimmed, StringComparison.OrdinalIgnoreCase))
                    return candidate;
            }

            return Difficulty.Even;
        }

        public override string ToString()
        {
            return Summary();
        }
    }
}

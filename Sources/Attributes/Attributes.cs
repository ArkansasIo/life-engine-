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

namespace LifeEngine.Attributes
{
    /// <summary>
    /// The eight attributes every creature has. These are the raw numbers a
    /// character sheet shows: nothing is derived here, and nothing is learned.
    /// </summary>
    public enum AttributeKind
    {
        /// <summary>Physical power. Sets melee damage and what can be carried.</summary>
        Strength,
        /// <summary>Agility and reflexes. Sets evasion and turn order.</summary>
        Dexterity,
        /// <summary>Hardiness. Sets health and resistance to poison.</summary>
        Constitution,
        /// <summary>Reasoning and memory. Sets skill growth and what can be learnt.</summary>
        Intelligence,
        /// <summary>Awareness. Sets accuracy and the chance to notice things.</summary>
        Perception,
        /// <summary>Force of personality. Sets morale and how well it leads.</summary>
        Presence,
        /// <summary>Grip on its own mind. Resists fear, panic and the moon.</summary>
        Willpower,
        /// <summary>Movement. Sets how far it can go and how fast it acts.</summary>
        Speed
    }

    /// <summary>
    /// One attribute: a score, and the training that has been put into it.
    ///
    /// The score is the raw number; the training is how far along the way to the
    /// next point it is. Keeping them apart means a creature can improve through
    /// use without its score jumping about, which is what makes progress feel
    /// earned rather than assigned.
    /// </summary>
    public sealed class AttributeScore
    {
        private AttributeKind m_kind;
        private int m_base;
        private int m_trained;
        private double m_progress;
        private double m_trainingRate;
        private int m_ceiling;

        public AttributeKind Kind
        {
            get { return m_kind; }
        }

        /// <summary>The score as it was created, before any training.</summary>
        public int Base
        {
            get { return m_base; }
        }

        /// <summary>Points earned by use since the creature was made.</summary>
        public int Trained
        {
            get { return m_trained; }
        }

        /// <summary>The score as it stands: base plus what has been earned.</summary>
        public int Score
        {
            get { return m_base + m_trained; }
        }

        /// <summary>How far along toward the next point, 0 to 1.</summary>
        public double Progress
        {
            get { return m_progress; }
        }

        /// <summary>
        /// How fast this attribute improves with use. A high rate means the
        /// creature takes to it naturally; a low one means it will have to work.
        /// </summary>
        public double TrainingRate
        {
            get { return m_trainingRate; }
            set { m_trainingRate = value < 0.0 ? 0.0 : value; }
        }

        /// <summary>The highest the score can reach, trained or not.</summary>
        public int Ceiling
        {
            get { return m_ceiling; }
            set { m_ceiling = value < m_base ? m_base : value; }
        }

        /// <summary>The score on the usual 1 to 20 scale, for a sheet.</summary>
        public int Bonus
        {
            get { return (Score - 10) / 2; }
        }

        /// <summary>A letter grade, for a quick read of the sheet.</summary>
        public string Grade()
        {
            int score = Score;

            if (score >= 20) return "A+";
            if (score >= 18) return "A";
            if (score >= 16) return "B+";
            if (score >= 14) return "B";
            if (score >= 12) return "C+";
            if (score >= 10) return "C";
            if (score >= 8) return "D+";
            if (score >= 6) return "D";
            if (score >= 4) return "E";

            return "F";
        }

        public AttributeScore(AttributeKind kind, int baseScore)
        {
            m_kind = kind;
            m_base = baseScore;
            m_trained = 0;
            m_progress = 0.0;
            m_trainingRate = 1.0;
            m_ceiling = 30;
        }

        /// <summary>
        /// Records use of the attribute. Enough use earns a point, which is the
        /// only way a score goes up.
        ///
        /// Returns true if a point was earned.
        /// </summary>
        public bool Exercise(double effort)
        {
            if (effort <= 0.0 || Score >= m_ceiling)
                return false;

            m_progress += effort * m_trainingRate;

            if (m_progress < 1.0)
                return false;

            m_progress -= 1.0;
            m_trained++;

            // Progress carries over, but not more than one point's worth, so a
            // single enormous effort cannot grant several points at once.
            if (m_progress > 1.0)
                m_progress = 1.0;

            return true;
        }

        /// <summary>Gives a point directly, for a reward or a potion.</summary>
        public bool Award(int points)
        {
            if (points <= 0)
                return false;

            int room = m_ceiling - Score;

            if (room <= 0)
                return false;

            m_trained += Math.Min(points, room);

            return true;
        }

        /// <summary>Puts the attribute back to where it started.</summary>
        public void Reset()
        {
            m_trained = 0;
            m_progress = 0.0;
        }

        public override string ToString()
        {
            return m_kind + " " + Score + " (" + Grade() + ")";
        }
    }

    /// <summary>
    /// The eight attributes of one creature, and the derived numbers that fall
    /// out of them.
    ///
    /// Derived numbers are worked out on demand rather than stored, so there is
    /// no second copy to go stale when an attribute changes.
    /// </summary>
    public sealed class AttributeSet
    {
        private Dictionary<AttributeKind, AttributeScore> m_scores;
        private List<AttributeScore> m_order;

        /// <summary>Every attribute, in the fixed order of the enum.</summary>
        public IList<AttributeScore> Scores
        {
            get { return m_order; }
        }

        public AttributeSet()
        {
            m_scores = new Dictionary<AttributeKind, AttributeScore>();
            m_order = new List<AttributeScore>();

            // The eight kinds, in the order a sheet lists them.
            AttributeKind[] kinds =
            {
                AttributeKind.Strength, AttributeKind.Dexterity,
                AttributeKind.Constitution, AttributeKind.Intelligence,
                AttributeKind.Perception, AttributeKind.Presence,
                AttributeKind.Willpower, AttributeKind.Speed
            };

            for (int i = 0; i < kinds.Length; i++)
            {
                AttributeScore score = new AttributeScore(kinds[i], 10);

                m_scores[kinds[i]] = score;
                m_order.Add(score);
            }
        }

        /// <summary>A set with all attributes at the same score.</summary>
        public static AttributeSet Uniform(int score)
        {
            AttributeSet set = new AttributeSet();

            for (int i = 0; i < set.m_order.Count; i++)
            {
                AttributeScore attribute = set.m_order[i];

                attribute.Ceiling = score + 10;

                // Base is set through a replacement, because a score is fixed
                // once made. This keeps the invariant that a score only rises.
                set.m_scores[attribute.Kind] = new AttributeScore(attribute.Kind, score);
                set.m_order[i] = set.m_scores[attribute.Kind];
            }

            return set;
        }

        /// <summary>Sets one attribute's base score.</summary>
        public AttributeScore Set(AttributeKind kind, int score)
        {
            AttributeScore replacement = new AttributeScore(kind, score);

            m_scores[kind] = replacement;

            for (int i = 0; i < m_order.Count; i++)
            {
                if (m_order[i].Kind == kind)
                {
                    m_order[i] = replacement;
                    break;
                }
            }

            return replacement;
        }

        /// <summary>One attribute, or null if the kind is not in the set.</summary>
        public AttributeScore Get(AttributeKind kind)
        {
            AttributeScore score;

            if (m_scores.TryGetValue(kind, out score))
                return score;

            return null;
        }

        /// <summary>The score of one attribute.</summary>
        public int ScoreOf(AttributeKind kind)
        {
            AttributeScore score = Get(kind);

            return score == null ? 0 : score.Score;
        }

        /// <summary>The 1-to-20 style bonus of one attribute.</summary>
        public int BonusOf(AttributeKind kind)
        {
            AttributeScore score = Get(kind);

            return score == null ? 0 : score.Bonus;
        }

        /// <summary>
        /// Records that an attribute was used, which is how it improves.
        /// Returns the kinds that earned a point.
        /// </summary>
        public List<AttributeKind> Exercise(AttributeKind kind, double effort)
        {
            List<AttributeKind> improved = new List<AttributeKind>();

            AttributeScore score = Get(kind);

            if (score != null && score.Exercise(effort))
                improved.Add(kind);

            return improved;
        }

        /// <summary>The total of every attribute, for comparing two creatures.</summary>
        public int Total()
        {
            int total = 0;

            for (int i = 0; i < m_order.Count; i++)
                total += m_order[i].Score;

            return total;
        }

        // ------------------------------------------------------------------
        // Derived numbers
        // ------------------------------------------------------------------

        /// <summary>
        /// Hit points, from Constitution and size. The multiplier is what makes
        /// a bear tougher than a wolf with the same Constitution.
        /// </summary>
        public int MaxHealth(double sizeMultiplier)
        {
            double health = 20.0 + ScoreOf(AttributeKind.Constitution) * 4.0;

            health *= sizeMultiplier;

            if (health < 1.0)
                health = 1.0;

            return (int)Math.Round(health);
        }

        /// <summary>How hard a blow lands, from Strength.</summary>
        public int DamageBonus()
        {
            return BonusOf(AttributeKind.Strength) + 2;
        }

        /// <summary>How hard it is to hit, from Dexterity and Speed.</summary>
        public double Evasion()
        {
            double value = 0.1 + BonusOf(AttributeKind.Dexterity) * 0.03 +
                                 BonusOf(AttributeKind.Speed) * 0.02;

            return Limit(value, 0.0, 0.8);
        }

        /// <summary>How likely a blow is to land, from Perception and Dexterity.</summary>
        public double Accuracy()
        {
            double value = 0.5 + BonusOf(AttributeKind.Perception) * 0.03 +
                                 BonusOf(AttributeKind.Dexterity) * 0.02;

            return Limit(value, 0.05, 1.0);
        }

        /// <summary>Resistance to being frightened or dominated, from Willpower.</summary>
        public double Resolve()
        {
            double value = 0.3 + BonusOf(AttributeKind.Willpower) * 0.05;

            return Limit(value, 0.0, 1.5);
        }

        /// <summary>How fast it acts, for turn order.</summary>
        public double Initiative()
        {
            return 0.3 + BonusOf(AttributeKind.Speed) * 0.04 +
                         BonusOf(AttributeKind.Perception) * 0.02;
        }

        /// <summary>How much it can learn at once, from Intelligence.</summary>
        public int SkillPointsPerLevel()
        {
            int points = 2 + BonusOf(AttributeKind.Intelligence);

            return points < 1 ? 1 : points;
        }

        /// <summary>How well it leads others, from Presence.</summary>
        public double Leadership()
        {
            double value = 0.2 + BonusOf(AttributeKind.Presence) * 0.05;

            return Limit(value, 0.0, 1.5);
        }

        /// <summary>How much it can carry, from Strength and Constitution.</summary>
        public double CarryCapacity()
        {
            return 10.0 + ScoreOf(AttributeKind.Strength) * 2.0 +
                          ScoreOf(AttributeKind.Constitution);
        }

        /// <summary>Everything a sheet would print.</summary>
        public string Describe()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.AppendLine("Attributes");

            for (int i = 0; i < m_order.Count; i++)
            {
                AttributeScore score = m_order[i];

                builder.AppendLine("  " + Pad(score.Kind.ToString(), 14) +
                                   Pad(score.Score.ToString(), 4) +
                                   Pad(score.Grade(), 4) +
                                   (score.Trained > 0 ? "(+" + score.Trained + " trained)" : string.Empty));
            }

            builder.AppendLine();
            builder.AppendLine("Derived");
            builder.AppendLine("  Max health     " + MaxHealth(1.0));
            builder.AppendLine("  Damage bonus   " + DamageBonus());
            builder.AppendLine("  Evasion        " + Math.Round(Evasion(), 3));
            builder.AppendLine("  Accuracy       " + Math.Round(Accuracy(), 3));
            builder.AppendLine("  Resolve        " + Math.Round(Resolve(), 3));
            builder.AppendLine("  Initiative     " + Math.Round(Initiative(), 3));
            builder.AppendLine("  Leadership     " + Math.Round(Leadership(), 3));
            builder.AppendLine("  Skill points   " + SkillPointsPerLevel());

            return builder.ToString();
        }

        private static double Limit(double value, double minimum, double maximum)
        {
            if (value < minimum)
                return minimum;

            if (value > maximum)
                return maximum;

            return value;
        }

        private static string Pad(string text, int width)
        {
            if (text == null)
                text = string.Empty;

            if (text.Length >= width)
                return text + " ";

            return text + new string(' ', width - text.Length);
        }

        public override string ToString()
        {
            return "Attributes (total " + Total() + ")";
        }
    }
}

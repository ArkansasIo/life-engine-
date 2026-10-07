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
    /// <summary>What an effect does when it is applied.</summary>
    public enum EffectKind
    {
        /// <summary>Helps the creature it is on.</summary>
        Buff,
        /// <summary>Hurts the creature it is on.</summary>
        Debuff,
        /// <summary>Neither: a wound, a scar, a mark.</summary>
        Condition
    }

    /// <summary>
    /// Which number an effect moves. Every one of these has a matching field on
    /// <see cref="Combatant"/>; the engine reads them all through
    /// <see cref="Combatant.GetStat"/>.
    /// </summary>
    public enum EffectTarget
    {
        Strength,
        Speed,
        Aggression,
        Defence,
        Accuracy,
        Evasion,
        Initiative,
        Damage,
        DamageReduction,
        Healing,
        MaxHealth,
        MaxStamina,
        StaminaRegeneration,
        Morale,
        Fear,
        Bleed,
        Poison
    }

    /// <summary>
    /// How several copies of the same effect combine.
    ///
    /// This matters more than it looks: three stacks of a bleeding wound should
    /// hurt three times as much, but three stacks of a courage buff should not
    /// make a wolf nine times as brave.
    /// </summary>
    public enum StackMode
    {
        /// <summary>Only the strongest applies.</summary>
        Strongest,
        /// <summary>Every stack applies.</summary>
        Additive,
        /// <summary>Refreshes the duration, keeps one instance.</summary>
        Refresh,
        /// <summary>Compounds, so two 50% bonuses give 125% rather than 100%.</summary>
        Multiplicative
    }

    /// <summary>
    /// One effect: a named change to a creature's numbers, for a while.
    ///
    /// An effect is data. It does not know what it is on, and it does not apply
    /// itself: <see cref="EffectCollection"/> owns that, so the arithmetic and
    /// the stacking rules live in one place rather than in every effect.
    /// </summary>
    public sealed class Effect
    {
        private string m_name;
        private string m_description;
        private EffectKind m_kind;
        private EffectTarget m_target;
        private StackMode m_stackMode;
        private double m_amount;
        private double m_duration;
        private double m_remaining;
        private int m_stacks;
        private int m_maxStacks;
        private double m_tickAmount;
        private bool m_isPermanent;
        private bool m_breaksOnDamage;
        private string m_source;

        /// <summary>What the effect is called, e.g. "Bleeding" or "Moon-Mad".</summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>A sentence explaining what it does, for a tooltip.</summary>
        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        public EffectKind Kind
        {
            get { return m_kind; }
            set { m_kind = value; }
        }

        /// <summary>Which number it moves.</summary>
        public EffectTarget Target
        {
            get { return m_target; }
            set { m_target = value; }
        }

        /// <summary>How copies of it combine.</summary>
        public StackMode StackMode
        {
            get { return m_stackMode; }
            set { m_stackMode = value; }
        }

        /// <summary>How much it moves the number by, per stack.</summary>
        public double Amount
        {
            get { return m_amount; }
            set { m_amount = value; }
        }

        /// <summary>How long it lasts, in combat rounds. Ignored if permanent.</summary>
        public double Duration
        {
            get { return m_duration; }
            set { m_duration = value; }
        }

        /// <summary>How long is left.</summary>
        public double Remaining
        {
            get { return m_remaining; }
        }

        /// <summary>How many copies are on the creature.</summary>
        public int Stacks
        {
            get { return m_stacks; }
        }

        /// <summary>The most stacks allowed. One means it does not stack.</summary>
        public int MaxStacks
        {
            get { return m_maxStacks; }
            set { m_maxStacks = value < 1 ? 1 : value; }
        }

        /// <summary>
        /// Damage taken each round, on top of the effect's stat change. This is
        /// what makes bleeding and poison different from a plain stat penalty:
        /// they keep hurting after the fight that caused them.
        /// </summary>
        public double TickAmount
        {
            get { return m_tickAmount; }
            set { m_tickAmount = value; }
        }

        /// <summary>True for a scar or a curse: it never wears off on its own.</summary>
        public bool IsPermanent
        {
            get { return m_isPermanent; }
            set { m_isPermanent = value; }
        }

        /// <summary>True if taking a hit shakes the effect off, e.g. a trance.</summary>
        public bool BreaksOnDamage
        {
            get { return m_breaksOnDamage; }
            set { m_breaksOnDamage = value; }
        }

        /// <summary>What caused it, for the battle log.</summary>
        public string Source
        {
            get { return m_source; }
            set { m_source = value; }
        }

        /// <summary>True when the effect has run out and should be removed.</summary>
        public bool IsExpired
        {
            get { return !m_isPermanent && m_remaining <= 0.0; }
        }

        public Effect(string name, EffectKind kind, EffectTarget target, double amount, double duration)
        {
            m_name = name;
            m_kind = kind;
            m_target = target;
            m_amount = amount;
            m_duration = duration;
            m_remaining = duration;
            m_stackMode = StackMode.Strongest;
            m_stacks = 1;
            m_maxStacks = 1;
            m_description = string.Empty;
            m_source = string.Empty;
        }

        /// <summary>Marks it as a condition that never wears off.</summary>
        public Effect Permanent()
        {
            m_isPermanent = true;
            m_remaining = double.MaxValue;
            return this;
        }

        /// <summary>Sets the stacking rule and the ceiling.</summary>
        public Effect Stacking(StackMode mode, int maxStacks)
        {
            m_stackMode = mode;
            m_maxStacks = maxStacks < 1 ? 1 : maxStacks;
            return this;
        }

        /// <summary>Gives it damage over time.</summary>
        public Effect Ticks(double amountPerRound)
        {
            m_tickAmount = amountPerRound;
            return this;
        }

        /// <summary>Makes a hit shake it off.</summary>
        public Effect Breaks()
        {
            m_breaksOnDamage = true;
            return this;
        }

        /// <summary>Records what caused it.</summary>
        public Effect From(string source)
        {
            m_source = source;
            return this;
        }

        /// <summary>Describes it, for a tooltip or a log line.</summary>
        public Effect Describe(string description)
        {
            m_description = description;
            return this;
        }

        /// <summary>
        /// Adds a stack, following the stacking rule. Returns false if the
        /// effect is already at its ceiling and nothing changed.
        /// </summary>
        internal bool AddStack()
        {
            if (m_stacks >= m_maxStacks)
            {
                // At the ceiling, a refresh still resets the clock.
                if (m_stackMode == StackMode.Refresh)
                    m_remaining = m_duration;

                return false;
            }

            if (m_stackMode == StackMode.Refresh)
            {
                m_remaining = m_duration;
                return true;
            }

            m_stacks++;

            // A renewed effect gets its full time back, which is what makes
            // re-applying a bleed worth doing.
            m_remaining = m_duration;

            return true;
        }

        /// <summary>Counts the effect down. Returns true if it has now expired.</summary>
        internal bool Tick(double rounds)
        {
            if (m_isPermanent)
                return false;

            m_remaining -= rounds;

            return m_remaining <= 0.0;
        }

        /// <summary>
        /// What this effect is worth to the stat, after stacking. This is the
        /// only place the stacking rules are implemented.
        /// </summary>
        public double EffectiveAmount()
        {
            switch (m_stackMode)
            {
                case StackMode.Additive:
                    return m_amount * m_stacks;

                case StackMode.Multiplicative:
                    return Math.Pow(1.0 + m_amount, m_stacks) - 1.0;

                case StackMode.Refresh:
                case StackMode.Strongest:
                default:
                    return m_amount;
            }
        }

        /// <summary>A copy, for handing to a creature without sharing the state.</summary>
        public Effect Clone()
        {
            Effect copy = new Effect(m_name, m_kind, m_target, m_amount, m_duration);

            copy.m_description = m_description;
            copy.m_stackMode = m_stackMode;
            copy.m_maxStacks = m_maxStacks;
            copy.m_tickAmount = m_tickAmount;
            copy.m_isPermanent = m_isPermanent;
            copy.m_breaksOnDamage = m_breaksOnDamage;
            copy.m_source = m_source;
            copy.m_stacks = m_stacks;
            copy.m_remaining = m_remaining;

            return copy;
        }

        public override string ToString()
        {
            string sign = m_amount >= 0.0 ? "+" : string.Empty;

            return m_name + " (" + m_kind + ", " + m_target + " " +
                   sign + Math.Round(m_amount, 2) +
                   (m_stacks > 1 ? " x" + m_stacks : string.Empty) + ")";
        }
    }

    /// <summary>
    /// Every effect on one creature, and the arithmetic that combines them.
    ///
    /// Adding an effect that is already present follows that effect's own
    /// stacking rule, so a wound deepens but a blessing does not multiply out
    /// of control.
    /// </summary>
    public sealed class EffectCollection
    {
        private List<Effect> m_effects;
        private List<Effect> m_pending;

        /// <summary>The effects currently on the creature.</summary>
        public IList<Effect> Effects
        {
            get { return m_effects; }
        }

        public int Count
        {
            get { return m_effects.Count; }
        }

        public EffectCollection()
        {
            m_effects = new List<Effect>();
            m_pending = new List<Effect>();
        }

        /// <summary>
        /// Applies an effect. A second copy of something already present follows
        /// its stacking rule rather than being added twice.
        /// </summary>
        public Effect Apply(Effect effect)
        {
            if (effect == null)
                return null;

            for (int i = 0; i < m_effects.Count; i++)
            {
                Effect existing = m_effects[i];

                if (string.Equals(existing.Name, effect.Name, StringComparison.OrdinalIgnoreCase) &&
                    existing.Target == effect.Target)
                {
                    existing.AddStack();
                    return existing;
                }
            }

            Effect added = effect.Clone();

            m_effects.Add(added);

            return added;
        }

        /// <summary>Removes every effect with a name, and returns how many went.</summary>
        public int Remove(string name)
        {
            int removed = 0;

            for (int i = m_effects.Count - 1; i >= 0; i--)
            {
                if (string.Equals(m_effects[i].Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    m_effects.RemoveAt(i);
                    removed++;
                }
            }

            return removed;
        }

        /// <summary>Removes the effects that a hit shakes off.</summary>
        public int BreakOnDamage()
        {
            int removed = 0;

            for (int i = m_effects.Count - 1; i >= 0; i--)
            {
                if (m_effects[i].BreaksOnDamage)
                {
                    m_effects.RemoveAt(i);
                    removed++;
                }
            }

            return removed;
        }

        public bool Has(string name)
        {
            for (int i = 0; i < m_effects.Count; i++)
            {
                if (string.Equals(m_effects[i].Name, name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public Effect Find(string name)
        {
            for (int i = 0; i < m_effects.Count; i++)
            {
                if (string.Equals(m_effects[i].Name, name, StringComparison.OrdinalIgnoreCase))
                    return m_effects[i];
            }

            return null;
        }

        /// <summary>
        /// The total change to one stat from every effect on it. This is what
        /// <see cref="Combatant.GetStat"/> adds to the base number.
        /// </summary>
        public double TotalFor(EffectTarget target)
        {
            double total = 0.0;

            for (int i = 0; i < m_effects.Count; i++)
            {
                if (m_effects[i].Target == target)
                    total += m_effects[i].EffectiveAmount();
            }

            return total;
        }

        /// <summary>Damage per round from everything that ticks.</summary>
        public double DamagePerRound()
        {
            double total = 0.0;

            for (int i = 0; i < m_effects.Count; i++)
                total += m_effects[i].TickAmount * m_effects[i].Stacks;

            return total;
        }

        /// <summary>
        /// Counts every effect down by the elapsed rounds, and drops the ones
        /// that have run out. Returns the names of what expired, for the log.
        /// </summary>
        public List<string> Tick(double rounds)
        {
            List<string> expired = new List<string>();

            for (int i = m_effects.Count - 1; i >= 0; i--)
            {
                if (m_effects[i].Tick(rounds))
                {
                    expired.Add(m_effects[i].Name);
                    m_effects.RemoveAt(i);
                }
            }

            return expired;
        }

        /// <summary>Everything of one kind, for a status panel.</summary>
        public List<Effect> OfKind(EffectKind kind)
        {
            List<Effect> result = new List<Effect>();

            for (int i = 0; i < m_effects.Count; i++)
            {
                if (m_effects[i].Kind == kind)
                    result.Add(m_effects[i]);
            }

            return result;
        }

        /// <summary>Every effect, one per line, for a report.</summary>
        public string Describe()
        {
            if (m_effects.Count == 0)
                return "none";

            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            for (int i = 0; i < m_effects.Count; i++)
            {
                Effect effect = m_effects[i];

                if (i > 0)
                    builder.Append(", ");

                builder.Append(effect.Name);

                if (effect.Stacks > 1)
                    builder.Append(" x" + effect.Stacks);

                if (!effect.IsPermanent)
                    builder.Append(" (" + Math.Round(effect.Remaining, 1) + " rounds)");
            }

            return builder.ToString();
        }

        /// <summary>Removes everything.</summary>
        public void Clear()
        {
            m_effects.Clear();
        }

        public override string ToString()
        {
            return "Effects (" + m_effects.Count + ")";
        }
    }
}

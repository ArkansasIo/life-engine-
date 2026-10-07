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

namespace LifeEngine.Talents
{
    /// <summary>
    /// Which discipline a talent belongs to. The tree is grouped by these, so a
    /// creature's talents read as a coherent set rather than a shopping list.
    /// </summary>
    public enum TalentBranch
    {
        /// <summary>Tooth and claw: hitting harder and taking hits.</summary>
        Ferocity,
        /// <summary>Running, dodging and closing ground.</summary>
        Swiftness,
        /// <summary>Endurance, healing and refusing to fall.</summary>
        Endurance,
        /// <summary>The pack: leading, coordinating and fighting together.</summary>
        Pack,
        /// <summary>Senses, tracking and knowing where the enemy is.</summary>
        Instinct,
        /// <summary>The moon, curses and what the change does to a mind.</summary>
        Moon
    }

    /// <summary>
    /// How strong a talent is. A talent is taken at one of these, and the rank
    /// is what the effect scales on.
    /// </summary>
    public enum TalentRank
    {
        /// <summary>Not taken.</summary>
        None = 0,
        /// <summary>Learned once.</summary>
        Basic = 1,
        /// <summary>Learned twice: the talent is refined.</summary>
        Improved = 2,
        /// <summary>Learned three times: mastered.</summary>
        Mastered = 3
    }

    /// <summary>
    /// What a talent changes. Every one of these is read by
    /// <see cref="TalentSet"/> when it works out the totals, so adding a new
    /// kind of talent means adding a field here and nothing else.
    /// </summary>
    public sealed class TalentEffect
    {
        private string m_description;
        private int m_damage;
        private int m_health;
        private double m_accuracy;
        private double m_evasion;
        private double m_initiative;
        private double m_damageReduction;
        private double m_healing;
        private double m_leadership;
        private double m_resolve;
        private double m_stealth;
        private double m_tracking;
        private double m_moonControl;

        /// <summary>Extra damage per blow.</summary>
        public int Damage
        {
            get { return m_damage; }
            set { m_damage = value; }
        }

        /// <summary>Extra health.</summary>
        public int Health
        {
            get { return m_health; }
            set { m_health = value; }
        }

        /// <summary>Added to the chance to land a blow.</summary>
        public double Accuracy
        {
            get { return m_accuracy; }
            set { m_accuracy = value; }
        }

        /// <summary>Added to the chance to avoid one.</summary>
        public double Evasion
        {
            get { return m_evasion; }
            set { m_evasion = value; }
        }

        /// <summary>Added to turn order.</summary>
        public double Initiative
        {
            get { return m_initiative; }
            set { m_initiative = value; }
        }

        /// <summary>Proportion of incoming damage ignored, 0 to 1.</summary>
        public double DamageReduction
        {
            get { return m_damageReduction; }
            set { m_damageReduction = value; }
        }

        /// <summary>Added to healing per round.</summary>
        public double Healing
        {
            get { return m_healing; }
            set { m_healing = value; }
        }

        /// <summary>How well it leads a pack.</summary>
        public double Leadership
        {
            get { return m_leadership; }
            set { m_leadership = value; }
        }

        /// <summary>Resistance to fear and domination.</summary>
        public double Resolve
        {
            get { return m_resolve; }
            set { m_resolve = value; }
        }

        /// <summary>How hard it is to notice.</summary>
        public double Stealth
        {
            get { return m_stealth; }
            set { m_stealth = value; }
        }

        /// <summary>How well it follows a scent or a trail.</summary>
        public double Tracking
        {
            get { return m_tracking; }
            set { m_tracking = value; }
        }

        /// <summary>How much of itself it keeps through the change.</summary>
        public double MoonControl
        {
            get { return m_moonControl; }
            set { m_moonControl = value; }
        }

        /// <summary>A sentence about what the talent does, for a tooltip.</summary>
        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        public TalentEffect()
        {
            m_description = string.Empty;
        }

        /// <summary>
        /// The effect scaled by rank. This is the only place rank is applied, so
        /// a talent's numbers are written once, at their basic strength.
        /// </summary>
        public TalentEffect Scaled(TalentRank rank)
        {
            int steps = (int)rank;

            if (steps < 1)
                steps = 1;

            TalentEffect scaled = new TalentEffect();

            scaled.m_description = m_description;
            scaled.m_damage = m_damage * steps;
            scaled.m_health = m_health * steps;
            scaled.m_accuracy = m_accuracy * steps;
            scaled.m_evasion = m_evasion * steps;
            scaled.m_initiative = m_initiative * steps;
            scaled.m_damageReduction = m_damageReduction * steps;
            scaled.m_healing = m_healing * steps;
            scaled.m_leadership = m_leadership * steps;
            scaled.m_resolve = m_resolve * steps;
            scaled.m_stealth = m_stealth * steps;
            scaled.m_tracking = m_tracking * steps;
            scaled.m_moonControl = m_moonControl * steps;

            return scaled;
        }

        /// <summary>Adds another effect's numbers into this one.</summary>
        public void Add(TalentEffect other)
        {
            if (other == null)
                return;

            m_damage += other.m_damage;
            m_health += other.m_health;
            m_accuracy += other.m_accuracy;
            m_evasion += other.m_evasion;
            m_initiative += other.m_initiative;
            m_damageReduction += other.m_damageReduction;
            m_healing += other.m_healing;
            m_leadership += other.m_leadership;
            m_resolve += other.m_resolve;
            m_stealth += other.m_stealth;
            m_tracking += other.m_tracking;
            m_moonControl += other.m_moonControl;
        }

        /// <summary>True if the effect changes nothing at all.</summary>
        public bool IsEmpty()
        {
            return m_damage == 0 && m_health == 0 && m_accuracy == 0.0 &&
                   m_evasion == 0.0 && m_initiative == 0.0 &&
                   m_damageReduction == 0.0 && m_healing == 0.0 &&
                   m_leadership == 0.0 && m_resolve == 0.0 &&
                   m_stealth == 0.0 && m_tracking == 0.0 && m_moonControl == 0.0;
        }

        /// <summary>A readable list of what the effect changes.</summary>
        public string Describe()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            Append(builder, "damage", m_damage);
            Append(builder, "health", m_health);
            Append(builder, "accuracy", m_accuracy);
            Append(builder, "evasion", m_evasion);
            Append(builder, "initiative", m_initiative);
            Append(builder, "damage reduction", m_damageReduction);
            Append(builder, "healing", m_healing);
            Append(builder, "leadership", m_leadership);
            Append(builder, "resolve", m_resolve);
            Append(builder, "stealth", m_stealth);
            Append(builder, "tracking", m_tracking);
            Append(builder, "moon control", m_moonControl);

            if (builder.Length == 0)
                return "no effect";

            return builder.ToString();
        }

        private static void Append(System.Text.StringBuilder builder, string label, double value)
        {
            if (value == 0.0)
                return;

            if (builder.Length > 0)
                builder.Append(", ");

            builder.Append((value > 0.0 ? "+" : string.Empty) + value + " " + label);
        }

        public override string ToString()
        {
            return Describe();
        }
    }

    /// <summary>
    /// One talent: a named thing a creature can learn, in a branch, with
    /// prerequisites and a maximum rank.
    ///
    /// A talent knows what it needs and what it gives. It does not know who has
    /// it; <see cref="TalentSet"/> owns that.
    /// </summary>
    public sealed class Talent
    {
        private string m_name;
        private TalentBranch m_branch;
        private TalentRank m_maxRank;
        private int m_tier;
        private string m_description;
        private TalentEffect m_effect;
        private List<string> m_requires;
        private int m_minimumAttributeScore;
        private Attributes.AttributeKind m_governingAttribute;
        private string m_grants;

        /// <summary>The talent's name.</summary>
        public string Name
        {
            get { return m_name; }
        }

        /// <summary>Which branch of the tree it sits in.</summary>
        public TalentBranch Branch
        {
            get { return m_branch; }
        }

        /// <summary>The highest rank it can be taken to.</summary>
        public TalentRank MaxRank
        {
            get { return m_maxRank; }
        }

        /// <summary>How far down the branch it sits. Tier 1 needs nothing.</summary>
        public int Tier
        {
            get { return m_tier; }
        }

        public string Description
        {
            get { return m_description; }
        }

        /// <summary>What it gives, at rank one.</summary>
        public TalentEffect Effect
        {
            get { return m_effect; }
        }

        /// <summary>The talents that must be taken before this one.</summary>
        public IList<string> Requires
        {
            get { return m_requires; }
        }

        /// <summary>The score the governing attribute must reach.</summary>
        public int MinimumAttributeScore
        {
            get { return m_minimumAttributeScore; }
            set { m_minimumAttributeScore = value; }
        }

        /// <summary>Which attribute gates it.</summary>
        public Attributes.AttributeKind GoverningAttribute
        {
            get { return m_governingAttribute; }
            set { m_governingAttribute = value; }
        }

        /// <summary>The name of the ability this talent unlocks, if any.</summary>
        public string Grants
        {
            get { return m_grants; }
        }

        public Talent(string name, TalentBranch branch, int tier)
        {
            m_name = name;
            m_branch = branch;
            m_tier = tier < 1 ? 1 : tier;
            m_maxRank = TalentRank.Mastered;
            m_description = string.Empty;
            m_effect = new TalentEffect();
            m_requires = new List<string>();
            m_minimumAttributeScore = 0;
            m_governingAttribute = Attributes.AttributeKind.Strength;
            m_grants = string.Empty;
        }

        /// <summary>Sets what the talent gives at rank one.</summary>
        public Talent Gives(TalentEffect effect)
        {
            if (effect != null)
                m_effect = effect;

            return this;
        }

        /// <summary>Requires another talent by name.</summary>
        public Talent Needs(string talentName)
        {
            if (!string.IsNullOrEmpty(talentName) && !m_requires.Contains(talentName))
                m_requires.Add(talentName);

            return this;
        }

        /// <summary>Caps the rank, for a talent that cannot be mastered.</summary>
        public Talent CappedAt(TalentRank rank)
        {
            m_maxRank = rank;
            return this;
        }

        /// <summary>Requires an attribute score.</summary>
        public Talent NeedsAttribute(Attributes.AttributeKind kind, int score)
        {
            m_governingAttribute = kind;
            m_minimumAttributeScore = score;

            return this;
        }

        /// <summary>Names the ability this unlocks.</summary>
        public Talent Unlocks(string abilityName)
        {
            m_grants = abilityName;
            return this;
        }

        /// <summary>Describes it.</summary>
        public Talent Described(string description)
        {
            m_description = description;
            return this;
        }

        /// <summary>
        /// Whether a creature may take this talent at all: prerequisites met and
        /// the governing attribute high enough.
        /// </summary>
        public bool IsAvailableTo(Attributes.AttributeSet attributes, TalentSet taken)
        {
            if (taken == null)
                return m_requires.Count == 0;

            for (int i = 0; i < m_requires.Count; i++)
            {
                if (taken.RankOf(m_requires[i]) == TalentRank.None)
                    return false;
            }

            if (m_minimumAttributeScore > 0 && attributes != null)
            {
                if (attributes.ScoreOf(m_governingAttribute) < m_minimumAttributeScore)
                    return false;
            }

            return true;
        }

        /// <summary>Why it is not available, for a tooltip.</summary>
        public string WhyUnavailable(Attributes.AttributeSet attributes, TalentSet taken)
        {
            if (taken != null)
            {
                for (int i = 0; i < m_requires.Count; i++)
                {
                    if (taken.RankOf(m_requires[i]) == TalentRank.None)
                        return "needs " + m_requires[i];
                }
            }

            if (m_minimumAttributeScore > 0 && attributes != null)
            {
                int score = attributes.ScoreOf(m_governingAttribute);

                if (score < m_minimumAttributeScore)
                    return "needs " + m_governingAttribute + " " + m_minimumAttributeScore +
                           " (has " + score + ")";
            }

            return string.Empty;
        }

        public override string ToString()
        {
            return m_name + " (" + m_branch + ", tier " + m_tier + ")";
        }
    }

    /// <summary>
    /// The talents one creature has taken, and the totals they add up to.
    ///
    /// The totals are recomputed from scratch on every read rather than kept in
    /// a running total, because a talent can be taken, refunded or improved and
    /// a running total would drift.
    /// </summary>
    public sealed class TalentSet
    {
        private Dictionary<string, TalentRank> m_taken;
        private List<string> m_order;
        private List<Talent> m_catalogue;

        /// <summary>The talents taken, in the order they were learnt.</summary>
        public IList<string> Taken
        {
            get { return m_order; }
        }

        public int Count
        {
            get { return m_order.Count; }
        }

        public TalentSet()
        {
            m_taken = new Dictionary<string, TalentRank>(StringComparer.OrdinalIgnoreCase);
            m_order = new List<string>();
            m_catalogue = new List<Talent>();
        }

        /// <summary>Puts a talent in the catalogue so it can be taken by name.</summary>
        public Talent Register(Talent talent)
        {
            if (talent == null)
                return null;

            for (int i = 0; i < m_catalogue.Count; i++)
            {
                if (string.Equals(m_catalogue[i].Name, talent.Name, StringComparison.OrdinalIgnoreCase))
                    return m_catalogue[i];
            }

            m_catalogue.Add(talent);

            return talent;
        }

        /// <summary>Registers every talent in a list.</summary>
        public void RegisterAll(IList<Talent> talents)
        {
            if (talents == null)
                return;

            for (int i = 0; i < talents.Count; i++)
                Register(talents[i]);
        }

        /// <summary>Finds a registered talent by name.</summary>
        public Talent Find(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            for (int i = 0; i < m_catalogue.Count; i++)
            {
                if (string.Equals(m_catalogue[i].Name, name, StringComparison.OrdinalIgnoreCase))
                    return m_catalogue[i];
            }

            return null;
        }

        public IList<Talent> Catalogue
        {
            get { return m_catalogue; }
        }

        /// <summary>What rank a talent is at. None if it has not been taken.</summary>
        public TalentRank RankOf(string name)
        {
            TalentRank rank;

            if (!string.IsNullOrEmpty(name) && m_taken.TryGetValue(name, out rank))
                return rank;

            return TalentRank.None;
        }

        public bool Has(string name)
        {
            return RankOf(name) != TalentRank.None;
        }

        /// <summary>
        /// Takes a talent, or improves one already taken.
        ///
        /// Returns false if it is not in the catalogue, is already mastered, or
        /// the creature does not qualify for it.
        /// </summary>
        public bool Learn(string name, Attributes.AttributeSet attributes)
        {
            Talent talent = Find(name);

            if (talent == null)
                return false;

            TalentRank current = RankOf(name);

            if (current >= talent.MaxRank)
                return false;

            if (current == TalentRank.None && !talent.IsAvailableTo(attributes, this))
                return false;

            TalentRank next = (TalentRank)((int)current + 1);

            m_taken[name] = next;

            if (current == TalentRank.None)
                m_order.Add(name);

            return true;
        }

        /// <summary>
        /// Gives back the last rank of a talent. Refused if a later talent
        /// depends on it, because refunding it would leave the creature with a
        /// talent it does not qualify for.
        /// </summary>
        public bool Refund(string name)
        {
            TalentRank current = RankOf(name);

            if (current == TalentRank.None)
                return false;

            if (current == TalentRank.Basic && IsRequiredBySomethingTaken(name))
                return false;

            TalentRank next = (TalentRank)((int)current - 1);

            if (next == TalentRank.None)
            {
                m_taken.Remove(name);
                m_order.Remove(name);
            }
            else
            {
                m_taken[name] = next;
            }

            return true;
        }

        private bool IsRequiredBySomethingTaken(string name)
        {
            for (int i = 0; i < m_order.Count; i++)
            {
                if (string.Equals(m_order[i], name, StringComparison.OrdinalIgnoreCase))
                    continue;

                Talent talent = Find(m_order[i]);

                if (talent == null)
                    continue;

                for (int r = 0; r < talent.Requires.Count; r++)
                {
                    if (string.Equals(talent.Requires[r], name, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        /// <summary>Every talent in the catalogue that could be taken next.</summary>
        public List<Talent> Available(Attributes.AttributeSet attributes)
        {
            List<Talent> available = new List<Talent>();

            for (int i = 0; i < m_catalogue.Count; i++)
            {
                Talent talent = m_catalogue[i];
                TalentRank rank = RankOf(talent.Name);

                if (rank >= talent.MaxRank)
                    continue;

                if (rank == TalentRank.None && !talent.IsAvailableTo(attributes, this))
                    continue;

                available.Add(talent);
            }

            return available;
        }

        /// <summary>Every talent taken in one branch.</summary>
        public List<Talent> InBranch(TalentBranch branch)
        {
            List<Talent> found = new List<Talent>();

            for (int i = 0; i < m_order.Count; i++)
            {
                Talent talent = Find(m_order[i]);

                if (talent != null && talent.Branch == branch)
                    found.Add(talent);
            }

            return found;
        }

        /// <summary>How many ranks have been spent in one branch.</summary>
        public int RanksInBranch(TalentBranch branch)
        {
            int ranks = 0;

            for (int i = 0; i < m_order.Count; i++)
            {
                Talent talent = Find(m_order[i]);

                if (talent != null && talent.Branch == branch)
                    ranks += (int)RankOf(m_order[i]);
            }

            return ranks;
        }

        // ------------------------------------------------------------------
        // Totals
        // ------------------------------------------------------------------

        /// <summary>
        /// Everything every taken talent adds up to, scaled by rank. Read by the
        /// combat and AI layers; nothing else needs to know the details.
        /// </summary>
        public TalentEffect Totals()
        {
            TalentEffect total = new TalentEffect();

            for (int i = 0; i < m_order.Count; i++)
            {
                Talent talent = Find(m_order[i]);

                if (talent == null)
                    continue;

                total.Add(talent.Effect.Scaled(RankOf(m_order[i])));
            }

            return total;
        }

        /// <summary>The names of the abilities the taken talents unlock.</summary>
        public List<string> GrantedAbilities()
        {
            List<string> abilities = new List<string>();

            for (int i = 0; i < m_order.Count; i++)
            {
                Talent talent = Find(m_order[i]);

                if (talent == null || string.IsNullOrEmpty(talent.Grants))
                    continue;

                if (!abilities.Contains(talent.Grants))
                    abilities.Add(talent.Grants);
            }

            return abilities;
        }

        /// <summary>A sheet of everything taken, branch by branch.</summary>
        public string Describe()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.AppendLine("Talents (" + m_order.Count + " taken)");
            builder.AppendLine();

            TalentBranch[] branches =
            {
                TalentBranch.Ferocity, TalentBranch.Swiftness, TalentBranch.Endurance,
                TalentBranch.Pack, TalentBranch.Instinct, TalentBranch.Moon
            };

            for (int b = 0; b < branches.Length; b++)
            {
                List<Talent> taken = InBranch(branches[b]);

                if (taken.Count == 0)
                    continue;

                builder.AppendLine(branches[b] + " (" + RanksInBranch(branches[b]) + " ranks)");

                for (int i = 0; i < taken.Count; i++)
                {
                    TalentRank rank = RankOf(taken[i].Name);

                    builder.AppendLine("  " + Pad(taken[i].Name, 22) + Pad(rank.ToString(), 11) +
                                       taken[i].Effect.Scaled(rank).Describe());
                }

                builder.AppendLine();
            }

            builder.AppendLine("Totals");
            builder.AppendLine("  " + Totals().Describe());

            List<string> abilities = GrantedAbilities();

            if (abilities.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Abilities unlocked");

                for (int i = 0; i < abilities.Count; i++)
                    builder.AppendLine("  " + abilities[i]);
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

        public override string ToString()
        {
            return "TalentSet (" + m_order.Count + " talents)";
        }
    }

    /// <summary>
    /// The talent tree itself: every talent the program knows, with its
    /// prerequisites, its branch and its tier.
    ///
    /// The tree is data. Nothing in the engine knows what a "Savage Bite" is;
    /// it only knows what the numbers add up to.
    /// </summary>
    public static class TalentTree
    {
        /// <summary>Every talent, in tree order: branch by branch, tier by tier.</summary>
        public static List<Talent> All()
        {
            List<Talent> talents = new List<Talent>();

            talents.AddRange(Ferocity());
            talents.AddRange(Swiftness());
            talents.AddRange(Endurance());
            talents.AddRange(Pack());
            talents.AddRange(Instinct());
            talents.AddRange(Moon());

            return talents;
        }

        /// <summary>Builds a set with the whole tree registered.</summary>
        public static TalentSet NewSet()
        {
            TalentSet set = new TalentSet();

            set.RegisterAll(All());

            return set;
        }

        // ------------------------------------------------------------------
        // Ferocity: hitting harder
        // ------------------------------------------------------------------

        public static List<Talent> Ferocity()
        {
            List<Talent> branch = new List<Talent>();

            TalentEffect bite = new TalentEffect();
            bite.Damage = 2;
            bite.Description = "A harder bite.";

            Talent savageBite = new Talent("Savage Bite", TalentBranch.Ferocity, 1)
                .Gives(bite)
                .Described("The jaws close harder than they should.")
                .NeedsAttribute(Attributes.AttributeKind.Strength, 11);
            branch.Add(savageBite);

            TalentEffect rend = new TalentEffect();
            rend.Damage = 3;
            rend.Description = "Raking claws.";

            Talent rending = new Talent("Rending Claws", TalentBranch.Ferocity, 2)
                .Gives(rend)
                .Needs("Savage Bite")
                .Described("Claws that open a wound rather than bruising.")
                .Unlocks("Rend")
                .NeedsAttribute(Attributes.AttributeKind.Strength, 13);
            branch.Add(rending);

            TalentEffect throat = new TalentEffect();
            throat.Damage = 5;
            throat.Accuracy = 0.05;
            throat.Description = "Goes for the throat.";

            Talent throatSeek = new Talent("Throat Seeker", TalentBranch.Ferocity, 3)
                .Gives(throat)
                .Needs("Rending Claws")
                .Described("Knows exactly where to put its teeth.")
                .CappedAt(TalentRank.Improved)
                .NeedsAttribute(Attributes.AttributeKind.Perception, 13);
            branch.Add(throatSeek);

            TalentEffect fury = new TalentEffect();
            fury.Damage = 4;
            fury.DamageReduction = -0.05;
            fury.Description = "Hits harder, guards less.";

            Talent bloodFury = new Talent("Blood Fury", TalentBranch.Ferocity, 4)
                .Gives(fury)
                .Needs("Throat Seeker")
                .Described("The more it bleeds, the less it cares about being hit.")
                .Unlocks("Blood Fury")
                .NeedsAttribute(Attributes.AttributeKind.Willpower, 12);
            branch.Add(bloodFury);

            return branch;
        }

        // ------------------------------------------------------------------
        // Swiftness: closing ground and not being hit
        // ------------------------------------------------------------------

        public static List<Talent> Swiftness()
        {
            List<Talent> branch = new List<Talent>();

            TalentEffect fleet = new TalentEffect();
            fleet.Initiative = 0.08;
            fleet.Description = "Quicker off the mark.";

            Talent fleetFoot = new Talent("Fleet Foot", TalentBranch.Swiftness, 1)
                .Gives(fleet)
                .Described("First away when the chase starts.")
                .NeedsAttribute(Attributes.AttributeKind.Speed, 11);
            branch.Add(fleetFoot);

            TalentEffect slip = new TalentEffect();
            slip.Evasion = 0.05;
            slip.Description = "Harder to pin down.";

            Talent slipAway = new Talent("Slip Away", TalentBranch.Swiftness, 2)
                .Gives(slip)
                .Needs("Fleet Foot")
                .Described("Twists out of a grip that should have held.")
                .Unlocks("Slip Away")
                .NeedsAttribute(Attributes.AttributeKind.Dexterity, 13);
            branch.Add(slipAway);

            TalentEffect pounce = new TalentEffect();
            pounce.Damage = 2;
            pounce.Accuracy = 0.08;
            pounce.Description = "Strikes first, and lands it.";

            Talent pouncing = new Talent("Pouncing Strike", TalentBranch.Swiftness, 3)
                .Gives(pounce)
                .Needs("Slip Away")
                .Described("The first blow of a fight is the one that counts.")
                .NeedsAttribute(Attributes.AttributeKind.Dexterity, 14);
            branch.Add(pouncing);

            TalentEffect pursuit = new TalentEffect();
            pursuit.Initiative = 0.15;
            pursuit.Tracking = 0.2;
            pursuit.Description = "Runs prey down and does not stop.";

            Talent tireless = new Talent("Tireless Pursuit", TalentBranch.Swiftness, 4)
                .Gives(pursuit)
                .Needs("Pouncing Strike")
                .Described("Outlasts anything it chases.")
                .Unlocks("Tireless Pursuit")
                .NeedsAttribute(Attributes.AttributeKind.Constitution, 13);
            branch.Add(tireless);

            return branch;
        }

        // ------------------------------------------------------------------
        // Endurance: taking hits and getting up again
        // ------------------------------------------------------------------

        public static List<Talent> Endurance()
        {
            List<Talent> branch = new List<Talent>();

            TalentEffect thick = new TalentEffect();
            thick.Health = 8;
            thick.Description = "A heavier frame.";

            Talent thickHide = new Talent("Thick Hide", TalentBranch.Endurance, 1)
                .Gives(thick)
                .Described("The hide has taken a beating before and holds.")
                .NeedsAttribute(Attributes.AttributeKind.Constitution, 11);
            branch.Add(thickHide);

            TalentEffect tough = new TalentEffect();
            tough.DamageReduction = 0.04;
            tough.Description = "Blows glance off.";

            Talent toughItOut = new Talent("Tough It Out", TalentBranch.Endurance, 2)
                .Gives(tough)
                .Needs("Thick Hide")
                .Described("Keeps standing when it should be down.")
                .NeedsAttribute(Attributes.AttributeKind.Constitution, 13);
            branch.Add(toughItOut);

            TalentEffect mend = new TalentEffect();
            mend.Healing = 0.2;
            mend.Description = "Wounds close faster.";

            Talent quickMender = new Talent("Quick Mender", TalentBranch.Endurance, 3)
                .Gives(mend)
                .Needs("Tough It Out")
                .Described("Heals between fights the way it never used to.")
                .Unlocks("Regenerate")
                .NeedsAttribute(Attributes.AttributeKind.Constitution, 14);
            branch.Add(quickMender);

            TalentEffect undying = new TalentEffect();
            undying.Health = 15;
            undying.Resolve = 0.15;
            undying.Description = "Refuses to fall.";

            Talent undyingWill = new Talent("Undying Will", TalentBranch.Endurance, 4)
                .Gives(undying)
                .Needs("Quick Mender")
                .Described("Fights on past the point the body should stop.")
                .CappedAt(TalentRank.Basic)
                .NeedsAttribute(Attributes.AttributeKind.Willpower, 15);
            branch.Add(undyingWill);

            return branch;
        }

        // ------------------------------------------------------------------
        // Pack: leading and fighting together
        // ------------------------------------------------------------------

        public static List<Talent> Pack()
        {
            List<Talent> branch = new List<Talent>();

            TalentEffect voice = new TalentEffect();
            voice.Leadership = 0.1;
            voice.Description = "Heard and followed.";

            Talent packVoice = new Talent("Pack Voice", TalentBranch.Pack, 1)
                .Gives(voice)
                .Described("Its howl is the one the others answer.")
                .NeedsAttribute(Attributes.AttributeKind.Presence, 11);
            branch.Add(packVoice);

            TalentEffect flank = new TalentEffect();
            flank.Accuracy = 0.06;
            flank.Leadership = 0.08;
            flank.Description = "Fights better with others.";

            Talent flanking = new Talent("Flanking Instinct", TalentBranch.Pack, 2)
                .Gives(flank)
                .Needs("Pack Voice")
                .Described("Knows where to stand when the pack closes in.")
                .Unlocks("Pack Flank")
                .NeedsAttribute(Attributes.AttributeKind.Perception, 12);
            branch.Add(flanking);

            TalentEffect rally = new TalentEffect();
            rally.Leadership = 0.2;
            rally.Resolve = 0.1;
            rally.Description = "Holds the pack together.";

            Talent rallying = new Talent("Rallying Howl", TalentBranch.Pack, 3)
                .Gives(rally)
                .Needs("Flanking Instinct")
                .Described("A call that steadies wolves that were about to break.")
                .Unlocks("Rally")
                .NeedsAttribute(Attributes.AttributeKind.Presence, 14);
            branch.Add(rallying);

            TalentEffect alpha = new TalentEffect();
            alpha.Leadership = 0.25;
            alpha.Damage = 3;
            alpha.Health = 10;
            alpha.Description = "Leads from the front.";

            Talent alphaPresence = new Talent("Alpha Presence", TalentBranch.Pack, 4)
                .Gives(alpha)
                .Needs("Rallying Howl")
                .Described("The pack fights harder because it is watching.")
                .CappedAt(TalentRank.Improved)
                .NeedsAttribute(Attributes.AttributeKind.Presence, 16);
            branch.Add(alphaPresence);

            return branch;
        }

        // ------------------------------------------------------------------
        // Instinct: senses and knowing
        // ------------------------------------------------------------------

        public static List<Talent> Instinct()
        {
            List<Talent> branch = new List<Talent>();

            TalentEffect scent = new TalentEffect();
            scent.Tracking = 0.25;
            scent.Description = "Reads a trail.";

            Talent keenScent = new Talent("Keen Scent", TalentBranch.Instinct, 1)
                .Gives(scent)
                .Described("Follows a scent across a day-old trail.")
                .NeedsAttribute(Attributes.AttributeKind.Perception, 11);
            branch.Add(keenScent);

            TalentEffect eyes = new TalentEffect();
            eyes.Accuracy = 0.07;
            eyes.Description = "Sees in the dark.";

            Talent nightSight = new Talent("Night Sight", TalentBranch.Instinct, 2)
                .Gives(eyes)
                .Needs("Keen Scent")
                .Described("The dark costs it nothing.")
                .NeedsAttribute(Attributes.AttributeKind.Perception, 13);
            branch.Add(nightSight);

            TalentEffect quiet = new TalentEffect();
            quiet.Stealth = 0.25;
            quiet.Evasion = 0.03;
            quiet.Description = "Moves without being heard.";

            Talent silentStep = new Talent("Silent Step", TalentBranch.Instinct, 3)
                .Gives(quiet)
                .Needs("Night Sight")
                .Described("Comes on the pack without the pack knowing.")
                .Unlocks("Stalk")
                .NeedsAttribute(Attributes.AttributeKind.Dexterity, 13);
            branch.Add(silentStep);

            TalentEffect read = new TalentEffect();
            read.Accuracy = 0.1;
            read.Evasion = 0.05;
            read.Initiative = 0.1;
            read.Description = "Sees what the enemy is about to do.";

            Talent reading = new Talent("Reading the Enemy", TalentBranch.Instinct, 4)
                .Gives(read)
                .Needs("Silent Step")
                .Described("It has learnt the shape of a fight before it starts.")
                .CappedAt(TalentRank.Basic)
                .NeedsAttribute(Attributes.AttributeKind.Intelligence, 14);
            branch.Add(reading);

            return branch;
        }

        // ------------------------------------------------------------------
        // Moon: the curse and what it costs
        // ------------------------------------------------------------------

        public static List<Talent> Moon()
        {
            List<Talent> branch = new List<Talent>();

            TalentEffect bind = new TalentEffect();
            bind.MoonControl = 0.2;
            bind.Description = "Keeps hold of itself.";

            Talent moonTether = new Talent("Moon Tether", TalentBranch.Moon, 1)
                .Gives(bind)
                .Described("Remembers its own name through the change.")
                .NeedsAttribute(Attributes.AttributeKind.Willpower, 12);
            branch.Add(moonTether);

            TalentEffect shift = new TalentEffect();
            shift.Damage = 3;
            shift.Health = 6;
            shift.MoonControl = -0.05;
            shift.Description = "Stronger in the changed shape, and further gone.";

            Talent willingChange = new Talent("Willing Change", TalentBranch.Moon, 2)
                .Gives(shift)
                .Needs("Moon Tether")
                .Described("Chooses the change, and pays for it.")
                .Unlocks("Shapeshift")
                .NeedsAttribute(Attributes.AttributeKind.Willpower, 13);
            branch.Add(willingChange);

            TalentEffect silver = new TalentEffect();
            silver.DamageReduction = 0.06;
            silver.Resolve = 0.15;
            silver.Description = "Harder to hurt with the old remedies.";

            Talent silverResist = new Talent("Silver Resistance", TalentBranch.Moon, 3)
                .Gives(silver)
                .Needs("Willing Change")
                .Described("The bite of silver is a wound like any other, now.")
                .NeedsAttribute(Attributes.AttributeKind.Constitution, 14);
            branch.Add(silverResist);

            TalentEffect master = new TalentEffect();
            master.MoonControl = 0.4;
            master.Resolve = 0.2;
            master.Damage = 4;
            master.Description = "The moon answers to it.";

            Talent moonMastery = new Talent("Moon Mastery", TalentBranch.Moon, 4)
                .Gives(master)
                .Needs("Silver Resistance")
                .Described("It decides when the change comes, not the moon.")
                .CappedAt(TalentRank.Basic)
                .NeedsAttribute(Attributes.AttributeKind.Willpower, 17);
            branch.Add(moonMastery);

            return branch;
        }

        /// <summary>How many talents there are, for a report.</summary>
        public static int Count()
        {
            return All().Count;
        }

        /// <summary>A description of the tree, for a log line.</summary>
        public static string Describe()
        {
            return "TalentTree (" + Count() + " talents)";
        }
    }
}

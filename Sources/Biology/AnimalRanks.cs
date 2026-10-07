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
    /// A <see cref="Taxon"/> that also carries a concrete class identifier, so a
    /// rung of the taxonomic ladder can be found and compared by enum rather
    /// than by string. This is the counterpart of <see cref="Phylum"/> one rung
    /// down.
    /// </summary>
    public sealed class Class : Taxon
    {
        private ChordateClass? m_chordateClass;
        private ArthropodClass? m_arthropodClass;
        private MolluscClass? m_molluscClass;
        private VertebrateClass? m_vertebrateClass;

        /// <summary>The class identifier as the ChordateClass enum, if it is one.</summary>
        public ChordateClass? ChordateClass
        {
            get { return m_chordateClass; }
        }

        /// <summary>The class identifier as the ArthropodClass enum, if it is one.</summary>
        public ArthropodClass? ArthropodClass
        {
            get { return m_arthropodClass; }
        }

        /// <summary>The class identifier as the MolluscClass enum, if it is one.</summary>
        public MolluscClass? MolluscClass
        {
            get { return m_molluscClass; }
        }

        /// <summary>The class identifier as the VertebrateClass enum, if it is one.</summary>
        public VertebrateClass? VertebrateClass
        {
            get { return m_vertebrateClass; }
        }

        /// <summary>
        /// Every name this class is known by: the node name plus the aliases
        /// that the individual class enums use for the same rung.
        /// </summary>
        public IList<string> Aliases
        {
            get { return m_aliases; }
        }

        private List<string> m_aliases;

        public Class(string name)
            : base(TaxonomyRank.Class, name)
        {
            m_aliases = new List<string>();
        }

        public Class(ChordateClass chordateClass)
            : base(TaxonomyRank.Class, chordateClass.ToString())
        {
            m_chordateClass = chordateClass;
            m_aliases = new List<string>();
        }

        public Class(ArthropodClass arthropodClass)
            : base(TaxonomyRank.Class, arthropodClass.ToString())
        {
            m_arthropodClass = arthropodClass;
            m_aliases = new List<string>();
        }

        public Class(MolluscClass molluscClass)
            : base(TaxonomyRank.Class, molluscClass.ToString())
        {
            m_molluscClass = molluscClass;
            m_aliases = new List<string>();
        }

        public Class(VertebrateClass vertebrateClass)
            : base(TaxonomyRank.Class, vertebrateClass.ToString())
        {
            m_vertebrateClass = vertebrateClass;
            m_aliases = new List<string>();
        }

        /// <summary>Records another name this class is known by.</summary>
        public void AddAlias(string alias)
        {
            if (!string.IsNullOrEmpty(alias) && !m_aliases.Contains(alias))
                m_aliases.Add(alias);
        }

        /// <summary>
        /// True if this class answers to a name, checking the node name and every
        /// recorded alias. Case insensitive.
        /// </summary>
        public bool Matches(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            if (string.Equals(Name, name, StringComparison.OrdinalIgnoreCase))
                return true;

            for (int i = 0; i < m_aliases.Count; i++)
            {
                if (string.Equals(m_aliases[i], name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public override string ToString()
        {
            return TaxonomyRank.Class + ": " + Name;
        }
    }
}

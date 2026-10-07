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
    /// One rung of the taxonomic ladder. Every rank can hold child ranks and
    /// a flat list of the species that sit directly at that rung.
    /// </summary>
    public class Taxon
    {
        private TaxonomyRank m_rank;
        private string m_name;
        private string m_description;
        private Taxon m_parent;
        private List<Taxon> m_children;
        private List<Species> m_species;

        public TaxonomyRank Rank
        {
            get { return m_rank; }
            set { m_rank = value; }
        }

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        public Taxon Parent
        {
            get { return m_parent; }
        }

        public IList<Taxon> Children
        {
            get { return m_children; }
        }

        public IList<Species> Species
        {
            get { return m_species; }
        }

        public Taxon(TaxonomyRank rank, string name)
        {
            m_rank = rank;
            m_name = name;
            m_description = string.Empty;
            m_children = new List<Taxon>();
            m_species = new List<Species>();
        }

        public Taxon AddChild(Taxon child)
        {
            if (child == null || child == this)
                return null;

            child.m_parent = this;

            if (!m_children.Contains(child))
                m_children.Add(child);

            return child;
        }

        public Taxon AddChild(TaxonomyRank rank, string name)
        {
            return AddChild(new Taxon(rank, name));
        }

        public Species AddSpecies(Species species)
        {
            if (species == null)
                return null;

            if (!m_species.Contains(species))
                m_species.Add(species);

            return species;
        }

        /// <summary>Every species beneath this taxon, at any depth.</summary>
        public List<Species> GetAllSpecies()
        {
            List<Species> result = new List<Species>(m_species);

            for (int i = 0; i < m_children.Count; i++)
                result.AddRange(m_children[i].GetAllSpecies());

            return result;
        }

        /// <summary>Total number of species beneath this taxon.</summary>
        public int CountSpecies()
        {
            int count = m_species.Count;

            for (int i = 0; i < m_children.Count; i++)
                count += m_children[i].CountSpecies();

            return count;
        }

        /// <summary>Walks up the tree and builds "Domain &gt; Kingdom &gt; ... &gt; name".</summary>
        public string GetLineage()
        {
            if (m_parent == null)
                return m_name;

            return m_parent.GetLineage() + " > " + m_name;
        }

        public override string ToString()
        {
            return m_rank + ": " + m_name;
        }
    }

    /// <summary>
    /// A phylum, i.e. a Kingdom-level child. Kept as a named type so phyla can
    /// carry an AnimalPhylum identifier.
    /// </summary>
    public sealed class Phylum : Taxon
    {
        private AnimalPhylum m_phylum;

        public AnimalPhylum PhylumEnum
        {
            get { return m_phylum; }
            set { m_phylum = value; }
        }

        public Phylum(string name)
            : base(TaxonomyRank.Phylum, name)
        {
        }

        public Phylum(AnimalPhylum phylum)
            : base(TaxonomyRank.Phylum, phylum.ToString())
        {
            m_phylum = phylum;
        }
    }
}

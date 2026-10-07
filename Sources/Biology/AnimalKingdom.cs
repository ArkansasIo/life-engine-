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
    /// A single named species. Every species knows its own name, its genus, its
    /// conservation status and the free-form traits that describe it.
    /// </summary>
    public class Species
    {
        private string m_name;
        private string m_scientificName;
        private string m_genus;
        private string m_family;
        private string m_conservationStatus;
        private int m_populationEstimate;
        private bool m_isExtinct;
        private List<string> m_traits;

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public string ScientificName
        {
            get { return m_scientificName; }
            set { m_scientificName = value; }
        }

        public string Genus
        {
            get { return m_genus; }
            set { m_genus = value; }
        }

        public string Family
        {
            get { return m_family; }
            set { m_family = value; }
        }

        public string ConservationStatus
        {
            get { return m_conservationStatus; }
            set { m_conservationStatus = value; }
        }

        public int PopulationEstimate
        {
            get { return m_populationEstimate; }
            set { m_populationEstimate = value; }
        }

        public bool IsExtinct
        {
            get { return m_isExtinct; }
            set { m_isExtinct = value; }
        }

        public IList<string> Traits
        {
            get { return m_traits; }
        }

        public Species(string name)
        {
            m_name = name;
            m_scientificName = string.Empty;
            m_genus = string.Empty;
            m_family = string.Empty;
            m_conservationStatus = "Not evaluated";
            m_traits = new List<string>();
        }

        public void AddTrait(string trait)
        {
            if (!string.IsNullOrEmpty(trait) && !m_traits.Contains(trait))
                m_traits.Add(trait);
        }

        public override string ToString()
        {
            return m_name;
        }
    }

    /// <summary>
    /// The animal kingdom (Animalia / Metazoa). Holds the animal phyla and the
    /// concrete animal types that sit beneath them.
    /// </summary>
    public sealed class AnimalKingdom
    {
        private Taxon m_root;
        private Dictionary<AnimalPhylum, Phylum> m_phyla;

        public Taxon Root
        {
            get { return m_root; }
        }

        public IList<Taxon> Phyla
        {
            get { return m_root.Children; }
        }

        public AnimalKingdom()
        {
            m_root = new Taxon(TaxonomyRank.Kingdom, "Animalia");
            m_root.Description = "Multicellular eukaryotes that ingest organic matter.";
            m_phyla = new Dictionary<AnimalPhylum, Phylum>();
        }

        /// <summary>Creates (or returns) the phylum node for a given phylum.</summary>
        public Phylum GetPhylum(AnimalPhylum phylum)
        {
            if (m_phyla.ContainsKey(phylum))
                return m_phyla[phylum];

            Phylum node = new Phylum(phylum);
            m_root.AddChild(node);
            m_phyla[phylum] = node;

            return node;
        }

        /// <summary>Registers a concrete animal under the correct phylum.</summary>
        public Animal RegisterAnimal(Animal animal)
        {
            if (animal == null)
                return null;

            Phylum node = GetPhylum(animal.Phylum);
            node.AddSpecies(animal.ToSpecies());

            return animal;
        }

        public override string ToString()
        {
            return "Animalia (" + m_root.Children.Count + " phyla, " +
                   m_root.CountSpecies() + " species)";
        }
    }

    /// <summary>
    /// An animal: a multicellular, motile (usually), heterotrophic eukaryote.
    /// This is the base class for the concrete creatures below.
    /// </summary>
    public class Animal : MulticellularOrganism
    {
        private AnimalPhylum m_phylum;
        private ChordateClass? m_chordateClass;
        private ArthropodClass? m_arthropodClass;
        private MolluscClass? m_molluscClass;
        private bool m_isVertebrate;
        private bool m_isDomesticated;
        private int m_offspringPerCycle;
        private double m_speedKph;

        public AnimalPhylum Phylum
        {
            get { return m_phylum; }
            set { m_phylum = value; }
        }

        public ChordateClass? ChordateClass
        {
            get { return m_chordateClass; }
            set { m_chordateClass = value; }
        }

        public ArthropodClass? ArthropodClass
        {
            get { return m_arthropodClass; }
            set { m_arthropodClass = value; }
        }

        public MolluscClass? MolluscClass
        {
            get { return m_molluscClass; }
            set { m_molluscClass = value; }
        }

        public bool IsVertebrate
        {
            get { return m_isVertebrate; }
            set { m_isVertebrate = value; }
        }

        public bool IsDomesticated
        {
            get { return m_isDomesticated; }
            set { m_isDomesticated = value; }
        }

        public int OffspringPerCycle
        {
            get { return m_offspringPerCycle; }
            set { m_offspringPerCycle = value; }
        }

        /// <summary>Typical travelling speed in kilometres per hour.</summary>
        public double SpeedKph
        {
            get { return m_speedKph; }
            set { m_speedKph = value; }
        }

        protected Animal(string name, AnimalPhylum phylum)
            : base(name)
        {
            Domain = Biology.Domain.Eukaryota;
            Kingdom = Biology.Kingdom.Animalia;
            m_phylum = phylum;
            m_reproduction = ReproductionMode.Sexual;
        }

        /// <summary>Projects this animal into a plain taxonomy Species record.</summary>
        public Species ToSpecies()
        {
            Species species = new Species(m_name);
            species.ScientificName = m_scientificName;
            species.AddTrait("diet: " + m_diet);
            species.AddTrait("phylum: " + m_phylum);
            species.AddTrait("symmetry: " + m_symmetry);

            if (m_chordateClass.HasValue)
                species.AddTrait("class: " + m_chordateClass.Value);

            if (m_arthropodClass.HasValue)
                species.AddTrait("class: " + m_arthropodClass.Value);

            if (m_molluscClass.HasValue)
                species.AddTrait("class: " + m_molluscClass.Value);

            return species;
        }
    }

    /// <summary>
    /// A vertebrate animal (phylum Chordata with a backbone): mammals, birds,
    /// reptiles, amphibians and fish.
    /// </summary>
    public class Vertebrate : Animal
    {
        private VertebrateClass m_class;
        private bool m_hasFur;
        private bool m_hasFeathers;
        private bool m_hasScales;
        private bool m_isWarmBlooded;
        private bool m_laysEggs;
        private int m_gestationDays;

        public VertebrateClass VertebrateClass
        {
            get { return m_class; }
            set { m_class = value; }
        }

        public bool HasFur
        {
            get { return m_hasFur; }
            set { m_hasFur = value; }
        }

        public bool HasFeathers
        {
            get { return m_hasFeathers; }
            set { m_hasFeathers = value; }
        }

        public bool HasScales
        {
            get { return m_hasScales; }
            set { m_hasScales = value; }
        }

        public bool IsWarmBlooded
        {
            get { return m_isWarmBlooded; }
            set { m_isWarmBlooded = value; }
        }

        public bool LaysEggs
        {
            get { return m_laysEggs; }
            set { m_laysEggs = value; }
        }

        public int GestationDays
        {
            get { return m_gestationDays; }
            set { m_gestationDays = value; }
        }

        public Vertebrate(string name, VertebrateClass vertebrateClass)
            : base(name, AnimalPhylum.Chordata)
        {
            m_class = vertebrateClass;
            IsVertebrate = true;

            switch (vertebrateClass)
            {
                case VertebrateClass.Mammal:
                    m_hasFur = true;
                    m_thermal = ThermalStrategy.Endotherm;
                    m_gestationDays = 180;
                    break;
                case VertebrateClass.Bird:
                    m_hasFeathers = true;
                    m_thermal = ThermalStrategy.Endotherm;
                    m_laysEggs = true;
                    break;
                case VertebrateClass.Reptile:
                    m_hasScales = true;
                    m_thermal = ThermalStrategy.Ectotherm;
                    m_laysEggs = true;
                    break;
                case VertebrateClass.Amphibian:
                    m_thermal = ThermalStrategy.Ectotherm;
                    m_laysEggs = true;
                    break;
                default:
                    m_hasScales = true;
                    m_thermal = ThermalStrategy.Ectotherm;
                    m_laysEggs = true;
                    break;
            }
        }
    }

    /// <summary>
    /// An invertebrate animal: arthropods, molluscs, worms, jellyfish and the
    /// rest of the non-chordate phyla.
    /// </summary>
    public class Invertebrate : Animal
    {
        /// <summary>
        /// Protected rather than private: a concrete invertebrate is what knows
        /// whether it has an exoskeleton, so its constructor sets these.
        /// </summary>
        protected bool m_hasExoskeleton;
        protected bool m_hasSegmentedBody;
        protected int m_legCount;
        public bool HasExoskeleton
        {
            get { return m_hasExoskeleton; }
            set { m_hasExoskeleton = value; }
        }

        public bool HasSegmentedBody
        {
            get { return m_hasSegmentedBody; }
            set { m_hasSegmentedBody = value; }
        }

        public int LegCount
        {
            get { return m_legCount; }
            set { m_legCount = value; }
        }

        public Invertebrate(string name, AnimalPhylum phylum)
            : base(name, phylum)
        {
            IsVertebrate = false;
            m_thermal = ThermalStrategy.Ectotherm;
        }
    }
}

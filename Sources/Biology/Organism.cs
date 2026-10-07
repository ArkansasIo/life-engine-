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
    /// Base class for any living (or acellular) entity. Everything that exists
    /// in the engine shares this: a name, a kingdom, a diet, a size, and the
    /// molecular machinery of life.
    /// </summary>
    public abstract class Organism
    {
        protected string m_name;
        protected string m_scientificName;
        private Domain m_domain;
        private Kingdom m_kingdom;
        private Cellularity m_cellularity;
        /// <summary>
        /// These are protected rather than private: a concrete organism is the
        /// only thing that knows its own diet, gait and size, so its constructor
        /// is what sets them.
        /// </summary>
        protected DietType m_diet;
        protected ReproductionMode m_reproduction;
        protected ThermalStrategy m_thermal;
        protected LocomotionType m_locomotion;
        protected BodySymmetry m_symmetry;
        protected double m_sizeMetres;
        protected double m_massKilograms;
        private int m_chromosomePairs;
        private int m_lifespanYears;
        private double m_energy;

        private Dna m_dna;
        private List<Rna> m_rna;
        private List<Gene> m_genes;

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

        public Domain Domain
        {
            get { return m_domain; }
            set { m_domain = value; }
        }

        public Kingdom Kingdom
        {
            get { return m_kingdom; }
            set { m_kingdom = value; }
        }

        public Cellularity Cellularity
        {
            get { return m_cellularity; }
            protected set { m_cellularity = value; }
        }

        public DietType Diet
        {
            get { return m_diet; }
            set { m_diet = value; }
        }

        public ReproductionMode Reproduction
        {
            get { return m_reproduction; }
            set { m_reproduction = value; }
        }

        public ThermalStrategy Thermal
        {
            get { return m_thermal; }
            set { m_thermal = value; }
        }

        public LocomotionType Locomotion
        {
            get { return m_locomotion; }
            set { m_locomotion = value; }
        }

        public BodySymmetry Symmetry
        {
            get { return m_symmetry; }
            set { m_symmetry = value; }
        }

        /// <summary>Characteristic body length in metres.</summary>
        public double SizeMetres
        {
            get { return m_sizeMetres; }
            set { m_sizeMetres = value; }
        }

        /// <summary>Characteristic body mass in kilograms.</summary>
        public double MassKilograms
        {
            get { return m_massKilograms; }
            set { m_massKilograms = value; }
        }

        public int ChromosomePairs
        {
            get { return m_chromosomePairs; }
            set { m_chromosomePairs = value; }
        }

        public int LifespanYears
        {
            get { return m_lifespanYears; }
            set { m_lifespanYears = value; }
        }

        /// <summary>A generic energy reserve, useful for simulation.</summary>
        public double Energy
        {
            get { return m_energy; }
            set { m_energy = value; }
        }

        /// <summary>The genome of this organism.</summary>
        public Dna Dna
        {
            get { return m_dna; }
            set { m_dna = value; }
        }

        /// <summary>Every RNA transcript currently held by the organism.</summary>
        public IList<Rna> Rna
        {
            get { return m_rna; }
        }

        public IList<Gene> Genes
        {
            get { return m_genes; }
        }

        protected Organism(string name)
        {
            m_name = name;
            m_scientificName = string.Empty;
            m_rna = new List<Rna>();
            m_genes = new List<Gene>();
            m_dna = new Dna();
        }

        public void AddGene(Gene gene)
        {
            if (gene == null || m_genes.Contains(gene))
                return;

            m_genes.Add(gene);
            m_dna.AppendGene(gene);
        }

        public void AddRna(Rna rna)
        {
            if (rna != null && !m_rna.Contains(rna))
                m_rna.Add(rna);
        }

        /// <summary>Transcribes the genome into a fresh messenger RNA strand.</summary>
        public Rna Transcribe()
        {
            // Qualified as Biology.Rna because this class has a property called
            // Rna, which would otherwise shadow the type name here.
            Rna messenger = Biology.Rna.Transcribe(m_dna);
            AddRna(messenger);
            return messenger;
        }

        public override string ToString()
        {
            return m_name;
        }
    }

    /// <summary>
    /// A unicellular organism: a single independent cell. Bacteria, archaea,
    /// protozoa and most algae live here.
    /// </summary>
    public class UnicellularOrganism : Organism
    {
        private CellDivisionType m_division;
        private bool m_hasNucleus;
        private double m_diameterMicrons;
        private bool m_motile;

        public CellDivisionType Division
        {
            get { return m_division; }
            set { m_division = value; }
        }

        /// <summary>True for eukaryotes, false for bacteria and archaea.</summary>
        public bool HasNucleus
        {
            get { return m_hasNucleus; }
            set { m_hasNucleus = value; }
        }

        public double DiameterMicrons
        {
            get { return m_diameterMicrons; }
            set { m_diameterMicrons = value; }
        }

        public bool Motile
        {
            get { return m_motile; }
            set { m_motile = value; }
        }

        public UnicellularOrganism(string name)
            : base(name)
        {
            Cellularity = Cellularity.Unicellular;
            m_division = CellDivisionType.BinaryFission;
            m_sizeMetres = 0.000001;
        }
    }

    /// <summary>
    /// A multicellular organism: many cells that divide labour between
    /// specialised tissues. This is the base of the animal and plant branches.
    /// </summary>
    public abstract class MulticellularOrganism : Organism
    {
        private long m_cellCount;
        private int m_tissueTypes;
        private bool m_hasNervousSystem;
        private bool m_hasCirculatorySystem;
        private bool m_hasSkeleton;
        private double m_growthRate;

        /// <summary>Approximate number of cells in the adult body.</summary>
        public long CellCount
        {
            get { return m_cellCount; }
            set { m_cellCount = value; }
        }

        public int TissueTypes
        {
            get { return m_tissueTypes; }
            set { m_tissueTypes = value; }
        }

        public bool HasNervousSystem
        {
            get { return m_hasNervousSystem; }
            set { m_hasNervousSystem = value; }
        }

        public bool HasCirculatorySystem
        {
            get { return m_hasCirculatorySystem; }
            set { m_hasCirculatorySystem = value; }
        }

        public bool HasSkeleton
        {
            get { return m_hasSkeleton; }
            set { m_hasSkeleton = value; }
        }

        /// <summary>Growth rate as a fraction of body mass per year.</summary>
        public double GrowthRate
        {
            get { return m_growthRate; }
            set { m_growthRate = value; }
        }

        protected MulticellularOrganism(string name)
            : base(name)
        {
            Cellularity = Cellularity.Multicellular;
            m_tissueTypes = 1;
        }
    }

    /// <summary>
    /// An acellular entity: no cells, just a genome in a protein coat. Viruses,
    /// viroids and prions live here.
    /// </summary>
    public sealed class Virus : Organism
    {
        private bool m_hasCapsid;
        private bool m_hasEnvelope;
        private string m_hostSpecies;
        private double m_incubationDays;

        public bool HasCapsid
        {
            get { return m_hasCapsid; }
            set { m_hasCapsid = value; }
        }

        public bool HasEnvelope
        {
            get { return m_hasEnvelope; }
            set { m_hasEnvelope = value; }
        }

        public string HostSpecies
        {
            get { return m_hostSpecies; }
            set { m_hostSpecies = value; }
        }

        public double IncubationDays
        {
            get { return m_incubationDays; }
            set { m_incubationDays = value; }
        }

        public Virus(string name)
            : base(name)
        {
            Domain = Biology.Domain.Viruses;
            Kingdom = Kingdom.Acellularia;
            Cellularity = Cellularity.Acellular;
            Reproduction = ReproductionMode.Viral;
            m_hasCapsid = true;
            m_sizeMetres = 1e-7;
        }
    }
}

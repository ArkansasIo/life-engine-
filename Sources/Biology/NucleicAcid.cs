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
using System.Text;

namespace LifeEngine.Biology
{
    /// <summary>
    /// The five nucleotide bases used by terrestrial life (U replaces T in RNA).
    /// </summary>
    public enum Nucleotide
    {
        Adenine,
        Cytosine,
        Guanine,
        Thymine,
        Uracil
    }

    /// <summary>
    /// Convenience helpers for nucleotide bases.
    /// </summary>
    public static class NucleotideExtensions
    {
        private static readonly Dictionary<char, Nucleotide> Letters =
            new Dictionary<char, Nucleotide>
            {
                { 'A', Nucleotide.Adenine },
                { 'C', Nucleotide.Cytosine },
                { 'G', Nucleotide.Guanine },
                { 'T', Nucleotide.Thymine },
                { 'U', Nucleotide.Uracil }
            };

        public static char ToLetter(this Nucleotide nucleotide)
        {
            switch (nucleotide)
            {
                case Nucleotide.Adenine: return 'A';
                case Nucleotide.Cytosine: return 'C';
                case Nucleotide.Guanine: return 'G';
                case Nucleotide.Thymine: return 'T';
                case Nucleotide.Uracil: return 'U';
                default: return '?';
            }
        }

        public static Nucleotide FromLetter(char letter)
        {
            char upper = char.ToUpperInvariant(letter);

            if (Letters.ContainsKey(upper))
                return Letters[upper];

            throw new ArgumentException("Not a nucleotide letter: " + letter, "letter");
        }

        /// <summary>Returns the complementary DNA base (A-T, C-G).</summary>
        public static Nucleotide DnaComplement(this Nucleotide nucleotide)
        {
            switch (nucleotide)
            {
                case Nucleotide.Adenine: return Nucleotide.Thymine;
                case Nucleotide.Thymine: return Nucleotide.Adenine;
                case Nucleotide.Cytosine: return Nucleotide.Guanine;
                case Nucleotide.Guanine: return Nucleotide.Cytosine;
                default: return nucleotide;
            }
        }

        /// <summary>Returns the complementary RNA base (A-U, C-G).</summary>
        public static Nucleotide RnaComplement(this Nucleotide nucleotide)
        {
            switch (nucleotide)
            {
                case Nucleotide.Adenine: return Nucleotide.Uracil;
                case Nucleotide.Uracil: return Nucleotide.Adenine;
                case Nucleotide.Cytosine: return Nucleotide.Guanine;
                case Nucleotide.Guanine: return Nucleotide.Cytosine;
                default: return nucleotide;
            }
        }
    }

    /// <summary>
    /// Which strand of a nucleic acid we are looking at.
    /// </summary>
    public enum StrandType
    {
        SingleStranded,
        DoubleStranded
    }

    /// <summary>
    /// The kind of nucleic acid molecule.
    /// </summary>
    public enum NucleicAcidType
    {
        Dna,
        Rna,
        /// <summary>Messenger RNA: carries the code out of the nucleus.</summary>
        MessengerRna,
        /// <summary>Transfer RNA: delivers amino acids to the ribosome.</summary>
        TransferRna,
        /// <summary>Ribosomal RNA: the catalytic core of the ribosome.</summary>
        RibosomalRna
    }

    /// <summary>
    /// A single gene: a stretch of bases with a name and an orientation.
    /// </summary>
    public sealed class Gene
    {
        private string m_name;
        private List<Nucleotide> m_bases;
        private GeneOrientation m_orientation;
        private string m_trait;
        private bool m_isDominant;

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public GeneOrientation Orientation
        {
            get { return m_orientation; }
            set { m_orientation = value; }
        }

        /// <summary>The phenotype this gene contributes to.</summary>
        public string Trait
        {
            get { return m_trait; }
            set { m_trait = value; }
        }

        public bool IsDominant
        {
            get { return m_isDominant; }
            set { m_isDominant = value; }
        }

        public int Length
        {
            get { return m_bases.Count; }
        }

        public IList<Nucleotide> Bases
        {
            get { return m_bases; }
        }

        public Gene(string name)
            : this(name, string.Empty)
        {
        }

        public Gene(string name, string sequence)
        {
            m_name = name;
            m_orientation = GeneOrientation.Forward;
            m_isDominant = true;
            m_bases = new List<Nucleotide>();

            if (!string.IsNullOrEmpty(sequence))
                SetSequence(sequence);
        }

        public void SetSequence(string sequence)
        {
            m_bases.Clear();

            for (int i = 0; i < sequence.Length; i++)
            {
                char letter = sequence[i];

                if (char.IsWhiteSpace(letter))
                    continue;

                m_bases.Add(NucleotideExtensions.FromLetter(letter));
            }
        }

        public string GetSequence()
        {
            StringBuilder builder = new StringBuilder(m_bases.Count);

            for (int i = 0; i < m_bases.Count; i++)
                builder.Append(m_bases[i].ToLetter());

            return builder.ToString();
        }

        public override string ToString()
        {
            return m_name + " (" + Length + " bp)";
        }
    }

    /// <summary>
    /// A full strand of DNA or RNA: an ordered list of nucleotides plus the
    /// genes and regulatory regions that live on it.
    /// </summary>
    public class NucleicAcid
    {
        private NucleicAcidType m_type;
        private StrandType m_strandType;
        private List<Nucleotide> m_bases;
        private List<Gene> m_genes;
        private double m_mutationRate;

        public NucleicAcidType Type
        {
            get { return m_type; }
        }

        public StrandType StrandType
        {
            get { return m_strandType; }
            set { m_strandType = value; }
        }

        public IList<Nucleotide> Bases
        {
            get { return m_bases; }
        }

        public IList<Gene> Genes
        {
            get { return m_genes; }
        }

        public int Length
        {
            get { return m_bases.Count; }
        }

        /// <summary>Probability per base, per replication, of a copying error.</summary>
        public double MutationRate
        {
            get { return m_mutationRate; }
            set { m_mutationRate = value; }
        }

        public NucleicAcid(NucleicAcidType type)
        {
            m_type = type;
            m_strandType = StrandType.DoubleStranded;
            m_bases = new List<Nucleotide>();
            m_genes = new List<Gene>();
            m_mutationRate = 1e-9;
        }

        public NucleicAcid(NucleicAcidType type, string sequence)
            : this(type)
        {
            AppendSequence(sequence);
        }

        public void AppendSequence(string sequence)
        {
            for (int i = 0; i < sequence.Length; i++)
            {
                char letter = sequence[i];

                if (char.IsWhiteSpace(letter))
                    continue;

                m_bases.Add(NucleotideExtensions.FromLetter(letter));
            }
        }

        public void AppendGene(Gene gene)
        {
            if (gene != null && !m_genes.Contains(gene))
                m_genes.Add(gene);
        }

        public string GetSequence()
        {
            StringBuilder builder = new StringBuilder(m_bases.Count);

            for (int i = 0; i < m_bases.Count; i++)
                builder.Append(m_bases[i].ToLetter());

            return builder.ToString();
        }

        /// <summary>Returns the complementary strand of this molecule.</summary>
        public NucleicAcid GetComplement()
        {
            NucleicAcid complement = new NucleicAcid(m_type);
            complement.m_strandType = m_strandType;

            for (int i = 0; i < m_bases.Count; i++)
            {
                complement.m_bases.Add(m_type == NucleicAcidType.Dna
                    ? m_bases[i].DnaComplement()
                    : m_bases[i].RnaComplement());
            }

            return complement;
        }

        public override string ToString()
        {
            return m_type + " strand of " + Length + " bases";
        }
    }

    /// <summary>
    /// A strand of deoxyribonucleic acid. Always holds thymine, never uracil.
    /// </summary>
    public sealed class Dna : NucleicAcid
    {
        public Dna()
            : base(NucleicAcidType.Dna)
        {
            StrandType = StrandType.DoubleStranded;
        }

        public Dna(string sequence)
            : base(NucleicAcidType.Dna, sequence)
        {
            StrandType = StrandType.DoubleStranded;
        }

        /// <summary>
        /// Builds a DNA molecule from a set of chromosomes (each one is a
        /// separate Dna object).
        /// </summary>
        public static Dna FromChromosomes(IList<Dna> chromosomes)
        {
            Dna genome = new Dna();

            if (chromosomes == null)
                return genome;

            for (int i = 0; i < chromosomes.Count; i++)
            {
                Dna chromosome = chromosomes[i];

                if (chromosome == null)
                    continue;

                for (int b = 0; b < chromosome.Bases.Count; b++)
                    genome.Bases.Add(chromosome.Bases[b]);

                for (int g = 0; g < chromosome.Genes.Count; g++)
                    genome.AppendGene(chromosome.Genes[g]);
            }

            return genome;
        }
    }

    /// <summary>
    /// A strand of ribonucleic acid. Holds uracil instead of thymine and is
    /// normally single stranded.
    /// </summary>
    public sealed class Rna : NucleicAcid
    {
        public Rna()
            : this(NucleicAcidType.Rna)
        {
        }

        public Rna(NucleicAcidType type)
            : base(type)
        {
            StrandType = StrandType.SingleStranded;
        }

        public Rna(NucleicAcidType type, string sequence)
            : base(type, sequence)
        {
            StrandType = StrandType.SingleStranded;
        }

        /// <summary>
        /// Transcribes a DNA strand into messenger RNA, swapping T for U.
        /// </summary>
        public static Rna Transcribe(Dna dna)
        {
            Rna messenger = new Rna(NucleicAcidType.MessengerRna);

            if (dna == null)
                return messenger;

            for (int i = 0; i < dna.Bases.Count; i++)
            {
                Nucleotide baseNucleotide = dna.Bases[i];
                Nucleotide transcribed = baseNucleotide == Nucleotide.Thymine
                    ? Nucleotide.Uracil
                    : baseNucleotide;

                messenger.Bases.Add(transcribed);
            }

            for (int g = 0; g < dna.Genes.Count; g++)
                messenger.AppendGene(dna.Genes[g]);

            return messenger;
        }
    }
}

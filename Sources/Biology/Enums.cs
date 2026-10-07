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

namespace LifeEngine.Biology
{
    /// <summary>
    /// The conventional rank hierarchy used by biological taxonomy.
    /// </summary>
    public enum TaxonomyRank
    {
        Domain,
        Kingdom,
        Phylum,
        Class,
        Order,
        Family,
        Genus,
        Species
    }

    /// <summary>
    /// The three domains of cellular (and acellular) life.
    /// </summary>
    public enum Domain
    {
        Bacteria,
        Archaea,
        Eukaryota,
        /// <summary>Acellular entities such as viruses and viroids.</summary>
        Viruses
    }

    /// <summary>
    /// The classical eukaryotic kingdoms. The two prokaryotic domains are also
    /// exposed as "kingdoms" so a single enum can hold every top level group.
    /// </summary>
    public enum Kingdom
    {
        Bacteria,
        Archaea,
        Protista,
        Fungi,
        Plantae,
        Animalia,
        /// <summary>Realm of acellular life (viruses, viroids, prions).</summary>
        Acellularia
    }

    /// <summary>
    /// Animal phyla (selected, with the major ones plus the notable minor ones).
    /// </summary>
    public enum AnimalPhylum
    {
        Porifera,
        Cnidaria,
        Ctenophora,
        Platyhelminthes,
        Nematoda,
        Annelida,
        Mollusca,
        Arthropoda,
        Echinodermata,
        Hemichordata,
        Chordata,
        Rotifera,
        Tardigrada,
        Onychophora,
        Nemertea,
        Brachiopoda,
        Bryozoa,
        Xenacoelomorpha
    }

    /// <summary>
    /// Classes within the phylum Chordata.
    /// </summary>
    public enum ChordateClass
    {
        Mammalia,
        Aves,
        Reptilia,
        Amphibia,
        Actinopterygii,
        Sarcopterygii,
        Chondrichthyes,
        Myxini,
        Petromyzontida,
        Ascidiacea,
        Thaliacea,
        Appendicularia
    }

    /// <summary>
    /// Classes within the phylum Arthropoda.
    /// </summary>
    public enum ArthropodClass
    {
        Insecta,
        Arachnida,
        Malacostraca,
        Branchiopoda,
        Chilopoda,
        Diplopoda,
        Pauropoda,
        Symphyla,
        Merostomata,
        Pycnogonida,
        Remipedia,
        Cephalocarida
    }

    /// <summary>
    /// Classes within the phylum Mollusca.
    /// </summary>
    public enum MolluscClass
    {
        Gastropoda,
        Bivalvia,
        Cephalopoda,
        Polyplacophora,
        Scaphopoda,
        Monoplacophora,
        Aplacophora
    }

    /// <summary>
    /// Broad grouping of the major vertebrate classes.
    /// </summary>
    public enum VertebrateClass
    {
        Mammal,
        Bird,
        Reptile,
        Amphibian,
        BonyFish,
        LobeFinnedFish,
        CartilaginousFish,
        JawlessFish
    }

    /// <summary>
    /// Feeding strategy of an organism.
    /// </summary>
    public enum DietType
    {
        Herbivore,
        Carnivore,
        Omnivore,
        Detritivore,
        FilterFeeder,
        Photosynthetic,
        Chemosynthetic,
        Saprotroph,
        Parasite
    }

    /// <summary>
    /// How the organism reproduces.
    /// </summary>
    public enum ReproductionMode
    {
        Sexual,
        Asexual,
        BinaryFission,
        Budding,
        Fragmentation,
        Parthenogenesis,
        Sporulation,
        Viral
    }

    /// <summary>
    /// Body temperature regulation strategy.
    /// </summary>
    public enum ThermalStrategy
    {
        Endotherm,
        Ectotherm,
        Mesotherm,
        Heterotherm
    }

    /// <summary>
    /// The structural grade of an organism, i.e. how many cells it is built from.
    /// </summary>
    public enum Cellularity
    {
        /// <summary>Acellular: no cells at all (viruses, viroids, prions).</summary>
        Acellular,
        Unicellular,
        Colonial,
        Multicellular
    }

    /// <summary>
    /// The type of locomotion an organism primarily uses.
    /// </summary>
    public enum LocomotionType
    {
        None,
        Flagellar,
        Ciliary,
        Amoeboid,
        Swimming,
        Crawling,
        Walking,
        Running,
        Flying,
        Gliding,
        Burrowing,
        Sessile
    }

    /// <summary>
    /// Asexual/sexual cellular division mechanism.
    /// </summary>
    public enum CellDivisionType
    {
        Mitosis,
        Meiosis,
        BinaryFission,
        Budding
    }

    /// <summary>
    /// Symmetry of the body plan.
    /// </summary>
    public enum BodySymmetry
    {
        Asymmetric,
        Radial,
        Bilateral,
        Spherical
    }

    /// <summary>
    /// A gene's struct/definition
    /// </summary>
    public enum GeneOrientation
    {
        Forward,
        Reverse
    }
}

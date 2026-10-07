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
    /// Base class for the non-animal eukaryotic kingdoms, i.e. plants, fungi and
    /// protists.
    /// </summary>
    public abstract class Eukaryote : MulticellularOrganism
    {
        private string m_clade;
        private bool m_hasCellWall;
        private string m_cellWallMaterial;

        public string Clade
        {
            get { return m_clade; }
            set { m_clade = value; }
        }

        public bool HasCellWall
        {
            get { return m_hasCellWall; }
            set { m_hasCellWall = value; }
        }

        public string CellWallMaterial
        {
            get { return m_cellWallMaterial; }
            set { m_cellWallMaterial = value; }
        }

        protected Eukaryote(string name, Kingdom kingdom)
            : base(name)
        {
            Domain = Biology.Domain.Eukaryota;
            Kingdom = kingdom;
            m_clade = string.Empty;
        }
    }

    /// <summary>
    /// A plant: a photosynthetic eukaryote with cellulose cell walls. May be
    /// unicellular (as the algae are) so it derives from Organism directly when
    /// needed; the class below covers the multicellular grade.
    /// </summary>
    public class Plant : Eukaryote
    {
        private string m_division;
        private bool m_hasVascularTissue;
        private bool m_hasSeeds;
        private bool m_hasFlowers;
        private bool m_hasRoots;
        private double m_maxHeightMetres;
        private string m_photosyntheticPigment;
        private bool m_isDeciduous;

        public string Division
        {
            get { return m_division; }
            set { m_division = value; }
        }

        public bool HasVascularTissue
        {
            get { return m_hasVascularTissue; }
            set { m_hasVascularTissue = value; }
        }

        public bool HasSeeds
        {
            get { return m_hasSeeds; }
            set { m_hasSeeds = value; }
        }

        public bool HasFlowers
        {
            get { return m_hasFlowers; }
            set { m_hasFlowers = value; }
        }

        public bool HasRoots
        {
            get { return m_hasRoots; }
            set { m_hasRoots = value; }
        }

        public double MaxHeightMetres
        {
            get { return m_maxHeightMetres; }
            set { m_maxHeightMetres = value; }
        }

        public string PhotosyntheticPigment
        {
            get { return m_photosyntheticPigment; }
            set { m_photosyntheticPigment = value; }
        }

        /// <summary>Sheds its leaves seasonally.</summary>
        public bool IsDeciduous
        {
            get { return m_isDeciduous; }
            set { m_isDeciduous = value; }
        }

        public Plant(string name)
            : base(name, Kingdom.Plantae)
        {
            HasCellWall = true;
            CellWallMaterial = "Cellulose";
            m_photosyntheticPigment = "Chlorophyll a";
            m_diet = DietType.Photosynthetic;
            m_reproduction = ReproductionMode.Sexual;
            m_locomotion = LocomotionType.Sessile;
            m_symmetry = BodySymmetry.Radial;
            m_division = "Unassigned";
        }
    }

    /// <summary>
    /// A tree: a woody plant with a trunk and secondary growth.
    /// </summary>
    public sealed class Tree : Plant
    {
        private double m_trunkDiameterMetres;
        private int m_canopyLayers;
        private bool m_isEvergreen;

        public double TrunkDiameterMetres
        {
            get { return m_trunkDiameterMetres; }
            set { m_trunkDiameterMetres = value; }
        }

        public int CanopyLayers
        {
            get { return m_canopyLayers; }
            set { m_canopyLayers = value; }
        }

        public bool IsEvergreen
        {
            get { return m_isEvergreen; }
            set { m_isEvergreen = value; }
        }

        public Tree(string name)
            : base(name)
        {
            HasVascularTissue = true;
            HasSeeds = true;
            HasRoots = true;
            m_canopyLayers = 3;
        }
    }

    /// <summary>
    /// A flowering plant: the angiosperms.
    /// </summary>
    public sealed class FloweringPlant : Plant
    {
        private string m_flowerColor;
        private string m_pollinationVector;
        private bool m_isFragrant;

        public string FlowerColor
        {
            get { return m_flowerColor; }
            set { m_flowerColor = value; }
        }

        /// <summary>How it is pollinated: wind, insect, bird, bat, water.</summary>
        public string PollinationVector
        {
            get { return m_pollinationVector; }
            set { m_pollinationVector = value; }
        }

        public bool IsFragrant
        {
            get { return m_isFragrant; }
            set { m_isFragrant = value; }
        }

        public FloweringPlant(string name)
            : base(name)
        {
            HasVascularTissue = true;
            HasSeeds = true;
            HasFlowers = true;
            HasRoots = true;
            m_pollinationVector = "Insect";
        }
    }

    /// <summary>
    /// A non-vascular plant: mosses, liverworts and hornworts.
    /// </summary>
    public sealed class Moss : Plant
    {
        private bool m_needsMoisture;
        private double m_cushionHeightMm;

        public bool NeedsMoisture
        {
            get { return m_needsMoisture; }
            set { m_needsMoisture = value; }
        }

        public double CushionHeightMm
        {
            get { return m_cushionHeightMm; }
            set { m_cushionHeightMm = value; }
        }

        public Moss(string name)
            : base(name)
        {
            HasVascularTissue = false;
            HasSeeds = false;
            m_needsMoisture = true;
            m_sizeMetres = 0.05;
        }
    }

    /// <summary>
    /// A fern: a vascular plant that reproduces by spores.
    /// </summary>
    public sealed class Fern : Plant
    {
        private bool m_hasFronds;
        private int m_frondCount;

        public bool HasFronds
        {
            get { return m_hasFronds; }
            set { m_hasFronds = value; }
        }

        public int FrondCount
        {
            get { return m_frondCount; }
            set { m_frondCount = value; }
        }

        public Fern(string name)
            : base(name)
        {
            HasVascularTissue = true;
            HasSeeds = false;
            m_hasFronds = true;
            m_reproduction = ReproductionMode.Sporulation;
        }
    }

    /// <summary>
    /// A fungus: a heterotrophic eukaryote with chitin cell walls. Includes
    /// yeasts, moulds and mushrooms.
    /// </summary>
    public class Fungus : Eukaryote
    {
        private string m_phylum;
        private bool m_isSingleCelled;
        private bool m_formsMycelium;
        private bool m_isParasitic;
        private bool m_isSymbiotic;
        private bool m_producesMushroom;
        private double m_capDiameterCm;

        public string Phylum
        {
            get { return m_phylum; }
            set { m_phylum = value; }
        }

        public bool IsSingleCelled
        {
            get { return m_isSingleCelled; }
            set { m_isSingleCelled = value; }
        }

        public bool FormsMycelium
        {
            get { return m_formsMycelium; }
            set { m_formsMycelium = value; }
        }

        public bool IsParasitic
        {
            get { return m_isParasitic; }
            set { m_isParasitic = value; }
        }

        /// <summary>Forms lichens or mycorrhizae with other organisms.</summary>
        public bool IsSymbiotic
        {
            get { return m_isSymbiotic; }
            set { m_isSymbiotic = value; }
        }

        /// <summary>Produces a visible fruiting body (mushroom).</summary>
        public bool ProducesMushroom
        {
            get { return m_producesMushroom; }
            set { m_producesMushroom = value; }
        }

        public double CapDiameterCm
        {
            get { return m_capDiameterCm; }
            set { m_capDiameterCm = value; }
        }

        public Fungus(string name)
            : base(name, Kingdom.Fungi)
        {
            HasCellWall = true;
            CellWallMaterial = "Chitin";
            m_diet = DietType.Saprotroph;
            m_reproduction = ReproductionMode.Sporulation;
            m_locomotion = LocomotionType.Sessile;
            m_formsMycelium = true;
        }
    }

    /// <summary>
    /// A protist: a eukaryote that is not a plant, animal or fungus. Most are
    /// unicellular, so this type also exposes the unicellular way of life.
    /// </summary>
    public class Protist : Organism
    {
        protected string m_group;
        protected bool m_isPhotosynthetic;
        private bool m_formsColonies;
        private bool m_hasFlagella;
        private bool m_hasCilia;
        private bool m_hasPseudopods;

        public string Group
        {
            get { return m_group; }
            set { m_group = value; }
        }

        public bool IsPhotosynthetic
        {
            get { return m_isPhotosynthetic; }
            set { m_isPhotosynthetic = value; }
        }

        public bool FormsColonies
        {
            get { return m_formsColonies; }
            set { m_formsColonies = value; }
        }

        public bool HasFlagella
        {
            get { return m_hasFlagella; }
            set { m_hasFlagella = value; }
        }

        public bool HasCilia
        {
            get { return m_hasCilia; }
            set { m_hasCilia = value; }
        }

        public bool HasPseudopods
        {
            get { return m_hasPseudopods; }
            set { m_hasPseudopods = value; }
        }

        public Protist(string name)
            : base(name)
        {
            Domain = Biology.Domain.Eukaryota;
            Kingdom = Kingdom.Protista;
            Cellularity = Cellularity.Unicellular;
            m_group = "Unassigned";
            m_sizeMetres = 0.00005;
            m_locomotion = LocomotionType.Amoeboid;
        }
    }

    /// <summary>
    /// A unicellular alga, i.e. a photosynthetic protist such as a diatom or a
    /// dinoflagellate.
    /// </summary>
    public sealed class Alga : Protist
    {
        private string m_pigment;
        private bool m_hasSilicaShell;

        public string Pigment
        {
            get { return m_pigment; }
            set { m_pigment = value; }
        }

        public bool HasSilicaShell
        {
            get { return m_hasSilicaShell; }
            set { m_hasSilicaShell = value; }
        }

        public Alga(string name)
            : base(name)
        {
            IsPhotosynthetic = true;
            m_diet = DietType.Photosynthetic;
            m_group = "Algae";
            m_pigment = "Chlorophyll a";
        }
    }

    /// <summary>
    /// A bacterium: a prokaryote in the domain Bacteria.
    /// </summary>
    public sealed class Bacterium : UnicellularOrganism
    {
        private string m_shapeT;
        private bool m_isGramPositive;
        private bool m_formsEndospores;
        private bool m_isPathogenic;
        private bool m_hasFlagella;

        public string Shape
        {
            get { return m_shapeT; }
            set { m_shapeT = value; }
        }

        public bool IsGramPositive
        {
            get { return m_isGramPositive; }
            set { m_isGramPositive = value; }
        }

        public bool FormsEndospores
        {
            get { return m_formsEndospores; }
            set { m_formsEndospores = value; }
        }

        public bool IsPathogenic
        {
            get { return m_isPathogenic; }
            set { m_isPathogenic = value; }
        }

        public bool HasFlagella
        {
            get { return m_hasFlagella; }
            set { m_hasFlagella = value; }
        }

        public Bacterium(string name)
            : base(name)
        {
            Domain = Biology.Domain.Bacteria;
            Kingdom = Kingdom.Bacteria;
            HasNucleus = false;
            m_shapeT = "Rod";
            m_diet = DietType.Chemosynthetic;
            m_locomotion = LocomotionType.Flagellar;
            m_sizeMetres = 0.000002;
        }
    }

    /// <summary>
    /// An archaeon: a prokaryote in the domain Archaea, often extremophilic.
    /// </summary>
    public sealed class Archaeon : UnicellularOrganism
    {
        private bool m_isExtremophile;
        private string m_environment;

        public bool IsExtremophile
        {
            get { return m_isExtremophile; }
            set { m_isExtremophile = value; }
        }

        public string Environment
        {
            get { return m_environment; }
            set { m_environment = value; }
        }

        public Archaeon(string name)
            : base(name)
        {
            Domain = Biology.Domain.Archaea;
            Kingdom = Kingdom.Archaea;
            HasNucleus = false;
            m_diet = DietType.Chemosynthetic;
            m_sizeMetres = 0.000002;
            m_environment = "Unassigned";
        }
    }
}

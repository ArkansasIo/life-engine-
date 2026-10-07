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

//
// The enums below are imported under alias names. Each concrete animal sets a
// property called ChordateClass, ArthropodClass or MolluscClass, and a property
// shadows a type of the same name inside that class — so "ChordateClass.Aves"
// on the right of the assignment would resolve to the property, not the enum.
// The aliases give the enums names nothing can shadow.
//
using ChordateEnum = LifeEngine.Biology.ChordateClass;
using ArthropodEnum = LifeEngine.Biology.ArthropodClass;
using MolluscEnum = LifeEngine.Biology.MolluscClass;

namespace LifeEngine.Biology
{
    /// <summary>
    /// A concrete mammal type. Instances describe one kind of mammal rather than
    /// one individual animal.
    /// </summary>
    public sealed class Mammal : Vertebrate
    {
        private string m_order;
        private bool m_isMarine;
        private bool m_hasHooves;
        private bool m_hasTrunk;

        public string Order
        {
            get { return m_order; }
            set { m_order = value; }
        }

        public bool IsMarine
        {
            get { return m_isMarine; }
            set { m_isMarine = value; }
        }

        public bool HasHooves
        {
            get { return m_hasHooves; }
            set { m_hasHooves = value; }
        }

        public bool HasTrunk
        {
            get { return m_hasTrunk; }
            set { m_hasTrunk = value; }
        }

        public Mammal(string name)
            : base(name, VertebrateClass.Mammal)
        {
            ChordateClass = ChordateEnum.Mammalia;
            m_order = "Unassigned";
            m_locomotion = LocomotionType.Walking;
            m_symmetry = BodySymmetry.Bilateral;
        }
    }

    /// <summary>
    /// A concrete bird type.
    /// </summary>
    public sealed class Bird : Vertebrate
    {
        private bool m_canFly;
        private double m_wingspanMetres;
        private bool m_isMigratory;
        private string m_beakType;

        public bool CanFly
        {
            get { return m_canFly; }
            set { m_canFly = value; }
        }

        public double WingspanMetres
        {
            get { return m_wingspanMetres; }
            set { m_wingspanMetres = value; }
        }

        public bool IsMigratory
        {
            get { return m_isMigratory; }
            set { m_isMigratory = value; }
        }

        public string BeakType
        {
            get { return m_beakType; }
            set { m_beakType = value; }
        }

        public Bird(string name)
            : base(name, VertebrateClass.Bird)
        {
            ChordateClass = ChordateEnum.Aves;
            m_canFly = true;
            m_locomotion = LocomotionType.Flying;
            m_symmetry = BodySymmetry.Bilateral;
        }
    }

    /// <summary>
    /// A concrete reptile type.
    /// </summary>
    public sealed class Reptile : Vertebrate
    {
        private bool m_isVenomous;
        private bool m_canRegrowTail;
        private string m_scaleType;

        public bool IsVenomous
        {
            get { return m_isVenomous; }
            set { m_isVenomous = value; }
        }

        public bool CanRegrowTail
        {
            get { return m_canRegrowTail; }
            set { m_canRegrowTail = value; }
        }

        public string ScaleType
        {
            get { return m_scaleType; }
            set { m_scaleType = value; }
        }

        public Reptile(string name)
            : base(name, VertebrateClass.Reptile)
        {
            ChordateClass = ChordateEnum.Reptilia;
            m_locomotion = LocomotionType.Crawling;
            m_symmetry = BodySymmetry.Bilateral;
        }
    }

    /// <summary>
    /// A concrete amphibian type.
    /// </summary>
    public sealed class Amphibian : Vertebrate
    {
        private bool m_needsWater;
        private bool m_isMetamorphic;
        private bool m_hasTadpoleStage;

        public bool NeedsWater
        {
            get { return m_needsWater; }
            set { m_needsWater = value; }
        }

        public bool IsMetamorphic
        {
            get { return m_isMetamorphic; }
            set { m_isMetamorphic = value; }
        }

        public bool HasTadpoleStage
        {
            get { return m_hasTadpoleStage; }
            set { m_hasTadpoleStage = value; }
        }

        public Amphibian(string name)
            : base(name, VertebrateClass.Amphibian)
        {
            ChordateClass = ChordateEnum.Amphibia;
            m_needsWater = true;
            m_hasTadpoleStage = true;
            m_locomotion = LocomotionType.Swimming;
            m_symmetry = BodySymmetry.Bilateral;
        }
    }

    /// <summary>
    /// A fish: bony, lobe-finned, cartilaginous or jawless.
    /// </summary>
    public sealed class Fish : Vertebrate
    {
        private bool m_isFreshwater;
        private bool m_isCartilaginous;
        private bool m_hasJaw;
        private bool m_isElectric;

        public bool IsFreshwater
        {
            get { return m_isFreshwater; }
            set { m_isFreshwater = value; }
        }

        public bool IsCartilaginous
        {
            get { return m_isCartilaginous; }
            set { m_isCartilaginous = value; }
        }

        public bool HasJaw
        {
            get { return m_hasJaw; }
            set { m_hasJaw = value; }
        }

        public bool IsElectric
        {
            get { return m_isElectric; }
            set { m_isElectric = value; }
        }

        public Fish(string name, VertebrateClass vertebrateClass)
            : base(name, vertebrateClass)
        {
            m_hasJaw = true;

            switch (vertebrateClass)
            {
                case VertebrateClass.CartilaginousFish:
                    ChordateClass = ChordateEnum.Chondrichthyes;
                    m_isCartilaginous = true;
                    break;
                case VertebrateClass.JawlessFish:
                    ChordateClass = ChordateEnum.Myxini;
                    m_hasJaw = false;
                    break;
                case VertebrateClass.LobeFinnedFish:
                    ChordateClass = ChordateEnum.Sarcopterygii;
                    break;
                default:
                    ChordateClass = ChordateEnum.Actinopterygii;
                    break;
            }

            m_locomotion = LocomotionType.Swimming;
            m_symmetry = BodySymmetry.Bilateral;
        }
    }

    /// <summary>
    /// An insect: six legs, three body segments, often winged.
    /// </summary>
    public sealed class Insect : Invertebrate
    {
        private bool m_canFly;
        private int m_wingCount;
        private bool m_isSocial;
        private bool m_metamorphosis;

        public bool CanFly
        {
            get { return m_canFly; }
            set { m_canFly = value; }
        }

        public int WingCount
        {
            get { return m_wingCount; }
            set { m_wingCount = value; }
        }

        public bool IsSocial
        {
            get { return m_isSocial; }
            set { m_isSocial = value; }
        }

        /// <summary>True for complete metamorphosis (egg, larva, pupa, adult).</summary>
        public bool Metamorphosis
        {
            get { return m_metamorphosis; }
            set { m_metamorphosis = value; }
        }

        public Insect(string name)
            : base(name, AnimalPhylum.Arthropoda)
        {
            ArthropodClass = ArthropodEnum.Insecta;
            LegCount = 6;
            m_hasSegmentedBody = true;
            m_hasExoskeleton = true;
            m_isSocial = false;
            m_locomotion = LocomotionType.Walking;
            m_symmetry = BodySymmetry.Bilateral;
        }
    }

    /// <summary>
    /// An arachnid: eight legs, two body segments, no wings.
    /// </summary>
    public sealed class Arachnid : Invertebrate
    {
        private bool m_isVenomous;
        private bool m_spinsSilk;
        private int m_eyeCount;

        public bool IsVenomous
        {
            get { return m_isVenomous; }
            set { m_isVenomous = value; }
        }

        public bool SpinsSilk
        {
            get { return m_spinsSilk; }
            set { m_spinsSilk = value; }
        }

        public int EyeCount
        {
            get { return m_eyeCount; }
            set { m_eyeCount = value; }
        }

        public Arachnid(string name)
            : base(name, AnimalPhylum.Arthropoda)
        {
            ArthropodClass = ArthropodEnum.Arachnida;
            LegCount = 8;
            m_eyeCount = 8;
            m_hasSegmentedBody = true;
            m_hasExoskeleton = true;
            m_locomotion = LocomotionType.Crawling;
            m_symmetry = BodySymmetry.Bilateral;
        }
    }

    /// <summary>
    /// A crustacean: crabs, lobsters, shrimp, barnacles and woodlice.
    /// </summary>
    public sealed class Crustacean : Invertebrate
    {
        private bool m_isMarine;
        private int m_clawCount;
        private bool m_hasCarapace;

        public bool IsMarine
        {
            get { return m_isMarine; }
            set { m_isMarine = value; }
        }

        public int ClawCount
        {
            get { return m_clawCount; }
            set { m_clawCount = value; }
        }

        public bool HasCarapace
        {
            get { return m_hasCarapace; }
            set { m_hasCarapace = value; }
        }

        public Crustacean(string name)
            : base(name, AnimalPhylum.Arthropoda)
        {
            ArthropodClass = ArthropodEnum.Malacostraca;
            m_isMarine = true;
            m_hasCarapace = true;
            m_hasExoskeleton = true;
            m_hasSegmentedBody = true;
            m_locomotion = LocomotionType.Crawling;
            m_symmetry = BodySymmetry.Bilateral;
        }
    }

    /// <summary>
    /// A cephalopod: octopus, squid, cuttlefish and nautilus.
    /// </summary>
    public sealed class Cephalopod : Invertebrate
    {
        private int m_tentacleCount;
        private bool m_hasInkSac;
        private bool m_canChangeColor;
        private bool m_hasShell;

        public int TentacleCount
        {
            get { return m_tentacleCount; }
            set { m_tentacleCount = value; }
        }

        public bool HasInkSac
        {
            get { return m_hasInkSac; }
            set { m_hasInkSac = value; }
        }

        public bool CanChangeColor
        {
            get { return m_canChangeColor; }
            set { m_canChangeColor = value; }
        }

        public bool HasShell
        {
            get { return m_hasShell; }
            set { m_hasShell = value; }
        }

        public Cephalopod(string name)
            : base(name, AnimalPhylum.Mollusca)
        {
            MolluscClass = MolluscEnum.Cephalopoda;
            m_tentacleCount = 8;
            m_hasInkSac = true;
            m_canChangeColor = true;
            m_locomotion = LocomotionType.Swimming;
            m_symmetry = BodySymmetry.Bilateral;
        }
    }

    /// <summary>
    /// A gastropod: snails and slugs.
    /// </summary>
    public sealed class Gastropod : Invertebrate
    {
        private bool m_hasShell;
        private bool m_isSlug;
        private bool m_isTerrestrial;

        public bool HasShell
        {
            get { return m_hasShell; }
            set { m_hasShell = value; }
        }

        public bool IsSlug
        {
            get { return m_isSlug; }
            set { m_isSlug = value; }
        }

        public bool IsTerrestrial
        {
            get { return m_isTerrestrial; }
            set { m_isTerrestrial = value; }
        }

        public Gastropod(string name)
            : base(name, AnimalPhylum.Mollusca)
        {
            MolluscClass = MolluscEnum.Gastropoda;
            m_hasShell = true;
            m_locomotion = LocomotionType.Crawling;
            m_symmetry = BodySymmetry.Bilateral;
        }
    }

    /// <summary>
    /// A bivalve: clams, oysters, mussels and scallops.
    /// </summary>
    public sealed class Bivalve : Invertebrate
    {
        private bool m_canProducePearl;
        private bool m_isBurrowing;
        private double m_shellLengthMm;

        public bool CanProducePearl
        {
            get { return m_canProducePearl; }
            set { m_canProducePearl = value; }
        }

        public bool IsBurrowing
        {
            get { return m_isBurrowing; }
            set { m_isBurrowing = value; }
        }

        public double ShellLengthMm
        {
            get { return m_shellLengthMm; }
            set { m_shellLengthMm = value; }
        }

        public Bivalve(string name)
            : base(name, AnimalPhylum.Mollusca)
        {
            MolluscClass = MolluscEnum.Bivalvia;
            m_diet = DietType.FilterFeeder;
            m_symmetry = BodySymmetry.Bilateral;
        }
    }

    /// <summary>
    /// A jellyfish, coral, sea anemone or hydroid (phylum Cnidaria).
    /// </summary>
    public sealed class Cnidarian : Invertebrate
    {
        private bool m_hasStingingCells;
        private bool m_isPolyp;
        private bool m_formsColony;

        public bool HasStingingCells
        {
            get { return m_hasStingingCells; }
            set { m_hasStingingCells = value; }
        }

        public bool IsPolyp
        {
            get { return m_isPolyp; }
            set { m_isPolyp = value; }
        }

        public bool FormsColony
        {
            get { return m_formsColony; }
            set { m_formsColony = value; }
        }

        public Cnidarian(string name)
            : base(name, AnimalPhylum.Cnidaria)
        {
            m_hasStingingCells = true;
            m_symmetry = BodySymmetry.Radial;
            m_locomotion = LocomotionType.Sessile;
        }
    }

    /// <summary>
    /// A sponge (phylum Porifera): a sessile filter feeder with no true tissues.
    /// </summary>
    public sealed class Sponge : Invertebrate
    {
        private bool m_hasSpicules;
        private string m_spiculeMaterial;

        public bool HasSpicules
        {
            get { return m_hasSpicules; }
            set { m_hasSpicules = value; }
        }

        public string SpiculeMaterial
        {
            get { return m_spiculeMaterial; }
            set { m_spiculeMaterial = value; }
        }

        public Sponge(string name)
            : base(name, AnimalPhylum.Porifera)
        {
            m_diet = DietType.FilterFeeder;
            m_symmetry = BodySymmetry.Asymmetric;
            m_locomotion = LocomotionType.Sessile;
            m_hasSpicules = true;
            m_spiculeMaterial = "Calcium carbonate";
        }
    }

    /// <summary>
    /// An annelid: earthworms, leeches and marine polychaete worms.
    /// </summary>
    public sealed class Annelid : Invertebrate
    {
        private bool m_isSegmented;
        private bool m_isParasitic;
        private bool m_hasBristles;

        public bool IsSegmented
        {
            get { return m_isSegmented; }
            set { m_isSegmented = value; }
        }

        public bool IsParasitic
        {
            get { return m_isParasitic; }
            set { m_isParasitic = value; }
        }

        public bool HasBristles
        {
            get { return m_hasBristles; }
            set { m_hasBristles = value; }
        }

        public Annelid(string name)
            : base(name, AnimalPhylum.Annelida)
        {
            m_isSegmented = true;
            m_hasBristles = true;
            m_locomotion = LocomotionType.Crawling;
            m_symmetry = BodySymmetry.Bilateral;
        }
    }
}

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
    /// Builds the animal half of the taxonomy: every phylum, class and order the
    /// concrete animals use, each with its rank and its scientific and common
    /// name, and the fifteen animal types filed beneath it.
    ///
    /// The catalogue builds monotone phyla too (Echinodermata, Nematoda and the
    /// others the engine has no creatures for yet), so the tree is the real
    /// animal kingdom rather than only the parts that have been implemented.
    /// </summary>
    public static class AnimalRanks
    {
        /// <summary>
        /// Fills a kingdom with the full animal catalogue. Safe to call more than
        /// once: existing nodes are reused rather than duplicated.
        /// </summary>
        public static AnimalKingdom BuildAnimalKingdom(AnimalKingdom kingdom)
        {
            if (kingdom == null)
                return null;

            Taxon root = kingdom.Root;

            BuildChordata(root);
            BuildArthropoda(root);
            BuildMollusca(root);
            BuildCnidaria(root);
            BuildPorifera(root);
            BuildAnnelida(root);
            BuildUnusedPhyla(root);

            return kingdom;
        }

        /// <summary>Every animal order the catalogue defines, in build order.</summary>
        public static List<Order> GetOrders(AnimalKingdom kingdom)
        {
            List<Order> orders = new List<Order>();

            if (kingdom == null)
                return orders;

            CollectOrders(kingdom.Root, orders);

            return orders;
        }

        private static void CollectOrders(Taxon node, List<Order> into)
        {
            for (int i = 0; i < node.Children.Count; i++)
            {
                Taxon child = node.Children[i];

                Order order = child as Order;

                if (order != null)
                    into.Add(order);

                CollectOrders(child, into);
            }
        }

        // ------------------------------------------------------------------
        // Chordata
        // ------------------------------------------------------------------

        private static void BuildChordata(Taxon root)
        {
            Phylum chordata = GetOrAddPhylum(root, AnimalPhylum.Chordata);

            Class mammalia = AddClass(chordata, "Mammalia", ChordateClass.Mammalia);
            AddOrder(mammalia, "Primates", "Primates");
            AddOrder(mammalia, "Carnivora", "Carnivora", "caniforms", "feliforms");
            AddOrder(mammalia, "Artiodactyla", "Artiodactyla", "even-toed ungulates");
            AddOrder(mammalia, "Perissodactyla", "Perissodactyla", "odd-toed ungulates");
            AddOrder(mammalia, "Chiroptera", "Chiroptera", "bats");
            AddOrder(mammalia, "Cetacea", "Cetacea", "whales", "dolphins", "porpoises");
            AddOrder(mammalia, "Rodentia", "Rodentia", "rodents");
            AddOrder(mammalia, "Proboscidea", "Proboscidea", "elephants");

            Class aves = AddClass(chordata, "Aves", ChordateClass.Aves);
            AddOrder(aves, "Accipitriformes", "Accipitriformes", "hawks", "eagles", "kites");
            AddOrder(aves, "Falconiformes", "Falconiformes", "falcons");
            AddOrder(aves, "Strigiformes", "Strigiformes", "owls");
            AddOrder(aves, "Passeriformes", "Passeriformes", "perching birds", "songbirds");
            AddOrder(aves, "Sphenisciformes", "Sphenisciformes", "penguins");
            AddOrder(aves, "Psittaciformes", "Psittaciformes", "parrots");

            Class reptilia = AddClass(chordata, "Reptilia", ChordateClass.Reptilia);
            AddOrder(reptilia, "Squamata", "Squamata", "lizards", "snakes");
            AddOrder(reptilia, "Testudines", "Testudines", "turtles", "tortoises");
            AddOrder(reptilia, "Crocodilia", "Crocodilia", "crocodiles", "alligators");
            AddOrder(reptilia, "Rhynchocephalia", "Rhynchocephalia", "tuatara");

            Class amphibia = AddClass(chordata, "Amphibia", ChordateClass.Amphibia);
            AddOrder(amphibia, "Anura", "Anura", "frogs", "toads");
            AddOrder(amphibia, "Caudata", "Caudata", "salamanders", "newts");
            AddOrder(amphibia, "Gymnophiona", "Gymnophiona", "caecilians");

            Class actinopterygii = AddClass(chordata, "Actinopterygii", ChordateClass.Actinopterygii);
            AddOrder(actinopterygii, "Salmoniformes", "Salmoniformes", "salmon", "trout");
            AddOrder(actinopterygii, "Perciformes", "Perciformes", "perch-like fishes");
            AddOrder(actinopterygii, "Cypriniformes", "Cypriniformes", "carps", "minnows");

            Class sarcopterygii = AddClass(chordata, "Sarcopterygii", ChordateClass.Sarcopterygii);
            AddOrder(sarcopterygii, "Coelacanthiformes", "Coelacanthiformes", "coelacanths");
            AddOrder(sarcopterygii, "Dipnoi", "Dipnoi", "lungfish");

            Class chondrichthyes = AddClass(chordata, "Chondrichthyes", ChordateClass.Chondrichthyes);
            AddOrder(chondrichthyes, "Carcharhiniformes", "Carcharhiniformes", "ground sharks");
            AddOrder(chondrichthyes, "Rajiformes", "Rajiformes", "rays", "skates");

            Class myxini = AddClass(chordata, "Myxini", ChordateClass.Myxini);
            AddOrder(myxini, "Myxiniformes", "Myxiniformes", "hagfish");

            Class petromyzontida = AddClass(chordata, "Petromyzontida", ChordateClass.Petromyzontida);
            AddOrder(petromyzontida, "Petromyzontiformes", "Petromyzontiformes", "lampreys");

            AddClass(chordata, "Ascidiacea", ChordateClass.Ascidiacea);
            AddClass(chordata, "Thaliacea", ChordateClass.Thaliacea);
            AddClass(chordata, "Appendicularia", ChordateClass.Appendicularia);
        }

        // ------------------------------------------------------------------
        // Arthropoda
        // ------------------------------------------------------------------

        private static void BuildArthropoda(Taxon root)
        {
            Phylum arthropoda = GetOrAddPhylum(root, AnimalPhylum.Arthropoda);

            Class insecta = AddClass(arthropoda, "Insecta", ArthropodClass.Insecta);
            AddOrder(insecta, "Hymenoptera", "Hymenoptera", "ants", "bees", "wasps");
            AddOrder(insecta, "Coleoptera", "Coleoptera", "beetles");
            AddOrder(insecta, "Lepidoptera", "Lepidoptera", "butterflies", "moths");
            AddOrder(insecta, "Diptera", "Diptera", "flies");
            AddOrder(insecta, "Odonata", "Odonata", "dragonflies", "damselflies");
            AddOrder(insecta, "Orthoptera", "Orthoptera", "grasshoppers", "crickets");

            Class arachnida = AddClass(arthropoda, "Arachnida", ArthropodClass.Arachnida);
            AddOrder(arachnida, "Araneae", "Araneae", "spiders");
            AddOrder(arachnida, "Scorpiones", "Scorpiones", "scorpions");
            AddOrder(arachnida, "Acari", "Acari", "mites", "ticks");
            AddOrder(arachnida, "Opiliones", "Opiliones", "harvestmen");

            Class malacostraca = AddClass(arthropoda, "Malacostraca", ArthropodClass.Malacostraca);
            AddOrder(malacostraca, "Decapoda", "Decapoda", "crabs", "lobsters", "shrimp");
            AddOrder(malacostraca, "Isopoda", "Isopoda", "woodlice", "pill bugs");
            AddOrder(malacostraca, "Amphipoda", "Amphipoda", "sandhoppers", "scuds");

            Class branchiopoda = AddClass(arthropoda, "Branchiopoda", ArthropodClass.Branchiopoda);
            AddOrder(branchiopoda, "Anostraca", "Anostraca", "fairy shrimp");

            AddClass(arthropoda, "Chilopoda", ArthropodClass.Chilopoda);
            AddClass(arthropoda, "Diplopoda", ArthropodClass.Diplopoda);
            AddClass(arthropoda, "Pauropoda", ArthropodClass.Pauropoda);
            AddClass(arthropoda, "Symphyla", ArthropodClass.Symphyla);
            AddClass(arthropoda, "Merostomata", ArthropodClass.Merostomata);
            AddClass(arthropoda, "Pycnogonida", ArthropodClass.Pycnogonida);
            AddClass(arthropoda, "Remipedia", ArthropodClass.Remipedia);
            AddClass(arthropoda, "Cephalocarida", ArthropodClass.Cephalocarida);
        }

        // ------------------------------------------------------------------
        // Mollusca
        // ------------------------------------------------------------------

        private static void BuildMollusca(Taxon root)
        {
            Phylum mollusca = GetOrAddPhylum(root, AnimalPhylum.Mollusca);

            Class cephalopoda = AddClass(mollusca, "Cephalopoda", MolluscClass.Cephalopoda);
            AddOrder(cephalopoda, "Octopoda", "Octopoda", "octopuses");
            AddOrder(cephalopoda, "Teuthida", "Teuthida", "squid");
            AddOrder(cephalopoda, "Sepiida", "Sepiida", "cuttlefish");
            AddOrder(cephalopoda, "Nautilida", "Nautilida", "nautilus");

            Class gastropoda = AddClass(mollusca, "Gastropoda", MolluscClass.Gastropoda);
            AddOrder(gastropoda, "Stylommatophora", "Stylommatophora", "land snails", "slugs");
            AddOrder(gastropoda, "Nudibranchia", "Nudibranchia", "sea slugs");

            Class bivalvia = AddClass(mollusca, "Bivalvia", MolluscClass.Bivalvia);
            AddOrder(bivalvia, "Ostreoida", "Ostreoida", "oysters");
            AddOrder(bivalvia, "Mytilida", "Mytilida", "mussels");
            AddOrder(bivalvia, "Pectinida", "Pectinida", "scallops");

            AddClass(mollusca, "Polyplacophora", MolluscClass.Polyplacophora);
            AddClass(mollusca, "Scaphopoda", MolluscClass.Scaphopoda);
            AddClass(mollusca, "Monoplacophora", MolluscClass.Monoplacophora);
            AddClass(mollusca, "Aplacophora", MolluscClass.Aplacophora);
        }

        // ------------------------------------------------------------------
        // The smaller phyla with creatures
        // ------------------------------------------------------------------

        private static void BuildCnidaria(Taxon root)
        {
            Phylum cnidaria = GetOrAddPhylum(root, AnimalPhylum.Cnidaria);

            Class scyphozoa = AddClass(cnidaria, "Scyphozoa", "Scyphozoa");
            AddOrder(scyphozoa, "Semaeostomeae", "Semaeostomeae", "jellyfish");

            Class anthozoa = AddClass(cnidaria, "Anthozoa", "Anthozoa");
            AddOrder(anthozoa, "Scleractinia", "Scleractinia", "stony corals");
            AddOrder(anthozoa, "Actiniaria", "Actiniaria", "sea anemones");

            Class hydrozoa = AddClass(cnidaria, "Hydrozoa", "Hydrozoa");
            AddOrder(hydrozoa, "Anthoathecata", "Anthoathecata", "hydroids");

            AddClass(cnidaria, "Cubozoa", "Cubozoa");
        }

        private static void BuildPorifera(Taxon root)
        {
            Phylum porifera = GetOrAddPhylum(root, AnimalPhylum.Porifera);

            Class demospongiae = AddClass(porifera, "Demospongiae", "Demospongiae");
            AddOrder(demospongiae, "Dictyoceratida", "Dictyoceratida", "bath sponges");

            AddClass(porifera, "Calcarea", "Calcarea");
            AddClass(porifera, "Hexactinellida", "Hexactinellida");
        }

        private static void BuildAnnelida(Taxon root)
        {
            Phylum annelida = GetOrAddPhylum(root, AnimalPhylum.Annelida);

            Class clitellata = AddClass(annelida, "Clitellata", "Clitellata");
            AddOrder(clitellata, "Crassiclitellata", "Crassiclitellata", "earthworms");
            AddOrder(clitellata, "Arhynchobdellida", "Arhynchobdellida", "leeches");

            Class polychaeta = AddClass(annelida, "Polychaeta", "Polychaeta");
            AddOrder(polychaeta, "Eunicida", "Eunicida", "bristle worms");
        }

        /// <summary>
        /// The phyla the engine recognises but has no concrete animals for yet.
        /// They are still registered so the kingdom reads as a real tree, and so
        /// adding a creature later is a one line change.
        /// </summary>
        private static void BuildUnusedPhyla(Taxon root)
        {
            AddEmptyPhylum(root, AnimalPhylum.Ctenophora, "Ctenophora");
            AddEmptyPhylum(root, AnimalPhylum.Platyhelminthes, "Platyhelminthes");
            AddEmptyPhylum(root, AnimalPhylum.Nematoda, "Nematoda");
            AddEmptyPhylum(root, AnimalPhylum.Echinodermata, "Echinodermata");
            AddEmptyPhylum(root, AnimalPhylum.Hemichordata, "Hemichordata");
            AddEmptyPhylum(root, AnimalPhylum.Rotifera, "Rotifera");
            AddEmptyPhylum(root, AnimalPhylum.Tardigrada, "Tardigrada");
            AddEmptyPhylum(root, AnimalPhylum.Onychophora, "Onychophora");
            AddEmptyPhylum(root, AnimalPhylum.Nemertea, "Nemertea");
            AddEmptyPhylum(root, AnimalPhylum.Brachiopoda, "Brachiopoda");
            AddEmptyPhylum(root, AnimalPhylum.Bryozoa, "Bryozoa");
            AddEmptyPhylum(root, AnimalPhylum.Xenacoelomorpha, "Xenacoelomorpha");
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static Phylum GetOrAddPhylum(Taxon root, AnimalPhylum phylum)
        {
            for (int i = 0; i < root.Children.Count; i++)
            {
                Phylum existing = root.Children[i] as Phylum;

                if (existing != null && existing.PhylumEnum == phylum)
                    return existing;
            }

            Phylum node = new Phylum(phylum);
            node.Description = DescribePhylum(phylum);
            root.AddChild(node);

            return node;
        }

        private static void AddEmptyPhylum(Taxon root, AnimalPhylum phylum, string title)
        {
            Phylum node = GetOrAddPhylum(root, phylum);
            node.Description = DescribePhylum(phylum);
        }

        private static Class GetOrAddClass(Taxon phylum, string name)
        {
            for (int i = 0; i < phylum.Children.Count; i++)
            {
                Class existing = phylum.Children[i] as Class;

                if (existing != null && existing.Matches(name))
                    return existing;
            }

            Class node = new Class(name);
            phylum.AddChild(node);

            return node;
        }

        private static Class AddClass(Taxon phylum, string name, ChordateClass identifier)
        {
            Class node = GetOrAddClass(phylum, name);
            node.Description = DescribeClass(name);
            return node;
        }

        private static Class AddClass(Taxon phylum, string name, ArthropodClass identifier)
        {
            Class node = GetOrAddClass(phylum, name);
            node.Description = DescribeClass(name);
            return node;
        }

        private static Class AddClass(Taxon phylum, string name, MolluscClass identifier)
        {
            Class node = GetOrAddClass(phylum, name);
            node.Description = DescribeClass(name);
            return node;
        }

        private static Class AddClass(Taxon phylum, string name, string commonName)
        {
            Class node = GetOrAddClass(phylum, name);
            node.Description = DescribeClass(name);
            node.AddAlias(commonName);
            return node;
        }

        /// <summary>
        /// Adds an order to a class. The first alias is the English name; any
        /// further ones are the common members people would search for.
        /// </summary>
        private static Order AddOrder(Class parent, string name, string commonName, params string[] aliases)
        {
            Order node = new Order(name);
            node.CommonName = commonName;

            for (int i = 0; i < aliases.Length; i++)
                node.AddAlias(aliases[i]);

            parent.AddChild(node);

            return node;
        }

        // ------------------------------------------------------------------
        // Descriptions
        // ------------------------------------------------------------------

        private static string DescribePhylum(AnimalPhylum phylum)
        {
            switch (phylum)
            {
                case AnimalPhylum.Chordata:
                    return "Animals with a notochord, a dorsal nerve cord and pharyngeal slits.";
                case AnimalPhylum.Arthropoda:
                    return "Segmented animals with an exoskeleton and jointed limbs. The largest phylum.";
                case AnimalPhylum.Mollusca:
                    return "Soft-bodied animals, usually with a shell, a mantle and a muscular foot.";
                case AnimalPhylum.Cnidaria:
                    return "Radially symmetric animals with stinging cells. Jellyfish, corals and anemones.";
                case AnimalPhylum.Porifera:
                    return "Sessile filter feeders with no true tissues. The sponges.";
                case AnimalPhylum.Annelida:
                    return "Segmented worms with a fluid-filled body cavity.";
                case AnimalPhylum.Ctenophora:
                    return "Comb jellies: gelatinous swimmers with rows of cilia.";
                case AnimalPhylum.Platyhelminthes:
                    return "Flatworms. Soft, flattened, mostly parasitic or aquatic.";
                case AnimalPhylum.Nematoda:
                    return "Roundworms. Unsegmented, with a tough cuticle.";
                case AnimalPhylum.Echinodermata:
                    return "Five-fold radial symmetry, a water vascular system and tube feet.";
                case AnimalPhylum.Hemichordata:
                    return "Acorn worms: a bridge between the chordates and the echinoderms.";
                case AnimalPhylum.Rotifera:
                    return "Microscopic animals with a rotating crown of cilia.";
                case AnimalPhylum.Tardigrada:
                    return "Tardigrades: eight-legged micro-animals that survive desiccation and vacuum.";
                case AnimalPhylum.Onychophora:
                    return "Velvet worms: soft-bodied hunters that spit glue.";
                case AnimalPhylum.Nemertea:
                    return "Ribbon worms with an eversible proboscis.";
                case AnimalPhylum.Brachiopoda:
                    return "Lamp shells: two-valved animals that attach by a stalk.";
                case AnimalPhylum.Bryozoa:
                    return "Moss animals: tiny colonial filter feeders in a limestone skeleton.";
                case AnimalPhylum.Xenacoelomorpha:
                    return "Simple marine worms of uncertain position in the animal tree.";
                default:
                    return string.Empty;
            }
        }

        private static string DescribeClass(string name)
        {
            switch (name)
            {
                case "Mammalia": return "Warm-blooded vertebrates that feed their young on milk.";
                case "Aves": return "Feathered, egg-laying vertebrates adapted for flight.";
                case "Reptilia": return "Scaled, cold-blooded vertebrates that lay shelled eggs on land.";
                case "Amphibia": return "Vertebrates that live in water as larvae and on land as adults.";
                case "Actinopterygii": return "Ray-finned fishes, the largest vertebrate class.";
                case "Sarcopterygii": return "Lobe-finned fishes, the line that gave rise to the land vertebrates.";
                case "Chondrichthyes": return "Cartilaginous fishes with skeletons of cartilage, not bone.";
                case "Myxini": return "Hagfish: jawless, boneless and famously slimy.";
                case "Petromyzontida": return "Lampreys: jawless fishes with a toothed sucking disc.";
                case "Ascidiacea": return "Sea squirts: sessile filter feeders with a larval notochord.";
                case "Thaliacea": return "Salps: free-floating barrel-shaped filter feeders.";
                case "Appendicularia": return "Larvaceans: tadpole-shaped filter feeders that build a mucus house.";

                case "Insecta": return "Six-legged arthropods with three body segments. The most numerous class.";
                case "Arachnida": return "Eight-legged arthropods with two body segments and no wings.";
                case "Malacostraca": return "Crabs, lobsters, shrimp and woodlice. The largest crustacean class.";
                case "Branchiopoda": return "Gill-footed crustaceans of temporary fresh water.";
                case "Chilopoda": return "Centipedes: one pair of legs per segment, and venomous.";
                case "Diplopoda": return "Millipedes: two pairs of legs per segment, and detritivorous.";
                case "Pauropoda": return "Tiny soil arthropods with branched antennae.";
                case "Symphyla": return "Soil-dwelling myriapods without eyes.";
                case "Merostomata": return "Horseshoe crabs: ancient marine chelicerates.";
                case "Pycnogonida": return "Sea spiders: legs and little else.";
                case "Remipedia": return "Blind crustaceans found in coastal caves.";
                case "Cephalocarida": return "Horseshoe shrimp: small benthic crustaceans.";

                case "Cephalopoda": return "Active marine molluscs with arms, a beak and a large brain.";
                case "Gastropoda": return "Snails and slugs, mobile and usually shelled.";
                case "Bivalvia": return "Two-shelled filter feeding molluscs.";
                case "Polyplacophora": return "Chitons: eight shell plates and a rasping tongue.";
                case "Scaphopoda": return "Tusk shells: burrowing molluscs with a tubular shell.";
                case "Monoplacophora": return "Deep sea molluscs with a single cap-shaped shell.";
                case "Aplacophora": return "Worm-like molluscs with no shell.";

                case "Scyphozoa": return "True jellyfish: a free-swimming medusa dominates the life cycle.";
                case "Anthozoa": return "Corals and sea anemones: polyps only, with no medusa stage.";
                case "Hydrozoa": return "Hydroids, siphonophores and the freshwater hydra.";
                case "Cubozoa": return "Box jellyfish, with eyes and a potent sting.";

                case "Demospongiae": return "The largest sponge class, with siliceous spicules or spongin.";
                case "Calcarea": return "Sponges with skeletons of calcium carbonate.";
                case "Hexactinellida": return "Glass sponges: six-rayed silica spicules.";

                case "Clitellata": return "Earthworms and leeches, ringed by a reproductive clitellum.";
                case "Polychaeta": return "Bristle worms, mostly marine, with paired parapodia.";
                default: return string.Empty;
            }
        }
    }

    /// <summary>
    /// One order of animals: a rung below class, above family. An order knows its
    /// scientific name, the English name it goes by, and the common names of the
    /// animals that live in it.
    /// </summary>
    public sealed class Order : Taxon
    {
        private string m_commonName;
        private List<string> m_aliases;

        /// <summary>The English name of the order, e.g. "Perching birds".</summary>
        public string CommonName
        {
            get { return m_commonName; }
            set { m_commonName = value; }
        }

        /// <summary>Common names of the members, e.g. "songbirds", "sparrows".</summary>
        public IList<string> Aliases
        {
            get { return m_aliases; }
        }

        public Order(string name)
            : base(TaxonomyRank.Order, name)
        {
            m_commonName = string.Empty;
            m_aliases = new List<string>();
        }

        public Order(string name, string commonName)
            : base(TaxonomyRank.Order, name)
        {
            m_commonName = commonName;
            m_aliases = new List<string>();
        }

        /// <summary>Records a common name of a member of this order.</summary>
        public void AddAlias(string alias)
        {
            if (!string.IsNullOrEmpty(alias) && !m_aliases.Contains(alias))
                m_aliases.Add(alias);
        }

        /// <summary>True if this order answers to a name, scientific or common.</summary>
        public bool Matches(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            if (string.Equals(Name, name, StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(m_commonName, name, StringComparison.OrdinalIgnoreCase))
                return true;

            for (int i = 0; i < m_aliases.Count; i++)
            {
                if (string.Equals(m_aliases[i], name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>Renders as "Carnivora (Carnivora)", or with the members listed.</summary>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(m_commonName))
                return Name;

            return Name + " (" + m_commonName + ")";
        }
    }
}

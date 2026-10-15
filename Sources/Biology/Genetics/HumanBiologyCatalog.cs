/** LifeEngine - GPL-3.0-or-later; Copyright (C) 2026 ArkansasIo */
using System.Collections.Generic;
namespace LifeEngine.Biology.Genetics
{
 public sealed class OrganSystemInfo
 {
  public string Name { get; private set; }
  public string PrimaryFunction { get; private set; }
  public string MajorStructures { get; private set; }
  public OrganSystemInfo(string name, string primaryFunction, string majorStructures) { Name = name; PrimaryFunction = primaryFunction; MajorStructures = majorStructures; }
 }
 /// <summary>Educational index of human organ systems; not a complete anatomy ontology or clinical system.</summary>
 public static class HumanBiologyCatalog
 {
  public const int TypicalDiploidChromosomeCount = 46;
  public const int TypicalAutosomePairs = 22;
  public static IList<OrganSystemInfo> GetOrganSystems()
  {
   return new List<OrganSystemInfo> {
    new OrganSystemInfo("Integumentary", "Barrier, temperature regulation and sensation", "Skin, hair, nails and glands"),
    new OrganSystemInfo("Skeletal", "Support, protection, mineral storage and blood-cell formation", "Bones, cartilage, ligaments and marrow"),
    new OrganSystemInfo("Muscular", "Movement, posture and heat production", "Skeletal, cardiac and smooth muscle"),
    new OrganSystemInfo("Nervous", "Rapid signalling, sensory processing and control", "Brain, spinal cord, peripheral nerves and sensory organs"),
    new OrganSystemInfo("Endocrine", "Hormonal signalling and regulation", "Pituitary, thyroid, parathyroids, adrenals, pancreas and gonads"),
    new OrganSystemInfo("Cardiovascular", "Transport of gases, nutrients, wastes and signals", "Heart, blood and blood vessels"),
    new OrganSystemInfo("Lymphatic and immune", "Fluid return and immune defence", "Lymph vessels, nodes, spleen, thymus and immune cells"),
    new OrganSystemInfo("Respiratory", "Gas exchange and acid-base contribution", "Airways, lungs, alveoli and diaphragm"),
    new OrganSystemInfo("Digestive", "Food processing and nutrient absorption", "Mouth, oesophagus, stomach, intestines, liver, gallbladder and pancreas"),
    new OrganSystemInfo("Urinary", "Waste excretion and fluid/electrolyte balance", "Kidneys, ureters, bladder and urethra"),
    new OrganSystemInfo("Reproductive", "Gamete production and reproduction", "Ovaries and associated structures; testes and associated structures"),
    new OrganSystemInfo("Hematologic", "Oxygen transport, clotting and circulating defence", "Blood cells, plasma, platelets and marrow")
   };
  }
  public static string ChromosomeReferenceNote() { return "Typical human somatic cells: 46 chromosomes (23 pairs); most gametes: 23. Exceptions exist, including aneuploidy and tissue-specific variation."; }
 }
}

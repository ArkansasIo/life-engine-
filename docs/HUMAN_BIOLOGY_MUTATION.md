# Human Biology and Mutation Module

**Namespace:** `LifeEngine.Biology.Genetics`  
**Status:** initial educational/simulation module  
**Target:** existing .NET Framework 4.5 project, C# 7.3

## Scope

This module adds an educational human organ-system index, DNA/RNA sequence validation, and reproducible synthetic sequence mutation operations to LifeEngine. It builds alongside existing `LifeEngine.Biology` types such as `Dna`, `Rna`, `Gene`, and `Organism` without replacing them.

It is intended for education, software testing, and game simulation. It is not a clinical variant interpretation system, a validated human mutation-rate model, a genome annotation pipeline, or medical advice.

## Files

- `Sources/Biology/Genetics/MutationModels.cs`: mutation types and result records.
- `Sources/Biology/Genetics/SequenceValidator.cs`: DNA/RNA alphabet validation and normalization.
- `Sources/Biology/Genetics/MutationEngine.cs`: seeded single-event mutation and per-base Bernoulli substitution model.
- `Sources/Biology/Genetics/HumanBiologyCatalog.cs`: organ-system reference records and typical chromosome counts.
- `data/biology/mutation_examples.csv`: synthetic, explicitly non-human sequences.
- `database/schema_human_mutation.sql`: provenance-aware MySQL schema.

## Example

```csharp
using LifeEngine.Biology.Genetics;

var engine = new MutationEngine(20261015); // fixed seed for reproducible tests
var result = engine.ApplyOne("ACGTACGT", SequenceKind.Dna, MutationKind.Substitution);
System.Console.WriteLine(result.OriginalSequence);
System.Console.WriteLine(result.MutatedSequence);
System.Console.WriteLine(result.Mutation);
```

`ApplyOne` supports substitution, insertion, or deletion. `ApplySubstitutionTrials` samples each base independently using a caller-supplied probability. That probability is an experimental parameter, not a claim about human mutation rates. Coordinates are zero-based offsets into the normalized input sequence. Insertions occur before the selected position; deletions remove the selected base.

## Scientific notes

- DNA changes may be germline or somatic; consequences depend on context, repair, gene regulation, transcript structure, protein effects, cell type, developmental timing, and environment.
- A sequence change is not automatically harmful, beneficial, or phenotypically visible. This module intentionally does not assign clinical significance.
- Typical human diploid somatic cells contain 46 chromosomes (23 pairs); most gametes contain 23. These are reference patterns, not universal rules without exceptions.
- The supplied example sequences are synthetic and are not taken from a person, clinical sample, or reference human genome.
- A fixed random seed supports repeatable software tests but does not make the model biologically validated.

## Build

From the repository root on Windows:

```powershell
dotnet build Sources/LifeEngine.csproj -c Release
```

The SDK-style project includes C# files beneath `Sources/` by default. Run the application on Windows with a compatible .NET Framework 4.5 runtime. The build must be run locally before claiming the package is release-verified.

## Recommended extensions

1. Add unit tests for validation, each mutation class, and deterministic seeds.
2. Add FASTA import/export and sequence provenance metadata.
3. Add transcript features (exons, introns, CDS, UTRs) and codon translation.
4. Add reference datasets only with license, citation, assembly, accession, and version metadata.
5. Store consequence annotations as evidence-linked records; do not infer medical meaning from a sequence change alone.

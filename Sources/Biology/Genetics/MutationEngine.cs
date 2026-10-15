/** LifeEngine - GPL-3.0-or-later; Copyright (C) 2026 ArkansasIo */
using System;
namespace LifeEngine.Biology.Genetics
{
 /// <summary>Seeded stochastic sequence mutation for synthetic research and game modelling; not a clinical variant caller.</summary>
 public sealed class MutationEngine
 {
  private readonly Random m_random;
  public MutationEngine(int seed) { m_random = new Random(seed); }
  public MutationResult ApplyOne(string sequence, SequenceKind kind, MutationKind mutationKind)
  {
   SequenceValidator.RequireValid(sequence, kind); string original = SequenceValidator.Normalize(sequence);
   int position = m_random.Next(original.Length); char before = original[position]; string mutated; string after;
   if (mutationKind == MutationKind.Substitution) { after = PickAlternative(before, kind).ToString(); mutated = original.Substring(0, position) + after + original.Substring(position + 1); }
   else if (mutationKind == MutationKind.Insertion) { after = PickBase(kind).ToString(); mutated = original.Insert(position, after); }
   else { after = string.Empty; mutated = original.Remove(position, 1); }
   MutationRecord record = new MutationRecord(mutationKind, position, before.ToString(), after, "Synthetic single-event mutation; biological effect not inferred.", true);
   return new MutationResult(original, mutated, record);
  }
  /// <summary>Independent per-base Bernoulli substitution toy model; probability is not a human mutation-rate estimate.</summary>
  public MutationResult ApplySubstitutionTrials(string sequence, SequenceKind kind, double probability)
  {
   SequenceValidator.RequireValid(sequence, kind);
   if (probability < 0.0 || probability > 1.0 || double.IsNaN(probability)) throw new ArgumentOutOfRangeException("probability");
   string original = SequenceValidator.Normalize(sequence); char[] bases = original.ToCharArray(); int first = -1; char oldBase = '\0'; char newBase = '\0'; int changes = 0;
   for (int i = 0; i < bases.Length; i++) if (m_random.NextDouble() < probability) { char old = bases[i]; char next = PickAlternative(old, kind); bases[i] = next; if (first < 0) { first = i; oldBase = old; newBase = next; } changes++; }
   MutationRecord record = first < 0
    ? new MutationRecord(MutationKind.Substitution, 0, original.Substring(0, 1), original.Substring(0, 1), "No changes sampled; total changes=0.", true)
    : new MutationRecord(MutationKind.Substitution, first, oldBase.ToString(), newBase.ToString(), "Per-base Bernoulli trials; total changes=" + changes + ".", true);
   return new MutationResult(original, new string(bases), record);
  }
  private char PickAlternative(char current, SequenceKind kind) { char c; do { c = PickBase(kind); } while (c == current); return c; }
  private char PickBase(SequenceKind kind) { string alphabet = kind == SequenceKind.Dna ? "ACGT" : "ACGU"; return alphabet[m_random.Next(alphabet.Length)]; }
 }
}

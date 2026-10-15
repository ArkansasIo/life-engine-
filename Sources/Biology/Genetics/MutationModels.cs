/** LifeEngine - GPL-3.0-or-later; Copyright (C) 2026 ArkansasIo */
using System;
namespace LifeEngine.Biology.Genetics
{
 public enum MutationKind { Substitution, Insertion, Deletion }
 public enum SequenceKind { Dna, Rna }
 public sealed class MutationRecord
 {
  public MutationKind Kind { get; private set; }
  public int Position { get; private set; }
  public string Before { get; private set; }
  public string After { get; private set; }
  public string Description { get; private set; }
  public bool IsSynthetic { get; private set; }
  public MutationRecord(MutationKind kind, int position, string before, string after, string description, bool isSynthetic)
  {
   if (position < 0) throw new ArgumentOutOfRangeException("position");
   Kind = kind; Position = position; Before = before ?? string.Empty; After = after ?? string.Empty;
   Description = description ?? string.Empty; IsSynthetic = isSynthetic;
  }
  public override string ToString() { return Kind + " at " + Position + ": " + Before + " -> " + After; }
 }
 public sealed class MutationResult
 {
  public string OriginalSequence { get; private set; }
  public string MutatedSequence { get; private set; }
  public MutationRecord Mutation { get; private set; }
  public MutationResult(string original, string mutated, MutationRecord mutation)
  { OriginalSequence = original; MutatedSequence = mutated; Mutation = mutation; }
 }
}

/** LifeEngine - GPL-3.0-or-later; Copyright (C) 2026 ArkansasIo */
using System;
using System.Text;
namespace LifeEngine.Biology.Genetics
{
 public static class SequenceValidator
 {
  public static string Normalize(string sequence)
  {
   if (sequence == null) throw new ArgumentNullException("sequence");
   StringBuilder b = new StringBuilder(sequence.Length);
   for (int i = 0; i < sequence.Length; i++) { char c = char.ToUpperInvariant(sequence[i]); if (char.IsWhiteSpace(c) || c == '-') continue; b.Append(c); }
   return b.ToString();
  }
  public static bool IsValid(string sequence, SequenceKind kind)
  {
   if (sequence == null) return false;
   string n = Normalize(sequence); string alphabet = kind == SequenceKind.Dna ? "ACGTN" : "ACGUN";
   for (int i = 0; i < n.Length; i++) if (alphabet.IndexOf(n[i]) < 0) return false;
   return n.Length > 0;
  }
  public static void RequireValid(string sequence, SequenceKind kind)
  { if (!IsValid(sequence, kind)) throw new ArgumentException("Sequence contains invalid or no bases for " + kind + ".", "sequence"); }
 }
}

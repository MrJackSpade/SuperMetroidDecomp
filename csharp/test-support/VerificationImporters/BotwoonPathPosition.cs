namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>Where a cartridge Botwoon movement sample lies among the typed paths.</summary>
/// <param name="Path">The path, an index into the authored paths.</param>
/// <param name="Step">The step within the path.</param>
internal readonly record struct BotwoonPathPosition(int Path, int Step);

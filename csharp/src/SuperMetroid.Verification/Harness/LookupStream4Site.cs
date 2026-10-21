namespace SuperMetroid.Verification.Harness;

/// <summary>The owner, frame, color and pointer of one lookup stream4 site.</summary>
/// <param name="Owner">The owner.</param>
/// <param name="Frame">The frame.</param>
/// <param name="Color">The color.</param>
/// <param name="Pointer">The pointer.</param>
internal readonly record struct LookupStream4Site(int Owner, int Frame, int Color, ushort Pointer);

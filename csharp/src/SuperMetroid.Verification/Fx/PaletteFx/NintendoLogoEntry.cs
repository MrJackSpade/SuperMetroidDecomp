namespace SuperMetroid.Verification.Fx.PaletteFx;

/// <summary>The owner and definition pointer of one nintendo logo entry.</summary>
/// <param name="Owner">The owner.</param>
/// <param name="DefinitionPointer">The definition pointer.</param>
internal readonly record struct NintendoLogoEntry(string Owner, ushort DefinitionPointer);

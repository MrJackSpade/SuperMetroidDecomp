namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>The header and program of one kraid draw mappings owner.</summary>
/// <param name="Header">The header.</param>
/// <param name="Program">The program.</param>
internal readonly record struct KraidDrawMappingsOwner(ushort Header, ushort Program);

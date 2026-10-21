namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>The header and index of one header list field result.</summary>
/// <param name="Header">The header.</param>
/// <param name="Index">The index.</param>
internal readonly record struct HeaderListFieldResult(ushort Header, int Index);

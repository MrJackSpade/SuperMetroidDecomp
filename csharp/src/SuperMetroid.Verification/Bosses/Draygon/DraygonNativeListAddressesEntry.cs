using SuperMetroid.Core.Bosses.Draygon;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>The program and entry of one draygon native list addresses entry.</summary>
/// <param name="Program">The program.</param>
/// <param name="Entry">The entry.</param>
internal readonly record struct DraygonNativeListAddressesEntry(DraygonProgram Program, ushort Entry);

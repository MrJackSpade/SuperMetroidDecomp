namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The pointer and runs of one plm content identity frame.</summary>
/// <param name="Pointer">The pointer.</param>
/// <param name="Runs">The runs.</param>
internal readonly record struct PlmContentIdentityFrame(ushort Pointer, ushort[][] Runs);

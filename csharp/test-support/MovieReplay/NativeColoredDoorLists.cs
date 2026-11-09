namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>Where each native bank-$84 list of one colour and facing of coloured cap begins (#1554).</summary>
/// <param name="Closing">The closing behind Samus, which falls into <paramref name="Waiting"/>.</param>
/// <param name="Waiting">The first look: the opened-door branch, the hit link, the shot wake and the closed image.</param>
/// <param name="Sleep">The sleep the cap rests on.</param>
/// <param name="Hit">The hit count and flash.</param>
/// <param name="Open">The opening and delete.</param>
internal sealed record NativeColoredDoorLists(ushort Closing, ushort Waiting, ushort Sleep, ushort Hit, ushort Open);

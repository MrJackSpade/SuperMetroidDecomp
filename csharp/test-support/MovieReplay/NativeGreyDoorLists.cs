namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>Where each native bank-$84 list of one grey cap begins (#1554).</summary>
/// <param name="Closing">The closing behind Samus.</param>
/// <param name="Waiting">The first look: the opened-door branch, the activation link, the condition wake and the closed image.</param>
/// <param name="Sleep">The sleep the locked cap rests on.</param>
/// <param name="Activate">The unlock: the hit link and the shot wake, falling into <paramref name="Flash"/>.</param>
/// <param name="Flash">The repeating flash.</param>
/// <param name="Hit">The hit count, falling into <paramref name="Open"/>.</param>
/// <param name="Open">The opening and delete.</param>
internal sealed record NativeGreyDoorLists(ushort Closing, ushort Waiting, ushort Sleep, ushort Activate, ushort Flash, ushort Hit, ushort Open);

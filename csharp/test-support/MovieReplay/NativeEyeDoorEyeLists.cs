namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>Where each native bank-$84 list of one facing's eye-door eye begins (#1554).</summary>
/// <param name="Start">The first look: the opened-door branch, falling into <paramref name="Idle"/>.</param>
/// <param name="Idle">The closed eye looking for Samus.</param>
/// <param name="Activate">The opening: the hit link, the missile wake and the half-open image, falling into <paramref name="Watch"/>.</param>
/// <param name="Watch">The watch: the in-front branch, the shots and blink, and the watch-again branch.</param>
/// <param name="Close">The blink while Samus stands in front.</param>
/// <param name="Hit">The hit: its sound, smoke, count and flinch.</param>
/// <param name="Destroyed">The destroyed eye's smoke and move onto its blue cap, falling into <paramref name="Opening"/>.</param>
/// <param name="Opening">The counted flashes, then the closed blue cap.</param>
/// <param name="Opened">An opened door's eye moving onto its blue cap.</param>
internal sealed record NativeEyeDoorEyeLists(ushort Start, ushort Idle, ushort Activate, ushort Watch, ushort Close, ushort Hit,
    ushort Destroyed, ushort Opening, ushort Opened);

using static SuperMetroid.Core.Bosses.Ridley.RidleyEyeFadeDefinitions;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// How far Ridley's eye fade has gone (#2228, #2247): how many of its shades it has shown, then how many updates of its
/// hold it has spent. It completes once it has shown every shade and held for the whole hold.
/// </summary>
/// <param name="ShadesShown">The shades shown, zero through <see cref="ShadeCount"/>.</param>
/// <param name="UpdatesHeld">The updates held after the last shade, zero through <see cref="HoldUpdates"/>.</param>
public readonly record struct RidleyEyeFadeProgress(int ShadesShown, int UpdatesHeld)
{
    /// <summary>
    /// The image the next update shows, from the fade's last (darkest) back to its first (his own eye colours), or null
    /// once every image has shown and the fade holds.
    /// </summary>
    internal RidleyFadeImage? NextImage => ShadesShown < ShadeCount ? RidleyFadeDefinitions.EyeFade.Images[^(ShadesShown + 1)] : null;

    /// <summary>Whether the fade has shown every shade and held for the whole hold.</summary>
    internal bool IsComplete => ShadesShown == ShadeCount && UpdatesHeld == HoldUpdates;

    /// <summary>The progress once the next update has shown its shade, or spent an update of the hold.</summary>
    internal RidleyEyeFadeProgress Next() =>
        NextImage is null ? this with { UpdatesHeld = UpdatesHeld + 1 } : this with { ShadesShown = ShadesShown + 1 };
}

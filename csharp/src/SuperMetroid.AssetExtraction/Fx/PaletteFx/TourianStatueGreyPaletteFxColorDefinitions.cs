using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// Immutable control words for the four Tourian entrance-statue grey-out palette programs.
/// </summary>
/// <remarks>
/// The four entries select a colour destination and converge on the
/// shared eight-frame program. Its 64 colours remain live presentation
/// data; only color-index setup, branches, durations, waits, and deletion are compiled.
/// Draygon, Kraid, Ridley, and Phantoon select colour bytes 192,
/// 224, 288, and 320 respectively. The first three entries
/// branch to the shared program; Phantoon falls through. Shared records
/// f=0..7 are 20 bytes apart, last eight frames, write eight live
/// colors, and end in a wait. Frame eight reaches deletion
/// after 64 timed frames. All 31 mechanics words match the
/// original programs; each guarded caller reaches deletion.
/// </remarks>
public static class TourianStatueGreyPaletteFxColorDefinitions
{

    /// <summary>Eight colours are presentation-owned by each timed record.</summary>
    public const int ColorsPerFrame = 8;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectAnimation.GreyOutTourianStatuePhantoon, ColorsPerFrame);
}

using SuperMetroid.Core.Areas.Tourian;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Immutable control words for the four Tourian entrance-statue grey-out palette programs.
/// </summary>
/// <remarks>
/// The four entries at $8D:E222-$E23A select a CGRAM destination and converge on the
/// shared eight-frame program at $8D:E23E. Its 64 BGR555 colors remain live presentation
/// data; only color-index setup, branches, durations, waits, and deletion are compiled.
/// Draygon, Kraid, Ridley, and Phantoon select CGRAM bytes $00C0,
/// $00E0, $0120, and $0140 respectively. The first three entries
/// explicitly goto $E23E; Phantoon falls through. Shared records
/// f=0..7 start at $E23E + 20*f, last eight frames, write eight live
/// colors, and end in $C595 wait. Frame eight reaches $C5CF delete
/// at $E2DE after 64 timed frames. All 31 mechanics words match the
/// pinned NTSC J/U v1.0 ROM; each guarded caller reaches deletion.
/// </remarks>
public static class TourianStatueGreyPaletteFxColorDefinitions
{

    /// <summary>Eight eight-frame records form the statue grey-out sequence.</summary>
    public const int FrameCount = 8;

    /// <summary>Eight BGR555 colors are presentation-owned by each timed record.</summary>
    public const int ColorsPerFrame = 8;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorRows Rows { get; } = new(279, FrameCount, ColorsPerFrame);

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public static PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}

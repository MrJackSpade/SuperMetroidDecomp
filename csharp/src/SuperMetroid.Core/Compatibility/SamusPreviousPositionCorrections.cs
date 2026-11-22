
namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How this update corrects the camera's record of where Samus was, before its scrolling measures how far she moved
/// (#2562). Owners that run before the scrolling write her previous whole position (a pose's collision correction, facing
/// forward's lift, a shinespark's lag, the gunship or a suit pickup placing her, Kraid's arena, the scrolling-speed cap),
/// a feet alignment after a pose change shifts its height, and Mother Brain's rainbow-beam push hides its fraction of a pixel
/// movement from it (<see cref="RainbowBeamCameraCheckpoint"/>). The scrolling hands the record over once, with every
/// correction applied, and the next update starts with none.
/// </summary>
/// <remarks>
/// Evidence: the game's owners that run before the scrolling write Samus's previous position in place, and its
/// scrolling reads the written position when it measures her movement. The record here is the camera's from the frame
/// boundary, which the update samples before any owner runs, so the corrections are kept here, beside Samus, and applied
/// to the sampled record where the scrolling reads it.
/// </remarks>
[CompatibilityService("owners before the scrolling correct the camera's record of where Samus was in place")]
public sealed partial class SamusPreviousPositionCorrections
{
    /// <summary>The previous whole-pixel X an owner wrote this update, or null when none did.</summary>
    private int? _x;

    /// <summary>The previous whole-pixel Y a correction wrote this update, or null when none did.</summary>
    private int? _y;

    /// <summary>How far the feet alignments since the last written height have moved the previous height.</summary>
    private int _feetShift;

    /// <summary>What the rainbow-beam push hides of its movement within a pixel from the record this update.</summary>
    internal RainbowBeamCameraCheckpoint RainbowPush { get; } = new();

    /// <summary>
    /// Writes Samus's previous whole X, as an owner that runs before the scrolling does; the record's fraction of a pixel is kept.
    /// </summary>
    internal void WriteX(int x) => _x = x;

    /// <summary>
    /// Writes Samus's previous whole Y, as a pose's collision correction or facing forward's lift does; the record's
    /// fraction of a pixel is kept, and the write discards any earlier feet alignment this update.
    /// </summary>
    internal void WriteY(int y)
    {
        _y = y;
        _feetShift = 0;
    }

    /// <summary>Shifts the previous height by <paramref name="delta"/> as a pose change keeps Samus's feet where they were.</summary>
    internal void ShiftFeet(int delta) => _feetShift += delta;

    /// <summary>The previous whole position <paramref name="record"/> stands for with this update's corrections so far.</summary>
    internal Position16<RoomPixels> Peek(FixedPosition<RoomPixels> record) => new(_x ?? record.X.Whole, HeightOf(record));

    /// <summary>
    /// Hands <paramref name="record"/> to the scrolling with every correction this update made applied: the written whole
    /// pixels, then what the rainbow-beam push hides of its movement within a pixel. The next update starts with none. The
    /// written X is a whole pixel of the room's coordinates, which <see cref="FixedCoordinate"/> holds (#1627).
    /// </summary>
    internal FixedPosition<RoomPixels> HandOver(FixedPosition<RoomPixels> record)
    {
        var written = new FixedPosition<RoomPixels>(
            FixedCoordinate.Of(_x ?? record.X.Whole, record.X.Fraction),
            FixedCoordinate.Of(HeightOf(record), record.Y.Fraction));
        _x = null;
        _y = null;
        _feetShift = 0;
        return RainbowPush.Consume(written);
    }

    /// <summary>
    /// The previous whole-pixel Y with this update's corrections: the written height or the record's, moved by the feet
    /// alignments since.
    /// </summary>
    /// <remarks>
    /// The sum stays in the room. The record lies above the room's top only on a departing up-elevator's last
    /// updates, when the locked input mode runs no pose change, no shinespark or Ceres shove holds her, and the ride keeps
    /// the boarding pose's radius, 21 or 24, which a hurt pose does not exceed, so nothing writes the height or moves it.
    /// Elsewhere every written height lies in the room, and a feet alignment lowers her (no pose-transition cue grows her
    /// body), raises her by two pixels as a shinespark crash's radius-19 body, which block collision keeps below the top,
    /// stands up, or ends the Ceres shove in Ceres Ridley's room, whose top row is solid.
    /// </remarks>
    private int HeightOf(FixedPosition<RoomPixels> record) => (_y ?? record.Y.Whole) + _feetShift;
}

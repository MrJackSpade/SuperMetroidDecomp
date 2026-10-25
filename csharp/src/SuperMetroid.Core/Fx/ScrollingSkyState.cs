using System.Collections.Immutable;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The scrolling sky's 23 parallax bands and BG2 vertical scroll (#1347).
/// </summary>
/// <remarks>
/// <see cref="ProcessFrame"/> ports <c>$88:ADC2</c> and the vertical scroll of <c>$88:AFA3</c>. The sky's rows are
/// drawn from <see cref="SkyTilemap"/>, so the cartridge's circular row streaming is not modeled; the bands are
/// published with <see cref="Publish"/>.
/// </remarks>
/// <param name="variant">The land or ocean chunk table the room main selects.</param>
/// <param name="horizontalHdmaConfigured">Whether the sky FX or setup spawned the band HDMA object.</param>
[RoomLifetime]
public sealed class ScrollingSkyState(ScrollingSkyChunkTable variant, bool horizontalHdmaConfigured = true)
{
    /// <summary>Only the sky FX/setup spawns the horizontal HDMA object; the room-main callback alone does not.</summary>
    public bool HorizontalHdmaConfigured { get; } = horizontalHdmaConfigured;

    /// <summary>The land or ocean chunk table the sky's rows follow.</summary>
    public ScrollingSkyChunkTable Variant { get; } = variant;

    /// <summary>23 native 16.16 HDMA data slots; the last slot is stationary.</summary>
    private readonly uint[] _fixedHorizontalScrolls =
        new uint[ScrollingSkyBandDefinitions.DataSlotCount];

    /// <summary>
    /// Returns whether a room-state callback dispatches to the translated land-sky
    /// implementation. This is the same bank-$8F dispatch decision made by the cartridge;
    /// callers must not infer it from a room number, area, door, or camera position.
    /// </summary>
    public static bool IsLandRoomMain(RoomMainCallback callback) =>
        callback is RoomMainCallback.ScrollingSkyLand or
            RoomMainCallback.ScrollingSkyLandZebesTimebombSet;

    /// <summary>Identifies the scrolling-sky room mains; HDMA setup is a separate decision.</summary>
    public static bool IsScrollingSkyRoomMain(RoomMainCallback callback) =>
        IsLandRoomMain(callback) || callback == RoomMainCallback.ScrollingSkyOcean;

    /// <summary>The chunk table a scrolling-sky room main reads its rows through.</summary>
    public static ScrollingSkyChunkTable VariantOf(RoomMainCallback callback) =>
        IsLandRoomMain(callback) ? ScrollingSkyChunkTable.Land
            : callback == RoomMainCallback.ScrollingSkyOcean ? ScrollingSkyChunkTable.Ocean
            : throw new ArgumentOutOfRangeException(nameof(callback), callback, "The room main runs no scrolling sky.");

    /// <summary>
    /// The BG2 scroll the sky publishes: <c>$88:AFB2</c> copies layer-one Y, with its sixteen-bit wrap, into Y.
    /// X stays zero because the sky never writes the uniform horizontal scroll; its bands shift each line instead.
    /// </summary>
    public Position16<Bg2Pixels> Bg2Scroll { get; private set; }

    /// <summary>Whether the indirect table has not been terminated by a frozen frame.</summary>
    public bool HdmaEnabled { get; private set; } = horizontalHdmaConfigured;

    /// <summary>Integer HDMA data words, indexed like the 23 four-byte slots at $7E:9F80.</summary>
    public ushort GetDataSlotPosition(int slot)
    {
        if ((uint)slot >= _fixedHorizontalScrolls.Length)
            throw new ArgumentOutOfRangeException(nameof(slot));
        return SnesFixedPosition.WholeOf(unchecked((int)_fixedHorizontalScrolls[slot]));
    }

    /// <summary>Advances the fixed-point sky bands and copies layer-one Y into the BG2 vertical scroll.</summary>
    public void ProcessFrame(ushort layer1YPosition, bool timeIsFrozen)
    {
        if (timeIsFrozen)
        {
            // $88:AFA8 writes a zero first HDMA entry, terminating the table. The
            // pre-instruction wrapper likewise skips fixed-point advancement.
            HdmaEnabled = false;
            return;
        }

        HdmaEnabled = HorizontalHdmaConfigured;
        if (HorizontalHdmaConfigured) AdvanceHorizontalScrolls();
        Bg2Scroll = new Position16<Bg2Pixels>(0, layer1YPosition);
    }

    /// <summary>
    /// The sky layer the frame shows, positioned from <paramref name="layer1YPosition"/>; null when the room spawned no
    /// band HDMA object, whose BG2 is the room's own background.
    /// </summary>
    /// <param name="layer1YPosition">The camera's layer-one Y the bands are positioned from.</param>
    public ScrollingSkyLayer? Publish(ushort layer1YPosition)
    {
        if (!HorizontalHdmaConfigured)
            return null;
        var offsets = ImmutableArray.CreateBuilder<ushort>(_fixedHorizontalScrolls.Length);
        for (int slot = 0; slot < _fixedHorizontalScrolls.Length; slot++)
            offsets.Add(GetDataSlotPosition(slot));
        return new ScrollingSkyLayer(layer1YPosition, offsets.MoveToImmutable(), HdmaEnabled);
    }

    /// <summary>Adds each strip's native 16.16 speed and resets the stationary bottom strip.</summary>
    private void AdvanceHorizontalScrolls()
    {
        for (int index = 0; index < ScrollingSkyBandDefinitions.SectionCount; index++)
        {
            SkyScrollSection section = ScrollingSkyBands.GetSection(index);
            _fixedHorizontalScrolls[section.DataSlot] = unchecked(
                _fixedHorizontalScrolls[section.DataSlot] + (uint)section.Velocity.RawValue);
        }

        // $88:ADF2-$88:ADFA clears the final fractional and integer words after all 23
        // additions, keeping the bottommost section stationary.
        _fixedHorizontalScrolls[ScrollingSkyBandDefinitions.DataSlotCount - 1] = 0;
    }
}

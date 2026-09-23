using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Landing Site's bank-$88 scrolling-sky tilemap updater and horizontal HDMA state.
/// </summary>
/// <remarks>
/// <see cref="ProcessFrame"/> ports <c>$88:ADC2</c> and <c>$88:AFA3</c>. The original
/// materializes an indirect HDMA byte table; this model retains the same 23 fixed-point
/// data slots and exposes their final per-gameplay-scanline BG2HOFS values directly.
/// </remarks>
public sealed class ScrollingSkyState
{
    private readonly uint[] _fixedHorizontalScrolls =
        new uint[RoomFxRomData.ScrollingSky.DataSlotCount];
    /// <summary>
    /// Returns whether a room-state callback dispatches to the translated land-sky
    /// implementation. This is the same bank-$8F dispatch decision made by the cartridge;
    /// callers must not infer it from a room number, area, door, or camera position.
    /// </summary>
    public static bool IsLandRoomMain(RoomMainCallback callback) =>
        callback is RoomMainCallback.ScrollingSkyLand or
            RoomMainCallback.ScrollingSkyLandZebesTimebombSet;

    /// <summary>Both sky wrappers share BG2 geometry, HDMA and row streaming; only their source table differs.</summary>
    public static bool IsScrollingSkyRoomMain(RoomMainCallback callback) =>
        IsLandRoomMain(callback) || callback == RoomMainCallback.ScrollingSkyOcean;

    /// <summary>BG2VOFS mirror written from layer-1 Y by <c>$88:AFB2</c>.</summary>
    public ushort VerticalScroll { get; private set; }

    /// <summary>Whether the indirect table has not been terminated by a frozen frame.</summary>
    public bool HdmaEnabled { get; private set; } = true;

    /// <summary>Integer HDMA data words, indexed like the 23 four-byte slots at $7E:9F80.</summary>
    public ushort GetDataSlotPosition(int slot)
    {
        if ((uint)slot >= _fixedHorizontalScrolls.Length)
            throw new ArgumentOutOfRangeException(nameof(slot));
        return (ushort)(_fixedHorizontalScrolls[slot] >> 16);
    }

    /// <summary>
    /// Advances the fixed-point sky strips and appends the four ordinary VRAM queue records
    /// used to keep the circular BG2 tilemap populated around the camera.
    /// </summary>
    public void ProcessFrame(ushort layer1YPosition, bool timeIsFrozen, VramWriteQueue writes,
        RoomMainCallback roomMainCallback = RoomMainCallback.ScrollingSkyLand)
    {
        ArgumentNullException.ThrowIfNull(writes);
        if (timeIsFrozen)
        {
            // $88:AFA8 writes a zero first HDMA entry, terminating the table, and queues no
            // rows. The pre-instruction wrapper likewise skips fixed-point advancement.
            HdmaEnabled = false;
            return;
        }

        HdmaEnabled = true;
        AdvanceHorizontalScrolls();
        VerticalScroll = layer1YPosition;
        QueueTilemapRows(layer1YPosition, writes, roomMainCallback == RoomMainCallback.ScrollingSkyOcean
            ? RoomFxRomData.ScrollingSky.OceanChunkPointerTableAddress
            : RoomFxRomData.ScrollingSky.LandChunkPointerTableAddress);
    }

    /// <summary>
    /// Resolves the indirect HDMA table into one BG2HOFS word per gameplay scanline.
    /// Index zero corresponds to physical scanline 32, immediately below the HUD.
    /// </summary>
    public ushort[] BuildGameplayHorizontalScrolls(ushort layer1YPosition, int lineCount = 192)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lineCount);
        var result = new ushort[lineCount];
        if (!HdmaEnabled)
            return result;

        ReadOnlySpan<SkyScrollSection> sections = RoomFxRomData.ScrollingSky.Sections;
        for (int line = 0; line < lineCount; line++)
        {
            ushort worldY = unchecked((ushort)(
                layer1YPosition + RoomFxRomData.ScrollingSky.GameplayFirstScanline + line));
            int sectionIndex = FindSection(worldY, sections);

            // The assembly's fallback entries point at direct-page BG2HOFS ($00B5). Its
            // ordinary value is zero for Landing Site; all positions >=$500 use that path.
            result[line] = sectionIndex >= 0
                ? GetDataSlotPosition(sections[sectionIndex].DataSlot)
                : (ushort)0;
        }
        return result;
    }

    private void AdvanceHorizontalScrolls()
    {
        foreach (SkyScrollSection section in RoomFxRomData.ScrollingSky.Sections)
        {
            uint velocity = ((uint)section.Speed << 16) | section.Subspeed;
            _fixedHorizontalScrolls[section.DataSlot] = unchecked(
                _fixedHorizontalScrolls[section.DataSlot] + velocity);
        }

        // $88:ADF2-$88:ADFA clears the final fractional and integer words after all 23
        // additions, keeping the bottommost section stationary.
        _fixedHorizontalScrolls[RoomFxRomData.ScrollingSky.DataSlotCount - 1] = 0;
    }

    private void QueueTilemapRows(ushort layer1YPosition, VramWriteQueue writes, int pointerTable)
    {
        // First pair: two rows immediately behind the HUD, beginning at cameraY-16 rounded
        // down to an 8-pixel boundary. All arithmetic is modular 16-bit as on the 65C816.
        ushort upperPosition = unchecked((ushort)(
            (layer1YPosition & RoomFxRomData.ScrollingSky.SourcePositionMask) -
            RoomFxRomData.ScrollingSky.UpperRowCameraOffset));
        int upperChunk = upperPosition >> 8;
        int upperByteOffset = (upperPosition & 0x00ff) * 8;
        int upperSource = RoomFxRomData.Banks.Tilemaps |
            unchecked((ushort)(ScrollingSkyChunkPointerDefinitions.Get(
                pointerTable, upperChunk) + upperByteOffset));

        // Second pair: two rows just below the 224-line screen, based on cameraY+$F0.
        ushort lowerPosition = unchecked((ushort)(
            (layer1YPosition & RoomFxRomData.ScrollingSky.SourcePositionMask) +
            RoomFxRomData.ScrollingSky.LowerRowCameraOffset));
        int lowerChunk = lowerPosition >> 8;
        int lowerByteOffset = (lowerPosition & 0x00ff) * 8;
        int lowerSource = RoomFxRomData.Banks.Tilemaps |
            unchecked((ushort)(ScrollingSkyChunkPointerDefinitions.Get(
                pointerTable, lowerChunk) + lowerByteOffset));

        ushort upperDestination = unchecked((ushort)(
            RoomFxRomData.ScrollingSky.Bg2TilemapBaseWord +
            4 * (unchecked((ushort)(
                layer1YPosition - RoomFxRomData.ScrollingSky.UpperRowCameraOffset)) &
                RoomFxRomData.ScrollingSky.TilemapPositionMask)));
        ushort lowerDestination = unchecked((ushort)(
            RoomFxRomData.ScrollingSky.Bg2TilemapBaseWord +
            4 * (unchecked((ushort)(
                layer1YPosition + RoomFxRomData.ScrollingSky.LowerRowCameraOffset)) &
                RoomFxRomData.ScrollingSky.TilemapPositionMask)));

        writes.Enqueue(RoomFxRomData.ScrollingSky.TilemapRowByteCount, upperSource, upperDestination);
        writes.Enqueue(
            RoomFxRomData.ScrollingSky.TilemapRowByteCount,
            upperSource + RoomFxRomData.ScrollingSky.TilemapRowByteCount,
            unchecked((ushort)(
                upperDestination + RoomFxRomData.ScrollingSky.TilemapHalfRowWordCount)));
        writes.Enqueue(RoomFxRomData.ScrollingSky.TilemapRowByteCount, lowerSource, lowerDestination);
        writes.Enqueue(
            RoomFxRomData.ScrollingSky.TilemapRowByteCount,
            lowerSource + RoomFxRomData.ScrollingSky.TilemapRowByteCount,
            unchecked((ushort)(
                lowerDestination + RoomFxRomData.ScrollingSky.TilemapHalfRowWordCount)));
    }

    private static int FindSection(ushort worldY, ReadOnlySpan<SkyScrollSection> sections)
    {
        for (int index = 0; index < sections.Length; index++)
        {
            ushort nextTop = index + 1 < sections.Length
                ? sections[index + 1].TopPosition
                : RoomFxRomData.ScrollingSky.WorldEndPosition;
            if (worldY >= sections[index].TopPosition && worldY < nextTop)
                return index;
        }
        return -1;
    }
}

/// <summary>One eight-byte row from the ROM table at <c>$88:AEC1</c>.</summary>
public readonly record struct SkyScrollSection(
    ushort TopPosition,
    ushort Subspeed,
    ushort Speed,
    int DataSlot);

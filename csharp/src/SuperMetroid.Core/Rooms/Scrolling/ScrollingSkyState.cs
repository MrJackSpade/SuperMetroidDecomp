using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// Landing Site's bank-$88 scrolling-sky tilemap updater and horizontal HDMA state.
/// </summary>
/// <remarks>
/// <see cref="ProcessFrame"/> ports <c>$88:ADC2</c> and <c>$88:AFA3</c>. The original
/// materializes an indirect HDMA byte table; this model retains the same 23 fixed-point
/// data slots and exposes their final per-gameplay-scanline BG2HOFS values directly.
/// </remarks>
[RoomLifetime]
public sealed class ScrollingSkyState(bool horizontalHdmaConfigured = true)
{
    /// <summary>Only the sky FX/setup spawns the horizontal HDMA object; the room-main callback alone does not.</summary>
    public bool HorizontalHdmaConfigured { get; } = horizontalHdmaConfigured;

    /// <summary>23 native 16.16 HDMA data slots; the last slot is stationary.</summary>
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

    /// <summary>Identifies room-main tilemap streaming; HDMA setup is a separate decision.</summary>
    public static bool IsScrollingSkyRoomMain(RoomMainCallback callback) =>
        IsLandRoomMain(callback) || callback == RoomMainCallback.ScrollingSkyOcean;

    /// <summary>
    /// The BG2 scroll the sky publishes: <c>$88:AFB2</c> copies layer-one Y, with its sixteen-bit wrap, into Y.
    /// X stays zero because the sky never writes the uniform horizontal scroll; its bands come from
    /// <see cref="BuildGameplayHorizontalScrolls"/> instead.
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

    /// <summary>
    /// Advances the fixed-point sky strips and appends the four ordinary VRAM queue records
    /// used to keep the circular BG2 tilemap populated around the camera.
    /// </summary>
    public void ProcessFrame(ushort layer1YPosition, bool timeIsFrozen, VramWriteQueue writes,
        RoomMainCallback roomMainCallback = RoomMainCallback.ScrollingSkyLand)
    {
        Ensure.NotNull(writes);
        if (timeIsFrozen)
        {
            // $88:AFA8 writes a zero first HDMA entry, terminating the table, and queues no
            // rows. The pre-instruction wrapper likewise skips fixed-point advancement.
            HdmaEnabled = false;
            return;
        }

        HdmaEnabled = HorizontalHdmaConfigured;
        if (HorizontalHdmaConfigured) AdvanceHorizontalScrolls();
        Bg2Scroll = new Position16<Bg2Pixels>(0, layer1YPosition);
        QueueTilemapRows(layer1YPosition, writes, roomMainCallback == RoomMainCallback.ScrollingSkyOcean
            ? ScrollingSkyChunkTable.Ocean
            : ScrollingSkyChunkTable.Land);
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

        for (int line = 0; line < lineCount; line++)
        {
            ushort worldY = unchecked((ushort)(
                layer1YPosition + RoomFxRomData.ScrollingSky.GameplayFirstScanline + line));
            int sectionIndex = FindSection(worldY);

            // The assembly's fallback entries point at direct-page BG2HOFS ($00B5). Its
            // ordinary value is zero for Landing Site; all positions >=$500 use that path.
            result[line] = sectionIndex >= 0
                ? GetDataSlotPosition(RoomFxRomData.ScrollingSky.GetSection(sectionIndex).DataSlot)
                : (ushort)0;
        }
        return result;
    }

    /// <summary>Adds each strip's native 16.16 speed and resets the stationary bottom strip.</summary>
    private void AdvanceHorizontalScrolls()
    {
        for (int index = 0; index < RoomFxRomData.ScrollingSky.SectionCount; index++)
        {
            SkyScrollSection section = RoomFxRomData.ScrollingSky.GetSection(index);
            _fixedHorizontalScrolls[section.DataSlot] = unchecked(
                _fixedHorizontalScrolls[section.DataSlot] + (uint)section.Velocity.RawValue);
        }

        // $88:ADF2-$88:ADFA clears the final fractional and integer words after all 23
        // additions, keeping the bottommost section stationary.
        _fixedHorizontalScrolls[RoomFxRomData.ScrollingSky.DataSlotCount - 1] = 0;
    }

    /// <summary>Streams paired BG2 tilemap rows behind the HUD and below the screen using bank-$88 chunks.</summary>
    private static void QueueTilemapRows(ushort layer1YPosition, VramWriteQueue writes, ScrollingSkyChunkTable pointerTable)
    {
        // First pair: two rows immediately behind the HUD, beginning at cameraY-16 rounded
        // down to an 8-pixel boundary. All arithmetic is modular 16-bit as on the 65C816.
        ushort upperPosition = unchecked((ushort)(
            (layer1YPosition & RoomFxRomData.ScrollingSky.SourcePositionMask) -
            RoomFxRomData.ScrollingSky.UpperRowCameraOffset));
        int upperChunk = SnesLittleEndian.HighByte(upperPosition);
        int upperByteOffset = (SnesLittleEndian.LowByte(upperPosition)) * 8;
        // Ocean index 255 reads the wrapper's TimeIsFrozenFlag address ($0A78) as its DMA source: live WRAM the port
        // does not keep there. The rows lie behind the HUD, so they upload blank (owner decision on #1318).
        bool upperBlank = pointerTable == ScrollingSkyChunkTable.Ocean && upperChunk == byte.MaxValue;
        int upperSource = upperBlank ? 0 : RoomFxRomData.Banks.Tilemaps |
            unchecked((ushort)(ScrollingSkyChunkDefinitions.Get(
                pointerTable, upperChunk) + upperByteOffset));

        // Second pair: two rows just below the 224-line screen, based on cameraY+$F0.
        ushort lowerPosition = unchecked((ushort)(
            (layer1YPosition & RoomFxRomData.ScrollingSky.SourcePositionMask) +
            RoomFxRomData.ScrollingSky.LowerRowCameraOffset));
        int lowerChunk = SnesLittleEndian.HighByte(lowerPosition);
        int lowerByteOffset = (SnesLittleEndian.LowByte(lowerPosition)) * 8;
        int lowerSource = RoomFxRomData.Banks.Tilemaps |
            unchecked((ushort)(ScrollingSkyChunkDefinitions.Get(
                pointerTable, lowerChunk) + lowerByteOffset));

        int upperRow = 4 * (unchecked((ushort)(
            layer1YPosition - RoomFxRomData.ScrollingSky.UpperRowCameraOffset)) &
            RoomFxRomData.ScrollingSky.TilemapPositionMask);
        int lowerRow = 4 * (unchecked((ushort)(
            layer1YPosition + RoomFxRomData.ScrollingSky.LowerRowCameraOffset)) &
            RoomFxRomData.ScrollingSky.TilemapPositionMask);

        if (upperBlank)
        {
            writes.EnqueueZeroFill(RoomFxRomData.ScrollingSky.TilemapRowByteCount, RoomFxVram.ScrollingSkyDma(upperRow));
            writes.EnqueueZeroFill(RoomFxRomData.ScrollingSky.TilemapRowByteCount,
                RoomFxVram.ScrollingSkyDma(upperRow + RoomFxRomData.ScrollingSky.TilemapHalfRowWordCount));
        }
        else
        {
            writes.Enqueue(RoomFxRomData.ScrollingSky.TilemapRowByteCount, upperSource, RoomFxVram.ScrollingSkyDma(upperRow));
            writes.Enqueue(
                RoomFxRomData.ScrollingSky.TilemapRowByteCount,
                upperSource + RoomFxRomData.ScrollingSky.TilemapRowByteCount,
                RoomFxVram.ScrollingSkyDma(upperRow + RoomFxRomData.ScrollingSky.TilemapHalfRowWordCount));
        }
        writes.Enqueue(RoomFxRomData.ScrollingSky.TilemapRowByteCount, lowerSource, RoomFxVram.ScrollingSkyDma(lowerRow));
        writes.Enqueue(
            RoomFxRomData.ScrollingSky.TilemapRowByteCount,
            lowerSource + RoomFxRomData.ScrollingSky.TilemapRowByteCount,
            RoomFxVram.ScrollingSkyDma(lowerRow + RoomFxRomData.ScrollingSky.TilemapHalfRowWordCount));
    }

    /// <summary>Finds the sky strip covering a world scanline, or the direct-page fallback below the sky.</summary>
    private static int FindSection(ushort worldY)
    {
        for (int index = 0; index < RoomFxRomData.ScrollingSky.SectionCount; index++)
        {
            int nextTop = index + 1 < RoomFxRomData.ScrollingSky.SectionCount
                ? RoomFxRomData.ScrollingSky.GetSection(index + 1).TopPosition
                : RoomFxRomData.ScrollingSky.WorldEndPosition;
            if (worldY >= RoomFxRomData.ScrollingSky.GetSection(index).TopPosition && worldY < nextTop)
                return index;
        }
        return -1;
    }
}

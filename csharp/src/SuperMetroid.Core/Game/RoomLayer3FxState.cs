using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge-owned Mode-1 BG3 room effect selected by one sixteen-byte bank-$83 FX record.
/// </summary>
/// <remarks>
/// This first shared owner implements the two effects required by the early playable route:
/// Landing Site rain (<c>$0A</c>) and Climb fog (<c>$0C</c>). Both use the same native FX
/// record selection, $8A tilemap upload, BG3SC=$5C layout, and NMI-published scroll state.
/// Unsupported nonzero types remain represented by <see cref="Type"/> but do not pretend to
/// render until their distinct liquid/window behavior is translated.
/// </remarks>
public sealed class RoomLayer3FxState
{
    private ushort verticalAccumulator;
    private ushort horizontalAccumulator;
    private ushort horizontalVelocity;
    private ushort previousCameraY;
    private ushort previousCameraX;
    private ushort animationTimer;
    private int animationFrame;

    /// <summary>The literal FX type byte selected from the active room/door record.</summary>
    public RoomFxType Type { get; private set; }

    /// <summary>FX B layer-blending selector installed by the effect pre-instruction.</summary>
    public LayerBlendingConfiguration LayerBlendConfiguration { get; private set; }

    /// <summary>Live BG3 horizontal-scroll register shadow.</summary>
    public ushort HorizontalScroll { get; private set; }

    /// <summary>Live BG3 vertical-scroll register shadow.</summary>
    public ushort VerticalScroll { get; private set; }

    /// <summary>Whether the translated effect supplies a gameplay-region BG3 plane.</summary>
    public bool IsRenderable => Type is RoomFxType.Rain or RoomFxType.Fog;

    /// <summary>Clears the prior room and selects the current door's native FX record.</summary>
    public void Load(
        ISnesAddressSpace bus,
        SnesVram vram,
        SnesCgram cgram,
        ushort fxPointer,
        ushort doorPointer,
        ushort randomNumber)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);
        Reset();
        if (fxPointer == 0)
            return;

        ushort record = RoomFxRomData.SelectRecord(bus, fxPointer, doorPointer);
        if (record == 0)
            return;

        Type = RoomFxTypes.FromCartridge(
            RoomFxRomData.ReadRecordByte(bus, record, RoomFxRomData.Record.TypeOffset),
            $"bank-$83 FX record ${record:X4}");
        LayerBlendConfiguration = LayerBlendingConfigurations.FromCartridge(
            RoomFxRomData.ReadRecordByte(
                bus,
                record,
                RoomFxRomData.Record.Layer3LayerBlendConfigurationOffset),
            $"bank-$83 FX record ${record:X4}");
        byte paletteBlend = RoomFxRomData.ReadRecordByte(
            bus,
            record,
            RoomFxRomData.Record.PaletteBlendOffset);
        if (paletteBlend == 0)
        {
            // LoadFXHeader clears only target-palette color $1B when no blend is selected.
            cgram.SetColor(RoomFxRomData.Layer3.EmptyPaletteColorIndex, 0);
        }
        else
        {
            int source = RoomFxRomData.Tables.PaletteBlendColors + (paletteBlend >> 1) * 2;
            cgram.LoadFromBus(
                bus,
                source,
                colorCount: RoomFxRomData.Layer3.PaletteBlendColorCount,
                destinationIndex: RoomFxRomData.Layer3.PaletteBlendDestinationIndex);
        }

        if (!IsRenderable)
            return;

        int typeIndex = ((byte)Type) >> 1;
        ushort tilemapPointer = ReadWord(
            bus,
            RoomFxRomData.Tables.Layer3TilemapPointers + typeIndex * 2);
        if (tilemapPointer == 0)
        {
            throw new InvalidDataException(
                $"Renderable room FX type ${(byte)Type:X2} has no bank-$8A tilemap pointer.");
        }
        vram.ExecuteHardwareDmaWrite(
            bus,
            RoomFxRomData.Banks.Tilemaps | tilemapPointer,
            RoomFxRomData.Layer3.TilemapByteCount,
            RoomFxRomData.Layer3.TilemapDestinationWord);

        if (Type == RoomFxType.Rain)
        {
            ReadOnlySpan<ushort> velocities = RoomFxRomData.Rain.HorizontalVelocities;
            // `$88:C4B9` masks the random word with six *after* shifting it once,
            // producing byte offsets 0/2/4/6 into a word table. Expressed as a C#
            // element index that is bits two and three of the original random word.
            horizontalVelocity = velocities[(randomNumber >> 2) & 3];
            animationTimer = 1;
        }
    }

    /// <summary>Runs the active bank-$88 pre-instruction and rain animtile handler.</summary>
    public void Step(
        ISnesAddressSpace bus,
        SnesVram vram,
        ushort cameraX,
        ushort cameraY,
        bool timeIsFrozen)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(vram);
        if (!IsRenderable || timeIsFrozen)
            return;

        if (Type == RoomFxType.Rain)
        {
            VerticalScroll = unchecked((ushort)(
                previousCameraY - cameraY + SignedHighByte(verticalAccumulator)));
            verticalAccumulator = unchecked((ushort)(
                verticalAccumulator - RoomFxRomData.Rain.VerticalVelocity));
            previousCameraY = cameraY;
            HorizontalScroll = unchecked((ushort)(
                previousCameraX - cameraX + SignedHighByte(horizontalAccumulator)));
            horizontalAccumulator = unchecked((ushort)(
                horizontalAccumulator + horizontalVelocity));
            previousCameraX = cameraX;
            StepRainAnimation(bus, vram);
            return;
        }

        // `$88:DB36` anchors fog to layer one and moves its texture by -$40/+50 in 8.8.
        VerticalScroll = unchecked((ushort)(cameraY + SignedHighByte(verticalAccumulator)));
        verticalAccumulator = unchecked((ushort)(
            verticalAccumulator - RoomFxRomData.Fog.VerticalVelocity));
        HorizontalScroll = unchecked((ushort)(cameraX + SignedHighByte(horizontalAccumulator)));
        horizontalAccumulator = unchecked((ushort)(
            horizontalAccumulator + RoomFxRomData.Fog.HorizontalVelocity));
    }

    /// <summary>Captures values visible alongside the current accepted NMI's OAM upload.</summary>
    public RoomLayer3FxRenderSnapshot? CaptureForDisplay() => IsRenderable
        ? new RoomLayer3FxRenderSnapshot(
            Type,
            LayerBlendConfiguration,
            HorizontalScroll,
            VerticalScroll)
        : null;

    private void StepRainAnimation(ISnesAddressSpace bus, SnesVram vram)
    {
        animationTimer = unchecked((ushort)(animationTimer - 1));
        if (animationTimer != 0)
            return;

        vram.ExecuteHardwareDmaWrite(
            bus,
            RoomFxRomData.Rain.AnimationFirstFrameAddress +
                animationFrame * RoomFxRomData.Rain.AnimationByteCount,
            RoomFxRomData.Rain.AnimationByteCount,
            RoomFxRomData.Rain.AnimationDestinationWord);
        animationFrame = (animationFrame + 1) % RoomFxRomData.Rain.AnimationFrameCount;
        animationTimer = RoomFxRomData.Rain.AnimationFrameDuration;
    }

    private void Reset()
    {
        Type = RoomFxType.None;
        LayerBlendConfiguration = default;
        HorizontalScroll = VerticalScroll = 0;
        verticalAccumulator = horizontalAccumulator = horizontalVelocity = 0;
        previousCameraY = previousCameraX = 0;
        animationTimer = 0;
        animationFrame = 0;
    }

    private static short SignedHighByte(ushort value) => unchecked((sbyte)(value >> 8));

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        RomDataReader.ReadWordFixedBank(bus, address);
}

/// <summary>Immutable gameplay BG3 values published by one accepted NMI.</summary>
public readonly record struct RoomLayer3FxRenderSnapshot(
    RoomFxType Type,
    LayerBlendingConfiguration LayerBlendConfiguration,
    ushort HorizontalScroll,
    ushort VerticalScroll);

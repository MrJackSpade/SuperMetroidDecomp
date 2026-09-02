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
    private const int FxBank = 0x830000;
    private const int FxRecordByteCount = 16;
    private const int FxTilemapPointerTable = 0x83abf0;
    private const int PaletteBlendTable = 0x89aa02;
    private const ushort FxTilemapDestinationWord = 0x5be0;
    private const ushort FxTilemapByteCount = 0x0840;
    private const ushort RainAnimationDestinationWord = 0x4280;
    private const ushort RainAnimationByteCount = 0x0050;
    private const int RainAnimationFrameCount = 5;
    private const ushort RainAnimationFrameDuration = 10;
    private const int RainAnimationFirstFrameAddress = 0x87a874;

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
    public byte LayerBlendConfiguration { get; private set; }

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

        ushort record = SelectFxRecord(bus, fxPointer, doorPointer);
        if (record == 0)
            return;

        Type = (RoomFxType)bus.ReadByte(FxBank | unchecked((ushort)(record + 9)));
        LayerBlendConfiguration = bus.ReadByte(FxBank | unchecked((ushort)(record + 11)));
        byte paletteBlend = bus.ReadByte(FxBank | unchecked((ushort)(record + 15)));
        if (paletteBlend == 0)
        {
            // LoadFXHeader clears only target-palette color $1B when no blend is selected.
            cgram.SetColor(27, 0);
        }
        else
        {
            int source = PaletteBlendTable + (paletteBlend >> 1) * 2;
            cgram.LoadFromBus(bus, source, colorCount: 3, destinationIndex: 25);
        }

        if (!IsRenderable)
            return;

        int typeIndex = ((byte)Type) >> 1;
        ushort tilemapPointer = ReadWord(bus, FxTilemapPointerTable + typeIndex * 2);
        if (tilemapPointer == 0)
        {
            throw new InvalidDataException(
                $"Renderable room FX type ${(byte)Type:X2} has no bank-$8A tilemap pointer.");
        }
        vram.ExecuteHardwareDmaWrite(
            bus,
            0x8a0000 | tilemapPointer,
            FxTilemapByteCount,
            FxTilemapDestinationWord);

        if (Type == RoomFxType.Rain)
        {
            ReadOnlySpan<ushort> velocities = [0xfa00, 0x0600, 0xfc00, 0x0400];
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
            verticalAccumulator = unchecked((ushort)(verticalAccumulator - 0x0600));
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
        verticalAccumulator = unchecked((ushort)(verticalAccumulator - 0x0040));
        HorizontalScroll = unchecked((ushort)(cameraX + SignedHighByte(horizontalAccumulator)));
        horizontalAccumulator = unchecked((ushort)(horizontalAccumulator + 0x0050));
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
            RainAnimationFirstFrameAddress + animationFrame * RainAnimationByteCount,
            RainAnimationByteCount,
            RainAnimationDestinationWord);
        animationFrame = (animationFrame + 1) % RainAnimationFrameCount;
        animationTimer = RainAnimationFrameDuration;
    }

    private void Reset()
    {
        Type = RoomFxType.None;
        LayerBlendConfiguration = 0;
        HorizontalScroll = VerticalScroll = 0;
        verticalAccumulator = horizontalAccumulator = horizontalVelocity = 0;
        previousCameraY = previousCameraX = 0;
        animationTimer = 0;
        animationFrame = 0;
    }

    private static ushort SelectFxRecord(
        ISnesAddressSpace bus,
        ushort fxPointer,
        ushort doorPointer)
    {
        ushort record = fxPointer;
        for (int guard = 0; guard < 256; guard++)
        {
            ushort candidateDoor = ReadWord(bus, FxBank | record);
            if (candidateDoor == 0 || candidateDoor == doorPointer)
                return record;
            if (candidateDoor == ushort.MaxValue)
                return 0;
            record = unchecked((ushort)(record + FxRecordByteCount));
        }

        throw new InvalidDataException(
            $"Room FX list $83:{fxPointer:X4} did not terminate for door $83:{doorPointer:X4}.");
    }

    private static short SignedHighByte(ushort value) => unchecked((sbyte)(value >> 8));

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        RomDataReader.ReadWordFixedBank(bus, address);
}

/// <summary>Immutable gameplay BG3 values published by one accepted NMI.</summary>
public readonly record struct RoomLayer3FxRenderSnapshot(
    RoomFxType Type,
    byte LayerBlendConfiguration,
    ushort HorizontalScroll,
    ushort VerticalScroll);

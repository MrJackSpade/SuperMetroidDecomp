using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge-owned room effect selected by one sixteen-byte bank-$83 FX record.
/// </summary>
/// <remarks>
/// Every room effect shares the native base/target position, packed 8.8 Y velocity, and
/// timer loaded by <c>$89:AB82</c>. This owner retains those words even when that effect's
/// renderer is not translated yet, allowing bank-$84 PLMs to make their native shared-state
/// writes without pretending that the corresponding bank-$88 liquid renderer is complete.
/// The currently rendered BG3 effects are Landing Site rain (<c>$0A</c>) and Climb fog
/// (<c>$0C</c>).
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
    private ushort waterHorizontalSubscroll;
    private ushort waterBg3WaveTimer;
    private ushort waterBg2WaveTimer;
    private int waterBg3WavePhase;
    private int waterBg2WavePhase;
    private short waterSurfaceScreenY;
    private ushort lavaAcidBg2WaveTimer;
    private int lavaAcidBg2WavePhase;

    /// <summary>The literal FX type byte selected from the active room/door record.</summary>
    public RoomFxType Type { get; private set; }

    /// <summary>Native <c>FX_BaseYPosition</c> word loaded from record offset two.</summary>
    public ushort BaseYPosition { get; private set; }

    /// <summary>Native <c>FX_TargetYPosition</c> word loaded from record offset four.</summary>
    public ushort TargetYPosition { get; private set; }

    /// <summary>
    /// Packed native <c>FX_YSubVelocity/FX_YVelocity</c> word loaded from offset six.
    /// The low byte is subvelocity and the high byte is signed whole-pixel velocity.
    /// </summary>
    public ushort PackedYVelocity { get; private set; }

    /// <summary>Native zero-extended FX timer byte loaded from record offset eight.</summary>
    public ushort Timer { get; private set; }

    /// <summary>FX B layer-blending selector installed by the effect pre-instruction.</summary>
    public LayerBlendingConfiguration LayerBlendConfiguration { get; private set; }

    /// <summary>Live BG3 horizontal-scroll register shadow.</summary>
    public ushort HorizontalScroll { get; private set; }

    /// <summary>Live BG3 vertical-scroll register shadow.</summary>
    public ushort VerticalScroll { get; private set; }

    /// <summary>Whether the translated effect supplies a gameplay-region BG3 plane.</summary>
    public bool IsRenderable => Type is
        RoomFxType.Lava or RoomFxType.Acid or RoomFxType.Water or RoomFxType.Rain or RoomFxType.Fog;

    /// <summary>Live liquid surface used by bank-$88 after rising/tide processing.</summary>
    public ushort CurrentYPosition { get; private set; } = ushort.MaxValue;

    /// <summary>Native liquid-options byte, zero-extended for consumers of WRAM <c>$197E</c>.</summary>
    public ushort LiquidOptions { get; private set; }

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

        BaseYPosition = RoomFxRomData.ReadRecordWord(
            bus,
            record,
            RoomFxRomData.Record.BaseYPositionOffset);
        TargetYPosition = RoomFxRomData.ReadRecordWord(
            bus,
            record,
            RoomFxRomData.Record.TargetYPositionOffset);
        PackedYVelocity = RoomFxRomData.ReadRecordWord(
            bus,
            record,
            RoomFxRomData.Record.YVelocityOffset);
        Timer = RoomFxRomData.ReadRecordByte(
            bus,
            record,
            RoomFxRomData.Record.TimerOffset);
        LiquidOptions = RoomFxRomData.ReadRecordByte(
            bus,
            record,
            RoomFxRomData.Record.LiquidOptionsOffset);
        CurrentYPosition = BaseYPosition;

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

        if (Type == RoomFxType.Water)
        {
            // Both spawned HDMA objects execute their phase initializer on the first
            // handler pass. A one-frame timer reproduces that first-call rotation.
            waterBg3WaveTimer = 1;
            waterBg2WaveTimer = 1;
        }
        else if (Type is RoomFxType.Lava or RoomFxType.Acid)
        {
            // `$88:B4CE` seeds timer B to one, so the first HDMA-object pass rotates A
            // from zero to $1E and then reloads either four or six. Expressing the already-
            // resolved waveform phase as zero produces the same first visible table.
            lavaAcidBg2WaveTimer = UsesLavaAcidVerticalWave
                ? RoomFxRomData.LavaAcid.VerticalWavePhaseDuration
                : RoomFxRomData.LavaAcid.HorizontalWavePhaseDuration;
            lavaAcidBg2WavePhase = 0;
        }
        else if (Type == RoomFxType.Rain)
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

        if (Type is RoomFxType.Lava or RoomFxType.Acid)
        {
            StepLavaAcid(cameraX, cameraY);
            return;
        }

        if (Type == RoomFxType.Water)
        {
            StepWater(cameraX, cameraY);
            return;
        }

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

    /// <summary>
    /// Seeds camera-dependent liquid registers during a room transition without advancing
    /// a wave clock. Destination rendering can therefore show the correct surface on its
    /// first accepted NMI instead of waiting for the first gameplay handler call.
    /// </summary>
    public void PrimeViewport(ushort cameraX, ushort cameraY)
    {
        if (Type is not (RoomFxType.Water or RoomFxType.Lava or RoomFxType.Acid))
            return;
        CurrentYPosition = BaseYPosition;
        waterSurfaceScreenY = unchecked((short)(CurrentYPosition - cameraY));
        HorizontalScroll = cameraX;
        VerticalScroll = Type == RoomFxType.Water
            ? ComputeWaterVerticalScroll(CurrentYPosition, cameraY)
            : (ushort)0;
    }

    /// <summary>Captures values visible alongside the current accepted NMI's OAM upload.</summary>
    public RoomLayer3FxRenderSnapshot? CaptureForDisplay() => IsRenderable
        ? new RoomLayer3FxRenderSnapshot(
            Type,
            LayerBlendConfiguration,
            HorizontalScroll,
            VerticalScroll,
            CurrentYPosition,
            LiquidOptions,
            waterBg3WavePhase,
            waterBg2WavePhase,
            waterSurfaceScreenY)
        {
            LavaAcidBg2WavePhase = lavaAcidBg2WavePhase,
        }
        : null;

    /// <summary>
    /// Publishes the selected room FX into Samus's native liquid words. Room rendering and
    /// movement therefore consume one cartridge-owned surface instead of independently
    /// configured debug values.
    /// </summary>
    public void ApplyToSamusLiquidPhysics(SamusLiquidPhysicsState liquid)
    {
        ArgumentNullException.ThrowIfNull(liquid);
        switch (Type)
        {
            case RoomFxType.Water:
                liquid.ConfigureWater(CurrentYPosition, LiquidOptions);
                return;
            case RoomFxType.Lava:
                liquid.ConfigureLavaAcid(BaseYPosition);
                return;
            case RoomFxType.Acid:
                liquid.ConfigureLavaAcid(BaseYPosition, acid: true);
                return;
            default:
                // Non-liquid FX still own $196E. Preserve that identity for atmospheric
                // effects while restoring both liquid positions to their negative sentinel.
                liquid.ConfigureNonLiquidRoomFx(Type);
                return;
        }
    }

    /// <summary>
    /// Applies the exact shared WRAM writes made by the Speed Booster escape PLM. These are
    /// intentionally not a rendering shortcut: bank $84 writes the same fields loaded by
    /// <c>$89:AB82</c>. A translated bank-$88 liquid handler can consume this state later;
    /// this method does not claim that the presently unsupported lava renderer exists.
    /// </summary>
    internal void ApplySpeedBoosterEscapeWrite(
        ushort? baseYPosition = null,
        ushort? targetYPosition = null,
        ushort? packedYVelocity = null,
        ushort? timer = null)
    {
        if (baseYPosition is ushort baseY)
            BaseYPosition = baseY;
        if (targetYPosition is ushort targetY)
            TargetYPosition = targetY;
        if (packedYVelocity is ushort velocity)
            PackedYVelocity = velocity;
        if (timer is ushort newTimer)
            Timer = newTimer;
    }

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

    /// <summary>
    /// Ports the observable static-water path of <c>$88:C48E</c> and both circular wave
    /// clocks. Rising/tidal rooms retain their loaded surface words; the presently reported
    /// room has zero velocity and therefore exercises the cartridge's normal static branch.
    /// </summary>
    private void StepWater(ushort cameraX, ushort cameraY)
    {
        CurrentYPosition = BaseYPosition;
        waterSurfaceScreenY = unchecked((short)(CurrentYPosition - cameraY));
        HorizontalScroll = unchecked((ushort)(
            cameraX + unchecked((sbyte)(waterHorizontalSubscroll >> 8))));
        VerticalScroll = ComputeWaterVerticalScroll(CurrentYPosition, cameraY);

        waterBg3WaveTimer = unchecked((ushort)(waterBg3WaveTimer - 1));
        if (waterBg3WaveTimer == 0)
        {
            waterBg3WaveTimer = RoomFxRomData.Water.Bg3WavePhaseDuration;
            waterBg3WavePhase =
                (waterBg3WavePhase + 1) % RoomFxRomData.Water.WaveDisplacementCount;
        }

        if ((LiquidOptions & 1) != 0)
        {
            waterHorizontalSubscroll = unchecked((ushort)(
                waterHorizontalSubscroll + RoomFxRomData.Water.HorizontalSubscrollVelocity));
        }

        if ((LiquidOptions & 2) == 0)
            return;
        waterBg2WaveTimer = unchecked((ushort)(waterBg2WaveTimer - 1));
        if (waterBg2WaveTimer == 0)
        {
            waterBg2WaveTimer = RoomFxRomData.Water.Bg2WavePhaseDuration;
            waterBg2WavePhase =
                (waterBg2WavePhase + 1) % RoomFxRomData.Water.WaveDisplacementCount;
        }
    }

    /// <summary>
    /// Ports the visible outputs of <c>$88:B3B0</c> and <c>$88:B4D5</c>. The liquid
    /// surface and BG3 plane follow the room camera, while one of two sixteen-scanline BG2
    /// waveforms rotates at the cadence selected by the record's liquid-options byte.
    /// </summary>
    private void StepLavaAcid(ushort cameraX, ushort cameraY)
    {
        CurrentYPosition = BaseYPosition;
        waterSurfaceScreenY = unchecked((short)(CurrentYPosition - cameraY));
        HorizontalScroll = cameraX;
        VerticalScroll = 0;

        if (!UsesLavaAcidVerticalWave && !UsesLavaAcidHorizontalWave)
            return;
        lavaAcidBg2WaveTimer = unchecked((ushort)(lavaAcidBg2WaveTimer - 1));
        if (lavaAcidBg2WaveTimer != 0)
            return;

        lavaAcidBg2WavePhase =
            (lavaAcidBg2WavePhase - 1 + RoomFxRomData.LavaAcid.WaveDisplacementCount) %
            RoomFxRomData.LavaAcid.WaveDisplacementCount;
        lavaAcidBg2WaveTimer = UsesLavaAcidVerticalWave
            ? RoomFxRomData.LavaAcid.VerticalWavePhaseDuration
            : RoomFxRomData.LavaAcid.HorizontalWavePhaseDuration;
    }

    private bool UsesLavaAcidVerticalWave =>
        (LiquidOptions & RoomFxRomData.LavaAcid.VerticalBg2WaveOption) != 0;

    private bool UsesLavaAcidHorizontalWave =>
        (LiquidOptions & RoomFxRomData.LavaAcid.HorizontalBg2WaveOption) != 0;

    private static ushort ComputeWaterVerticalScroll(ushort surfaceY, ushort cameraY)
    {
        if (unchecked((short)surfaceY) < 0)
            return 0;
        short relative = unchecked((short)(surfaceY - cameraY));
        if (relative <= 0)
            return unchecked((ushort)(((relative ^ 0x001f) & 0x001f) | 0x0100));
        return relative < 0x0100
            ? unchecked((ushort)((~relative) & 0x00ff))
            : (ushort)0;
    }

    private void Reset()
    {
        Type = RoomFxType.None;
        BaseYPosition = 0;
        TargetYPosition = 0;
        PackedYVelocity = 0;
        Timer = 0;
        LayerBlendConfiguration = default;
        HorizontalScroll = VerticalScroll = 0;
        verticalAccumulator = horizontalAccumulator = horizontalVelocity = 0;
        previousCameraY = previousCameraX = 0;
        animationTimer = 0;
        animationFrame = 0;
        CurrentYPosition = ushort.MaxValue;
        LiquidOptions = 0;
        waterHorizontalSubscroll = 0;
        waterBg3WaveTimer = waterBg2WaveTimer = 0;
        waterBg3WavePhase = waterBg2WavePhase = 0;
        waterSurfaceScreenY = short.MaxValue;
        lavaAcidBg2WaveTimer = 0;
        lavaAcidBg2WavePhase = 0;
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
    ushort VerticalScroll,
    ushort CurrentYPosition = ushort.MaxValue,
    ushort LiquidOptions = 0,
    int WaterBg3WavePhase = 0,
    int WaterBg2WavePhase = 0,
    short WaterSurfaceScreenY = short.MaxValue)
{
    /// <summary>
    /// Effective circular phase of the lava/acid BG2 HDMA table. Zero is the first table
    /// produced after <c>$88:B4CE</c>; subsequent phases move backward through 16 entries.
    /// </summary>
    public int LavaAcidBg2WavePhase { get; init; }
}

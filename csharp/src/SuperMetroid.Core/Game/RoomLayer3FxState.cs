using SuperMetroid.Core.Audio;
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
/// writes without inventing another representation for the corresponding bank-$88 state.
/// The translated BG3 effects are lava, acid, water, Landing Site rain, and Climb fog.
/// </remarks>
public sealed class RoomLayer3FxState
{
    private readonly RoomFxAnimatedTilesState animatedTiles = new();
    private ushort verticalAccumulator;
    private ushort horizontalAccumulator;
    private ushort horizontalVelocity;
    private ushort previousCameraY;
    private ushort previousCameraX;
    private ushort baseYSubposition;
    private ushort tidePhase;
    private int tideFixedOffset;
    private ushort waterHorizontalSubscroll;
    private ushort waterBg3WaveTimer;
    private ushort waterBg2WaveTimer;
    private int waterBg3WavePhase;
    private int waterBg2WavePhase;
    private short waterSurfaceScreenY;
    private ushort lavaAcidBg2WaveTimer;
    private int lavaAcidBg2WavePhase;
    private LiquidRisePhase liquidRisePhase;
    private readonly List<RoomFxSoundRequest> soundRequests = [];
    private ushort earthquakeSoundTimer;
    private int earthquakeSoundSequenceIndex;

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

    /// <summary>
    /// Sound requests emitted by the current bank-$88 effect pass in cartridge queue order.
    /// The list is cleared at the beginning of every <see cref="Step"/> call.
    /// </summary>
    public IReadOnlyList<RoomFxSoundRequest> SoundRequests => soundRequests;

    /// <summary>
    /// Global earthquake words requested by the current effect pass. The timer value is a
    /// bit mask because the cartridge uses <c>TSB</c>, not assignment.
    /// </summary>
    public RoomFxEarthquakeRequest? EarthquakeRequest { get; private set; }

    /// <summary>Clears the prior room and selects the current door's native FX record.</summary>
    public void Load(
        ISnesAddressSpace bus,
        SnesVram vram,
        SnesCgram cgram,
        ushort fxPointer,
        ushort doorPointer,
        ushort randomNumber,
        ushort roomHeaderPointer = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);
        Reset();

        // Both native room-load paths clear VRAM $5880-$5FFF to $184E immediately before
        // copying the selected bank-$8A effect tilemap at $5BE0. This is not optional
        // cleanup for rooms without FX: liquids deliberately expose the cleared first page
        // above their surface. Retaining zeros or a previous room's HUD/effect words makes
        // the standard orange `1` glyph (and sometimes a displaced HUD clone) scroll with
        // the atmosphere.
        var clearedTilemap = new ushort[RoomFxRomData.Layer3.ClearWordCount];
        Array.Fill(clearedTilemap, RoomFxRomData.Layer3.ClearTilemapWord);
        vram.ExecuteWordTransfer(
            clearedTilemap,
            RoomFxRomData.Layer3.ClearDestinationWord,
            wordIncrement: 1);

        if (RoomFxRomData.Earthquake.SoundSuppressedRooms.All.Contains(roomHeaderPointer))
            earthquakeSoundTimer = ushort.MaxValue;
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
        animatedTiles.Load(bus, Type);
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

        if (Type == RoomFxType.Fireflea)
            FirefleaRoomFx.Initialize(bus);
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
        }
    }

    /// <summary>Runs the active bank-$88 pre-instruction and rain animtile handler.</summary>
    public void Step(
        ISnesAddressSpace bus,
        SnesVram vram,
        ushort cameraX,
        ushort cameraY,
        bool timeIsFrozen,
        ushort randomNumber = 0,
        ushort firefleaDarknessLevel = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(vram);
        soundRequests.Clear();
        EarthquakeRequest = null;
        if (Type == RoomFxType.Fireflea)
        {
            LayerBlendConfiguration = LayerBlendingConfiguration.Fireflea;
            FirefleaRoomFx.Step(bus, timeIsFrozen, firefleaDarknessLevel);
            return;
        }
        if (!IsRenderable || timeIsFrozen)
            return;

        // The shared bank-$87 handler runs independently of the bank-$88 HDMA
        // pre-instruction. In particular, lava/acid need this transfer before their BG3
        // tilemap can name anything other than stale standard-HUD characters.
        animatedTiles.Step(bus, vram);

        if (Type is RoomFxType.Lava or RoomFxType.Acid)
        {
            StepLavaAcid(bus, cameraX, cameraY, randomNumber);
            return;
        }

        if (Type == RoomFxType.Water)
        {
            StepWater(bus, cameraX, cameraY, randomNumber);
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
        VerticalScroll = ComputeLiquidVerticalScroll(CurrentYPosition, cameraY);
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
                liquid.ConfigureLavaAcid(CurrentYPosition);
                return;
            case RoomFxType.Acid:
                liquid.ConfigureLavaAcid(CurrentYPosition, acid: true);
                return;
            default:
                // Non-liquid FX still own $196E. Preserve that identity for atmospheric
                // effects while restoring both liquid positions to their negative sentinel.
                liquid.ConfigureNonLiquidRoomFx(Type);
                return;
        }
    }

    /// <summary>
    /// Applies shared liquid WRAM writes made by room PLMs and enemy instructions. These are
    /// intentionally not a rendering shortcut: bank $84 writes the same fields loaded by
    /// <c>$89:AB82</c>. The translated bank-$88 liquid handler consumes those shared words
    /// on its next ordinary effect pass, just as the cartridge does.
    /// </summary>
    internal void ApplyCartridgeMotionWrites(
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

    /// <summary>
    /// Applies <c>Instruction_PLM_EnableWaterPhysics</c> at <c>$84:D525</c> to the
    /// cartridge-owned liquid-options word. Bank $84 uses <c>TRB</c> on WRAM $197E, so
    /// this mutation must live on the room-FX owner that republishes the word to Samus;
    /// changing only Samus's projected copy would be undone by the next FX frame.
    /// </summary>
    internal void EnableWaterPhysics() =>
        LiquidOptions = unchecked((ushort)(
            LiquidOptions & ~RoomFxRomData.Water.PhysicsDisabledOption));

    /// <summary>
    /// Ports the observable static-water path of <c>$88:C48E</c> and both circular wave
    /// clocks. Rising/tidal rooms retain their loaded surface words; the presently reported
    /// room has zero velocity and therefore exercises the cartridge's normal static branch.
    /// </summary>
    private void StepWater(
        ISnesAddressSpace bus,
        ushort cameraX,
        ushort cameraY,
        ushort randomNumber)
    {
        StepLiquidRise(randomNumber);
        StepLiquidTide(bus);
        CurrentYPosition = ComputeTidalYPosition();
        waterSurfaceScreenY = unchecked((short)(CurrentYPosition - cameraY));
        HorizontalScroll = unchecked((ushort)(
            cameraX + unchecked((sbyte)(waterHorizontalSubscroll >> 8))));
        VerticalScroll = ComputeLiquidVerticalScroll(CurrentYPosition, cameraY);

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
    private void StepLavaAcid(
        ISnesAddressSpace bus,
        ushort cameraX,
        ushort cameraY,
        ushort randomNumber)
    {
        StepLiquidRise(randomNumber);
        StepLiquidTide(bus);
        CurrentYPosition = ComputeTidalYPosition();
        waterSurfaceScreenY = unchecked((short)(CurrentYPosition - cameraY));
        HorizontalScroll = cameraX;
        // Lava/acid $88:B3B0 uses the same surface-relative BG3VOFS equation as water
        // $88:C48E. Zeroing it anchored the animated characters to the screen instead of
        // beginning their first texture row immediately below the liquid boundary.
        VerticalScroll = ComputeLiquidVerticalScroll(CurrentYPosition, cameraY);

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

    /// <summary>
    /// Executes the parallel native callback trios at $88:B343/$B367/$B382 for lava/acid
    /// and $88:C428/$C44C/$C458 for water. A nonzero velocity first arms the wait phase,
    /// the record timer then expires, and only then does the signed 8.8 velocity move the
    /// 16.16 base position toward the target. Door-specific Norfair entries rely on this
    /// sequence to raise lava into the visible viewport.
    /// </summary>
    private void StepLiquidRise(ushort randomNumber = 0)
    {
        switch (liquidRisePhase)
        {
            case LiquidRisePhase.Dormant:
            {
                short velocity = unchecked((short)PackedYVelocity);
                bool movesTowardTarget = velocity > 0
                    ? TargetYPosition > BaseYPosition
                    : velocity < 0 && TargetYPosition < BaseYPosition;
                if (movesTowardTarget)
                    liquidRisePhase = LiquidRisePhase.Waiting;
                return;
            }

            case LiquidRisePhase.Waiting:
                PublishRisingLiquidFeedback(randomNumber);
                Timer = unchecked((ushort)(Timer - 1));
                if (Timer == 0)
                    liquidRisePhase = LiquidRisePhase.Moving;
                return;

            case LiquidRisePhase.Moving:
                PublishRisingLiquidFeedback(randomNumber);
                if (AdvanceBaseYToTarget())
                {
                    PackedYVelocity = 0;
                    liquidRisePhase = LiquidRisePhase.Dormant;
                }
                return;

            default:
                throw new InvalidDataException(
                    $"Unknown room-liquid rise phase {liquidRisePhase}.");
        }
    }

    /// <summary>
    /// Executes <c>$88:B21D</c> and the shared <c>$88:B36A-$B373/$B385-$B38E</c>
    /// writes used while lava or acid waits and moves. Sound cadence and screen shake are
    /// parallel outputs of the cartridge FX actor; neither is inferred by the renderer.
    /// </summary>
    private void PublishRisingLiquidFeedback(ushort randomNumber)
    {
        HandleEarthquakeSoundEffect(randomNumber);
        EarthquakeRequest = new RoomFxEarthquakeRequest(
            RoomFxRomData.Earthquake.RisingLiquidType,
            RoomFxRomData.Earthquake.RisingLiquidTimerBits);
    }

    /// <summary>Ports the signed timer and eight-entry loop at <c>$88:B21D-$B254</c>.</summary>
    private void HandleEarthquakeSoundEffect(ushort randomNumber)
    {
        if (unchecked((short)earthquakeSoundTimer) < 0)
            return;

        earthquakeSoundTimer = unchecked((ushort)(earthquakeSoundTimer - 1));
        if (unchecked((short)earthquakeSoundTimer) >= 0)
            return;

        ReadOnlySpan<ushort> baseTimers =
            RoomFxRomData.Earthquake.RisingLiquidSoundBaseTimers;
        if ((uint)earthquakeSoundSequenceIndex >= (uint)baseTimers.Length)
            earthquakeSoundSequenceIndex = 0;

        soundRequests.Add(new RoomFxSoundRequest(
            SoundEffectLibrary2Sounds.Earthquake,
            MaximumQueued: 6));
        earthquakeSoundTimer = unchecked((ushort)(
            baseTimers[earthquakeSoundSequenceIndex] + (randomNumber & 3)));
        earthquakeSoundSequenceIndex++;
    }

    /// <summary>Ports <c>RaiseOrLowerFx</c> at $88:868C for the shared liquid words.</summary>
    private bool AdvanceBaseYToTarget()
    {
        if ((TargetYPosition & 0x8000) != 0)
            return true;

        short velocity = unchecked((short)PackedYVelocity);
        int delta = velocity << 8;
        uint fixedPosition = ((uint)BaseYPosition << 16) | baseYSubposition;
        fixedPosition = unchecked(fixedPosition + (uint)delta);
        ushort candidate = unchecked((ushort)(fixedPosition >> 16));
        baseYSubposition = unchecked((ushort)fixedPosition);

        if (velocity >= 0)
        {
            if ((candidate & 0x8000) != 0)
                candidate = ushort.MaxValue;
            BaseYPosition = candidate;
            if (candidate <= TargetYPosition)
                return false;
        }
        else
        {
            if ((candidate & 0x8000) != 0)
                candidate = 0;
            BaseYPosition = candidate;
            if (candidate > TargetYPosition)
                return false;
        }

        BaseYPosition = TargetYPosition;
        baseYSubposition = 0;
        return true;
    }

    /// <summary>
    /// Ports <c>FxHandleTide</c> at $88:B2C9. Native writes the scaled sine word starting
    /// one byte into the 16.16 offset pair, equivalent to the eight-bit shift below. The
    /// asymmetric phase deltas deliberately spend different durations in each half-wave.
    /// </summary>
    private void StepLiquidTide(ISnesAddressSpace bus)
    {
        int scale;
        ushort positiveDelta;
        ushort negativeDelta;
        if ((LiquidOptions & RoomFxRomData.LiquidTide.SmallTideOption) != 0)
        {
            scale = RoomFxRomData.LiquidTide.SmallTideScale;
            positiveDelta = RoomFxRomData.LiquidTide.SmallTidePositivePhaseDelta;
            negativeDelta = RoomFxRomData.LiquidTide.SmallTideNegativePhaseDelta;
        }
        else if ((LiquidOptions & RoomFxRomData.LiquidTide.LargeTideOption) != 0)
        {
            scale = RoomFxRomData.LiquidTide.LargeTideScale;
            positiveDelta = RoomFxRomData.LiquidTide.LargeTidePositivePhaseDelta;
            negativeDelta = RoomFxRomData.LiquidTide.LargeTideNegativePhaseDelta;
        }
        else
        {
            tideFixedOffset = 0;
            return;
        }

        short sample = unchecked((short)ReadWord(
            bus,
            RoomFxRomData.LiquidTide.SignedSineTableAddress + (tidePhase >> 8) * 2));
        tideFixedOffset = sample * scale << 8;
        tidePhase = unchecked((ushort)(
            tidePhase + (sample >= 0 ? positiveDelta : negativeDelta)));
    }

    private ushort ComputeTidalYPosition()
    {
        uint baseFixed = ((uint)BaseYPosition << 16) | baseYSubposition;
        return unchecked((ushort)((baseFixed + (uint)tideFixedOffset) >> 16));
    }

    private bool UsesLavaAcidVerticalWave =>
        (LiquidOptions & RoomFxRomData.LavaAcid.VerticalBg2WaveOption) != 0;

    private bool UsesLavaAcidHorizontalWave =>
        (LiquidOptions & RoomFxRomData.LavaAcid.HorizontalBg2WaveOption) != 0;

    private static ushort ComputeLiquidVerticalScroll(ushort surfaceY, ushort cameraY)
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
        animatedTiles.Reset();
        Type = RoomFxType.None;
        BaseYPosition = 0;
        TargetYPosition = 0;
        PackedYVelocity = 0;
        Timer = 0;
        LayerBlendConfiguration = default;
        HorizontalScroll = VerticalScroll = 0;
        verticalAccumulator = horizontalAccumulator = horizontalVelocity = 0;
        previousCameraY = previousCameraX = 0;
        baseYSubposition = 0x8000;
        tidePhase = 0;
        tideFixedOffset = 0;
        CurrentYPosition = ushort.MaxValue;
        LiquidOptions = 0;
        waterHorizontalSubscroll = 0;
        waterBg3WaveTimer = waterBg2WaveTimer = 0;
        waterBg3WavePhase = waterBg2WavePhase = 0;
        waterSurfaceScreenY = short.MaxValue;
        lavaAcidBg2WaveTimer = 0;
        lavaAcidBg2WavePhase = 0;
        liquidRisePhase = LiquidRisePhase.Dormant;
        soundRequests.Clear();
        EarthquakeRequest = null;
        earthquakeSoundTimer = 0;
        earthquakeSoundSequenceIndex = 0;
    }

    private static short SignedHighByte(ushort value) => unchecked((sbyte)(value >> 8));

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        RomDataReader.ReadWordFixedBank(bus, address);

    /// <summary>The mutually exclusive callbacks installed in the native rise-function word.</summary>
    private enum LiquidRisePhase
    {
        Dormant,
        Waiting,
        Moving,
    }
}

/// <summary>One bank-$88 room-FX request for the global cartridge sound queue.</summary>
public readonly record struct RoomFxSoundRequest(
    SoundEffectId SoundEffect,
    byte MaximumQueued);

/// <summary>One bank-$88 request for the global room-shake type and timer bits.</summary>
public readonly record struct RoomFxEarthquakeRequest(
    ushort Type,
    ushort TimerBits);

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

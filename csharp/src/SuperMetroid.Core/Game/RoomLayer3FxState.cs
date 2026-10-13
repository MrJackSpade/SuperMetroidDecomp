using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge-owned room effect selected by one sixteen-byte bank-$83 FX record.
/// </summary>
/// <remarks>
/// Every room effect shares the native base/target position, packed 8.8 Y velocity, and
/// timer loaded by <c>$89:AB82</c>. This owner retains those words even when that effect's
/// renderer is not translated yet, allowing bank-$84 PLMs to make their native shared-state
/// writes without inventing another representation for the corresponding bank-$88 state.
/// The translated BG3 effects are lava, acid, water, spores, Landing Site rain, and Climb fog.
/// </remarks>
public sealed class RoomLayer3FxState
{
    /// <summary>Tracks frame selection for the liquid, rain, fog, and spore tile animations.</summary>
    private readonly RoomFxAnimatedTilesState animatedTiles = new();

    /// <summary>Current host-owned liquid/rain frame art; never stored in debugger state.</summary>
    [field: NonSerialized]
    public RoomFxAnimatedTileAtlas? AnimatedTileArtwork { get; set; }

    /// <summary>Current host-owned BG3 effect tilemaps; never stored in debugger state.</summary>
    [field: NonSerialized]
    public RoomFxLayer3TilemapCatalog? Layer3Tilemaps { get; set; }

    /// <summary>Current host-owned room-FX blend colors; never stored in debugger state.</summary>
    [field: NonSerialized]
    public RoomFxPaletteBlendCatalog? PaletteBlendColors { get; set; }
    /// <summary>
    /// Rain, fog, and spores texture drift: one 8.8 accumulator per axis. Its signed high
    /// byte offsets BG3 scroll before the next velocity step.
    /// </summary>
    private Offset16<Bg3Subpixels> drift;
    /// <summary>Native horizontal drift velocity for atmospheric BG3 effects.</summary>
    private ushort horizontalVelocity;
    /// <summary>Previous layer-one camera point, used to make rain follow camera movement.</summary>
    private Position16<RoomPixels> previousCamera;
    /// <summary>Fractional part of the liquid surface before tide displacement.</summary>
    private ushort baseYSubposition;
    /// <summary>Negative-cosine lookup phase for the liquid tide.</summary>
    private ushort tidePhase;
    /// <summary>Signed fixed-point tide displacement added to the base surface.</summary>
    private int tideFixedOffset;
    /// <summary>Water BG3 horizontal drift accumulated in subpixels.</summary>
    private ushort waterHorizontalSubscroll;
    /// <summary>Countdown before advancing the water BG3 waveform.</summary>
    private ushort waterBg3WaveTimer;
    /// <summary>Countdown for the optional water BG2 horizontal wave.</summary>
    private ushort waterBg2WaveTimer;
    /// <summary>Selected BG3 water displacement frame.</summary>
    private int waterBg3WavePhase;
    /// <summary>Selected BG2 water displacement frame.</summary>
    private int waterBg2WavePhase;
    /// <summary>Liquid surface relative to the camera, used to clip the effect plane.</summary>
    private short waterSurfaceScreenY;
    /// <summary>Countdown for lava or acid's optional BG2 wave.</summary>
    private ushort lavaAcidBg2WaveTimer;
    /// <summary>Selected lava or acid BG2 wave displacement frame.</summary>
    private int lavaAcidBg2WavePhase;
    /// <summary>
    /// Set after the first liquid BG3 HDMA pass installs its pre-instruction. Later passes
    /// run it before main-loop RNG; water uses this latch despite its older lava/acid name.
    /// </summary>
    private bool lavaAcidBg3PreInstructionInstalled;
    /// <summary>
    /// Records direct deletion of liquid scroll HDMA objects at $A9:8C0C, which stops
    /// their per-frame updates until the next room load.
    /// </summary>
    private bool liquidHdmaObjectsDeleted;

    /// <summary>HDMA-object variable two of the lava/acid BG3 object: the ambient sound timer.</summary>
    private ushort lavaSoundTimer;
    /// <summary>Current liquid rise or fall state selected by the FX record.</summary>
    private LiquidRisePhase liquidRisePhase;
    /// <summary>Audio requests emitted in native order during the current effect pass.</summary>
    private readonly List<RoomFxSoundRequest> soundRequests = [];
    /// <summary>Rebound sound recipient for effect requests after a debugger restore.</summary>
    [NonSerialized] private SamusPowerBombExplosionState? audioPowerBomb;
    /// <summary>Countdown between quake sound requests in earthquake FX rooms.</summary>
    private ushort earthquakeSoundTimer;
    /// <summary>Position in the room earthquake sound sequence.</summary>
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
    public LayerBlendingConfiguration LayerBlendConfiguration { get; private set; } = LayerBlendingConfiguration.Unconfigured;

    /// <summary>Live BG3 scroll-register shadows (BG3HOFS/BG3VOFS).</summary>
    public Position16<Bg3Pixels> Scroll { get; private set; }

    /// <summary>Whether the translated effect supplies a gameplay-region BG3 plane.</summary>
    public bool IsRenderable => Type is
        RoomFxType.Lava or RoomFxType.Acid or RoomFxType.Water or RoomFxType.TourianEntranceStatue or
        RoomFxType.Rain or RoomFxType.Fog or RoomFxType.Spores;

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
        ushort roomHeaderPointer = 0) =>
        LoadCore(bus, vram, cgram, new RoomFxLoadSource { FxPointer = fxPointer, DoorPointer = doorPointer, RandomNumber = randomNumber, RoomHeaderPointer = roomHeaderPointer, SuppliedDefinition = null });

    /// <summary>
    /// Clears the prior BG3 tilemap, selects the door's FX record, and initializes the
    /// liquid and palette state shared by room effects and PLMs.
    /// </summary>
    private void LoadCore(ISnesAddressSpace bus, SnesVram vram, SnesCgram cgram, RoomFxLoadSource source)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        Reset();

        // Both native room-load paths clear VRAM $5880-$5FFF to $184E immediately before
        // copying the selected bank-$8A effect tilemap at $5BE0. This is not optional
        // cleanup for rooms without FX: liquids deliberately expose the cleared first page
        // above their surface. Retaining zeros or a previous room's HUD/effect words makes
        // the standard orange `1` glyph (and sometimes a displaced HUD clone) scroll with
        // the atmosphere.
        VramRegion layer3Area = new RoomFxVram(vram).Layer3Area;
        var clearedTilemap = new ushort[layer3Area.WordCount];
        Array.Fill(clearedTilemap, RoomFxRomData.Layer3.ClearTilemapWord);
        layer3Area.Write(clearedTilemap);

        if (RoomFxRomData.Earthquake.SoundSuppressedRooms.Contains(source.RoomHeaderPointer))
            earthquakeSoundTimer = ushort.MaxValue;
        if (source.FxPointer == 0)
            return;

        ushort record = source.SuppliedDefinition?.Pointer ?? RoomFxRecordDefinitions.Select(source.FxPointer, source.DoorPointer);
        if (record == 0)
            return;
        RoomFxRecordDefinition definition = source.SuppliedDefinition ?? RoomFxRecordDefinitions.Get(record);

        BaseYPosition = definition.BaseYPosition;
        TargetYPosition = definition.TargetYPosition;
        PackedYVelocity = definition.PackedYVelocity;
        Timer = definition.Timer;
        LiquidOptions = definition.LiquidOptions;
        CurrentYPosition = BaseYPosition;

        Type = RoomFxTypes.FromCartridge(
            definition.Type,
            $"bank-$83 FX record ${record:X4}");
        LayerBlendConfiguration = LayerBlendingConfigurations.FromCartridge(
            definition.Layer3LayerBlend,
            $"bank-$83 FX record ${record:X4}");
        animatedTiles.Load(bus, Type);
        ApplyPaletteBlend(cgram, definition.PaletteBlend);

        if (Type == RoomFxType.Fireflea)
            FirefleaRoomFx.Initialize(SnesMemoryBusRequirement.Require(bus, "Fireflea FX requires live WRAM."));
        if (!IsRenderable)
            return;

        new RoomFxVram(vram).Layer3Tilemap.Load((Layer3Tilemaps ?? throw new InvalidOperationException(
                "Renderable room FX requires installed layer-3 tilemaps."))
            .Resolve(Type).Span);

        if (RoomFxTypes.UsesWater(Type))
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
            // `$88:D981` masks the random word with six *after* shifting it once,
            // producing byte offsets 0/2/4/6 into a word table. Expressed as a C#
            // element index that is bits two and three of the original random word.
            horizontalVelocity = RoomFxRomData.Rain.HorizontalVelocity((source.RandomNumber >> 2) & 3);
        }
    }

    /// <summary>
    /// True for the liquids whose BG3 HDMA pre-instruction moves the surface: lava and
    /// acid ($88:B3B0) and water ($88:C48E). That motion belongs to the HDMA pass.
    /// </summary>
    public bool MovesLiquidInHdmaPass =>
        Type is RoomFxType.Lava or RoomFxType.Acid || RoomFxTypes.UsesWater(Type);

    /// <summary>
    /// Runs liquid motion and shared-state writes from $88:B3B0 and $88:C48E before
    /// main-loop RNG generation. $88:C3E9 and $88:D85E install the callback on the
    /// first HDMA pass; lava/acid's $88:B44A-B44E then swaps the shared RNG bytes on
    /// subsequent unfrozen passes, even off screen. Rising/tidal motion also runs during
    /// door fades and pause entry. Visual/VRAM updates remain in <see cref="Step"/>,
    /// which must not advance that motion twice.
    /// </summary>
    public void AdvanceHdmaSharedState(Bank80SystemState system, bool timeIsFrozen)
    {
        Ensure.NotNull(system);
        if (!MovesLiquidInHdmaPass || liquidHdmaObjectsDeleted)
            return;
        bool lavaOrAcid = Type is RoomFxType.Lava or RoomFxType.Acid;
        EarthquakeRequest = null;
        soundRequests.Clear();
        if (!lavaAcidBg3PreInstructionInstalled)
        {
            lavaAcidBg3PreInstructionInstalled = true;
            // $88:C3E7 sets the sound timer in the same pass that installs the pre-instruction.
            if (lavaOrAcid)
                lavaSoundTimer = RoomFxRomData.LavaAcid.AmbientSoundPeriod;
            return;
        }
        if (timeIsFrozen)
            return;
        if (!lavaOrAcid)
        {
            // $88:C4A3-C4BB: the rising function and tide; water leaves the RNG alone.
            AdvanceLiquidMotion(system.RandomNumber, system.MainGameLoopCarry);
            return;
        }
        ushort random = system.RandomNumber;
        AdvanceLiquidMotion(random, system.MainGameLoopCarry);
        QueueLavaAmbientSound(random);
        system.SetRandomNumber(SnesLittleEndian.SwapBytes(random));
    }

    /// <summary>
    /// Ports <c>$88:B421-$B446</c>: while a lava surface's Y is not negative, its timer
    /// counts down and, on reaching zero, reloads and queues a random ambient sound.
    /// Acid shares the pre-instruction but not this sound.
    /// </summary>
    private void QueueLavaAmbientSound(ushort random)
    {
        if (Type != RoomFxType.Lava || unchecked((short)CurrentYPosition) < 0)
            return;
        lavaSoundTimer = unchecked((ushort)(lavaSoundTimer - 1));
        if (lavaSoundTimer != 0)
            return;
        lavaSoundTimer = RoomFxRomData.LavaAcid.AmbientSoundPeriod;
        soundRequests.Add(new RoomFxSoundRequest(
            RoomFxRomData.LavaAcid.AmbientSound(random),
            RoomFxRomData.LavaAcid.AmbientSoundMaximumQueued,
            SoundSuppressed: audioPowerBomb?.IsActive == true));
    }

    /// <summary>Runs the active bank-$88 pre-instruction and rain animtile handler.</summary>
    public void Step(ISnesAddressSpace bus, SnesVram vram, RoomLayer3FxFrame frame)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(vram);
        audioPowerBomb = frame.PowerBomb;
        if (!frame.LiquidMotionAlreadyAdvanced)
        {
            soundRequests.Clear();
            EarthquakeRequest = null;
        }
        if (Type == RoomFxType.Fireflea)
        {
            LayerBlendConfiguration = LayerBlendingConfiguration.Fireflea;
            FirefleaRoomFx.Step(SnesMemoryBusRequirement.Require(bus, "Fireflea FX requires live WRAM."),
                frame.TimeIsFrozen, frame.FirefleaDarknessLevel);
            return;
        }
        if (!IsRenderable || frame.TimeIsFrozen)
            return;

        // The shared bank-$87 handler runs independently of the bank-$88 HDMA
        // pre-instruction. In particular, lava/acid need this transfer before their BG3
        // tilemap can name anything other than stale standard-HUD characters.
        animatedTiles.Step(bus, vram, artwork: AnimatedTileArtwork);

        if (liquidHdmaObjectsDeleted && MovesLiquidInHdmaPass)
            return;

        if (Type is RoomFxType.Lava or RoomFxType.Acid)
        {
            StepLavaAcid(frame.Camera, frame.RandomNumber, frame.MainGameLoopCarry, frame.LiquidMotionAlreadyAdvanced);
            return;
        }

        if (RoomFxTypes.UsesWater(Type))
        {
            StepWater(frame.Camera, frame.RandomNumber, frame.MainGameLoopCarry, frame.LiquidMotionAlreadyAdvanced);
            return;
        }

        if (Type == RoomFxType.Rain)
        {
            Offset16<Bg3Pixels> cameraDelta = previousCamera.AsBg3Scroll() - frame.Camera.AsBg3Scroll();
            Scroll = Position16<Bg3Pixels>.Zero + cameraDelta + drift.WholePixels();
            drift = new(unchecked((short)(drift.X + horizontalVelocity)),
                unchecked((short)(drift.Y - RoomFxRomData.Rain.VerticalVelocity)));
            previousCamera = frame.Camera;
            return;
        }

        if (Type == RoomFxType.Spores)
        {
            // $88:DA47 samples each signed accumulator before adding its velocity.
            // Unlike rain, spores are anchored to the current camera, not its delta.
            Scroll = frame.Camera.AsBg3Scroll() + drift.WholePixels();
            drift = drift.WithY(unchecked((short)(drift.Y - RoomFxRomData.Spores.VerticalVelocity)));
            return;
        }

        // `$88:DB36` anchors fog to layer one and moves its texture by -$40/+50 in 8.8.
        Scroll = frame.Camera.AsBg3Scroll() + drift.WholePixels();
        drift = new(unchecked((short)(drift.X + RoomFxRomData.Fog.HorizontalVelocity)),
            unchecked((short)(drift.Y - RoomFxRomData.Fog.VerticalVelocity)));
    }

    /// <summary>
    /// Seeds camera-dependent liquid registers during a room transition without advancing
    /// a wave clock. Destination rendering can therefore show the correct surface on its
    /// first accepted NMI instead of waiting for the first gameplay handler call.
    /// </summary>
    public void PrimeViewport(Position16<RoomPixels> camera)
    {
        if (Type is not (RoomFxType.Water or RoomFxType.TourianEntranceStatue or RoomFxType.Lava or RoomFxType.Acid))
            return;
        CurrentYPosition = BaseYPosition;
        waterSurfaceScreenY = unchecked((short)(CurrentYPosition - camera.Y));
        Scroll = camera.AsBg3Scroll().WithY(ComputeLiquidVerticalScroll(CurrentYPosition, camera.Y));
    }

    /// <summary>Captures values visible alongside the current accepted NMI's OAM upload.</summary>
    public RoomLayer3FxRenderSnapshot? CaptureForDisplay() => IsRenderable
        ? new RoomLayer3FxRenderSnapshot(
            Type,
            LayerBlendConfiguration,
            Scroll,
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
        Ensure.NotNull(liquid);
        switch (Type)
        {
            case RoomFxType.Water:
            case RoomFxType.TourianEntranceStatue:
                liquid.ConfigureWater(CurrentYPosition, LiquidOptions, Type);
                return;
            case RoomFxType.Lava:
                liquid.ConfigureLavaAcid(CurrentYPosition);
                return;
            case RoomFxType.Acid:
                liquid.ConfigureLavaAcid(CurrentYPosition, acid: true);
                return;
            case RoomFxType.None:
            case RoomFxType.Spores:
            case RoomFxType.Rain:
            case RoomFxType.Fog:
            case RoomFxType.ScrollingSky:
            case RoomFxType.UnusedScrollingSky:
            case RoomFxType.Fireflea:
            case RoomFxType.CeresRidley:
            case RoomFxType.CeresElevator:
            case RoomFxType.CeresHaze:
                // Non-liquid FX still own $196E. Preserve that identity for atmospheric
                // effects while restoring both liquid positions to their negative sentinel.
                liquid.ConfigureNonLiquidRoomFx(Type);
                return;
            default:
                throw new InvalidOperationException($"Undefined RoomFxType {Type}.");
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
    /// $89:AB02 LoadFxEntry reloads motion, blending and three colors, but does not restart
    /// HDMA, animated tiles, or the liquid's current motion phase as a room load would.
    /// </summary>
    internal LayerBlendingConfiguration ApplyEntry(SnesCgram cgram,
        ushort record)
    {
        RoomFxRecordDefinition definition = RoomFxRecordDefinitions.Get(record);
        BaseYPosition = definition.BaseYPosition;
        TargetYPosition = definition.TargetYPosition;
        PackedYVelocity = definition.PackedYVelocity;
        Timer = definition.Timer;
        LiquidOptions = definition.LiquidOptions;
        LayerBlendConfiguration = LayerBlendingConfigurations.FromCartridge(
            definition.Layer3LayerBlend, "LoadFxEntry");
        ApplyPaletteBlend(cgram, definition.PaletteBlend);
        return LayerBlendingConfigurations.FromCartridge(
            definition.DefaultLayerBlend, "LoadFxEntry");
    }

    /// <summary>Applies the selected FX blend colors from installed presentation data.</summary>
    private void ApplyPaletteBlend(SnesCgram cgram, byte selection)
    {
        (PaletteBlendColors ?? throw new InvalidOperationException(
            "Room FX requires installed palette-blend colors.")).Apply(cgram,
            ClosedNativeWords.Decode<RoomFxPaletteBlend>(selection, "room-FX palette blend"));
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
        Position16<RoomPixels> camera,
        ushort randomNumber,
        bool mainGameLoopCarry,
        bool liquidMotionAlreadyAdvanced)
    {
        if (!liquidMotionAlreadyAdvanced)
            AdvanceLiquidMotion(randomNumber, mainGameLoopCarry);
        waterSurfaceScreenY = unchecked((short)(CurrentYPosition - camera.Y));
        var subscroll = new Offset16<Bg3Pixels>(unchecked((sbyte)(SnesLittleEndian.HighByte(waterHorizontalSubscroll))), 0);
        Scroll = (camera.AsBg3Scroll() + subscroll).WithY(ComputeLiquidVerticalScroll(CurrentYPosition, camera.Y));

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

        // FX $26 spawns only the BG3 water object. Its second HDMA object moves
        // the statue vertically instead of installing the ordinary BG2 X wave.
        if (Type != RoomFxType.Water || (LiquidOptions & 2) == 0)
            return;
        waterBg2WaveTimer = unchecked((ushort)(waterBg2WaveTimer - 1));
        if (waterBg2WaveTimer == 0)
        {
            waterBg2WaveTimer = RoomFxRomData.Water.Bg2WavePhaseDuration;
            waterBg2WavePhase =
                (waterBg2WavePhase + 1) % RoomFxRomData.Water.WaveDisplacementCount;
        }
    }

    /// <summary>Advances rise and tide state before publishing the current liquid surface.</summary>
    private void AdvanceLiquidMotion(ushort randomNumber, bool mainGameLoopCarry)
    {
        StepLiquidRise(randomNumber, mainGameLoopCarry);
        StepLiquidTide();
        CurrentYPosition = ComputeTidalYPosition();
    }

    /// <summary>
    /// Ports the visible outputs of <c>$88:B3B0</c> and <c>$88:B4D5</c>. The liquid
    /// surface and BG3 plane follow the room camera, while one of two sixteen-scanline BG2
    /// waveforms rotates at the cadence selected by the record's liquid-options byte.
    /// </summary>
    private void StepLavaAcid(
        Position16<RoomPixels> camera,
        ushort randomNumber,
        bool mainGameLoopCarry,
        bool liquidMotionAlreadyAdvanced)
    {
        if (!liquidMotionAlreadyAdvanced)
            AdvanceLiquidMotion(randomNumber, mainGameLoopCarry);
        waterSurfaceScreenY = unchecked((short)(CurrentYPosition - camera.Y));
        // Lava/acid $88:B3B0 uses the same surface-relative BG3VOFS equation as water
        // $88:C48E. Zeroing it anchored the animated characters to the screen instead of
        // beginning their first texture row immediately below the liquid boundary.
        Scroll = camera.AsBg3Scroll().WithY(ComputeLiquidVerticalScroll(CurrentYPosition, camera.Y));

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
    private void StepLiquidRise(ushort randomNumber, bool mainGameLoopCarry)
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
                PublishRisingLiquidFeedback(randomNumber, mainGameLoopCarry);
                Timer = unchecked((ushort)(Timer - 1));
                if (Timer == 0)
                    liquidRisePhase = LiquidRisePhase.Moving;
                return;

            case LiquidRisePhase.Moving:
                PublishRisingLiquidFeedback(randomNumber, mainGameLoopCarry);
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
    private void PublishRisingLiquidFeedback(ushort randomNumber, bool mainGameLoopCarry)
    {
        // Water's $88:C44C/$C458 callbacks only wait/move. Lava and acid's
        // parallel callbacks additionally request sound and a global earthquake.
        if (Type is not (RoomFxType.Lava or RoomFxType.Acid))
            return;
        HandleEarthquakeSoundEffect(randomNumber, mainGameLoopCarry);
        EarthquakeRequest = new RoomFxEarthquakeRequest(
            RoomFxRomData.Earthquake.RisingLiquidType,
            RoomFxRomData.Earthquake.RisingLiquidTimerBits);
    }

    /// <summary>
    /// Ports the signed timer and eight-entry loop at <c>$88:B21D-$B254</c>.
    /// All eight native sound words are library-two Earthquake; the negative ninth
    /// word resets selection to zero. Constant sound selection and the index bound
    /// replace those words; the independently retained rhythm supplies only delays.
    /// </summary>
    /// <param name="randomNumber">Current RNG word, sampled without advancing it; its low two bits add zero through three updates to the next earthquake-sound delay.</param>
    /// <param name="mainGameLoopCarry">
    /// Carry entering the routine. <c>QueueSound_Lib2_Max6</c> restores it with <c>PLP</c>,
    /// so <c>ADC .baseTimer</c> at $88:B245 adds it. Both callers' HDMA objects are the first
    /// their FX spawns after the door's <c>Delete_HDMAObjects</c>, so they run in slot zero,
    /// which inherits <see cref="Bank80SystemState.MainGameLoopCarry"/>; a later slot would
    /// see the clear carry from <c>CPX #$0C</c>.
    /// </param>
    private void HandleEarthquakeSoundEffect(ushort randomNumber, bool mainGameLoopCarry)
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
            MaximumQueued: 6,
            SoundSuppressed: audioPowerBomb?.IsActive == true));
        earthquakeSoundTimer = unchecked((ushort)(
            baseTimers[earthquakeSoundSequenceIndex] + (randomNumber & 3) + (mainGameLoopCarry ? 1 : 0)));
        earthquakeSoundSequenceIndex++;
    }

    /// <summary>The statue HDMA pre-instructions call the same $88:B21D earthquake sound owner.</summary>
    internal void PublishStatueEarthquakeSound(ushort randomNumber, bool mainGameLoopCarry) =>
        HandleEarthquakeSoundEffect(randomNumber, mainGameLoopCarry);

    /// <summary>Ports <c>RaiseOrLowerFx</c> at $88:868C for the shared liquid words.</summary>
    private bool AdvanceBaseYToTarget()
    {
        if ((TargetYPosition & 0x8000) != 0)
            return true;

        short velocity = unchecked((short)PackedYVelocity);
        int delta = SnesSixteenSixteen.FromEightEight(velocity);
        uint fixedPosition = SnesSixteenSixteen.Compose(BaseYPosition, baseYSubposition);
        fixedPosition = unchecked(fixedPosition + (uint)delta);
        ushort candidate = SnesFixedPosition.WholeOf(unchecked((int)fixedPosition));
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
    private void StepLiquidTide()
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

        // $88:B2DF/$B316 index the negative-cosine prefix, not the sine origin.
        short sample = EnemyTrigonometryTables.SignedNegativeCosineWord(SnesLittleEndian.HighByte(tidePhase));
        tideFixedOffset = SnesSixteenSixteen.FromEightEight(sample * scale);
        tidePhase = unchecked((ushort)(
            tidePhase + (sample >= 0 ? positiveDelta : negativeDelta)));
    }

    /// <summary>Adds signed fixed-point tide displacement to the base liquid surface.</summary>
    private ushort ComputeTidalYPosition()
    {
        uint baseFixed = SnesSixteenSixteen.Compose(BaseYPosition, baseYSubposition);
        return SnesFixedPosition.WholeOf(unchecked((int)(baseFixed + (uint)tideFixedOffset)));
    }

    /// <summary>Whether the FX options select vertical BG2 displacement for lava or acid.</summary>
    private bool UsesLavaAcidVerticalWave =>
        (LiquidOptions & RoomFxRomData.LavaAcid.VerticalBg2WaveOption) != 0;

    /// <summary>Whether the FX options select horizontal BG2 displacement for lava or acid.</summary>
    private bool UsesLavaAcidHorizontalWave =>
        (LiquidOptions & RoomFxRomData.LavaAcid.HorizontalBg2WaveOption) != 0;

    /// <summary>Computes native BG3 vertical scroll from the liquid surface relative to camera Y.</summary>
    private static ushort ComputeLiquidVerticalScroll(ushort surfaceY, ushort cameraY)
    {
        if (unchecked((short)surfaceY) < 0)
            return 0;
        short relative = unchecked((short)(surfaceY - cameraY));
        if (relative <= 0)
            return unchecked((ushort)(((relative ^ 0x001f) & 0x001f) | 0x0100));
        return relative < 0x0100
            ? unchecked((ushort)(byte)~relative)
            : (ushort)0;
    }

    /// <summary>Clears prior-room effect state before a new door's FX record is selected.</summary>
    private void Reset()
    {
        animatedTiles.Reset();
        Type = RoomFxType.None;
        BaseYPosition = 0;
        TargetYPosition = 0;
        PackedYVelocity = 0;
        Timer = 0;
        LayerBlendConfiguration = default;
        Scroll = Position16<Bg3Pixels>.Zero;
        drift = Offset16<Bg3Subpixels>.Zero;
        horizontalVelocity = 0;
        previousCamera = Position16<RoomPixels>.Zero;
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
        lavaAcidBg3PreInstructionInstalled = false;
        liquidHdmaObjectsDeleted = false;
        lavaSoundTimer = 0;
        soundRequests.Clear();
        EarthquakeRequest = null;
        earthquakeSoundTimer = 0;
        earthquakeSoundSequenceIndex = 0;
    }

    /// <summary>
    /// Deletes the liquid's scroll HDMA objects, as zeroing their channel words does: no
    /// later pass moves the liquid, queues its ambient sound or (for lava/acid) swaps the
    /// RNG bytes. Effects without HDMA-owned liquid motion are unaffected.
    /// </summary>
    public void DeleteLiquidHdmaObjects() => liquidHdmaObjectsDeleted = true;

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
    byte MaximumQueued,
    bool SoundSuppressed = false);

/// <summary>One bank-$88 request for the global room-shake type and timer bits.</summary>
public readonly record struct RoomFxEarthquakeRequest(
    ushort Type,
    ushort TimerBits);

/// <summary>Immutable gameplay BG3 values published by one accepted NMI.</summary>
public readonly record struct RoomLayer3FxRenderSnapshot(
    RoomFxType Type,
    LayerBlendingConfiguration LayerBlendConfiguration,
    Position16<Bg3Pixels> Scroll,
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

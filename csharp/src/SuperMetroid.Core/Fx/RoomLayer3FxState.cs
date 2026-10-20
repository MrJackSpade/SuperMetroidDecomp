using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Fx;

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
[RoomLifetime]
public sealed class RoomLayer3FxState
{
    /// <summary>Tracks frame selection for the liquid, rain, fog, and spore tile animations.</summary>
    private readonly RoomFxAnimatedTilesState animatedTiles = new();

    /// <summary>The Fireflea state, COLDATA shadows and installed FX artwork, which outlive each room.</summary>
    private readonly RoomFxSession _session;

    /// <summary>The Fireflea FX state. It is session-lived like the cartridge's WRAM, so a room load does not reset it.</summary>
    public FirefleaFxState Fireflea => _session.Fireflea;

    /// <summary>The shared COLDATA shadows. They are session-lived, so they keep the last writer's values across rooms.</summary>
    public FixedColorState FixedColors => _session.FixedColors;

    /// <summary>Current host-owned liquid/rain frame art; never stored in debugger state.</summary>
    public RoomFxAnimatedTileAtlas? AnimatedTileArtwork => _session.AnimatedTileArtwork;

    /// <summary>Current host-owned BG3 effect tilemaps; never stored in debugger state.</summary>
    public RoomFxLayer3TilemapCatalog? Layer3Tilemaps => _session.Layer3Tilemaps;

    /// <summary>The effect whose authored page the room's BG3 plane shows, chosen when the room loaded; null when it shows none.</summary>
    public RoomFxType? EffectPageType { get; private set; }

    /// <summary>The words of the page the room's BG3 plane shows (#1348); empty when it shows none.</summary>
    public ushort[] EffectPageWords()
    {
        if (EffectPageType is not { } type)
            return [];
        ReadOnlySpan<byte> page = (Layer3Tilemaps ?? throw new InvalidOperationException(
            "A room effect's page requires installed layer-3 tilemaps.")).Resolve(type).Span;
        var words = new ushort[page.Length / sizeof(ushort)];
        for (int word = 0; word < words.Length; word++)
            words[word] = SnesLittleEndian.Word(page[word * 2], page[word * 2 + 1]);
        return words;
    }

    /// <summary>Current host-owned room-FX blend colors; never stored in debugger state.</summary>
    public RoomFxPaletteBlendCatalog? PaletteBlendColors => _session.PaletteBlendColors;
    /// <summary>
    /// Rain, fog, and spores texture drift: one 8.8 accumulator per axis. Its signed high
    /// byte offsets BG3 scroll before the next velocity step.
    /// </summary>
    internal Offset16<Bg3Subpixels> drift;
    /// <summary>
    /// Rain's horizontal texture drift per frame in 8.8 pixels, chosen from the random number when the room loads;
    /// only X is set. The fall is <see cref="RainFallVelocity"/>.
    /// </summary>
    private Offset16<Bg3Subpixels> rainWindVelocity;
    /// <summary>Previous layer-one camera point, used to make rain follow camera movement.</summary>
    internal Position16<RoomPixels> previousCamera;
    /// <summary>How many tide-phase units make one full turn of the negative-cosine lookup.</summary>
    private const int TidePhasePeriod = 65536;
    /// <summary>
    /// Negative-cosine lookup phase for the liquid tide, 0 through 65535: its whole 1/256 turns select the sample, and
    /// it returns to zero after a full turn through <see cref="AdvanceTidePhase"/>.
    /// </summary>
    internal int tidePhase;
    /// <summary>Signed fixed-point tide displacement added to the base surface.</summary>
    internal int tideFixedOffset;
    /// <summary>
    /// Water's BG3 surface drift: an 8.8 horizontal accumulator whose signed whole pixels offset the surface
    /// texture. Only X moves.
    /// </summary>
    private Offset16<Bg3Subpixels> waterDrift;
    /// <summary>Countdown before advancing the water BG3 waveform.</summary>
    private NativeCountdown waterBg3WaveTimer;
    /// <summary>Countdown for the optional water BG2 horizontal wave.</summary>
    private NativeCountdown waterBg2WaveTimer;
    /// <summary>Selected BG3 water displacement frame.</summary>
    private int waterBg3WavePhase;
    /// <summary>Selected BG2 water displacement frame.</summary>
    private int waterBg2WavePhase;
    /// <summary>Liquid surface relative to the camera, used to clip the effect plane.</summary>
    private short waterSurfaceScreenY;
    /// <summary>Countdown for lava or acid's optional BG2 wave.</summary>
    private NativeCountdown lavaAcidBg2WaveTimer;
    /// <summary>Selected lava or acid BG2 wave displacement frame.</summary>
    private int lavaAcidBg2WavePhase;
    /// <summary>
    /// Set after the first liquid BG3 HDMA pass installs its pre-instruction. Later passes
    /// run it before main-loop RNG; water uses this latch despite its older lava/acid name.
    /// </summary>
    internal bool lavaAcidBg3PreInstructionInstalled;
    /// <summary>
    /// Records direct deletion of liquid scroll HDMA objects at $A9:8C0C, which stops
    /// their per-frame updates until the next room load.
    /// </summary>
    internal bool liquidHdmaObjectsDeleted;

    /// <summary>HDMA-object variable two of the lava/acid BG3 object: the ambient sound timer.</summary>
    private NativeCountdown lavaSoundTimer;
    /// <summary>Current liquid rise or fall state selected by the FX record.</summary>
    private LiquidRisePhase liquidRisePhase;
    /// <summary>Audio requests emitted in native order during the current effect pass.</summary>
    private readonly List<RoomFxSoundRequest> soundRequests = [];
    /// <summary>Rebound sound recipient for effect requests after a debugger restore.</summary>
    [NonSerialized] private SamusPowerBombExplosionState? audioPowerBomb;
    /// <summary>Countdown between quake sound requests in earthquake FX rooms.</summary>
    internal NativeCountdown earthquakeSoundTimer;
    /// <summary>Position in the room earthquake sound sequence.</summary>
    internal int earthquakeSoundSequenceIndex;

    /// <summary>The literal FX type byte selected from the active room/door record.</summary>
    public RoomFxType Type { get; internal set; }

    /// <summary>
    /// The liquid surface before tide displacement, a room Y with subpixels: the FX record's base whole pixels (a room
    /// load starts the fraction at half a pixel), moved toward <see cref="TargetSurface"/> by <see cref="YVelocity"/>.
    /// </summary>
    public SnesFixedPosition BaseY { get; internal set; }

    /// <summary>
    /// Where a rising or falling liquid stops, at X zero. A Y from 32768 up disables the motion, as the cartridge's
    /// sign test does.
    /// </summary>
    public Position16<RoomPixels> TargetSurface { get; private set; }

    /// <summary>The liquid's signed 8.8 vertical velocity in room pixels per update while it moves toward its target.</summary>
    public SnesSignedEightEight YVelocity { get; private set; }

    /// <summary>Native zero-extended FX timer byte loaded from record offset eight.</summary>
    public NativeCountdown Timer { get; private set; }

    /// <summary>FX B layer-blending selector installed by the effect pre-instruction.</summary>
    public LayerBlendingConfiguration LayerBlendConfiguration { get; private set; } = LayerBlendingConfiguration.Unconfigured;

    /// <summary>Live BG3 scroll-register shadows (BG3HOFS/BG3VOFS).</summary>
    public Position16<Bg3Pixels> Scroll { get; private set; }

    /// <summary>Whether the translated effect supplies a gameplay-region BG3 plane.</summary>
    public bool IsRenderable => Type is
        RoomFxType.Lava or RoomFxType.Acid or RoomFxType.Water or RoomFxType.TourianEntranceStatue or
        RoomFxType.Rain or RoomFxType.Fog or RoomFxType.Spores;

    /// <summary>
    /// The live liquid surface after rising and tide processing, at X zero. A Y from 32768 up (65535 with no liquid)
    /// lies above the room, as the cartridge's sign test reads it.
    /// </summary>
    public Position16<RoomPixels> Surface { get; internal set; } = NoSurface;

    /// <summary>The surface published while no liquid is loaded.</summary>
    internal static Position16<RoomPixels> NoSurface { get; } = new(0, ushort.MaxValue);

    /// <summary>The liquid options the FX record selected, as PLMs have since changed them.</summary>
    public RoomLiquidOptions LiquidOptions { get; internal set; }

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

    /// <summary>Builds one room's layer-3 FX from the current door's native FX record.</summary>
    /// <param name="session">The Fireflea state, COLDATA shadows and installed FX artwork, which outlive each room.</param>
    /// <param name="cgram">CGRAM receiving the record's palette blend.</param>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the FX record.</param>
    /// <param name="randomNumber">The session random number rain reads.</param>
    /// <param name="room">The room, for rooms that suppress the earthquake sound.</param>
    public static RoomLayer3FxState ForRoom(
        RoomFxSession session,
        IPaletteColors cgram,
        RoomFxDefinition? fx,
        CartridgeDoorHeader? door,
        int randomNumber,
        RoomHeaderDefinition? room = null) =>
        ForRecord(session, cgram, new RoomFxLoadSource { Fx = fx, Door = door, RandomNumber = randomNumber, Room = room, SuppliedDefinition = null });

    /// <summary>Builds one room's layer-3 FX from a load source, which may supply its record directly.</summary>
    /// <param name="session">The Fireflea state, COLDATA shadows and installed FX artwork, which outlive each room.</param>
    /// <param name="cgram">CGRAM receiving the record's palette blend.</param>
    /// <param name="source">The FX record's source.</param>
    internal static RoomLayer3FxState ForRecord(RoomFxSession session, IPaletteColors cgram, RoomFxLoadSource source)
    {
        var fx = new RoomLayer3FxState(session);
        fx.SelectRecord(cgram, source);
        return fx;
    }

    /// <summary>
    /// Selects the door's FX record and initializes the liquid and palette state shared by room effects and PLMs.
    /// The BG3 plane the room shows is cleared under the HUD with the effect's page from row 31 (<see cref="Layer3Plane"/>).
    /// </summary>
    private void SelectRecord(IPaletteColors cgram, RoomFxLoadSource source)
    {
        Ensure.NotNull(cgram);

        if (source.Room is { } room && RoomFxRomData.Earthquake.SoundSuppressedRooms.Contains(room))
            earthquakeSoundTimer = NativeCountdown.Start(ushort.MaxValue);
        if ((source.SuppliedDefinition ?? source.Fx?.Select(source.Door)) is not { } definition)
            return;

        BaseY = BaseY.WithWhole(definition.BaseYPosition);
        TargetSurface = new(0, definition.TargetYPosition);
        YVelocity = new(definition.PackedYVelocity);
        Timer = NativeCountdown.FromRaw(definition.Timer);
        LiquidOptions = RoomLiquidOptionsCodec.FromRecordByte(definition.LiquidOptions);
        Surface = new(0, BaseY.Whole);

        Type = RoomFxTypes.FromCartridge(
            definition.Type,
            $"room FX {source.Fx?.Name}");
        LayerBlendConfiguration = LayerBlendingConfigurations.FromCartridge(
            definition.Layer3LayerBlend,
            $"room FX {source.Fx?.Name}");
        animatedTiles.Load(Type);
        ApplyPaletteBlend(cgram, definition.PaletteBlend);

        if (Type == RoomFxType.Fireflea)
            Fireflea.Initialize();
        if (!IsRenderable)
            return;

        // The room shows this effect's page under the HUD until the next room.
        EffectPageType = Type;

        if (RoomFxTypes.UsesWater(Type))
        {
            // Both spawned HDMA objects execute their phase initializer on the first
            // handler pass. A one-frame timer reproduces that first-call rotation.
            waterBg3WaveTimer = NativeCountdown.Start(1);
            waterBg2WaveTimer = NativeCountdown.Start(1);
        }
        else if (Type is RoomFxType.Lava or RoomFxType.Acid)
        {
            // `$88:B4CE` seeds timer B to one, so the first HDMA-object pass rotates A
            // from zero to $1E and then reloads either four or six. Expressing the already-
            // resolved waveform phase as zero produces the same first visible table.
            lavaAcidBg2WaveTimer = NativeCountdown.FromRaw(UsesLavaAcidVerticalWave
                ? RoomFxRomData.LavaAcid.VerticalWavePhaseDuration
                : RoomFxRomData.LavaAcid.HorizontalWavePhaseDuration);
            lavaAcidBg2WavePhase = 0;
        }
        else if (Type == RoomFxType.Rain)
        {
            // `$88:D981` masks the random word with six *after* shifting it once,
            // producing byte offsets 0/2/4/6 into a word table. Expressed as a C#
            // element index that is bits two and three of the original random word.
            rainWindVelocity = new Offset16<Bg3Subpixels>(
                unchecked((short)RoomFxRomData.Rain.HorizontalVelocity((source.RandomNumber >> 2) & 3)), 0);
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
                lavaSoundTimer = NativeCountdown.Start(RoomFxRomData.LavaAcid.AmbientSoundPeriod);
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
        int random = system.RandomNumber;
        AdvanceLiquidMotion(random, system.MainGameLoopCarry);
        QueueLavaAmbientSound(random);
        system.SwapRandomBytes();
    }

    /// <summary>
    /// Ports <c>$88:B421-$B446</c>: while a lava surface's Y is not negative, its timer
    /// counts down and, on reaching zero, reloads and queues a random ambient sound.
    /// Acid shares the pre-instruction but not this sound.
    /// </summary>
    private void QueueLavaAmbientSound(int random)
    {
        if (Type != RoomFxType.Lava || unchecked((short)Surface.Y) < 0)
            return;
        lavaSoundTimer = lavaSoundTimer.Decrement().Remaining;
        if (!lavaSoundTimer.IsZero)
            return;
        lavaSoundTimer = NativeCountdown.Start(RoomFxRomData.LavaAcid.AmbientSoundPeriod);
        soundRequests.Add(new RoomFxSoundRequest(
            RoomFxRomData.LavaAcid.AmbientSound(random),
            RoomFxRomData.LavaAcid.AmbientSoundMaximumQueued,
            SoundSuppressed: audioPowerBomb?.IsActive == true));
    }

    /// <summary>Runs the active bank-$88 pre-instruction and rain animtile handler.</summary>
    public void Step(BgArtworkBindings backgrounds, RoomLayer3FxFrame frame)
    {
        Ensure.NotNull(backgrounds);
        audioPowerBomb = frame.PowerBomb;
        if (!frame.LiquidMotionAlreadyAdvanced)
        {
            soundRequests.Clear();
            EarthquakeRequest = null;
        }
        if (Type == RoomFxType.Fireflea)
        {
            LayerBlendConfiguration = LayerBlendingConfiguration.Fireflea;
            Fireflea.Step(FixedColors, frame.TimeIsFrozen, frame.FirefleaDarknessLevel);
            return;
        }
        if (!IsRenderable || frame.TimeIsFrozen)
            return;

        // The shared bank-$87 handler runs independently of the bank-$88 HDMA
        // pre-instruction. In particular, lava/acid need this transfer before their BG3
        // tilemap can name anything other than stale standard-HUD characters.
        animatedTiles.Step(backgrounds, artwork: AnimatedTileArtwork);

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
            drift += rainWindVelocity + RainFallVelocity;
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
        Surface = new(0, BaseY.Whole);
        waterSurfaceScreenY = unchecked((short)(Surface.Y - camera.Y));
        Scroll = camera.AsBg3Scroll().WithY(ComputeLiquidVerticalScroll(Surface.Y, camera.Y));
    }

    /// <summary>Captures values visible alongside the current accepted NMI's OAM upload.</summary>
    public RoomLayer3FxRenderSnapshot? CaptureForDisplay() => IsRenderable
        ? new RoomLayer3FxRenderSnapshot(
            Type,
            LayerBlendConfiguration,
            Scroll,
            Surface,
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
                liquid.ConfigureWater(Surface.Y, LiquidOptions, Type);
                return;
            case RoomFxType.Lava:
                liquid.ConfigureLavaAcid(Surface.Y);
                return;
            case RoomFxType.Acid:
                liquid.ConfigureLavaAcid(Surface.Y, acid: true);
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
            BaseY = BaseY.WithWhole(baseY);
        if (targetYPosition is ushort targetY)
            TargetSurface = new(0, targetY);
        if (packedYVelocity is ushort velocity)
            YVelocity = new(velocity);
        if (timer is ushort newTimer)
            Timer = NativeCountdown.FromRaw(newTimer);
    }

    /// <summary>
    /// $89:AB02 LoadFxEntry reloads motion, blending and three colors, but does not restart
    /// HDMA, animated tiles, or the liquid's current motion phase as a room load would.
    /// </summary>
    internal LayerBlendingConfiguration ApplyEntry(IPaletteColors cgram,
        RoomFxRecordDefinition definition)
    {
        Ensure.NotNull(definition);
        BaseY = BaseY.WithWhole(definition.BaseYPosition);
        TargetSurface = new(0, definition.TargetYPosition);
        YVelocity = new(definition.PackedYVelocity);
        Timer = NativeCountdown.FromRaw(definition.Timer);
        LiquidOptions = RoomLiquidOptionsCodec.FromRecordByte(definition.LiquidOptions);
        LayerBlendConfiguration = LayerBlendingConfigurations.FromCartridge(
            definition.Layer3LayerBlend, "LoadFxEntry");
        ApplyPaletteBlend(cgram, definition.PaletteBlend);
        return LayerBlendingConfigurations.FromCartridge(
            definition.DefaultLayerBlend, "LoadFxEntry");
    }

    /// <summary>Applies the selected FX blend colors from installed presentation data.</summary>
    private void ApplyPaletteBlend(IPaletteColors cgram, byte selection)
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
        LiquidOptions &= ~RoomLiquidOptions.WaterPhysicsDisabledOrHorizontalHeatWave;

    /// <summary>
    /// Ports the observable static-water path of <c>$88:C48E</c> and both circular wave
    /// clocks. Rising/tidal rooms retain their loaded surface words; the presently reported
    /// room has zero velocity and therefore exercises the cartridge's normal static branch.
    /// </summary>
    private void StepWater(
        Position16<RoomPixels> camera,
        int randomNumber,
        bool mainGameLoopCarry,
        bool liquidMotionAlreadyAdvanced)
    {
        if (!liquidMotionAlreadyAdvanced)
            AdvanceLiquidMotion(randomNumber, mainGameLoopCarry);
        waterSurfaceScreenY = unchecked((short)(Surface.Y - camera.Y));
        Scroll = (camera.AsBg3Scroll() + waterDrift.WholePixels()).WithY(ComputeLiquidVerticalScroll(Surface.Y, camera.Y));

        waterBg3WaveTimer = waterBg3WaveTimer.Decrement().Remaining;
        if (waterBg3WaveTimer.IsZero)
        {
            waterBg3WaveTimer = NativeCountdown.Start(RoomFxRomData.Water.Bg3WavePhaseDuration);
            waterBg3WavePhase =
                (waterBg3WavePhase + 1) % RoomFxRomData.Water.WaveDisplacementCount;
        }

        if ((LiquidOptions & RoomLiquidOptions.SurfaceDrift) != 0)
            waterDrift += WaterDriftVelocity;

        // FX $26 spawns only the BG3 water object. Its second HDMA object moves
        // the statue vertically instead of installing the ordinary BG2 X wave.
        if (Type != RoomFxType.Water || (LiquidOptions & RoomLiquidOptions.Bg2Wave) == 0)
            return;
        waterBg2WaveTimer = waterBg2WaveTimer.Decrement().Remaining;
        if (waterBg2WaveTimer.IsZero)
        {
            waterBg2WaveTimer = NativeCountdown.Start(RoomFxRomData.Water.Bg2WavePhaseDuration);
            waterBg2WavePhase =
                (waterBg2WavePhase + 1) % RoomFxRomData.Water.WaveDisplacementCount;
        }
    }

    /// <summary>Advances rise and tide state before publishing the current liquid surface.</summary>
    private void AdvanceLiquidMotion(int randomNumber, bool mainGameLoopCarry)
    {
        StepLiquidRise(randomNumber, mainGameLoopCarry);
        StepLiquidTide();
        Surface = new(0, ComputeTidalYPosition());
    }

    /// <summary>
    /// Ports the visible outputs of <c>$88:B3B0</c> and <c>$88:B4D5</c>. The liquid
    /// surface and BG3 plane follow the room camera, while one of two sixteen-scanline BG2
    /// waveforms rotates at the cadence selected by the record's liquid-options byte.
    /// </summary>
    private void StepLavaAcid(
        Position16<RoomPixels> camera,
        int randomNumber,
        bool mainGameLoopCarry,
        bool liquidMotionAlreadyAdvanced)
    {
        if (!liquidMotionAlreadyAdvanced)
            AdvanceLiquidMotion(randomNumber, mainGameLoopCarry);
        waterSurfaceScreenY = unchecked((short)(Surface.Y - camera.Y));
        // Lava/acid $88:B3B0 uses the same surface-relative BG3VOFS equation as water
        // $88:C48E. Zeroing it anchored the animated characters to the screen instead of
        // beginning their first texture row immediately below the liquid boundary.
        Scroll = camera.AsBg3Scroll().WithY(ComputeLiquidVerticalScroll(Surface.Y, camera.Y));

        if (!UsesLavaAcidVerticalWave && !UsesLavaAcidHorizontalWave)
            return;
        lavaAcidBg2WaveTimer = lavaAcidBg2WaveTimer.Decrement().Remaining;
        if (!lavaAcidBg2WaveTimer.IsZero)
            return;

        lavaAcidBg2WavePhase =
            (lavaAcidBg2WavePhase - 1 + RoomFxRomData.LavaAcid.WaveDisplacementCount) %
            RoomFxRomData.LavaAcid.WaveDisplacementCount;
        lavaAcidBg2WaveTimer = NativeCountdown.FromRaw(UsesLavaAcidVerticalWave
            ? RoomFxRomData.LavaAcid.VerticalWavePhaseDuration
            : RoomFxRomData.LavaAcid.HorizontalWavePhaseDuration);
    }

    /// <summary>
    /// Executes the parallel native callback trios at $88:B343/$B367/$B382 for lava/acid
    /// and $88:C428/$C44C/$C458 for water. A nonzero velocity first arms the wait phase,
    /// the record timer then expires, and only then does the signed 8.8 velocity move the
    /// 16.16 base position toward the target. Door-specific Norfair entries rely on this
    /// sequence to raise lava into the visible viewport.
    /// </summary>
    private void StepLiquidRise(int randomNumber, bool mainGameLoopCarry)
    {
        switch (liquidRisePhase)
        {
            case LiquidRisePhase.Dormant:
            {
                short velocity = YVelocity.SignedRawValue;
                bool movesTowardTarget = velocity > 0
                    ? TargetSurface.Y > BaseY.Whole
                    : velocity < 0 && TargetSurface.Y < BaseY.Whole;
                if (movesTowardTarget)
                    liquidRisePhase = LiquidRisePhase.Waiting;
                return;
            }

            case LiquidRisePhase.Waiting:
                PublishRisingLiquidFeedback(randomNumber, mainGameLoopCarry);
                Timer = Timer.Decrement().Remaining;
                if (Timer.IsZero)
                    liquidRisePhase = LiquidRisePhase.Moving;
                return;

            case LiquidRisePhase.Moving:
                PublishRisingLiquidFeedback(randomNumber, mainGameLoopCarry);
                if (AdvanceBaseYToTarget())
                {
                    YVelocity = default;
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
    private void PublishRisingLiquidFeedback(int randomNumber, bool mainGameLoopCarry)
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
    private void HandleEarthquakeSoundEffect(int randomNumber, bool mainGameLoopCarry)
    {
        if (earthquakeSoundTimer.IsNegative)
            return;

        earthquakeSoundTimer = earthquakeSoundTimer.Decrement().Remaining;
        if (!earthquakeSoundTimer.IsNegative)
            return;

        ReadOnlySpan<ushort> baseTimers =
            RoomFxRomData.Earthquake.RisingLiquidSoundBaseTimers;
        if ((uint)earthquakeSoundSequenceIndex >= (uint)baseTimers.Length)
            earthquakeSoundSequenceIndex = 0;

        soundRequests.Add(new RoomFxSoundRequest(
            SoundEffectLibrary2Sounds.Earthquake,
            MaximumQueued: 6,
            SoundSuppressed: audioPowerBomb?.IsActive == true));
        earthquakeSoundTimer = NativeCountdown.FromRaw(unchecked((ushort)(
            baseTimers[earthquakeSoundSequenceIndex] + (randomNumber & 3) + (mainGameLoopCarry ? 1 : 0))));
        earthquakeSoundSequenceIndex++;
    }

    /// <summary>The statue HDMA pre-instructions call the same $88:B21D earthquake sound owner.</summary>
    internal void PublishStatueEarthquakeSound(int randomNumber, bool mainGameLoopCarry) =>
        HandleEarthquakeSoundEffect(randomNumber, mainGameLoopCarry);

    /// <summary>Ports <c>RaiseOrLowerFx</c> at $88:868C for the shared liquid words.</summary>
    private bool AdvanceBaseYToTarget()
    {
        if (unchecked((short)TargetSurface.Y) < 0)
            return true;

        short velocity = YVelocity.SignedRawValue;
        int delta = SnesSixteenSixteen.FromEightEight(velocity);
        uint fixedPosition = unchecked(BaseY.RawValue + (uint)delta);
        ushort candidate = SnesFixedPosition.WholeOf(unchecked((int)fixedPosition));
        BaseY = SnesFixedPosition.FromRaw(fixedPosition);

        if (velocity >= 0)
        {
            if (unchecked((short)candidate) < 0)
                candidate = ushort.MaxValue;
            BaseY = BaseY.WithWhole(candidate);
            if (candidate <= TargetSurface.Y)
                return false;
        }
        else
        {
            if (unchecked((short)candidate) < 0)
                candidate = 0;
            BaseY = BaseY.WithWhole(candidate);
            if (candidate > TargetSurface.Y)
                return false;
        }

        BaseY = new SnesFixedPosition(TargetSurface.Y, 0);
        return true;
    }

    /// <summary>
    /// Ports <c>FxHandleTide</c> at $88:B2C9. Native writes the scaled sine word starting
    /// one byte into the 16.16 offset pair, equivalent to the eight-bit shift below. The
    /// asymmetric phase deltas deliberately spend different durations in each half-wave.
    /// </summary>
    internal void StepLiquidTide()
    {
        int scale;
        ushort positiveDelta;
        ushort negativeDelta;
        if ((LiquidOptions & RoomLiquidOptions.SmallTide) != 0)
        {
            scale = RoomFxRomData.LiquidTide.SmallTideScale;
            positiveDelta = RoomFxRomData.LiquidTide.SmallTidePositivePhaseDelta;
            negativeDelta = RoomFxRomData.LiquidTide.SmallTideNegativePhaseDelta;
        }
        else if ((LiquidOptions & RoomLiquidOptions.LargeTide) != 0)
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
        short sample = EnemyTrigonometryTables.SignedNegativeCosineWord(checked((byte)SnesEightEight.WholeOf(tidePhase)));
        tideFixedOffset = SnesSixteenSixteen.FromEightEight(sample * scale);
        AdvanceTidePhase(sample >= 0 ? positiveDelta : negativeDelta);
    }

    /// <summary>Moves the tide phase on by <paramref name="delta"/> units, returning to zero after a full turn.</summary>
    private void AdvanceTidePhase(int delta) => tidePhase = (tidePhase + delta) % TidePhasePeriod;

    /// <summary>Adds signed fixed-point tide displacement to the base liquid surface.</summary>
    private ushort ComputeTidalYPosition()
    {
        uint baseFixed = BaseY.RawValue;
        return SnesFixedPosition.WholeOf(unchecked((int)(baseFixed + (uint)tideFixedOffset)));
    }

    /// <summary>Whether the FX options select vertical BG2 displacement for lava or acid.</summary>
    private bool UsesLavaAcidVerticalWave =>
        (LiquidOptions & RoomLiquidOptions.Bg2Wave) != 0;

    /// <summary>Whether the FX options select horizontal BG2 displacement for lava or acid.</summary>
    private bool UsesLavaAcidHorizontalWave =>
        (LiquidOptions & RoomLiquidOptions.WaterPhysicsDisabledOrHorizontalHeatWave) != 0;

    /// <summary>Computes native BG3 vertical scroll from the liquid surface relative to camera Y.</summary>
    private static ushort ComputeLiquidVerticalScroll(ushort surfaceY, ushort cameraY)
    {
        if (unchecked((short)surfaceY) < 0)
            return 0;
        short relative = unchecked((short)(surfaceY - cameraY));
        if (relative <= 0)
            return unchecked((ushort)(((relative ^ 31) & 31) | 256));
        return relative < 256
            ? unchecked((byte)~relative)
            : (ushort)0;
    }

    /// <summary>Creates one room's effect state, with no FX record selected, over the session's carried state.</summary>
    /// <param name="session">The Fireflea state, COLDATA shadows and installed FX artwork, which outlive each room.</param>
    public RoomLayer3FxState(RoomFxSession session)
    {
        _session = Ensure.NotNull(session);
        Type = RoomFxType.None;
        TargetSurface = Position16<RoomPixels>.Zero;
        YVelocity = default;
        Timer = NativeCountdown.Start(0);
        LayerBlendConfiguration = default;
        Scroll = Position16<Bg3Pixels>.Zero;
        drift = Offset16<Bg3Subpixels>.Zero;
        rainWindVelocity = Offset16<Bg3Subpixels>.Zero;
        previousCamera = Position16<RoomPixels>.Zero;
        // Half a pixel: the liquid starts centred on its whole position.
        BaseY = new SnesFixedPosition(0, 1 << 15);
        tidePhase = 0;
        tideFixedOffset = 0;
        Surface = NoSurface;
        LiquidOptions = RoomLiquidOptions.None;
        waterDrift = Offset16<Bg3Subpixels>.Zero;
        waterBg2WaveTimer = NativeCountdown.Start(0);
        waterBg3WaveTimer = NativeCountdown.Start(0);
        waterBg3WavePhase = waterBg2WavePhase = 0;
        waterSurfaceScreenY = short.MaxValue;
        lavaAcidBg2WaveTimer = NativeCountdown.Start(0);
        lavaAcidBg2WavePhase = 0;
        liquidRisePhase = LiquidRisePhase.Dormant;
        lavaAcidBg3PreInstructionInstalled = false;
        liquidHdmaObjectsDeleted = false;
        lavaSoundTimer = NativeCountdown.Start(0);
        EarthquakeRequest = null;
        earthquakeSoundTimer = NativeCountdown.Start(0);
        earthquakeSoundSequenceIndex = 0;
    }

    /// <summary>
    /// Deletes the liquid's scroll HDMA objects, as zeroing their channel words does: no
    /// later pass moves the liquid, queues its ambient sound or (for lava/acid) swaps the
    /// RNG bytes. Effects without HDMA-owned liquid motion are unaffected.
    /// </summary>
    public void DeleteLiquidHdmaObjects() => liquidHdmaObjectsDeleted = true;

    /// <summary>Rain's texture fall each frame, in 8.8 pixels.</summary>
    private static readonly Offset16<Bg3Subpixels> RainFallVelocity =
        new(0, unchecked(-RoomFxRomData.Rain.VerticalVelocity));

    /// <summary>Water's surface drift each frame its <see cref="RoomLiquidOptions.SurfaceDrift"/> option is set.</summary>
    private static readonly Offset16<Bg3Subpixels> WaterDriftVelocity =
        new(unchecked((short)RoomFxRomData.Water.HorizontalSubscrollVelocity), 0);

    /// <summary>The mutually exclusive callbacks installed in the native rise-function word.</summary>
    private enum LiquidRisePhase
    {
        /// <summary>Waits until signed velocity points from the current base toward the target.</summary>
        Dormant,
        /// <summary>Counts down the room FX timer while publishing rising-liquid sound and shake.</summary>
        Waiting,
        /// <summary>Advances the liquid base each update until it reaches the target and clears velocity.</summary>
        Moving,
    }
}

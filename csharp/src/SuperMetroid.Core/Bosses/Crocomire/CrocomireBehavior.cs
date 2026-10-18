using SuperMetroid.Core.Input;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The Crocomire species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class CrocomireBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly EnemyFrameResultBuilder _frameResults;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Crocomire, EnemyDefinitionId.CrocomireTongue];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new CrocomireEntity(this, slot, common);

    /// <summary>Accesses the active room, enemy slots, and shared RNG.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Moves Crocomire against the arena's collision blocks.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Spawns Crocomire's attacks, dust, smoke, and post-death drops.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes the bridge and wall-break quake effects.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Allocates arena debris and cinematic sprite objects.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;
    /// <summary>Owns Crocomire body, skeleton, and melting tile uploads.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies frame timing and room effects to Crocomire AI.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Resolves Crocomire body and tongue contact with Samus.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Produces the native distributed drops at skeleton collapse.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Owns camera-distance mode changes during Crocomire's arena sequence.</summary>
    private readonly ZebetiteBehavior _zebetiteBehavior;
    /// <summary>Updates boss completion and room-state flags.</summary>
    private readonly EnemyRoomStatus _roomStatus;

    /// <summary>Connects Crocomire AI to room collision, rendering, projectiles, and boss-status owners.</summary>
    internal CrocomireBehavior(CrocomireBehaviorDependencies dependencies)
    {
        _frameResults = dependencies.FrameResults;
        _zebetiteBehavior = dependencies.ZebetiteBehavior;
        _room = dependencies.Room;
        _terrain = dependencies.Terrain;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _spriteObjects = dependencies.SpriteObjects;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _collision = dependencies.Collision;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _roomStatus = dependencies.RoomStatus;
    }

    /// <summary>Body X threshold at which Crocomire begins breaking the arena bridge.</summary>
    private const ushort CrocomireBridgeThreshold = 1600;
    /// <summary>Body X threshold at the spiked wall that starts the death sequence.</summary>
    private const ushort CrocomireSpikeWallThreshold = 768;
    /// <summary>The melt's BG2 distortion step stops growing at 80 pixels (8.8).</summary>
    private const int MaximumCrocomireDistortionStep = 20480;

    /// <summary>The tongue's horizontal offset from the body, in pixels, set when the tongue initializes.</summary>
    private const int CrocomireTongueBodyXOffset = 23;
    /// <summary>The projectile attack's last spawn parameter: its ninth shot, after which Crocomire steps forward.</summary>
    private const int LastProjectileSpawnParameter = 18;
    /// <summary>Camera position tracked during Crocomire's BG2 body and death presentation; its X places the BG2 body.</summary>
    private Position16<RoomPixels> _crocomireCamera;

    /// <summary>Acceleration the sink adds each frame, in 1/256 pixel per frame per frame: three and a half pixels.</summary>
    private const int SinkAccelerationIncrement = 896;
    /// <summary>The sink's acceleration stops growing at forty-eight whole pixels, keeping its fraction.</summary>
    private const int MaximumSinkAccelerationWhole = 48;
    /// <summary>The sink's speed stops growing at three whole pixels per frame, keeping its fraction.</summary>
    private const int MaximumSinkSpeedWhole = 3;
    /// <summary>The rise's acceleration stops growing at thirty-one pixels per frame per frame.</summary>
    private const int MaximumRiseAccelerationWhole = 31;
    /// <summary>The speed a rise keeps when its deceleration would take it below zero: 255/256 pixel per frame.</summary>
    private const int RiseSpeedAfterStalling = 255;
    /// <summary>Starting acceleration of every sink: eight pixels per frame per frame.</summary>
    private const int SinkStartingAccelerationWhole = 8;
    /// <summary>The skeleton's fixed fall through its arc: 224/256 pixel per frame.</summary>
    private static readonly SnesSignedSixteenSixteen SkeletonArcFallSpeed =
        SnesSignedSixteenSixteen.FromRaw(SnesSixteenSixteen.FromEightEight(224));

    /// <summary>Debugger-visible Crocomire owner while the current room contains $DDBF.</summary>
    public CrocomireEnemyState? Crocomire { get; internal set; }

    /// <summary>Typed WRAM extension used by Crocomire's bridge/melting/skeleton graph.</summary>
    public CrocomireDeathState? CrocomireDeath { get; internal set; }

    /// <summary>Delayed music publication from the current Crocomire frame.</summary>
    public CrocomireMusicRequest? LastCrocomireMusicRequest { get; private set; }


    /// <summary>Literal BG2 scroll owned by Crocomire while its extended tilemap is active.</summary>
    public Position16<Bg2Pixels> CrocomireBg2Scroll { get; private set; }

    /// <summary>Most recent library-two sound publication of the current frame.</summary>
    public SoundEffectId? LastCrocomireSoundEffect { get; private set; }

    /// <summary>True after the $640 X threshold starts the native bridge-collapse phase.</summary>
    public bool CrocomireBridgeCollapseStarted { get; private set; }

    /// <summary>Clears Crocomire-owned room state and queued arena mutations on room entry.</summary>
    internal void ResetCrocomireRoomState()
    {
        Crocomire = null;
        CrocomireDeath = null;
        _crocomireCamera = Position16<RoomPixels>.Zero;
        LastCrocomireSoundEffect = null;
        LastCrocomireMusicRequest = null;
        CrocomireBg2Scroll = Position16<Bg2Pixels>.Zero;
        CrocomireBridgeCollapseStarted = false;
    }

    /// <summary>Ports <c>InitAI_Crocomire</c> at $A4:8A5A.</summary>
    internal void InitializeCrocomire(RoomEnemySlot slot)
    {
        _roomStatus.BossId = 6;
        var state = new CrocomireEnemyState(slot);
        slot.SetSpeciesState(state, state.ClearWithRecord);
        Crocomire = state;
        CrocomireDeath = new CrocomireDeathState();
        ClearCrocomireBg2WorkingTilemap();

        if (_room.Services.Bosses.IsDefeated(BossBits.AreaMiniBoss))
        {
            // The dead-room state keeps only Crocomire's skeleton display actor. Property
            // mask $7BFF and these radii/coordinates are literal writes at $A4:8AEA-$8B35.
            slot.Properties = slot.Properties.Replace(
                EnemyProperties.SolidToSamus |
                    EnemyProperties.IgnoreSamusCollision,
                EnemyProperties.IgnoreSamusCollision);
            state.DeathSequenceIndex = CrocomireDeathPhase.DefeatedRoomAdvance;
            StartCrocomireBodyProgram(state, CrocomireBodyProgram.Dead);
            slot.Position = slot.Position.WithWhole(new(576, 144));
            slot.Radius = new(40, 28);
            _room.Services.RoomScroll.Write(0, RoomScrollState.Blue);
            _room.Services.RoomScroll.Write(1, RoomScrollState.Blue);
            _room.Services.RoomScroll.Write(2, RoomScrollState.Blue);
            _room.Services.RoomScroll.Write(3, RoomScrollState.Blue);
            PublishCrocomirePlm(new Position16<RoomBlocks>(32, 3), PlmHeaderId.ClearCrocomireInvisibleWall);
            PublishCrocomirePlm(new Position16<RoomBlocks>(30, 3), PlmHeaderId.ClearCrocomireInvisibleWall);
            PublishCrocomirePlm(new Position16<RoomBlocks>(97, 11), PlmHeaderId.ClearCrocomireBridge);
            TransferCrocomireBg2Words(0, 1024);
            return;
        }

        state.DeathSequenceIndex = CrocomireDeathPhase.Fighting;
        state.ReactionTimer = 0;
        state.FightFunction = CrocomireFightFunction.Sleeping;
        // $A4:8ABA-8ABD, on the living branch only.
        _zebetiteBehavior.CameraDistanceIndex = CameraDistanceMode.BossTracking;
        StartCrocomireBodyProgram(state, CrocomireBodyProgram.Initial);
        slot.ExtraProperties = slot.ExtraProperties.With(
            EnemyExtraProperties.UsesExtendedSpritemap);
        _room.Services.RoomScroll.Write(0, RoomScrollState.RedBoundary);
        _room.Services.RoomScroll.Write(1, RoomScrollState.RedBoundary);

        // The initializer copies seventeen words, not sixteen: X starts at $20 and reaches
        // zero inclusively. Preserve that palette-boundary write because later fades compare
        // the exact target image produced by the cartridge.
        (_room.Artwork?.CrocomireColors ?? throw new InvalidOperationException(
            "Crocomire requires installed color artwork.")).ApplyInitial(_presentation.Cgram);
    }

    /// <summary>Ports <c>InitAI_CrocomireTongue</c> at $A4:F67A.</summary>
    private void InitializeCrocomireTongue(RoomEnemySlot slot)
    {
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaMiniBoss))
        {
            slot.Properties = slot.Properties.Replace(
                EnemyProperties.ProcessInstructions |
                    EnemyProperties.Deleted |
                    EnemyProperties.Invisible,
                EnemyProperties.Deleted | EnemyProperties.Invisible);
            return;
        }

        CrocomireEnemyState state = Crocomire ??
            throw new InvalidOperationException("Crocomire's tongue initialized before its body.");
        state.Tongue = slot;
        StartCrocomireTongueProgram(state, CrocomireTongueProgram.Fight, NativeCountdown.Start(1));
        slot.ExtraProperties = slot.ExtraProperties
            .With(EnemyExtraProperties.UsesExtendedSpritemap)
            .WithUntranslatedBits(
                EnemyExtraPropertyRawBits.CrocomireInitializerBit0400,
                "Crocomire initializer");
        var tongueState = new CrocomireTongueState { BodyXOffset = CrocomireTongueBodyXOffset };
        slot.SetSpeciesState(tongueState, tongueState.ClearWithRecord);
        slot.PaletteIndex = EnemyPaletteBits.Palette7;
    }

    /// <summary>Runs the live-fight phase of <c>MainAI_Crocomire</c> at $A4:8C04.</summary>
    internal void RunCrocomireMain(
        RoomEnemySlot slot,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        CrocomireEnemyState state = RequireCrocomire(slot);
        _crocomireCamera = camera;

        if (state.DeathSequenceIndex == CrocomireDeathPhase.Fighting)
        {
            HandleCrocomireBridgeThreshold(state);
            // The native word write runs even on the frame that starts bridge collapse.
            // Later death phases retain these zones until the skeleton enters the river.
            _room.Services.RoomScroll.Write(CrocomireCameraDefinitions.BridgeLeftScreen,
                samus is not null && unchecked((short)(samus.Position.X.Whole - CrocomireCameraDefinitions.BridgeVisibleSamusX)) >= 0
                    ? RoomScrollState.RedBoundary : RoomScrollState.Blue);
            _room.Services.RoomScroll.Write(CrocomireCameraDefinitions.BridgeScreen, RoomScrollState.Blue);
            // Main state zero always finishes through $A4:8B5B, including the frame in
            // which $8D5E changes the death index to two.
            UpdateCrocomireBg2Scroll(state, includeVerticalPosition: true);
        }
        else
        {
            RunCrocomireDeathSequence(state, samus);
        }

        HandleCrocomireInvisibleWall(slot, state, samus);
        ApplyCrocomireHurtPalette(slot);
    }

    /// <summary>
    /// Ports the state transition at $A4:8D5E. The individual bridge PLMs and subsequent
    /// melting sequence remain a separate translation boundary, but the actor/tongue state
    /// changes occur on the exact native X threshold.
    /// </summary>
    private void HandleCrocomireBridgeThreshold(CrocomireEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        HandleCrocomireBridgeApproach(body);
        if (unchecked((short)(body.Position.X.Whole - CrocomireBridgeThreshold)) < 0)
            return;

        CrocomireBridgeCollapseStarted = true;
        state.DeathSequenceIndex = CrocomireDeathPhase.CrumbleBridgeAndSink;
        StartCrocomireBodyProgram(state, CrocomireBodyProgram.BridgeCollapsed);
        body.Properties = body.Properties.With(EnemyProperties.SolidToSamus);
        state.StartVerticalMotion(SnesEightEight.FromWhole(SinkStartingAccelerationWhole));
        body.Radius = body.Radius.WithY(16);
        LastCrocomireSoundEffect = CrocomireSoundDefinitions.BridgeCollapse;
        CrocomireDeathState death = RequireCrocomireDeath();
        death.BridgeFragmentsSpawned = 0;
        death.AcidSmokeTimer = NativeCountdown.Start(1);
        death.AcidSoundTimer = NativeCountdown.Start(1);

        PublishCrocomireBridgeCollapsePlms();

        if (state.Tongue is { } tongue)
        {
            // The sleep is installed behind the native maximum timer, so it is reached only after 32767 updates.
            StartCrocomireTongueProgram(state, CrocomireTongueProgram.Sleep, NativeCountdown.Start(32767));
            tongue.Properties = tongue.Properties.With(EnemyProperties.Invisible);
        }
    }

    /// <summary>Ports the artificial left-side wall in <c>$A4:8C95</c>.</summary>
    private void HandleCrocomireInvisibleWall(
        RoomEnemySlot body,
        CrocomireEnemyState state,
        SamusState? samus)
    {
        if (samus is null || state.DeathSequenceIndex != CrocomireDeathPhase.Fighting)
            return;
        ushort leftEdge = unchecked((ushort)(
            body.Position.X.Whole - body.Radius.X - samus.Kinematics.Radius.X));
        if (unchecked((short)(leftEdge - samus.Position.X.Whole)) >= 0)
            return;

        _collision.ResolveNormalEnemyTouch(body, samus);
        samus.Position = samus.Position.WithWholeX(leftEdge);
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWhole(new(-4, -1));
    }

    /// <summary>Ports Crocomire's eight-color body hurt flash at $A4:8CCB.</summary>
    internal void ApplyCrocomireHurtPalette(RoomEnemySlot body)
    {
        bool white = !body.FlashTimer.IsZero && (_frameContext._randomEnemyCounter & 2) != 0;
        if (!white)
        {
            (_room.Artwork?.CrocomireColors ?? throw new InvalidDataException(
                "Crocomire hurt palette requires installed artwork.")).ApplyFightBody(_presentation.Cgram);
            return;
        }
        _presentation.BossPalettes.CrocomireFightBody.Fill(Bgr555.White);
    }

    /// <summary>Switches an actor's instruction pointer and resets its timer for the next native list.</summary>
    /// <summary>Starts the tongue's <paramref name="program"/> from its first step once <paramref name="timer"/> expires.</summary>
    private static void StartCrocomireTongueProgram(CrocomireEnemyState state, CrocomireTongueProgram program, NativeCountdown timer)
    {
        RoomEnemySlot tongue = state.Tongue ??
            throw new InvalidOperationException("Crocomire's tongue program started without a tongue.");
        state.TongueProgram = program;
        state.TonguePosition = 0;
        tongue.InstructionTimer = timer;
        tongue.Timer = NativeCountdown.Start(0);
    }

    /// <summary>
    /// Runs the tongue's animation where the enemy instruction interpreter would, with the native timer gate
    /// and new-frame flag.
    /// </summary>
    internal void AdvanceCrocomireTongueAnimation(RoomEnemySlot tongue)
    {
        CrocomireEnemyState state = Crocomire is { } owner && ReferenceEquals(owner.Tongue, tongue)
            ? owner
            : throw new InvalidOperationException("Crocomire's tongue has no body owner.");
        ushort oldTimer = tongue.InstructionTimer.RawValue;
        tongue.InstructionTimer = NativeCountdown.FromRaw(unchecked((ushort)(oldTimer - 1)));
        if (oldTimer != 1)
        {
            tongue.ExtraProperties = tongue.ExtraProperties.Without(EnemyExtraProperties.NewInstructionFrame);
            return;
        }

        int position = state.TonguePosition;
        AnimationAdvance<CrocomireTongueFrame> advance = CrocomireAnimations.Tongue(state.TongueProgram)
            .Advance(ref position, cue => throw new InvalidOperationException($"A Crocomire tongue timeline published cue {cue}."));
        state.TonguePosition = position;
        // A hold is the native sleep: the timer wraps from zero and the last frame stays.
        if (advance.Held)
            return;
        tongue.InstructionTimer = NativeCountdown.FromRaw((ushort)advance.Duration);
        tongue.SpritemapPointer = (ushort)advance.Frame;
        tongue.ExtraProperties = tongue.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
    }

    /// <summary>Starts <paramref name="program"/> on the body from its first step on the next update, as a native list install does.</summary>
    private static void StartCrocomireBodyProgram(CrocomireEnemyState state, CrocomireBodyProgram program)
    {
        state.BodyCursor = AnimationProgramCursor<CrocomireBodyProgram>.Start(program);
        state.Body.InstructionTimer = NativeCountdown.Start(1);
        state.Body.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Finds the Crocomire encounter owning an enemy slot, failing if its room state is absent.</summary>
    private CrocomireEnemyState RequireCrocomire(RoomEnemySlot slot) =>
        Crocomire is { } state && ReferenceEquals(state.Body, slot)
            ? state
            : throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} has no initialized Crocomire body state.");

    /// <summary>Requires the active death-sequence state before processing skeleton or wall instructions.</summary>
    private CrocomireDeathState RequireCrocomireDeath() =>
        CrocomireDeath ?? throw new InvalidOperationException(
            "Crocomire death extension is not initialized for the current room.");

    /// <summary>Registers component-specific bomb hit routines, including the native no-op gates.</summary>
    internal void RegisterCrocomireBombReactions(EnemySharedReactions table)
    {
        // Specials consulted before the selected literal no-op gate, in the original order.
        // Crocomire has no header shot callback; every live component supplies its own bank-$A4 routine. Bomb
        // family $0500 therefore reaches the same no-op counter, dust, or mouth reaction selected by the map.
        table.AddBombSpecial(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire &&
                                  c.Enemy.Definition.ShotAiPointer == 0 && c.UsesExtendedHitboxes &&
                                  c.SelectedShotAi != EnemyInteractionCallback.NormalEnemyShot,
            c => ResolveCrocomireHitboxShot(c.Bomb.Type, c.Bomb.Position.Whole, c.SelectedShotAiPointer));
    }

    /// <summary>Registers Crocomire's owned projectile setup, movement, and instruction handlers.</summary>
    internal void RegisterCrocomireProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            StartCrocomireProjectileFlight(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.CrocomiresProjectile_Setup);

        table.RegisterPreInstruction(context =>
        {
            RunCrocomireProjectileFlight(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.CrocomiresProjectile_Fired);

        table.RegisterPreInstruction(context =>
        {
            RunCrocomireSpikeWallPiece(context.Projectile);
        }, EnemyProjectilePreInstruction.CrocomireSpikeWallPieces);

        table.RegisterPreInstruction(context =>
        {
            RunCrocomireBridgeFragment(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.CrocomireBridgeCrumbling);
    }

    /// <summary>Ports Crocomire's initialization AI.</summary>
    internal void RunCrocomireInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeCrocomire(slot);
    }

    /// <summary>Ports CrocomireTongue's initialization AI.</summary>
    internal void RunCrocomireTongueInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeCrocomireTongue(slot);
    }

    /// <summary>Routes power-bomb hits on each Crocomire component to their authored reactions.</summary>
    internal void RegisterCrocomirePowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.CrocomirePowerBomb)
        {
            ReplacesCommon = (enemy, _) => ResolveCrocomirePowerBombReaction(enemy),
        });
    }

    /// <summary>Routes body, mouth, and auxiliary hitboxes to their distinct shot reactions.</summary>
    internal void RegisterCrocomireShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire &&
                                                     enemy.Definition.ShotAiPointer == 0 ||
                                                     enemy.EnemyDefinitionPointer == EnemyDefinitionId.CrocomireTongue &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.NormalEnemyShot)
        {
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (c.HitboxShotAi == EnemyInteractionCallback.NormalEnemyShot)
                    return false;
                if (!c.UsesExtendedHitboxes)
                    throw new InvalidDataException("Crocomire components require extended-spritemap collision.");
                c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                ResolveCrocomireHitboxShot(c.Projectile.Type, c.Projectile.Position.Whole, c.HitboxShotAiPointer);
                return true;
            },
        });
    }

    /// <summary>Registers Crocomire component contact, including normal enemy touch damage.</summary>
    internal void RegisterCrocomireTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.CrocomireHeaderTouch ||
                               slot.EnemyDefinitionPointer == EnemyDefinitionId.CrocomireTongue &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.NormalEnemyTouch,
            context =>
            {
                if (!context.UsesExtendedHitboxes)
                    throw new InvalidDataException("Crocomire components require extended-spritemap collision.");
                if (context.HitboxTouchAi == EnemyInteractionCallback.CrocomireHeaderTouch)
                    return; // $A4:B950 is a literal RTL.
                if (context.HitboxTouchAi is not (
                        EnemyInteractionCallback.CrocomireClawTouch or EnemyInteractionCallback.NormalEnemyTouch))
                {
                    throw new InvalidDataException(
                        $"Crocomire hitbox touch AI $A4:{context.HitboxTouchAiPointer:X4} is not translated.");
                }

                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
                if (context.HitboxTouchAi == EnemyInteractionCallback.CrocomireClawTouch)
                {
                    CrocomireEnemyState crocomire = Crocomire ??
                        throw new InvalidOperationException("Crocomire claw has no body owner.");
                    crocomire.FightFlags |= CrocomireFightFlags.ClawTouchedSamus;
                    context.Samus.Kinematics.ExtraDisplacement = context.Samus.Kinematics.ExtraDisplacement.WithWholeX(-4);
                }
            });
    }
    /// <summary>Queues a native PLM block change for later room processing.</summary>
    private void PublishCrocomirePlm(Position16<RoomBlocks> block, PlmHeaderId header) =>
        _frameResults.CrocomirePlms.Add(new CrocomirePlmRequest(block, header));

    /// <summary>
    /// Ports the bridge effects in $A4:8D5E, staged at Crocomire X $0600, $0610 and $0620.
    /// Each stage's dust lands one block ahead, at $0600, $0620 and $0630.
    /// </summary>
    private void HandleCrocomireBridgeApproach(RoomEnemySlot body)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        if (unchecked((short)(body.Position.X.Whole - 1536)) < 0)
        {
            death.BridgeDustAt1536Spawned = false;
            death.BridgeBlockAt1568Crumbling = false;
            death.BridgeBlocksAt1584Crumbling = false;
            return;
        }

        if (unchecked((short)(body.Position.X.Whole - 1552)) < 0)
        {
            if (!death.BridgeDustAt1536Spawned)
            {
                death.BridgeDustAt1536Spawned = true;
                _projectiles.SpawnRoomGraphicsDustExplosion(new(1536, 176), MiscDustAnimation.BigDustCloud);
            }
            return;
        }

        // $A4:8DA4-8DDB: one block deep crumbles the first bridge block. The flag
        // resets at $8DDC and $8E2D sit behind comparisons these ranges already passed
        // and never execute.
        if (unchecked((short)(body.Position.X.Whole - 1568)) < 0)
        {
            if (!death.BridgeBlockAt1568Crumbling)
            {
                death.BridgeBlockAt1568Crumbling = true;
                PublishCrocomirePlm(
                    new Position16<RoomBlocks>(97, 11), PlmHeaderId.CrumbleCrocomireBridgeBlock);
                _projectiles.SpawnRoomGraphicsDustExplosion(new(1568, 176), MiscDustAnimation.BigDustCloud);
            }
            return;
        }

        // $A4:8DED-8E2C: two blocks deep crumbles the next pair.
        if (unchecked((short)(body.Position.X.Whole - 1584)) < 0 &&
            !death.BridgeBlocksAt1584Crumbling)
        {
            death.BridgeBlocksAt1584Crumbling = true;
            PublishCrocomirePlm(
                new Position16<RoomBlocks>(98, 11), PlmHeaderId.CrumbleCrocomireBridgeBlock);
            PublishCrocomirePlm(
                new Position16<RoomBlocks>(99, 11), PlmHeaderId.CrumbleCrocomireBridgeBlock);
            _projectiles.SpawnRoomGraphicsDustExplosion(new(1584, 176), MiscDustAnimation.BigDustCloud);
        }
    }

    /// <summary>
    /// Ports <c>Crocomire_8EE5</c>: clear all ten bridge blocks and burst dust.
    /// The ten native PLM operand records at $A4:8EE9 + 8*i exactly use
    /// block X=$61+i, block Y=$0B, and header $B74F for i=0..9. The
    /// inclusive production loop preserves their ascending publication
    /// order; $A4:8F35 starts dust setup, not an eleventh clear record.
    /// The seven dust positions in the pinned NTSC J/U v1.0 ROM follow the
    /// exact bounded rule X=$0600+$0010*i for i=0..6; Y=$00B0 at i=0 or 2
    /// and $00C0 otherwise. Native $A4:8F35-$8FB4 encodes these as ordered
    /// immediate operands, not a contiguous table. All seven pairs and their
    /// order match the local span; there is no eighth dust spawn.
    /// </summary>
    private void PublishCrocomireBridgeCollapsePlms()
    {
        for (byte blockX = 97; blockX <= 106; blockX++)
            PublishCrocomirePlm(
                new Position16<RoomBlocks>(blockX, 11), PlmHeaderId.ClearCrocomireBridgeBlock);

        for (int puff = 0; puff < CrocomireBridgeDustCount; puff++)
        {
            (ushort x, ushort y) = CrocomireBridgeDustPosition(puff);
            _projectiles.SpawnRoomGraphicsDustExplosion(new(x, y), MiscDustAnimation.BigDustCloud);
        }

        PublishCrocomirePlm(new Position16<RoomBlocks>(78, 3), PlmHeaderId.CreateCrocomireInvisibleWall);
    }

    /// <summary>Number of dust puffs emitted across the bridge-collapse path.</summary>
    private const int CrocomireBridgeDustCount = 7;

    /// <summary>
    /// $A4:8F35-8FC1 (seven unrolled spawns): puffs sixteen pixels apart along the bridge from
    /// X $600; the first and third rise sixteen pixels above the others' Y $C0.
    /// </summary>
    internal static (ushort X, ushort Y) CrocomireBridgeDustPosition(int puff) =>
        (uint)puff < CrocomireBridgeDustCount
            ? ((ushort)(1536 + 16 * puff), (ushort)(puff is 0 or 2 ? 176 : 192))
            : throw new ArgumentOutOfRangeException(nameof(puff));
    /// <summary>BG2 tile word used to clear Crocomire's working background rows.</summary>
    private static ushort CrocomireBlankBg2Tile => CrocomireMeltingTilemap.BlankGlyph.Raw;

    /// <summary>Clears the working BG2 map before death and melting scene transfers.</summary>
    private void ClearCrocomireBg2WorkingTilemap()
    {
        Span<ushort> tilemap = RequireCrocomireDeath().MutableBg2WorkingTilemap;
        tilemap.Fill(CrocomireBlankBg2Tile);
    }

    /// <summary>Copies a bounded working-map span into the corresponding BG2 VRAM area.</summary>
    private void TransferCrocomireBg2Words(int startWord, int wordCount)
    {
        if (startWord < 0 || wordCount < 0 ||
            startWord + wordCount > CrocomireDeathState.Bg2WorkingWordCount)
        {
            throw new ArgumentOutOfRangeException(nameof(startWord));
        }

        new EnemyVram(_presentation.Vram).EnemyBg2Tilemap.Write(
            RequireCrocomireDeath().Bg2WorkingTilemap.Slice(startWord, wordCount), startWord);
    }

    /// <summary>Writes a uniform vertical offset to the per-scanline BG2 scroll table.</summary>
    private void FillCrocomireBg2ScrollTable(ushort verticalScroll) =>
        RequireCrocomireDeath().MutableBg2ScrollByScanline.Fill(verticalScroll);

    /// <summary>
    /// Erases the 32-tile circular-map row selected by <c>(BG2VOFS+288)&amp;$FFF8</c> while
    /// Crocomire sinks. This is the visual producer that makes the body disappear into acid;
    /// merely moving the enemy's Y word leaves a stale full-height BG2 image behind.
    /// </summary>
    private void ClearCrocomireBg2SinkRow()
    {
        int byteOffset = 8 * (unchecked((ushort)(CrocomireBg2Scroll.Y + 288)) & ~7);
        int firstWord = byteOffset >> 1;
        Span<ushort> tilemap = RequireCrocomireDeath().MutableBg2WorkingTilemap;
        if (firstWord < 0 || firstWord + 32 > tilemap.Length)
        {
            throw new InvalidDataException(
                $"Crocomire sink selected BG2 word {firstWord}, outside the {tilemap.Length}-word buffer.");
        }
        tilemap.Slice(firstWord, 32).Fill(CrocomireBlankBg2Tile);
        TransferCrocomireBg2Words(firstWord, 32);
    }

    /// <summary>Ports $A4:8B5B and its fallthrough into $A4:8BA4.</summary>
    private void UpdateCrocomireBg2Scroll(
        CrocomireEnemyState state,
        bool includeVerticalPosition)
    {
        RoomEnemySlot body = state.Body;
        if (includeVerticalPosition)
            CrocomireBg2Scroll = CrocomireBg2Scroll.WithY(CrocomireBg2ScrollDefinitions.VerticalScroll(
                body.Position.Y.Whole, body.SpritemapPointer));

        // The tongue is a normal second enemy actor placed at its body-relative X offset.
        // $8BA4 updates it even while the body's bulk is represented by BG2.
        if (state.Tongue is { } tongue)
        {
            int offset = (tongue.SpeciesState<CrocomireTongueState>()
                ?? throw new InvalidOperationException("Crocomire's tongue has no state.")).BodyXOffset;
            tongue.Position = tongue.Position.WithWhole(new(unchecked((ushort)(body.Position.X.Whole + offset)), body.Position.Y.Whole));
        }

        ushort horizontal;
        if (unchecked((short)(body.Position.X.Whole - _crocomireCamera.X)) >= 0)
        {
            horizontal = unchecked((short)(
                body.Position.X.Whole - 128 - (_crocomireCamera.X + 256))) < 0
                ? CalculateOnScreenCrocomireBg2Horizontal(body.Position.X.Whole)
                : (ushort)256;
        }
        else
        {
            horizontal = unchecked((short)(
                body.Position.X.Whole + 128 - _crocomireCamera.X)) < 0
                ? (ushort)256
                : CalculateOnScreenCrocomireBg2Horizontal(body.Position.X.Whole);
        }
        CrocomireBg2Scroll = CrocomireBg2Scroll.WithX(horizontal);
    }

    /// <summary>Converts the body X position into the BG2 horizontal scroll used to keep Crocomire onscreen.</summary>
    private ushort CalculateOnScreenCrocomireBg2Horizontal(ushort bodyX)
    {
        ushort relative = unchecked((ushort)(_crocomireCamera.X - bodyX + 51));
        return EnemyMath.WrappedMagnitude(relative) >= 284 ? (ushort)256 : relative;
    }
    /// <summary>Dispatches $A4:B951/$B968/$BA05/$BAB4 after multibox collision.</summary>
    private void ResolveCrocomireHitboxShot(
        SamusProjectileTypeWord projectileType,
        Position16<RoomPixels> projectile,
        ushort callback)
    {
        CrocomireEnemyState state = Crocomire ??
            throw new InvalidOperationException("Crocomire hitbox has no body owner.");
        RoomEnemySlot body = state.Body;

        CrocomireHitboxShotCallback shot =
            ClosedNativeWords.Decode<CrocomireHitboxShotCallback>(callback, "Crocomire hitbox shot AI");
        switch (shot)
        {
            case CrocomireHitboxShotCallback.NoOp:
            {
                // $B951 increments a temporary low-nibble value, then ORs it back into the
                // flags word instead of replacing the nibble. Preserve that odd instruction
                // sequence; changing it to an ordinary counter disagrees for several values.
                state.FightFlags |= CrocomireFightFlagBits.AsHitCountField(state.FightFlags.NextHitCount());
                return;
            }

            case CrocomireHitboxShotCallback.Dust:
            case CrocomireHitboxShotCallback.AlternateDust:
                SpawnCrocomireShotDust(projectileType, projectile);
                return;

            case CrocomireHitboxShotCallback.Mouth:
                ResolveCrocomireMouthShot(
                    state,
                    body,
                    projectileType,
                    projectile);
                return;

            case CrocomireHitboxShotCallback.HeaderReturn:
                // Some extended-map records deliberately point at the body header's RTL.
                return;

            default:
                throw new InvalidOperationException($"Undefined CrocomireHitboxShotCallback {shot}.");
        }
    }

    /// <summary>Ports Crocomire's mouth reaction at $A4:BA05.</summary>
    private void ResolveCrocomireMouthShot(
        CrocomireEnemyState state,
        RoomEnemySlot body,
        SamusProjectileTypeWord projectileType,
        Position16<RoomPixels> projectile)
    {
        body.InvincibilityTimer = NativeCountdown.Start(0);

        // The off-screen branch bypasses projectile-family selection and still publishes
        // the pending-hit state. This wrapped signed comparison is the cartridge's exact
        // `body left edge - (camera + 256)` test.
        bool bodyLeftEdgeIsOnScreen = unchecked((short)(
            body.Position.X.Whole - body.Radius.X - 256 - _crocomireCamera.X)) < 0;
        int stepCount = 0;
        SamusProjectileFamily family = projectileType.Family;
        if (bodyLeftEdgeIsOnScreen)
        {
            if (family == 0)
            {
                if (!projectileType.IsChargedBeam)
                {
                    body.InstructionTimer = NativeCountdown.Start(8);
                    SpawnCrocomireShotDust(projectileType, projectile);
                    return;
                }
                stepCount = 2;
            }
            else if (family == SamusProjectileFamily.Missile)
            {
                stepCount = 1;
            }
            else if (family == SamusProjectileFamily.SuperMissile)
            {
                stepCount = 3;
            }
        }

        if (!bodyLeftEdgeIsOnScreen || stepCount != 0)
        {
            state.StepCounter += stepCount;
            int hitCount = state.FightFlags.NextHitCount();
            if ((state.FightFlags & CrocomireFightFlags.DamageReactionPending) == 0)
            {
                // Both regional constants at $A4:8692/$8694 are eight. Keep the branch
                // explicit because the source distinguishes projectile-attack state even
                // though this ROM revision stores identical delay values.
                body.InstructionTimer = NativeCountdown.FromRaw(unchecked((ushort)(
                    body.InstructionTimer.RawValue +
                    (state.FightFunction == CrocomireFightFunction.ProjectileAttack ? 8 : 8))));
            }
            state.FightFlags = CrocomireFightFlagBits.AsHitCountField(hitCount) |
                (state.FightFlags & ~(CrocomireFightFlags.HitCountField | CrocomireFightFlags.DamageReactionPending | CrocomireFightFlags.ClawTouchedSamus)) |
                CrocomireFightFlags.DamageReactionPending;
            state.ReactionTimer = 10;
        }

        body.FlashTimer = NativeCountdown.FromRaw(unchecked((ushort)(body.FlashTimer.RawValue + 14)));
        body.AiStates |= EnemyAiStates.Hurt;
    }

    /// <summary>Places a dust effect at the shot impact while preserving its component-relative offset.</summary>
    private void SpawnCrocomireShotDust(
        SamusProjectileTypeWord projectileType,
        Position16<RoomPixels> projectile)
    {
        MiscDustAnimation animationIndex = projectileType.Family != SamusProjectileFamily.SuperMissile
            ? MiscDustAnimation.DudShotTinyExplosion
            : MiscDustAnimation.BigExplosion;
        _projectiles.SpawnRoomGraphicsDustExplosion(projectile, animationIndex);
    }

    /// <summary>Ports $A4:B992 without applying normal power-bomb HP damage.</summary>
    private void ResolveCrocomirePowerBombReaction(RoomEnemySlot body)
    {
        CrocomireEnemyState state = RequireCrocomire(body);
        if (state.DeathSequenceIndex != CrocomireDeathPhase.Fighting)
            return;

        state.StepCounter = 3;
        if (state.FightFunction == CrocomireFightFunction.PowerBombCharge)
            return;

        state.FightFlags = (state.FightFlags & ~(CrocomireFightFlags.HitCountField | CrocomireFightFlags.ClawTouchedSamus | CrocomireFightFlags.Engaged)) |
            CrocomireFightFlags.Engaged;
        state.ReactionTimer = 10;
        body.FlashTimer = NativeCountdown.FromRaw(unchecked((ushort)(body.FlashTimer.RawValue + 4)));
        body.AiStates |= EnemyAiStates.Hurt;
        state.FightFunction = CrocomireFightFunction.PowerBombCharge;

        StartCrocomireBodyProgram(state, SelectCrocomirePowerBombProgram(state));
    }

    /// <summary>
    /// Selects the fully-open, part-open or closed reaction from the mouth of the frame shown, as $A4:B9D8 does
    /// by scanning the frame's spritemap. Before any frame is shown the body has no spritemap and reacts closed.
    /// </summary>
    private static CrocomireBodyProgram SelectCrocomirePowerBombProgram(CrocomireEnemyState state) =>
        state.BodyFrame is { } frame
            ? CrocomirePowerBombReactionDefinitions.ForFrame(frame)
            : CrocomireBodyProgram.PowerBombReactionMouthNotOpen;
    /// <summary>Dispatches Crocomire's sinking, skeleton, wall-break, and arena-completion phases.</summary>
    private void RunCrocomireDeathSequence(CrocomireEnemyState state, SamusState? samus)
    {
        switch (state.DeathSequenceIndex)
        {
            case CrocomireDeathPhase.CrumbleBridgeAndSink:
                SpawnNextCrocomireBridgeFragment();
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: true);
                return;
            case CrocomireDeathPhase.FirstSubmergedPause:
                RunCrocomireAcidSoundTimer();
                UpdateCrocomireBg2Scroll(state, includeVerticalPosition: false);
                RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhase.FirstHopRise:
                RunCrocomireRisingComposite(state, samus, tickAcidSound: true);
                return;
            case CrocomireDeathPhase.FirstHopSink:
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: true);
                return;
            case CrocomireDeathPhase.SecondSubmergedPause:
                RunCrocomireAcidSoundTimer();
                UpdateCrocomireBg2Scroll(state, includeVerticalPosition: false);
                RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhase.SecondHopRise:
                RunCrocomireRisingComposite(state, samus, tickAcidSound: true);
                return;
            case CrocomireDeathPhase.SecondHopSink:
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: true);
                return;
            case CrocomireDeathPhase.InstallFirstMeltImage:
                LoadFirstCrocomireMeltingTilemap(state);
                return;
            case CrocomireDeathPhase.CopyFirstMeltGraphics:
                InitializeCrocomireMeltingGraphics(state);
                return;
            case CrocomireDeathPhase.UploadFirstMeltGraphics:
                UploadNextCrocomireMeltingGraphicsSlice(state);
                return;
            case CrocomireDeathPhase.ThirdHopRise:
                RunCrocomireRisingComposite(state, samus, tickAcidSound: false);
                return;
            case CrocomireDeathPhase.StartFirstDissolve:
                LastCrocomireSoundEffect = CrocomireSoundDefinitions.FirstMeltDissolve;
                BeginCrocomireMelting(state);
                return;
            case CrocomireDeathPhase.DissolveFirstImage:
                RunCrocomireMelting(state, samus);
                return;
            case CrocomireDeathPhase.ClearFirstMeltImage:
                FinishCrocomireMeltingPass(state);
                return;
            case CrocomireDeathPhase.FourthHopSink:
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: false);
                return;
            case CrocomireDeathPhase.ThirdSubmergedPause:
                RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhase.FourthHopRise:
                RunCrocomireRisingComposite(state, samus, tickAcidSound: false);
                return;
            case CrocomireDeathPhase.FifthHopSink:
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: false);
                return;
            case CrocomireDeathPhase.FourthSubmergedPause:
                RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhase.FifthHopRise:
                RunCrocomireRisingComposite(state, samus, tickAcidSound: false);
                return;
            case CrocomireDeathPhase.SixthHopSink:
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: false);
                return;
            case CrocomireDeathPhase.InstallSecondMeltImage:
                LoadSecondCrocomireMeltingTilemap(state);
                return;
            case CrocomireDeathPhase.CopySecondMeltGraphics:
                InitializeCrocomireMeltingGraphics(state);
                return;
            case CrocomireDeathPhase.UploadSecondMeltGraphics:
                UploadNextCrocomireMeltingGraphicsSlice(state);
                return;
            case CrocomireDeathPhase.ShippedSpacer:
                state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
                return;
            case CrocomireDeathPhase.SixthHopRise:
                SelectCrocomireRisingInstruction(state);
                SpawnCrocomireAcidSmoke(state, samus);
                RunCrocomireRise(state);
                return;
            case CrocomireDeathPhase.StartSecondDissolve:
                LastCrocomireSoundEffect = CrocomireSoundDefinitions.SecondMeltDissolve;
                BeginCrocomireMelting(state);
                return;
            case CrocomireDeathPhase.DissolveSecondImage:
                RunCrocomireMelting(state, samus);
                return;
            case CrocomireDeathPhase.ClearSecondMeltImage:
                FinishCrocomireMeltingPass(state);
                return;
            case CrocomireDeathPhase.FinalSink:
                RunCrocomireFinalSink(state, samus);
                return;
            case CrocomireDeathPhase.WaitForSamusAtWall:
                RunCrocomireWaitBehindWall(state, samus);
                return;
            case CrocomireDeathPhase.RumbleHiddenWall:
                RunCrocomireWallRumble(state);
                return;
            case CrocomireDeathPhase.BreakSpikeWall:
                RunCrocomireSkeletonTileLoadAndWallBreak(state);
                return;
            case CrocomireDeathPhase.DelaySkeletonFall:
                RunCrocomireWallBreakDelay(state);
                return;
            case CrocomireDeathPhase.ArcSkeletonIntoArena:
                RunCrocomireSkeletonArc(state);
                return;
            case CrocomireDeathPhase.WaitForSkeletonTerminalImage:
                RunCrocomireSkeletonCollapse(state);
                return;
            case CrocomireDeathPhase.ClearWallAndOpenScrolls:
                FinishCrocomireArenaScrolls(state);
                return;
            case CrocomireDeathPhase.WaitForStableSkeleton:
                if (state.BodyResidentProgram >= CrocomireBodyProgram.SkeletonStable)
                    state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
                return;
            case CrocomireDeathPhase.NativeOneFrameSpacer:
                state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
                return;
            case CrocomireDeathPhase.PublishDefeatAndRestoreMusic:
                CompleteCrocomireBoss(state);
                return;
            case CrocomireDeathPhase.InertCorpse:
                return;
            case CrocomireDeathPhase.DefeatedRoomAdvance:
                state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
                return;
            case CrocomireDeathPhase.PinDefeatedRoomBg2Scroll:
                CrocomireBg2Scroll = Position16<Bg2Pixels>.Zero;
                return;
            case CrocomireDeathPhase.RiverSkeletonDetour:
                RunCrocomireSkeletonRiver(state);
                return;
            case CrocomireDeathPhase.Fighting:
                throw new InvalidDataException("Crocomire's death sequence runs before its bridge collapse began.");
            default:
                throw new InvalidOperationException($"Undefined CrocomireDeathPhase {state.DeathSequenceIndex}.");
        }
    }

    /// <summary>Runs the paired body and BG2 presentation while Crocomire sinks into acid.</summary>
    private void RunCrocomireSinkingComposite(
        CrocomireEnemyState state,
        SamusState? samus,
        bool tickAcidSound)
    {
        if (tickAcidSound)
            RunCrocomireAcidSoundTimer();
        SpawnCrocomireAcidSmoke(state, samus);
        RunCrocomireSink(state);
    }

    /// <summary>Runs the paired body and BG2 presentation as Crocomire resurfaces.</summary>
    private void RunCrocomireRisingComposite(
        CrocomireEnemyState state,
        SamusState? samus,
        bool tickAcidSound)
    {
        if (tickAcidSound)
            RunCrocomireAcidSoundTimer();
        SpawnCrocomireAcidSmoke(state, samus);
        RunCrocomireRise(state);
    }

    /// <summary>Queues periodic acid sounds according to the death-scene timer.</summary>
    private void RunCrocomireAcidSoundTimer()
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        if (death.AcidSoundTimer.IsZero)
            return;
        death.AcidSoundTimer = death.AcidSoundTimer.Decrement().Remaining;
        if (!death.AcidSoundTimer.IsZero)
            return;
        death.AcidSoundTimer = NativeCountdown.Start(32);
        LastCrocomireSoundEffect = CrocomireSoundDefinitions.AcidBurn;
    }

    /// <summary>Holds the submerged skeleton for its authored delay before rising begins.</summary>
    private static void RunCrocomireSubmergedPause(CrocomireEnemyState state)
    {
        if (!state.DeathPhaseDelay.IsZero)
        {
            state.DeathPhaseDelay = state.DeathPhaseDelay.Decrement().Remaining;
            return;
        }
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
        state.VerticalSpeed = SnesEightEight.FromWhole(MaximumSinkSpeedWhole);
    }

    /// <summary>Byte-for-byte fixed-point fall from <c>SinkCrocomireDown</c> at $A4:91C1.</summary>
    internal void RunCrocomireSink(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        FillCrocomireBg2ScrollTable(CrocomireBg2Scroll.Y);
        state.FightFlags &= ~CrocomireFightFlags.DamageReactionPending;
        UpdateCrocomireBg2Scroll(state, includeVerticalPosition: true);
        RoomEnemySlot body = state.Body;

        if (unchecked((short)(body.Position.Y.Whole - 280)) >= 0)
        {
            state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
            state.DeathPhaseDelay = NativeCountdown.Start(48);
            return;
        }

        ClearCrocomireBg2SinkRow();
        body.ExtraProperties = body.ExtraProperties.Without(
            EnemyExtraProperties.NewInstructionFrame);

        // The acceleration's fraction carries into its whole part before the whole part is clamped, and the
        // speed's into its whole part before that is clamped: each clamp keeps the fraction it carried from.
        int acceleration = ClampWholePixels(
            state.VerticalAcceleration + SinkAccelerationIncrement, MaximumSinkAccelerationWhole);
        state.VerticalAcceleration = acceleration;
        // The speed grows by the acceleration's whole part read as 1/256 pixels.
        int speed = ClampWholePixels(
            state.VerticalSpeed + SnesEightEight.WholeOf(acceleration), MaximumSinkSpeedWhole);
        state.VerticalSpeed = speed;

        int travelled = state.VerticalSubpixel + speed;
        state.VerticalSubpixel = SnesEightEight.FractionOf(travelled);
        body.Position = body.Position.WithWholeY(unchecked((ushort)(
            body.Position.Y.Whole + SnesEightEight.WholeOf(travelled))));
    }

    /// <summary>
    /// An 8.8 quantity in 1/256 pixels with its whole part clamped to <paramref name="maximumWhole"/> and its
    /// fraction kept, as the sink's byte-wise clamps leave it.
    /// </summary>
    private static int ClampWholePixels(int value, int maximumWhole) =>
        SnesEightEight.FromWhole(Math.Min(SnesEightEight.WholeOf(value), maximumWhole)) +
        SnesEightEight.FractionOf(value);

    /// <summary>Byte-for-byte fixed-point rise from $A4:92D8.</summary>
    internal void RunCrocomireRise(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        FillCrocomireBg2ScrollTable(CrocomireBg2Scroll.Y);
        RoomEnemySlot body = state.Body;
        if (unchecked((short)(body.Position.Y.Whole - 218)) < 0)
        {
            state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
            return;
        }

        UpdateCrocomireBg2Scroll(state, includeVerticalPosition: true);
        int acceleration = Math.Min(
            state.VerticalAcceleration + SnesEightEight.FromWhole(1),
            SnesEightEight.FromWhole(MaximumRiseAccelerationWhole));
        state.VerticalAcceleration = acceleration;

        // The speed loses the acceleration's whole part read as 1/256 pixels; a speed that would fall below zero
        // stalls just under one pixel per frame instead of reversing.
        int speed = state.VerticalSpeed - SnesEightEight.WholeOf(acceleration);
        if (speed < 0)
            speed = RiseSpeedAfterStalling;
        state.VerticalSpeed = speed;

        int remaining = state.VerticalSubpixel - speed;
        state.VerticalSubpixel = SnesEightEight.FractionOf(remaining);
        body.Position = body.Position.WithWholeY(unchecked((ushort)(
            body.Position.Y.Whole + SnesEightEight.WholeOf(remaining))));
    }

    /// <summary>Emits smoke at the acid surface during Crocomire's death presentation.</summary>
    private void SpawnCrocomireAcidSmoke(CrocomireEnemyState state, SamusState? samus)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        death.AcidSmokeTimer = death.AcidSmokeTimer.Decrement().Remaining;
        if (!death.AcidSmokeTimer.IsZero)
            return;
        death.AcidSmokeTimer = NativeCountdown.Start(6);

        ushort random = ReadCrocomireRandom();
        ushort xOffset = unchecked((ushort)(random & 63));
        if ((random & 2) == 0)
            xOffset = unchecked((ushort)~xOffset);
        int acidY = samus?.LiquidPhysics.LavaAcidYPosition ?? 224;
        _spriteObjects.SpawnRoomSpriteObject(
            new(unchecked((ushort)(state.Body.Position.X.Whole + xOffset)), unchecked((ushort)(acidY + 16 - ((SnesLittleEndian.HighByte(random) & 31))))),
            RoomSpriteObjectKind.CrocomireAcidSmoke,
            graphicsIndex: 0);
    }

    /// <summary>Selects the body melting program for the rows still above the acid.</summary>
    private static void SelectCrocomireRisingInstruction(CrocomireEnemyState state)
    {
        ushort y = state.Body.Position.Y.Whole;
        CrocomireBodyProgram program = y < 248
            ? CrocomireBodyProgram.MeltingTwoTopFourRows
            : y < 264
                ? CrocomireBodyProgram.MeltingTwoTopThreeRows
                : y < 280
                    ? CrocomireBodyProgram.MeltingTwoTopTwoRows
                    : CrocomireBodyProgram.MeltingTwoTopRow;
        StartCrocomireBodyProgram(state, program);
    }

    /// <summary>Completes the acid sink and hands control to the skeleton river sequence.</summary>
    private void RunCrocomireFinalSink(CrocomireEnemyState state, SamusState? samus)
    {
        SelectCrocomireRisingInstruction(state);
        SpawnCrocomireAcidSmoke(state, samus);
        RunCrocomireSink(state);
        if (state.DeathSequenceIndex != CrocomireDeathPhase.WaitForSamusAtWall)
            return;

        LastCrocomireMusicRequest = new CrocomireMusicRequest(
            MusicCommand.SelectTrack(6),
            MusicCommandDelay.EightFrames);
        state.DeathSequenceIndex = CrocomireDeathPhase.RiverSkeletonDetour;
        StartCrocomireBodyProgram(state, CrocomireBodyProgram.SkeletonFlowingDownRiver);
        _room.Services.RoomScroll.Write(4, RoomScrollState.Blue);
        _room.Services.RoomScroll.Write(5, RoomScrollState.Blue);
        if (state.Tongue is { } tongue)
            tongue.Properties = tongue.Properties.With(EnemyProperties.Deleted);
        PublishCrocomirePlm(new Position16<RoomBlocks>(78, 3), PlmHeaderId.ClearCrocomireInvisibleWall);
        _zebetiteBehavior.CameraDistanceIndex = CameraDistanceMode.NormalTracking; // $A4:90D8
        RequireCrocomireDeath().SkeletonTileChunk = 0;
    }

    /// <summary>Moves the skeleton through the acid river toward the arena's breakable wall.</summary>
    private static void RunCrocomireSkeletonRiver(CrocomireEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        body.Position = body.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole - 2)));
        if (unchecked((short)(body.Position.X.Whole - 480)) < 0)
        {
            body.Position = body.Position.WithWhole(new(480, 54));
            state.DeathSequenceIndex = CrocomireDeathPhase.WaitForSamusAtWall;
        }
        else
        {
            body.Position = body.Position.WithWholeY(220);
        }
    }

    /// <summary>Waits with the skeleton behind the wall before its rumble and tile-load sequence.</summary>
    private void RunCrocomireWaitBehindWall(CrocomireEnemyState state, SamusState? samus)
    {
        if (samus is null || unchecked((short)(samus.Position.X.Whole - 640)) >= 0)
            return;

        LastCrocomireMusicRequest = new CrocomireMusicRequest(
            MusicCommand.SelectTrack(5),
            MusicCommandDelay.EightFrames);
        _room.Services.RoomScroll.Write(3, RoomScrollState.RedBoundary);
        _room.Services.RoomScroll.Write(4, RoomScrollState.Blue);
        PublishCrocomirePlm(new Position16<RoomBlocks>(48, 3), PlmHeaderId.CreateCrocomireInvisibleWall);
        _zebetiteBehavior.CameraDistanceIndex = CameraDistanceMode.RightEdge; // $A4:97F3-97F6
        RoomEnemySlot body = state.Body;
        body.Properties = body.Properties.Replace(
            EnemyProperties.SolidToSamus | EnemyProperties.IgnoreSamusCollision,
            EnemyProperties.IgnoreSamusCollision);
        if (state.Tongue is { } tongue)
            tongue.Properties = tongue.Properties.With(
                EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
        state.RumbleTableOffset = 4;

        CrocomireDeathState death = RequireCrocomireDeath();
        death.RumbleYOffset = 0;
        death.RumbleCooldown = NativeCountdown.Start(10);
        death.RumbleDelta = 1;
        state.FightFlags = 0;
        body.Radius = body.Radius.WithY(56);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

    /// <summary>Shakes the arena wall before the skeleton's break-through and advances its authored delay.</summary>
    internal void RunCrocomireWallRumble(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        CrocomireRumbleDefinition definition =
            CrocomireRumbleDefinitions.AtOffset(state.RumbleTableOffset);
        int target = definition.TargetYOffset;
        if (definition.IsTerminator)
        {
            // The rumble hands its shared word back to the melt's shake counter, with both bytes at 128.
            death.MeltShakeCounter = NativeCountdown.FromRaw(MeltShakeCounterAfterRumble);
            state.DeathPhaseDelay = NativeCountdown.Start(128);
            (_room.Artwork?.CrocomireColors ?? throw new InvalidOperationException(
                "Crocomire wall spikes require installed colors.")).ApplyWallSpikes(_presentation.Cgram);
            state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
            return;
        }

        if (death.RumbleYOffset == target)
        {
            if (target < 0)
            {
                if (!death.RumbleCooldown.IsZero)
                {
                    death.RumbleCooldown = death.RumbleCooldown.Decrement().Remaining;
                    state.RumbleTableOffset -= 2;
                    LastCrocomireSoundEffect = CrocomireSoundDefinitions.WallRumble;
                    return;
                }

                death.RumbleCooldown = NativeCountdown.FromRaw(definition.Cooldown);
                death.RumbleDelta = definition.Delta;
            }
            state.RumbleTableOffset = definition.NextTargetOffset;
            return;
        }

        death.RumbleYOffset = death.RumbleYOffset >= target
            ? death.RumbleYOffset - death.RumbleDelta
            : death.RumbleYOffset + death.RumbleDelta;
    }

    /// <summary>The melt shake counter's word as the hidden-wall rumble's terminator leaves it: both bytes 128.</summary>
    private const ushort MeltShakeCounterAfterRumble = 32896;

    /// <summary>Loads skeleton graphics and replaces the wall blocks as the bones break through.</summary>
    internal void RunCrocomireSkeletonTileLoadAndWallBreak(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        if (!state.DeathPhaseDelay.IsZero)
        {
            state.DeathPhaseDelay = state.DeathPhaseDelay.Decrement().Remaining;
            UploadNextCrocomireSkeletonTileChunk(death);
            return;
        }

        RoomEnemySlot body = state.Body;
        body.Position = body.Position.WithWhole(new(480, 54));
        death.RumbleCooldown = NativeCountdown.Start(80);
        state.SkeletonVelocity = SnesSignedSixteenSixteen.Zero;
        PublishCrocomirePlm(new Position16<RoomBlocks>(32, 3), PlmHeaderId.ClearCrocomireInvisibleWall);
        PublishCrocomirePlm(new Position16<RoomBlocks>(30, 3), PlmHeaderId.CreateCrocomireInvisibleWall);
        PublishCrocomirePlm(new Position16<RoomBlocks>(112, 11), PlmHeaderId.ClearCrocomireBridge);
        LastCrocomireSoundEffect = CrocomireSoundDefinitions.Crumble;
        StartCrocomireBodyProgram(state, CrocomireBodyProgram.SkeletonFallsApart);
        body.PaletteIndex = 0;
        (_room.Artwork?.CrocomireColors ?? throw new InvalidOperationException(
            "Crocomire skeleton requires installed colors.")).ApplySkeletonArm(_presentation.Cgram);

        foreach (RoomEnemyProjectileSlot projectile in _projectiles.EnemyProjectiles)
            projectile.Clear();
        SpawnCrocomireSpikeWallPieces();
        LastCrocomireSoundEffect = CrocomireSoundDefinitions.SpikeWallBreak;
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

    /// <summary>Holds the broken-wall pose before launching the skeleton arc.</summary>
    internal void RunCrocomireWallBreakDelay(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        if (unchecked((short)(state.Body.Position.X.Whole - 224)) < 0)
        {
            state.SkeletonVelocity = AccelerateCrocomireSkeleton(state.SkeletonVelocity, accelerationEightEight: 128, maximumWhole: 2);
            state.Body.Position = state.Body.Position.WithX(
                state.SkeletonVelocity.AddTo(state.Body.Position.X.Whole, state.Body.Position.X.Fraction));
        }

        if (death.RumbleCooldown.IsZero)
            return;
        death.RumbleCooldown = death.RumbleCooldown.Decrement().Remaining;
        if (!death.RumbleCooldown.IsZero)
            return;
        state.SkeletonVelocity = state.SkeletonVelocity.WithFraction(0);
        StartCrocomireBodyProgram(state, CrocomireBodyProgram.SkeletonFalling);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

    /// <summary>Moves the skeleton through its airborne arc after the wall break.</summary>
    internal void RunCrocomireSkeletonArc(CrocomireEnemyState state)
    {
        state.SkeletonVelocity = AccelerateCrocomireSkeleton(state.SkeletonVelocity, accelerationEightEight: 8, maximumWhole: 5);
        state.Body.Position = state.Body.Position.WithY(
            SkeletonArcFallSpeed.AddTo(state.Body.Position.Y.Whole, state.Body.Position.Y.Fraction));
        state.Body.Position = state.Body.Position.WithX(
            state.SkeletonVelocity.AddTo(state.Body.Position.X.Whole, state.Body.Position.X.Fraction));
        if (unchecked((short)(state.Body.Position.X.Whole - 576)) < 0)
            return;

        LastCrocomireSoundEffect = CrocomireSoundDefinitions.BigExplosion;
        if (state.Tongue is { } tongue)
            tongue.PaletteIndex = state.Body.PaletteIndex;
        StartCrocomireBodyProgram(state, CrocomireBodyProgram.SkeletonFallsApart);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

    /// <summary>Finishes the skeleton collapse and scatters sixteen independently chosen enemy drops.</summary>
    internal void RunCrocomireSkeletonCollapse(CrocomireEnemyState state)
    {
        if (state.BodyResidentProgram < CrocomireBodyProgram.SkeletonStable)
        {
            state.SkeletonVelocity = AccelerateCrocomireSkeleton(state.SkeletonVelocity, accelerationEightEight: 16, maximumWhole: 6);
            return;
        }

        StartCrocomireBodyProgram(state, CrocomireBodyProgram.Dead);
        state.Body.Position += new Offset16<RoomPixels>(64, 21);
        state.Body.Radius = new(40, 28);
        PublishCrocomirePlm(new Position16<RoomBlocks>(48, 3), PlmHeaderId.ClearCrocomireInvisibleWall);

        // $A0:B995 emits sixteen independent $F337 projectiles across Crocomire's
        // authored arena rectangle. These are not a single pickup at the corpse position:
        // each actor advances RNG again while selecting its own drop from header $DDBF.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.Crocomire,
            count: 16,
            xBase: 576,
            xMask: 127,
            yBase: 96,
            yMask: SnesLittleEndian.Word(0, 63));
        RequireCrocomireDeath().ItemDropRequested = true;
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

    /// <summary>Restores arena scroll and camera tracking after the skeleton sequence.</summary>
    private void FinishCrocomireArenaScrolls(CrocomireEnemyState state)
    {
        for (int index = 0; index < 4; index++)
            _room.Services.RoomScroll.Write(index, RoomScrollState.Blue);
        PublishCrocomirePlm(new Position16<RoomBlocks>(30, 3), PlmHeaderId.ClearCrocomireInvisibleWall);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

    /// <summary>Marks Crocomire's boss encounter complete and releases the death-scene presentation.</summary>
    private void CompleteCrocomireBoss(CrocomireEnemyState state)
    {
        LastCrocomireMusicRequest = new CrocomireMusicRequest(
            MusicCommand.SelectTrack(6),
            MusicCommandDelay.EightFrames);
        _zebetiteBehavior.CameraDistanceIndex = CameraDistanceMode.NormalTracking; // $A4:9B8D
        _room.Services.Bosses.SetDefeated(BossBits.AreaMiniBoss);
        RequireCrocomireDeath().BossBitSet = true;
        SpawnCrocomireDust(state, -16);
        SpawnCrocomireDust(state, 16);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

    /// <summary>Uploads the next ordered skeleton character chunk to its native OBJ VRAM position.</summary>
    internal void UploadNextCrocomireSkeletonTileChunk(CrocomireDeathState death)
    {
        int entry = death.SkeletonTileChunk;
        var artwork = _room.Artwork?.CrocomireSkeleton ?? throw new InvalidDataException(
            "Installed Crocomire artwork is missing its skeleton characters.");
        if (!CrocomireSkeletonTransferDefinitions.TryGet(entry, out
                CrocomireSkeletonTransferDefinition frame))
            return;
        // The native OBSEL base contributes $6000 words. Only character
        // pixels are editable; the transfer order and VRAM position are not.
        new EnemyVram(_presentation.Vram).CrocomireSkeletonTiles.Load(artwork.Chunk(entry).Span,
            frame.DestinationOffset * sizeof(ushort));
        death.SkeletonTileChunk++;
    }

    /// <summary>
    /// Adds an acceleration of <paramref name="accelerationEightEight"/>/256 pixel per frame per frame to the
    /// skeleton's 16.16 speed and clamps its whole pixels at <paramref name="maximumWhole"/>, keeping the fraction.
    /// The speed starts at rest and only grows, so its whole pixels never leave the clamp's range.
    /// </summary>
    private static SnesSignedSixteenSixteen AccelerateCrocomireSkeleton(
        SnesSignedSixteenSixteen velocity,
        int accelerationEightEight,
        int maximumWhole)
    {
        SnesSignedSixteenSixteen accelerated =
            velocity + SnesSignedSixteenSixteen.FromRaw(SnesSixteenSixteen.FromEightEight(accelerationEightEight));
        return accelerated.WholePixels >= maximumWhole
            ? accelerated.WithWholePixels(checked((short)maximumWhole))
            : accelerated;
    }
    /// <summary>Advances Crocomire's typed animation.</summary>
    internal void AdvanceCrocomireTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context)
    {
        AdvanceCrocomireBodyAnimation(slot, context.Samus, context.Level, context.Camera.X);
    }

    /// <summary>Advances CrocomireTongue's typed animation.</summary>
    internal void AdvanceCrocomireTongueTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        AdvanceCrocomireTongueAnimation(slot);
    }

    /// <summary>
    /// Runs Crocomire's body program where the enemy instruction interpreter would: the frame expires on the
    /// update its native timer would, and a newly shown frame raises the new-frame flag exactly as a native
    /// timed frame does.
    /// </summary>
    internal void AdvanceCrocomireBodyAnimation(RoomEnemySlot body, SamusState? samus, RoomLevelData? level, ushort cameraX)
    {
        CrocomireEnemyState state = RequireCrocomire(body);
        ushort oldTimer = body.InstructionTimer.RawValue;
        body.InstructionTimer = NativeCountdown.FromRaw(unchecked((ushort)(oldTimer - 1)));
        if (oldTimer != 1)
        {
            body.ExtraProperties = body.ExtraProperties.Without(EnemyExtraProperties.NewInstructionFrame);
            return;
        }

        AnimationProgramCursor<CrocomireBodyProgram> cursor = state.BodyCursor;
        NativeCountdown counter = body.Timer;
        AnimationProgramAdvance<CrocomireBodyFrame> advance = CrocomireBodyAnimations.Programs.Advance(ref cursor, ref counter,
            (cue, continuation) => ApplyCrocomireBodyCue(state, samus, level, cameraX, cue,
                CrocomireBodyAnimations.Programs.ResidentProgram(continuation)));
        state.BodyCursor = cursor;
        body.Timer = counter;
        // A hold is the native sleep: the timer wraps from zero and the last frame stays. The body programs
        // have no waits or stops.
        if (advance.Kind != AnimationProgramAdvanceKind.Show)
        {
            if (advance.Kind != AnimationProgramAdvanceKind.Hold)
                throw new InvalidOperationException($"Crocomire's body program ended an update with {advance.Kind}.");
            return;
        }
        body.InstructionTimer = NativeCountdown.FromRaw((ushort)advance.Duration);
        state.BodyFrame = advance.Frame;
        body.SpritemapPointer = advance.Frame.Spritemap;
        body.ExtraProperties = body.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
    }

    /// <summary>
    /// Carries out one body-program cue. Movement, wall, sound, shake and dust cues keep their authored order;
    /// the fight AI and the spike-wall move may start another program, which the program set runs at once.
    /// </summary>
    /// <param name="state">The Crocomire encounter whose body runs the program.</param>
    /// <param name="samus">Samus, whom the fight AI measures against; null in fixtures without her.</param>
    /// <param name="level">The room's collision level, which the movement cues require.</param>
    /// <param name="cameraX">The camera's X, which the on-screen movement tests use.</param>
    /// <param name="cue">The cue to carry out.</param>
    /// <param name="continuation">The program the cursor rests in after the cue, which the fight AI compares.</param>
    /// <returns>The program to start, or null to continue after the cue.</returns>
    private CrocomireBodyProgram? ApplyCrocomireBodyCue(
        CrocomireEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        ushort cameraX,
        CrocomireBodyCue cue,
        CrocomireBodyProgram continuation)
    {
        RoomEnemySlot body = state.Body;
        switch (cue.Kind)
        {
            case CrocomireBodyCueKind.FightAi:
                return RunCrocomireFightInstruction(state, samus, continuation);

            case CrocomireBodyCueKind.MaybeStartProjectileAttack:
                if ((ReadCrocomireRandom() & 4095) < 1024)
                {
                    state.FightFunction = CrocomireFightFunction.ProjectileAttack;
                    state.ProjectileCounter = 0;
                    return CrocomireBodyProgram.ProjectileAttack;
                }
                return null;

            case CrocomireBodyCueKind.Cry:
                LastCrocomireSoundEffect = CrocomireSoundDefinitions.Cry;
                return null;
            case CrocomireBodyCueKind.BigExplosionSound:
                LastCrocomireSoundEffect = CrocomireSoundDefinitions.BigExplosion;
                return null;
            case CrocomireBodyCueKind.SkeletonCollapseSound:
                LastCrocomireSoundEffect = CrocomireSoundDefinitions.SkeletonCollapse;
                return null;
            case CrocomireBodyCueKind.ShakeScreen:
                _earthquake.EarthquakeType = 4;
                _earthquake.EarthquakeTimer = NativeCountdown.Start(5);
                LastCrocomireSoundEffect = CrocomireSoundDefinitions.ScreenShake;
                return null;

            case CrocomireBodyCueKind.MoveLeft:
                RequireCrocomireLevel(level);
                if ((state.FightFlags & CrocomireFightFlags.DamageReactionPending) == 0)
                    MoveCrocomire(body, level, -4);
                return null;
            case CrocomireBodyCueKind.MoveLeftWithDust:
                SpawnCrocomireRandomFootDust(state);
                RequireCrocomireLevel(level);
                if ((state.FightFlags & CrocomireFightFlags.DamageReactionPending) == 0)
                    MoveCrocomire(body, level, -4);
                return null;
            case CrocomireBodyCueKind.MoveLeftTowardSpikeWall:
                RequireCrocomireLevel(level);
                if (MoveCrocomire(body, level, -4))
                {
                    state.FightFunction = CrocomireFightFunction.BackingOffSpikeWall;
                    return CrocomireBodyProgram.BackOffFromSpikeWall;
                }
                ushort random = ReadCrocomireRandom();
                int baseOffset = unchecked((short)(random - 2048)) >= 0 ? -32 : 32;
                SpawnCrocomireDust(state, baseOffset + (random & 15));
                return null;
            case CrocomireBodyCueKind.MoveRightIfOnScreen:
                RequireCrocomireLevel(level);
                if (CrocomireBodyIsOnScreen(body, cameraX))
                    MoveCrocomire(body, level, 4);
                return null;
            case CrocomireBodyCueKind.MoveRight:
                RequireCrocomireLevel(level);
                MoveCrocomire(body, level, 4);
                return null;
            case CrocomireBodyCueKind.MoveRightIfOnScreenWithDust:
                SpawnCrocomireRandomFootDust(state);
                RequireCrocomireLevel(level);
                if (CrocomireBodyIsOnScreen(body, cameraX))
                    MoveCrocomire(body, level, 4);
                return null;
            case CrocomireBodyCueKind.MoveRightWithDust:
                SpawnCrocomireRandomFootDust(state);
                RequireCrocomireLevel(level);
                MoveCrocomire(body, level, 4);
                return null;
            case CrocomireBodyCueKind.Dust:
                SpawnCrocomireDust(state, cue.DustOffset);
                return null;

            default:
                throw new InvalidOperationException($"Undefined {nameof(CrocomireBodyCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Whether the body's left edge lies within 260 pixels right of the camera: the step-back moves' on-screen test.</summary>
    private static bool CrocomireBodyIsOnScreen(RoomEnemySlot body, ushort cameraX) =>
        unchecked((short)(body.Position.X.Whole - body.Radius.X - 260 - cameraX)) < 0;

    /// <summary>Ports $A4:86B3-$8A39, including the shipped but normally unused states.</summary>
    private CrocomireBodyProgram? RunCrocomireFightInstruction(
        CrocomireEnemyState state,
        SamusState? samus,
        CrocomireBodyProgram continuation)
    {
        switch (state.FightFunction)
        {
            case CrocomireFightFunction.ResetAnimation:
                state.Body.InstructionTimer = NativeCountdown.Start(1);
                return CrocomireBodyProgram.Initial;

            case CrocomireFightFunction.StepForward:
                state.FightFunction = CrocomireFightFunction.Sleeping;
                return CrocomireBodyProgram.StepForward;

            case CrocomireFightFunction.Sleeping:
                if (samus is not null && EnemyMath.WrappedMagnitude(unchecked((ushort)(
                        state.Body.Position.X.Whole - samus.Position.X.Whole))) < 224)
                {
                    state.FightFlags |= CrocomireFightFlags.Engaged;
                    state.FightFunction = CrocomireFightFunction.WaitingForFirstDamage;
                    return CrocomireBodyProgram.WaitForFirstSecondDamage;
                }
                return null;

            case CrocomireFightFunction.SteppingForward:
                if ((state.FightFlags & CrocomireFightFlags.DamageReactionPending) != 0)
                {
                    state.FightFlags &= ~CrocomireFightFlags.DamageReactionPending;
                    if (state.StepCounter != 0)
                    {
                        state.FightFunction = CrocomireFightFunction.SteppingBack;
                        return CrocomireBodyProgram.StepBack;
                    }
                }
                if (unchecked((short)(
                        state.Body.Position.X.Whole - CrocomireSpikeWallThreshold)) < 0)
                {
                    state.FightFunction = CrocomireFightFunction.NearSpikeWallCharge;
                    return CrocomireBodyProgram.NearSpikeWallCharge;
                }
                return continuation >= CrocomireBodyProgram.SteppingBack
                    ? CrocomireBodyProgram.StepForward
                    : null;

            case CrocomireFightFunction.ProjectileAttack:
                if ((state.FightFlags & CrocomireFightFlags.DamageReactionPending) != 0)
                {
                    state.FightFlags &= ~CrocomireFightFlags.DamageReactionPending;
                    state.FightFunction = CrocomireFightFunction.SteppingBack;
                    return CrocomireBodyProgram.StepBack;
                }
                if (state.ProjectileCounter < LastProjectileSpawnParameter)
                {
                    state.ProjectileCounter += 2;
                    SpawnCrocomireProjectile(state.Body, state.ProjectileCounter);
                    LastCrocomireSoundEffect = CrocomireSoundDefinitions.ProjectileSpit;
                    return null;
                }
                state.FightFunction = CrocomireFightFunction.SteppingForward;
                return CrocomireBodyProgram.StepForwardAfterDelay;

            case CrocomireFightFunction.NearSpikeWallCharge:
                if ((state.FightFlags & CrocomireFightFlags.DamageReactionPending) != 0)
                {
                    state.FightFlags &= ~CrocomireFightFlags.DamageReactionPending;
                    state.FightFunction = CrocomireFightFunction.SteppingBack;
                    return CrocomireBodyProgram.StepBack;
                }
                return null;

            case CrocomireFightFunction.SteppingBack:
                if (state.StepCounter != 0)
                    state.StepCounter--;
                if (state.StepCounter != 0)
                    return CrocomireBodyProgram.SteppingBack;
                state.FightFunction = CrocomireFightFunction.SteppingForward;
                return CrocomireBodyProgram.StepForward;

            case CrocomireFightFunction.BackingOffSpikeWall:
                if (unchecked((short)(
                        state.Body.Position.X.Whole - CrocomireSpikeWallThreshold)) >= 0)
                {
                    state.FightFunction = CrocomireFightFunction.SteppingForward;
                    return CrocomireBodyProgram.StepForward;
                }
                return null;

            case CrocomireFightFunction.RoarAndStepForwardUnused:
                state.FightFunction = CrocomireFightFunction.SteppingForward;
                return CrocomireBodyProgram.Roar;

            case CrocomireFightFunction.WaitingForFirstDamage:
                return RunCrocomireWaitingForDamage(
                    state,
                    continuation,
                    CrocomireFightFunction.WaitingForSecondDamage);

            case CrocomireFightFunction.WaitingForSecondDamage:
            case CrocomireFightFunction.WaitingForSecondDamageUnused:
                return RunCrocomireWaitingForDamage(
                    state,
                    continuation,
                    CrocomireFightFunction.SteppingBack);

            case CrocomireFightFunction.PowerBombCharge:
            {
                // The charge consumes one step and ends once fewer than two remain; a count that would drop below
                // zero ends it the same way, as the cartridge's wrapped word reads negative.
                int remaining = state.StepCounter - 1;
                if (remaining < 2)
                {
                    state.StepCounter = 0;
                    state.FightFunction = CrocomireFightFunction.SteppingForward;
                    return CrocomireBodyProgram.StepForward;
                }
                state.StepCounter = remaining;
                return null;
            }

            case CrocomireFightFunction.NearSpikeWallChargeUnused:
                if ((state.FightFlags & CrocomireFightFlags.DamageReactionPending) != 0)
                {
                    state.FightFlags = (state.FightFlags & (CrocomireFightFlags.UnusedPreferClawAttack | CrocomireFightFlags.UnusedAttackSequenceStarted |
                        CrocomireFightFlags.UnusedClawsMoving | CrocomireFightFlags.DamageReactionPending | CrocomireFightFlags.UnresolvedBit12)) |
                        CrocomireFightFlags.Engaged | CrocomireFightFlags.UnusedForwardStepsDone;
                    state.StepCounter = 1;
                    state.ReactionTimer = 10;
                    state.FightFunction = CrocomireFightFunction.SteppingBack;
                    LastCrocomireSoundEffect = CrocomireSoundDefinitions.UnusedChargeInterrupted;
                    return null;
                }
                state.FightFunction = CrocomireFightFunction.NearSpikeWallCharge;
                return CrocomireBodyProgram.RoarCloseMouth;

            case CrocomireFightFunction.ResetAnimationUnused:
                state.Body.InstructionTimer = NativeCountdown.Start(1);
                state.FightFlags |= CrocomireFightFlags.UnusedAttackSequenceStarted;
                state.StepCounter = 32;
                state.FightFunction = CrocomireFightFunction.ChooseAttackUnused;
                return CrocomireBodyProgram.Initial;

            case CrocomireFightFunction.ChooseAttackUnused:
                if ((state.FightFlags & CrocomireFightFlags.UnusedPreferClawAttack) != 0)
                {
                    state.Body.InstructionTimer = NativeCountdown.Start(1);
                    state.StepCounter = 16;
                    state.FightFunction = CrocomireFightFunction.MoveUntilSamusUnused;
                    return CrocomireBodyProgram.Initial;
                }
                state.FightFunction = CrocomireFightFunction.StepForwardUnused;
                return CrocomireBodyProgram.UnusedChargeForwardOneStep;

            case CrocomireFightFunction.StepForwardUnused:
                state.Body.InstructionTimer = NativeCountdown.Start(1);
                if (state.StepCounter == 0)
                {
                    state.FightFlags |= CrocomireFightFlags.UnusedForwardStepsDone;
                    state.FightFunction = CrocomireFightFunction.MoveClawsUnused;
                    return CrocomireBodyProgram.StepForward;
                }
                return CrocomireBodyProgram.Initial;

            case CrocomireFightFunction.MoveUntilSamusUnused:
                if (unchecked((short)(state.Body.Position.X.Whole - 672)) < 0)
                {
                    state.FightFunction = CrocomireFightFunction.MoveClawsUnused;
                    state.StepCounter = 3;
                    return CrocomireBodyProgram.StepForward;
                }
                if ((state.FightFlags & CrocomireFightFlags.ClawTouchedSamus) == 0)
                {
                    state.FightFunction = CrocomireFightFunction.StepForwardVariantUnused;
                    state.FightFlags &= ~CrocomireFightFlags.UnusedClawsMoving;
                    return CrocomireBodyProgram.MovingClaws;
                }
                state.StepCounter = 5;
                state.SavedFightFunction = state.FightFunction;
                state.FightFunction = CrocomireFightFunction.UnwrittenTableEntry;
                return CrocomireBodyProgram.MovingClaws;

            case CrocomireFightFunction.MoveClawsUnused:
                if (state.StepCounter == 0 || --state.StepCounter == 0)
                {
                    state.FightFunction = CrocomireFightFunction.MovingClawsUnused;
                    state.FightFlags &= ~CrocomireFightFlags.UnusedClawsMoving;
                    return CrocomireBodyProgram.StepForward;
                }
                state.FightFunction = CrocomireFightFunction.MoveClawsUnused;
                state.FightFlags |= CrocomireFightFlags.UnusedClawsMoving;
                return CrocomireBodyProgram.MovingClaws;

            case CrocomireFightFunction.StepForwardVariantUnused:
                if ((state.FightFlags & CrocomireFightFlags.UnusedForwardStepsDone) == 0)
                    state.FightFlags &= ~(CrocomireFightFlags.UnusedPreferClawAttack | CrocomireFightFlags.UnusedAttackSequenceStarted);
                state.FightFunction = CrocomireFightFunction.MovingClawsUnused;
                return CrocomireBodyProgram.StepForward;

            case CrocomireFightFunction.MovingClawsUnused:
                if (state.StepCounter == 0)
                {
                    state.FightFlags &= ~CrocomireFightFlags.ClawTouchedSamus;
                    state.Body.InstructionTimer = NativeCountdown.Start(1);
                    state.FightFunction = state.SavedFightFunction;
                    return CrocomireBodyProgram.MovingClaws;
                }
                if ((state.FightFlags & CrocomireFightFlags.ClawTouchedSamus) != 0)
                {
                    state.StepCounter--;
                    LastCrocomireSoundEffect = CrocomireSoundDefinitions.ClawStrike;
                    return CrocomireBodyProgram.MovingClaws;
                }
                state.FightFlags &= ~CrocomireFightFlags.ClawTouchedSamus;
                state.FightFunction = CrocomireFightFunction.SteppingBack;
                return null;

            default:
                throw new InvalidDataException(
                    $"Crocomire fight function {state.FightFunction} is not translated.");
        }
    }

    /// <summary>Holds the damage-reaction instruction until its native timer permits the next step.</summary>
    private static CrocomireBodyProgram? RunCrocomireWaitingForDamage(
        CrocomireEnemyState state,
        CrocomireBodyProgram continuation,
        CrocomireFightFunction damagedDestination)
    {
        if ((state.FightFlags & CrocomireFightFlags.DamageReactionPending) != 0)
        {
            state.FightFlags &= ~CrocomireFightFlags.DamageReactionPending;
            state.FightFunction = damagedDestination;
            return CrocomireBodyProgram.StepBack;
        }
        return continuation >= CrocomireBodyProgram.RoarCloseMouthLoop
            ? CrocomireBodyProgram.Roar
            : null;
    }

    /// <summary>Moves Crocomire through ordinary ignore-slopes collision with 16.16 displacement.</summary>
    private bool MoveCrocomire(RoomEnemySlot slot, RoomLevelData level, int pixels) =>
        // The instruction callbacks pass INT16_SHL16(±4) into the ordinary ignore-slopes
        // enemy mover; retaining 16.16 here preserves collision-edge alignment.
        slot.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire &&
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, SnesSixteenSixteen.FromWhole(pixels));

    /// <summary>Consumes the room's next RNG word for Crocomire effects and choices.</summary>
    private ushort ReadCrocomireRandom() => _room.Random.Read();

    /// <summary>Chooses a foot-dust offset from room RNG and spawns the matching effect.</summary>
    private void SpawnCrocomireRandomFootDust(CrocomireEnemyState state)
    {
        ushort random = ReadCrocomireRandom();
        int offset = random & 31;
        if (unchecked((short)(random - 4096)) >= 0)
            offset = -offset;
        SpawnCrocomireDust(state, offset);
    }

    /// <summary>Places a foot dust puff at the body offset with an additional three-bit RNG jitter.</summary>
    private void SpawnCrocomireDust(CrocomireEnemyState state, int xOffset)
    {
        ushort random = ReadCrocomireRandom();
        _projectiles.SpawnRoomGraphicsDustExplosion(
            new(unchecked((ushort)(state.Body.Position.X.Whole + xOffset + (random & 7))), unchecked((ushort)(state.Body.Position.Y.Whole + state.Body.Radius.Y - 16))),
            MiscDustAnimation.BigDustCloud);
    }

    /// <summary>Rejects death and melting operations when the arena collision level is unavailable.</summary>
    private static void RequireCrocomireLevel([NotNull] RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Crocomire movement requires room level data.");
    }
    /// <summary>
    /// <c>MainAI_Crocomire_DeathSequence_10_Hop_3_LoadMeltingTilemap</c> ($A4:9341). Only
    /// this first melt hands the tongue its melting program and moves it onto the body.
    /// </summary>
    internal void LoadFirstCrocomireMeltingTilemap(CrocomireEnemyState state)
    {
        ReadOnlySpan<ushort> tilemap = RequireCrocomireMeltingTilemap(
            CrocomireMeltingTilemapAddress.FirstTilemap);
        StartCrocomireMeltingTilemap(state, CrocomireBodyProgram.MeltingOneTopRow);

        if (state.Tongue is { } tongue)
        {
            StartCrocomireTongueProgram(state, CrocomireTongueProgram.Melting, NativeCountdown.Start(1));
            tongue.Properties = tongue.Properties.Replace(
                EnemyProperties.ProcessInstructions |
                    EnemyProperties.ProcessOffScreen |
                    EnemyProperties.IgnoreSamusCollision |
                    EnemyProperties.Invisible,
                EnemyProperties.ProcessInstructions |
                    EnemyProperties.ProcessOffScreen |
                    EnemyProperties.IgnoreSamusCollision);
            tongue.Position = tongue.Position.WithWhole(new(state.Body.Position.X.Whole, unchecked((ushort)(state.Body.Position.Y.Whole + 16))));
        }

        // $A4:938D-93A2 clears the first $400 bytes.
        WriteCrocomireMeltingTilemap(tilemap, clearedWords: 512);
    }

    /// <summary>
    /// <c>MainAI_Crocomire_DeathSequence_2C_Hop_6_LoadMeltingTilemap</c> ($A4:93ED). It
    /// first refills the BG2 Y-scroll HDMA table ($A4:93DF) and leaves the tongue alone.
    /// </summary>
    internal void LoadSecondCrocomireMeltingTilemap(CrocomireEnemyState state)
    {
        ReadOnlySpan<ushort> tilemap = RequireCrocomireMeltingTilemap(
            CrocomireMeltingTilemapAddress.SecondTilemap);
        FillCrocomireBg2ScrollTable(CrocomireBg2Scroll.Y);
        StartCrocomireMeltingTilemap(state, CrocomireBodyProgram.MeltingTwoTopRow);
        // $A4:940E-941D clears the first $800 bytes.
        WriteCrocomireMeltingTilemap(tilemap, clearedWords: 1024);
    }

    /// <summary>Requires a complete installed melting map before phase, actor, scratch, or VRAM state changes.</summary>
    internal ReadOnlySpan<ushort> RequireCrocomireMeltingTilemap(CrocomireMeltingTilemapAddress tilemapAddress) =>
        (_room.Artwork?.CrocomireMelting ?? throw new InvalidDataException(
            "Crocomire melting requires installed tilemap artwork.")).Tilemap(tilemapAddress);

    /// <summary>Starts the selected body melting list and synchronizes actor state with the new BG2 map.</summary>
    private void StartCrocomireMeltingTilemap(CrocomireEnemyState state, CrocomireBodyProgram bodyProgram)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        death.PixelsToErasePerColumn = 48;
        death.MeltTargetHeight = 48;
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
        StartCrocomireBodyProgram(state, bodyProgram);
    }

    /// <summary>Stages the melting map with its native blank lead-in and uploads the bounded BG2 span.</summary>
    private void WriteCrocomireMeltingTilemap(ReadOnlySpan<ushort> tilemap, int clearedWords)
    {
        Span<ushort> working = RequireCrocomireDeath().MutableBg2WorkingTilemap;
        working[..clearedWords].Fill(CrocomireBlankBg2Tile);
        int copiedWords = tilemap.Length;
        tilemap.CopyTo(working[32..]);

        // $A4:93BE receives byte count `tilemap bytes + $0400`, so the initial upload also
        // includes 512 blank leading words and retains blank space after the compact image.
        TransferCrocomireBg2Words(0, 512 + copiedWords);
    }

    /// <summary>Ports the shipped off-by-one ROM-to-$7E:4000 copies at $A4:943D.</summary>
    internal void InitializeCrocomireMeltingGraphics(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        CrocomireMeltingPass pass = CrocomireMeltingTransferDefinitions.Header(
            death.MeltingTableOffset);
        var artwork = _room.Artwork?.CrocomireMelting ?? throw new InvalidDataException(
            "Crocomire melting requires installed graphics artwork.");

        // Both assets have already passed geometry/padding validation during installation.
        // CopyPassTo checks scratch capacity before its first write. Only then publish the
        // new phase and distortion state; no cartridge reader is available as a fallback.
        artwork.CopyPassTo(pass.HeaderOffset, death.MutableMeltingGraphics);
        FillCrocomireBg2ScrollTable(CrocomireBg2Scroll.Y);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
        death.DistortionStep = 256;
        death.MeltingColumnCursor = 0;

        death.MaximumAdjustedDestinationY = pass.MaximumAdjustedDestinationY;
        death.AdjustedDestinationY = death.MaximumAdjustedDestinationY;
        death.DistortionEndY = pass.DistortionEndY;

        // Keep the native cursor so serialized mid-melt states resume at the same record.
        death.MeltingTableOffset = pass.TransferStartOffset;
        death.MeltingTransferOffset = 0;
        death.MutableMeltingColumnHeights.Clear();
    }

    /// <summary>Transfers the next installed melting record and advances the serializable upload cursor.</summary>
    internal void UploadNextCrocomireMeltingGraphicsSlice(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        int recordOffset = death.MeltingTableOffset + death.MeltingTransferOffset;
        if (!CrocomireMeltingTransferDefinitions.TryUpload(
            recordOffset, out CrocomireMeltingUpload upload))
        {
            state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
            death.MeltingTransferOffset = 0;
            return;
        }

        UploadCrocomireMeltingRecord(upload);
        death.MeltingTransferOffset += 8;
    }

    /// <summary>Writes one compiled melting graphics record to its native VRAM destination.</summary>
    [PpuLayoutBoundary("codec: Crocomire melting VRAM transfer records' native destinations")]
    private void UploadCrocomireMeltingRecord(CrocomireMeltingUpload upload)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        int byteCount = upload.ByteCount;
        int destinationWord = upload.DestinationWord;
        int sourceBank = upload.SourceBank;
        int source = upload.SourceWord;
        if (sourceBank != CrocomireMeltingTransferDefinitions.ScratchBank)
        {
            throw new InvalidDataException(
                $"Crocomire melting VRAM record uses unexpected source bank ${sourceBank:X2}.");
        }

        int sourceOffset = unchecked((ushort)(source - CrocomireMeltingTransferDefinitions.ScratchStart));
        if (sourceOffset < 0 || sourceOffset + byteCount > death.MeltingGraphics.Length)
            throw new InvalidDataException("Crocomire melting VRAM record exceeds the scratch image.");
        _presentation.Vram.LoadBytes(destinationWord * 2, death.MeltingGraphics.Slice(sourceOffset, byteCount));
    }

    /// <summary>Starts Crocomire's dissolve after installed graphics and tilemap data are ready.</summary>
    internal void BeginCrocomireMelting(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        death.MeltingHdmaActive = true;
        int lineCount = Math.Clamp((state.Tongue?.Position.Y.Whole ?? state.Body.Position.Y.Whole) - 72, 0, 256);
        Span<ushort> scrolls = death.MutableBg2ScrollByScanline;
        scrolls.Slice(0, lineCount).Fill(CrocomireBg2Scroll.Y);
        if (lineCount < scrolls.Length)
            scrolls.Slice(lineCount).Fill(CrocomireBg2Scroll.Y);

        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
        death.BodyXBeforeMelting = state.Body.Position.X.Whole;
    }

    /// <summary>Updates the acid dissolve, BG2 distortion, tongue placement, and scheduled melting graphics.</summary>
    internal void RunCrocomireMelting(CrocomireEnemyState state, SamusState? samus)
    {
        SpawnCrocomireAcidSmoke(state, samus);
        CrocomireDeathState death = RequireCrocomireDeath();
        death.MeltShakeCounter = death.MeltShakeCounter.Decrement().Remaining;
        state.Body.Position = state.Body.Position.WithWholeX(unchecked((ushort)(
            death.BodyXBeforeMelting + ((death.MeltShakeCounter.RawValue & 2) != 0 ? 4 : 0))));

        if (!EraseNextCrocomireMeltingColumn())
        {
            CompleteCrocomireMeltingDissolve(state);
            return;
        }

        // $A4:95D5 refreshes the BG2 scroll, which also moves the tongue onto the body.
        UpdateCrocomireBg2Scroll(state, includeVerticalPosition: true);
        int nextAdjustedY = death.AdjustedDestinationY - 3;
        if (death.AdjustedDestinationY < 19)
        {
            if (death.DistortionStep >= MaximumCrocomireDistortionStep)
            {
                CompleteCrocomireMeltingDissolve(state);
                return;
            }
            nextAdjustedY = 16;
        }
        death.AdjustedDestinationY = nextAdjustedY;
        death.DistortionStep = Math.Min(
            death.DistortionStep + 384,
            MaximumCrocomireDistortionStep);
        BuildCrocomireMeltingScrollTable(state, death);
    }

    /// <summary>Ports the retail column-order and four-bitplane mask loop at $A4:96C8.</summary>
    internal bool EraseNextCrocomireMeltingColumn()
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        Span<byte> heights = death.MutableMeltingColumnHeights;
        Span<byte> graphics = death.MutableMeltingGraphics;

        int cursor = death.MeltingColumnCursor;
        int xColumn;
        while (true)
        {
            // The retail table at $A4:9697 contains one permutation of X=0..48 and then
            // immediately falls into executable opcodes. The upstream C translation adds
            // this same `index > 48` guard to prevent those opcodes from becoming bogus X
            // coordinates while the distortion coefficient finishes its last few frames.
            // Returning success is important: it keeps the HDMA contraction running; false
            // would skip directly to the next death state several frames too early.
            if (cursor >= CrocomireMeltingDefinitions.ColumnCount)
            {
                death.MeltingColumnCursor = cursor;
                return true;
            }
            xColumn = CrocomireMeltingDefinitions.SelectColumn(cursor);
            if (heights[xColumn] < death.MeltTargetHeight)
                break;
            cursor++;
        }
        death.MeltingColumnCursor = cursor;

        // The shipped routine masks by [table index & 7], not [selected X & 7]. This is one
        // of its documented indexing bugs and materially changes the dissolve silhouette.
        byte mask = CrocomireMeltingDefinitions.SelectMask(cursor);
        int remaining = death.PixelsToErasePerColumn;
        do
        {
            int y = heights[xColumn];
            int byteIndex = 2 * (y & 7) + ((y & ~7) << 6) + 4 * (xColumn & ~7);
            if (byteIndex + 17 >= graphics.Length)
                throw new InvalidDataException("Crocomire melting column addressed outside its graphics image.");
            graphics[byteIndex] &= mask;
            graphics[byteIndex + 1] &= mask;
            graphics[byteIndex + 16] &= mask;
            graphics[byteIndex + 17] &= mask;
            if (heights[xColumn] == 48)
                break;
            heights[xColumn]++;
        } while (--remaining != 0);

        int recordOffset = death.MeltingTableOffset + death.MeltingTransferOffset;
        if (!CrocomireMeltingTransferDefinitions.TryUpload(
            recordOffset, out CrocomireMeltingUpload upload))
        {
            death.MeltingTransferOffset = 0;
            recordOffset = death.MeltingTableOffset;
            if (!CrocomireMeltingTransferDefinitions.TryUpload(recordOffset, out upload))
                throw new InvalidDataException("Crocomire melt pass has no first transfer.");
        }
        UploadCrocomireMeltingRecord(upload);
        death.MeltingTransferOffset += 8;
        return true;
    }

    /// <summary>Builds scanline BG2 offsets that distort Crocomire's body through the melting pass.</summary>
    private void BuildCrocomireMeltingScrollTable(
        CrocomireEnemyState state,
        CrocomireDeathState death)
    {
        Span<ushort> output = death.MutableBg2ScrollByScanline;
        int line = Math.Clamp((state.Tongue?.Position.Y.Whole ?? state.Body.Position.Y.Whole) - 72, 0, 256);
        int adjustedY = death.AdjustedDestinationY;
        int currentY = adjustedY;
        int fraction = 0;
        while (line < output.Length &&
               adjustedY < death.MaximumAdjustedDestinationY)
        {
            output[line++] = unchecked((ushort)(
                CrocomireBg2Scroll.Y + adjustedY - currentY));
            int sum = fraction + death.DistortionStep;
            bool carry = sum > ushort.MaxValue;
            fraction = SnesSixteenSixteen.FractionOf(sum);
            if (!carry)
                adjustedY++;
            currentY++;
        }
        if (line < output.Length)
            output.Slice(line).Fill(CrocomireBg2Scroll.Y);
    }

    /// <summary>Ends the current dissolve and selects the next death-scene instruction phase.</summary>
    internal void CompleteCrocomireMeltingDissolve(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        death.MeltingHdmaActive = false;
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();

        death.MeltingTableOffset = CrocomireMeltingTransferDefinitions.Transfers(
            death.MeltingTableOffset).NextHeaderOffset;
        death.MeltingTransferOffset = 0;
    }

    /// <summary>Restores the BG2 presentation and actor state after the final melting pass.</summary>
    internal void FinishCrocomireMeltingPass(CrocomireEnemyState state)
    {
        state.StartVerticalMotion(SnesEightEight.FromWhole(SinkStartingAccelerationWhole));
        ClearCrocomireBg2WorkingTilemap();
        TransferCrocomireBg2Words(0, 1024);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }
    /// <summary>Ports <c>EprojInit_CrocomireProjectile</c> at $86:9023.</summary>
    internal void SpawnCrocomireProjectile(RoomEnemySlot body, int spawnParameter)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CrocomireProjectile,
            graphicsIndex: EnemyPaletteBits.Palette5);
        projectile.StartAnimation(new CrocomireProjectileAnimation(CrocomireProjectileFrameKind.Mouth));
        projectile.Velocity = new(SnesSignedEightEight.FromSignedRaw(-512), new SnesSignedEightEight(1));
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(body.Position.X.Whole - 32)), unchecked((ushort)(body.Position.Y.Whole - 16))));
        projectile.GeneralTimer = 0;
        projectile.Position = projectile.Position.WithFractionX(0).WithFractionY(0);
        projectile.DirectionParameter = checked((ushort)spawnParameter);
    }

    /// <summary>Ports the one-frame vector setup at $86:906B.</summary>
    internal void StartCrocomireProjectileFlight(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        // The native setup performs one horizontal collision move and deliberately ignores
        // carry. It also writes the graphics word of physical projectile slot zero rather
        // than the current slot; retain that shipped indexing bug for debugger parity.
        EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true);
        _projectiles.EnemyProjectiles[0].GraphicsIndex = EnemyPaletteBits.Palette5;
        projectile.GeneralTimer = unchecked((ushort)(
            projectile.GeneralTimer + projectile.Velocity.X.RawValue));

        // Spawn parameters are 2,4,...,18. The last index reads one word beyond the declared
        // nine-word gradient table. The compiled definition includes those exact setup
        // instruction bytes; do not clamp the final shot to the last authored gradient.
        short gradient = CrocomireProjectileRomData.GradientForSpawnParameter(projectile.DirectionParameter);
        byte angle = EnemyMath.CalculateCartridgeAngle(new(-64, gradient));
        // The native angle starts at up, not right: Y uses negative cosine.
        projectile.Velocity = CalculateCrocomireProjectileVelocity(angle);
        projectile.PreInstruction =
            (ushort)EnemyProjectilePreInstruction.CrocomiresProjectile_Fired;
    }

    /// <summary>Ports <c>sub_8690B3</c>: delete on the first horizontal/vertical wall hit.</summary>
    internal static void RunCrocomireProjectileFlight(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.Clear();
        }
    }

    /// <summary>Projects a projectile angle into signed 8.8 X/Y speed using native word shifts.</summary>
    internal static EightEightVector<RoomPixels> CalculateCrocomireProjectileVelocity(byte angle)
    {
        // Preserve the two native word shifts, including negative two's-complement bits.
        return new(
            unchecked((short)(EnemyTrigonometryTables.SignedSine(angle) *
                CrocomireProjectileRomData.VelocityMultiplier)),
            unchecked((short)(EnemyTrigonometryTables.SignedNegativeCosineWord(angle) *
                CrocomireProjectileRomData.VelocityMultiplier)));
    }

    /// <summary>Ports <c>Crocomire_Func_52</c> and initializer $86:9286.</summary>
    internal void SpawnNextCrocomireBridgeFragment()
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        int fragment = death.BridgeFragmentsSpawned;
        if (fragment >= CrocomireBridgeFragmentDefinitions.FragmentCount)
            return;

        // The fragment counts as launched even when no projectile slot is free. The graphics argument the
        // cartridge passes alongside it is immaterial: the definition initializer immediately replaces it.
        death.BridgeFragmentsSpawned = fragment + 1;
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CrocomireBridgeCrumbling,
            graphicsIndex: 0);
        projectile.StartAnimation(new CrocomireProjectileAnimation(CrocomireProjectileFrameKind.BridgeFragment));
        projectile.Position = projectile.Position.WithWholeX(CrocomireBridgeFragmentDefinitions.XPosition(fragment));
        projectile.Position = projectile.Position.WithWholeY(187).WithFractionX(0).WithFractionY(0);
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(0));
        // $86:92A5 reads RandomNumberSeed; it does not advance it.
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)((_room.Random.Read() & 63) + 64))));
        projectile.GraphicsIndex = EnemyPaletteBits.Palette2;
    }

    /// <summary>
    /// Ports the eight descending allocations at $A4:99B6 and initializer
    /// $86:90CF. Its Y-position table at $86:9105-$9114 has eight unsigned
    /// words, all exactly $0038 + $0010*i for i=0..7 in the pinned NTSC
    /// J/U v1.0 ROM. Native even byte offsets 0..14 correspond to physical
    /// projectile slots 10..17; the checked managed index is slot minus 10.
    /// Clearing the pool before this call makes the spawn order Y=$A8 down
    /// to $38. $86:9115 starts the next routine, not a ninth table word.
    /// </summary>
    internal void SpawnCrocomireSpikeWallPieces()
    {
        CrocomireEnemyState state = Crocomire ??
            throw new InvalidOperationException("Crocomire spike wall spawned without an owner.");
        ushort graphicsIndex = unchecked((ushort)(
            state.Body.VramTilesIndex | state.Body.PaletteIndex));

        // The initializer indexes Y by native projectile index minus $14. Clearing the pool
        // immediately before this call guarantees slots 17..10 and Y=$38,$48,...,$A8.
        for (int count = 0; count < 8; count++)
        {
            RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
            if (projectile is null)
                return;

            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                projectile,
                RoomEnemyProjectileKind.CrocomireSpikeWallPieces,
                graphicsIndex);
            projectile.StartAnimation(new CrocomireProjectileAnimation(CrocomireProjectileFrameKind.SpikeWallPiece));
            int yTableIndex = projectile.SlotIndex - 10;
            if ((uint)yTableIndex >= 8)
            {
                throw new InvalidDataException(
                    $"Crocomire spike wall occupied unexpected projectile slot {projectile.SlotIndex}.");
            }

            projectile.Position = projectile.Position.WithWhole(new(528, unchecked((ushort)(56 + yTableIndex * 16))));
            projectile.Velocity = new(new SnesSignedEightEight(0), SnesSignedEightEight.FromSignedRaw(-5));
            projectile.Variable0 = 0;
            projectile.Variable1 = 34816;
            projectile.Position = projectile.Position.WithFractionX(0).WithFractionY(0);
        }
    }

    /// <summary>Ports the byte-precise slot-dependent shard motion at $86:9115.</summary>
    internal void RunCrocomireSpikeWallPiece(RoomEnemyProjectileSlot projectile)
    {
        int slot = projectile.SlotIndex;
        ushort maximumAcceleration = CrocomireSpikeMotionDefinitions.MaximumAcceleration(slot);
        ushort acceleration = projectile.Variable0;
        if (acceleration != maximumAcceleration)
        {
            acceleration = unchecked((ushort)(
                acceleration + CrocomireSpikeMotionDefinitions.AccelerationDelta(slot)));
            if (acceleration >= maximumAcceleration)
                acceleration = maximumAcceleration;
        }
        projectile.Variable0 = acceleration;

        // The 65816 switches to 8-bit A here. The high acceleration byte is added to the
        // fractional velocity byte; carry advances the signed integral velocity byte, which
        // is then capped by this physical slot's maximum.
        int velocityFractionSum = (SnesEightEight.FractionOf(projectile.Velocity.X.RawValue)) + (SnesLittleEndian.HighByte(acceleration));
        byte velocityFraction = unchecked((byte)velocityFractionSum);
        byte velocityWhole = unchecked((byte)(
            (SnesEightEight.WholeOf(projectile.Velocity.X.RawValue)) + (SnesCarry.OutOfByte(velocityFractionSum))));
        byte maximumVelocity = CrocomireSpikeMotionDefinitions.MaximumVelocity(slot);
        if (unchecked((sbyte)(velocityWhole - maximumVelocity)) >= 0)
            velocityWhole = maximumVelocity;
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(SnesLittleEndian.Word(velocityFraction, velocityWhole)))));

        // Move X with the same split-byte math. Only the high subposition byte participates;
        // its low byte remains untouched, exactly like EnemyProjectile_XSubPositions+1,X.
        SnesFixedPosition x = new SnesFixedPosition(projectile.Position.X.Whole, projectile.Position.X.Fraction)
            .AddEightEightToHighFraction(SnesLittleEndian.Word(velocityFraction, velocityWhole));
        projectile.Position = projectile.Position.WithFractionX(x.Fraction).WithWholeX(x.Whole);

        // Y velocity and position are ordinary signed 16.16 pairs, with $3000 acceleration.
        uint velocity = unchecked(SnesSixteenSixteen.Compose(projectile.Velocity.Y.RawValue, projectile.Variable1));
        velocity = unchecked(velocity + 12288u);
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(SnesSixteenSixteen.WholeOf(velocity)))));
        projectile.Variable1 = unchecked((ushort)velocity);
        uint position = unchecked(projectile.Position.Y.RawValue);
        position = unchecked(position + velocity);
        projectile.Position = projectile.Position.WithY(SnesFixedPosition.FromRaw(unchecked((uint)position)));

        if (projectile.Position.Y.Whole < 168)
            return;

        ushort impactX = projectile.Position.X.Whole;
        ushort impactY = projectile.Position.Y.Whole;
        int nativeIndex = projectile.SlotIndex * 2;
        projectile.Clear();
        if ((nativeIndex & 2) == 0)
            LastCrocomireSoundEffect = CrocomireSoundDefinitions.Crumble;
        _projectiles.SpawnRoomGraphicsDustExplosion(new(impactX, impactY), MiscDustAnimation.BigDustCloud);
        LastCrocomireSoundEffect = CrocomireSoundDefinitions.BigExplosion;
    }

    /// <summary>Ports bridge-fragment vertical collision and gravity at $86:92BA.</summary>
    internal static void RunCrocomireBridgeFragment(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.Clear();
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(
            (projectile.Velocity.Y.RawValue + 24) & 16383))));
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetCrocomireRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastCrocomireSoundEffect = null;
        LastCrocomireMusicRequest = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastCrocomireSoundEffect is { } sound)
            audio.QueueEnemySound(sound, maximumQueued: 6);
    }

    /// <summary>Queues this frame's published music requests through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacyMusic(EnemyAudioQueue audio)
    {
        audio.QueueLegacyMusic(LastCrocomireMusicRequest?.Command, LastCrocomireMusicRequest?.Delay);
    }

    /// <summary>Ports Crocomire's main AI.</summary>
    internal void RunCrocomireMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunCrocomireMain(slot, frame.Samus, frame.Camera);
    }

    /// <summary>Ports CrocomireTongue's main AI.</summary>
    internal static void RunCrocomireTongueMainAi(RoomEnemySlot _, IEnemyAiFrame _1)
    {
        // $A4:F6BB is a literal RTL. The tongue's bank-$A4 instruction list and
        // extended map position the component relative to Crocomire's body.
    }

    /// <summary>
    /// Registers the mouth projectile's typed shot program and its cues: the drop is allocated with Crocomire's
    /// chances before the deletion frees the exploding actor.
    /// </summary>
    internal void RegisterCrocomireDropProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterShotAnimation(RoomEnemyProjectileKind.CrocomireProjectile,
            () => new CrocomireProjectileAnimation(CrocomireProjectileFrameKind.Shot));
        table.RegisterCue<CrocomireProjectileCue>((projectile, samus, cue) =>
        {
            switch (cue)
            {
                case CrocomireProjectileCue.SpawnDrops:
                    _deathsAndDrops.SpawnEnemyDropFromEnemyHeader(projectile.Position.Whole, EnemyDefinitionId.Crocomire);
                    return;
                case CrocomireProjectileCue.Delete:
                    projectile.Clear();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(CrocomireProjectileCue)} {cue}.");
            }
        });
    }

    /// <summary>The callback of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindCrocomireBodyHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out ushort callback)
    {
        callback = 0;
        if (!(enemy.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire &&
            enemy.Definition.Bank == CrocomireBodyCollisionDefinitions.Bank &&
            CrocomireBodyCollisionDefinitions.HasFrame(enemy.SpritemapPointer)))
            return null;

        // Physical body components and their callback lists are compiled
        // independently of Crocomire's replaceable OAM/BG2 presentation.
        // Preserve native component/hitbox order: the first overlap wins.
        foreach (CrocomireBodyCollisionComponent component in
                 CrocomireBodyCollisionDefinitions.ComponentsAt(enemy.SpritemapPointer))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (CrocomireBodyCollisionHitbox hitbox in component.Hitboxes)
            {
                ushort left = unchecked((ushort)(componentOrigin.X + hitbox.Left));
                ushort top = unchecked((ushort)(componentOrigin.Y + hitbox.Top));
                ushort right = unchecked((ushort)(componentOrigin.X + hitbox.Right));
                ushort bottom = unchecked((ushort)(componentOrigin.Y + hitbox.Bottom));
                if (!probe.Overlaps(left, top, right, bottom))
                    continue;
                callback = probe.SelectShotCallback ? CrocomireHitboxCallbackCodec.ToCallbackWord(hitbox.Shot) : CrocomireHitboxCallbackCodec.ToCallbackWord(hitbox.Touch);
                return true;
            }
        }
        return false;
    
    }

    /// <summary>The callback of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindCrocomireTongueHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out ushort callback)
    {
        callback = 0;
        if (!(enemy.EnemyDefinitionPointer == EnemyDefinitionId.CrocomireTongue &&
            enemy.Definition.Bank == CrocomireTongueCollisionDefinitions.Bank &&
            (CrocomireTongueCollisionDefinitions.HasFrame(enemy.SpritemapPointer) ||
            probe.ArtworkInstalled)))
            return null;

        // These nine selected frames have one component each, but both
        // referenced native hitbox lists are empty. Validate the exact
        // engine-owned frame identity and return no touch/shot callback.
        CrocomireTongueCollisionComponent component =
            CrocomireTongueCollisionDefinitions.ComponentAt(enemy.SpritemapPointer);
        _ = CrocomireTongueCollisionDefinitions.HitboxCountAt(
            component.HitboxPointer);
        return false;
    
    }
}

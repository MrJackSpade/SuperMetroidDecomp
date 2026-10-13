using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Crocomire species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class CrocomireBehavior : IEnemySpecies
{
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
    private const ushort CrocomireBridgeThreshold = 0x0640;
    /// <summary>Body X threshold at the spiked wall that starts the death sequence.</summary>
    private const ushort CrocomireSpikeWallThreshold = 0x0300;
    /// <summary>Block mutations queued for the bridge, spike wall, and boss aftermath.</summary>
    internal readonly List<CrocomirePlmRequest> _crocomirePlmRequests = new();
    /// <summary>Camera X tracked during Crocomire's BG2 body and death presentation.</summary>
    private ushort _crocomireCameraX;

    /// <summary>Debugger-visible Crocomire owner while the current room contains $DDBF.</summary>
    public CrocomireEnemyState? Crocomire { get; internal set; }

    /// <summary>Typed WRAM extension used by Crocomire's bridge/melting/skeleton graph.</summary>
    public CrocomireDeathState? CrocomireDeath { get; internal set; }

    /// <summary>Hardcoded bank-$84 arena mutations published during the current frame.</summary>
    public IReadOnlyList<CrocomirePlmRequest> CrocomirePlmRequests => _crocomirePlmRequests;

    /// <summary>Delayed music publication from the current Crocomire frame.</summary>
    public CrocomireMusicRequest? LastCrocomireMusicRequest { get; private set; }

    /// <summary>The boss-specific item-drop request emitted after the skeleton collapses.</summary>
    public CrocomireDropRequest? LastCrocomireDropRequest { get; private set; }

    /// <summary>Literal BG2 scroll owned by Crocomire while its extended tilemap is active.</summary>
    public Position16<Bg2Pixels> CrocomireBg2Scroll { get; private set; }

    /// <summary>Most recent bank-$A4 library-two sound publication.</summary>
    public ushort? LastCrocomireSoundEffect { get; private set; }

    /// <summary>True after the $640 X threshold starts the native bridge-collapse phase.</summary>
    public bool CrocomireBridgeCollapseStarted { get; private set; }

    /// <summary>Clears Crocomire-owned room state and queued arena mutations on room entry.</summary>
    internal void ResetCrocomireRoomState()
    {
        Crocomire = null;
        CrocomireDeath = null;
        _crocomirePlmRequests.Clear();
        _crocomireCameraX = 0;
        LastCrocomireSoundEffect = null;
        LastCrocomireMusicRequest = null;
        LastCrocomireDropRequest = null;
        CrocomireBg2Scroll = Position16<Bg2Pixels>.Zero;
        CrocomireBridgeCollapseStarted = false;
    }

    /// <summary>Ports <c>InitAI_Crocomire</c> at $A4:8A5A.</summary>
    internal void InitializeCrocomire(RoomEnemySlot slot)
    {
        _roomStatus.BossId = 6;
        var state = new CrocomireEnemyState(slot);
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
            InstallCrocomireInstructionList(
                slot,
                CrocomireInstructionProgramDefinitions.Dead);
            slot.Position = slot.Position.WithWhole(new(0x0240, 0x0090));
            slot.Radius = new(0x0028, 0x001c);
            _room.Services.RoomScroll.Write(0, RoomScrollState.Blue);
            _room.Services.RoomScroll.Write(1, RoomScrollState.Blue);
            _room.Services.RoomScroll.Write(2, RoomScrollState.Blue);
            _room.Services.RoomScroll.Write(3, RoomScrollState.Blue);
            PublishCrocomirePlm(new Position16<RoomBlocks>(0x20, 0x03), PlmHeaderId.ClearCrocomireInvisibleWall);
            PublishCrocomirePlm(new Position16<RoomBlocks>(0x1e, 0x03), PlmHeaderId.ClearCrocomireInvisibleWall);
            PublishCrocomirePlm(new Position16<RoomBlocks>(0x61, 0x0b), PlmHeaderId.ClearCrocomireBridge);
            TransferCrocomireBg2Words(0, 1024);
            return;
        }

        state.DeathSequenceIndex = CrocomireDeathPhase.Fighting;
        state.ReactionTimer = 0;
        state.FightFunction = CrocomireFightFunction.Sleeping;
        // $A4:8ABA-8ABD, on the living branch only.
        _zebetiteBehavior.CameraDistanceIndex = CameraDistanceMode.BossTracking;
        InstallCrocomireInstructionList(
            slot,
            CrocomireInstructionProgramDefinitions.Initial);
        slot.ExtraProperties = slot.ExtraProperties.With(
            EnemyExtraProperties.UsesExtendedSpritemap);
        _room.Services.RoomScroll.Write(0, RoomScrollState.RedBoundary);
        _room.Services.RoomScroll.Write(1, RoomScrollState.RedBoundary);

        // The initializer copies seventeen words, not sixteen: X starts at $20 and reaches
        // zero inclusively. Preserve that palette-boundary write because later fades compare
        // the exact target image produced by the cartridge.
        (_room.Artwork?.CrocomireColors ?? throw new InvalidOperationException(
            "Crocomire requires installed color artwork.")).ApplyInitial(_presentation._cgram!);
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

        InstallCrocomireInstructionList(
            slot,
            CrocomireTongueInstructionProgramDefinitions.Fight);
        slot.ExtraProperties = slot.ExtraProperties
            .With(EnemyExtraProperties.UsesExtendedSpritemap)
            .WithUntranslatedExtraBits(
                EnemyExtraPropertyRawBits.CrocomireInitializerBit0400,
                "Crocomire initializer");
        slot.VariableA = 23;
        slot.PaletteIndex = EnemyPaletteBits.Palette7;
        if (Crocomire is not null)
            Crocomire.Tongue = slot;
    }

    /// <summary>Runs the live-fight phase of <c>MainAI_Crocomire</c> at $A4:8C04.</summary>
    internal void RunCrocomireMain(
        RoomEnemySlot slot,
        SamusState? samus,
        ushort cameraX)
    {
        CrocomireEnemyState state = RequireCrocomire(slot);
        _crocomireCameraX = cameraX;

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
        InstallCrocomireInstructionList(
            body,
            CrocomireInstructionProgramDefinitions.BridgeCollapsed);
        body.Properties = body.Properties.With(EnemyProperties.SolidToSamus);
        state.ReactionTimer = 0;
        state.ProjectileCounter = 0;
        state.VerticalAcceleration = 0x0800;
        body.Radius = body.Radius.WithY(16);
        LastCrocomireSoundEffect = 0x003b;
        CrocomireDeathState death = RequireCrocomireDeath();
        death.BridgeFragmentCursor = 0;
        death.AcidSmokeTimer = NativeCountdown.Start(1);
        death.AcidSoundTimer = NativeCountdown.Start(1);

        PublishCrocomireBridgeCollapsePlms();

        if (state.Tongue is { } tongue)
        {
            tongue.InstructionTimer = NativeCountdown.Start(0x7fff);
            tongue.CurrentInstruction =
                CrocomireTongueInstructionProgramDefinitions.Sleep;
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
                "Crocomire hurt palette requires installed artwork.")).ApplyFightBody(_presentation._cgram!);
            return;
        }
        _presentation.BossPalettes.CrocomireFightBody.Fill(Bgr555.White);
    }

    /// <summary>Switches an actor's instruction pointer and resets its timer for the next native list.</summary>
    private static void InstallCrocomireInstructionList(RoomEnemySlot slot, ushort pointer)
    {
        slot.CurrentInstruction = pointer;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
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
    internal void RegisterCrocomireBombReactions(EnemyAiTable table)
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

    /// <summary>Connects Crocomire body and component initialization pointers to encounter setup.</summary>
    internal void RegisterCrocomireInitializationAi(EnemyAiTable table)
    {
        table.RegisterInitialization(EnemyAiRoutine.InitAI_Crocomire, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeCrocomire(slot);
        });

        table.RegisterInitialization(EnemyAiRoutine.InitAI_CrocomireTongue, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.CrocomireTongue, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeCrocomireTongue(slot);
        });
    }

    /// <summary>Connects live Crocomire, tongue, and death-sequence dispatch pointers.</summary>
    internal void RegisterCrocomireMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Crocomire, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire, (slot, frame, nmiFrameCounter8) =>
        {
            RunCrocomireMain(slot, frame.Samus, frame.Camera.X);
        });
    }

    /// <summary>Installs Crocomire's bank-$A4 instruction callbacks for motion and arena events.</summary>
    internal void RegisterCrocomireOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessCrocomireInstruction(slot, context.Samus, context.Level, word, ref cursor, context.Camera.X);
        });
    }

    /// <summary>Routes power-bomb hits on each Crocomire component to their authored reactions.</summary>
    internal void RegisterCrocomirePowerBombReactions(EnemyAiTable table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.CrocomirePowerBomb)
        {
            ReplacesCommon = (enemy, _) => ResolveCrocomirePowerBombReaction(enemy),
        });
    }

    /// <summary>Routes body, mouth, and auxiliary hitboxes to their distinct shot reactions.</summary>
    internal void RegisterCrocomireShotReactions(EnemyAiTable table)
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
    internal void RegisterCrocomireTouch(EnemyAiTable table)
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
                    crocomire.FightFlags |= 0x4000;
                    context.Samus.Kinematics.ExtraDisplacement = context.Samus.Kinematics.ExtraDisplacement.WithWholeX(-4);
                }
            });
    }
    /// <summary>Queues a native PLM block change for later room processing.</summary>
    private void PublishCrocomirePlm(Position16<RoomBlocks> block, PlmHeaderId header) =>
        _crocomirePlmRequests.Add(new CrocomirePlmRequest(block, header));

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
                _projectiles.SpawnRoomGraphicsDustExplosion(new(1536, 176), animationIndex: 0x0015);
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
                    new Position16<RoomBlocks>(0x61, 0x0b), PlmHeaderId.CrumbleCrocomireBridgeBlock);
                _projectiles.SpawnRoomGraphicsDustExplosion(new(1568, 176), animationIndex: 0x0015);
            }
            return;
        }

        // $A4:8DED-8E2C: two blocks deep crumbles the next pair.
        if (unchecked((short)(body.Position.X.Whole - 1584)) < 0 &&
            !death.BridgeBlocksAt1584Crumbling)
        {
            death.BridgeBlocksAt1584Crumbling = true;
            PublishCrocomirePlm(
                new Position16<RoomBlocks>(0x62, 0x0b), PlmHeaderId.CrumbleCrocomireBridgeBlock);
            PublishCrocomirePlm(
                new Position16<RoomBlocks>(0x63, 0x0b), PlmHeaderId.CrumbleCrocomireBridgeBlock);
            _projectiles.SpawnRoomGraphicsDustExplosion(new(1584, 176), animationIndex: 0x0015);
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
        for (byte blockX = 0x61; blockX <= 0x6a; blockX++)
            PublishCrocomirePlm(
                new Position16<RoomBlocks>(blockX, 0x0b), PlmHeaderId.ClearCrocomireBridgeBlock);

        for (int puff = 0; puff < CrocomireBridgeDustCount; puff++)
        {
            (ushort x, ushort y) = CrocomireBridgeDustPosition(puff);
            _projectiles.SpawnRoomGraphicsDustExplosion(new(x, y), animationIndex: 0x0015);
        }

        PublishCrocomirePlm(new Position16<RoomBlocks>(0x4e, 0x03), PlmHeaderId.CreateCrocomireInvisibleWall);
    }

    /// <summary>Number of dust puffs emitted across the bridge-collapse path.</summary>
    private const int CrocomireBridgeDustCount = 7;

    /// <summary>
    /// $A4:8F35-8FC1 (seven unrolled spawns): puffs sixteen pixels apart along the bridge from
    /// X $600; the first and third rise sixteen pixels above the others' Y $C0.
    /// </summary>
    internal static (ushort X, ushort Y) CrocomireBridgeDustPosition(int puff) =>
        (uint)puff < CrocomireBridgeDustCount
            ? ((ushort)(0x0600 + 0x10 * puff), (ushort)(puff is 0 or 2 ? 0x00b0 : 0x00c0))
            : throw new ArgumentOutOfRangeException(nameof(puff));
    /// <summary>BG2 tile word used to clear Crocomire's working background rows.</summary>
    private const ushort CrocomireBlankBg2Tile = 0x0338;

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

        new EnemyVram(_presentation._vram!).EnemyBg2Tilemap.Write(
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
        int byteOffset = 8 * ((CrocomireBg2Scroll.Y + 288) & 0xfff8);
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

        // The tongue is a normal second enemy actor whose var A is the body-relative X
        // offset. $8BA4 updates it even while the body's bulk is represented by BG2.
        if (state.Tongue is { } tongue)
        {
            tongue.Position = tongue.Position.WithWhole(new(unchecked((ushort)(body.Position.X.Whole + tongue.VariableA)), body.Position.Y.Whole));
        }

        ushort horizontal;
        if (unchecked((short)(body.Position.X.Whole - _crocomireCameraX)) >= 0)
        {
            horizontal = unchecked((short)(
                body.Position.X.Whole - 128 - (_crocomireCameraX + 256))) < 0
                ? CalculateOnScreenCrocomireBg2Horizontal(body.Position.X.Whole)
                : (ushort)256;
        }
        else
        {
            horizontal = unchecked((short)(
                body.Position.X.Whole + 128 - _crocomireCameraX)) < 0
                ? (ushort)256
                : CalculateOnScreenCrocomireBg2Horizontal(body.Position.X.Whole);
        }
        CrocomireBg2Scroll = CrocomireBg2Scroll.WithX(horizontal);
    }

    /// <summary>Converts the body X position into the BG2 horizontal scroll used to keep Crocomire onscreen.</summary>
    private ushort CalculateOnScreenCrocomireBg2Horizontal(ushort bodyX)
    {
        ushort relative = unchecked((ushort)(_crocomireCameraX - bodyX + 51));
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
                ushort value = unchecked((ushort)(state.FightFlags & 0x000f));
                if (unchecked((short)(value - 15)) < 0)
                    value++;
                state.FightFlags |= value;
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
            body.Position.X.Whole - body.Radius.X - 256 - _crocomireCameraX)) < 0;
        ushort stepCount = 0;
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
            state.StepCounter = unchecked((ushort)(state.StepCounter + stepCount));
            ushort lowCount = unchecked((ushort)(state.FightFlags & 0x000f));
            if (unchecked((short)(lowCount - 15)) < 0)
                lowCount++;

            if ((state.FightFlags & 0x0800) == 0)
            {
                // Both regional constants at $A4:8692/$8694 are eight. Keep the branch
                // explicit because the source distinguishes projectile-attack state even
                // though this ROM revision stores identical delay values.
                body.InstructionTimer = NativeCountdown.FromRaw(unchecked((ushort)(
                    body.InstructionTimer.RawValue +
                    (state.FightFunction == CrocomireFightFunction.ProjectileAttack ? 8 : 8))));
            }
            state.FightFlags = unchecked((ushort)(
                lowCount | (state.FightFlags & 0xb7f0) | 0x0800));
            state.ReactionTimer = 10;
        }

        body.FlashTimer = NativeCountdown.FromRaw(unchecked((ushort)(body.FlashTimer.RawValue + 14)));
        body.AiHandlerBits |= 0x0002;
    }

    /// <summary>Places a dust effect at the shot impact while preserving its component-relative offset.</summary>
    private void SpawnCrocomireShotDust(
        SamusProjectileTypeWord projectileType,
        Position16<RoomPixels> projectile)
    {
        ushort animationIndex = projectileType.Family != SamusProjectileFamily.SuperMissile
            ? (ushort)6
            : (ushort)29;
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

        state.FightFlags = unchecked((ushort)((state.FightFlags & 0x3ff0) | 0x8000));
        state.ReactionTimer = 10;
        body.FlashTimer = NativeCountdown.FromRaw(unchecked((ushort)(body.FlashTimer.RawValue + 4)));
        body.AiHandlerBits |= 0x0002;
        state.FightFunction = CrocomireFightFunction.PowerBombCharge;

        ushort reactionList = SelectCrocomirePowerBombInstructionList(body);
        InstallCrocomireInstructionList(body, reactionList);
    }

    /// <summary>
    /// Selects the fully-open/part-open/closed reaction list by scanning the ordinary
    /// spritemap pointers embedded in the current extended map, exactly as $A4:B9D8 does.
    /// </summary>
    private static ushort SelectCrocomirePowerBombInstructionList(RoomEnemySlot body) =>
        (body.SpritemapPointer & 0x8000) == 0
            ? CrocomireInstructionProgramDefinitions.PowerBombReactionMouthNotOpen
            : CrocomirePowerBombReactionDefinitions.ForFrame(body.SpritemapPointer);
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
                LastCrocomireSoundEffect = 0x0077;
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
                SelectCrocomireRisingInstruction(state.Body);
                SpawnCrocomireAcidSmoke(state, samus);
                RunCrocomireRise(state);
                return;
            case CrocomireDeathPhase.StartSecondDissolve:
                LastCrocomireSoundEffect = 0x002d;
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
                if (unchecked((short)(
                        state.Body.CurrentInstruction -
                        CrocomireInstructionProgramDefinitions.SkeletonStable)) >= 0)
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
        LastCrocomireSoundEffect = 0x0022;
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
        state.ReactionTimer = 0x0300;
    }

    /// <summary>Byte-for-byte fixed-point fall from <c>SinkCrocomireDown</c> at $A4:91C1.</summary>
    private void RunCrocomireSink(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        FillCrocomireBg2ScrollTable(CrocomireBg2Scroll.Y);
        state.FightFlags &= 0xf7ff;
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

        byte accelerationFraction = (byte)state.VerticalAcceleration;
        int accelerationCarry = accelerationFraction + 0x80 > 0xff ? 1 : 0;
        accelerationFraction = unchecked((byte)(accelerationFraction + 0x80));
        int accelerationWhole = SnesUnsignedEightEight.Whole(state.VerticalAcceleration) + 3 + accelerationCarry;
        accelerationWhole = Math.Min(accelerationWhole, 48);
        state.VerticalAcceleration = unchecked((ushort)((SnesEightEight.FromWhole(accelerationWhole)) | accelerationFraction));

        byte speedFraction = (byte)state.ReactionTimer;
        int speedCarry = speedFraction + accelerationWhole > 0xff ? 1 : 0;
        speedFraction = unchecked((byte)(speedFraction + accelerationWhole));
        int speedWhole = SnesUnsignedEightEight.Whole(state.ReactionTimer) + speedCarry;
        speedWhole = Math.Min(speedWhole, 3);
        state.ReactionTimer = unchecked((ushort)((SnesEightEight.FromWhole(speedWhole)) | speedFraction));

        byte subpositionHigh = SnesLittleEndian.HighByte(state.ProjectileCounter);
        int subpositionSum = subpositionHigh + speedFraction;
        subpositionHigh = unchecked((byte)subpositionSum);
        state.ProjectileCounter = unchecked((ushort)(
            SnesLittleEndian.Word(SnesLittleEndian.LowByte(state.ProjectileCounter), subpositionHigh)));
        int yDelta = speedWhole + (subpositionSum > 0xff ? 1 : 0);
        body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + yDelta)));
    }

    /// <summary>Byte-for-byte fixed-point rise from $A4:92D8.</summary>
    private void RunCrocomireRise(CrocomireEnemyState state)
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
        int acceleration = state.VerticalAcceleration + 0x0100;
        state.VerticalAcceleration = unchecked((ushort)Math.Min(acceleration, 0x1f00));
        int accelerationWhole = SnesLittleEndian.HighByte(state.VerticalAcceleration);

        byte speedFraction = (byte)state.ReactionTimer;
        bool speedBorrow = speedFraction < accelerationWhole;
        speedFraction = unchecked((byte)(speedFraction - accelerationWhole));
        int speedWhole = SnesUnsignedEightEight.Whole(state.ReactionTimer) - (speedBorrow ? 1 : 0);
        if (speedWhole < 0)
        {
            speedFraction = 0xff;
            speedWhole = 0;
        }
        state.ReactionTimer = unchecked((ushort)((SnesEightEight.FromWhole(speedWhole)) | speedFraction));

        byte subpositionHigh = SnesLittleEndian.HighByte(state.ProjectileCounter);
        bool subpositionBorrow = subpositionHigh < speedFraction;
        subpositionHigh = unchecked((byte)(subpositionHigh - speedFraction));
        state.ProjectileCounter = unchecked((ushort)(
            SnesLittleEndian.Word(SnesLittleEndian.LowByte(state.ProjectileCounter), subpositionHigh)));
        int yDelta = speedWhole + (subpositionBorrow ? 1 : 0);
        body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - yDelta)));
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
        ushort xOffset = unchecked((ushort)(random & 0x003f));
        if ((random & 2) == 0)
            xOffset = unchecked((ushort)~xOffset);
        ushort acidY = samus?.LiquidPhysics.LavaAcidYPosition ?? 0x00e0;
        _spriteObjects.SpawnRoomSpriteObject(
            new(unchecked((ushort)(state.Body.Position.X.Whole + xOffset)), unchecked((ushort)(acidY + 16 - ((SnesLittleEndian.HighByte(random) & 0x1f))))),
            RoomSpriteObjectKind.CrocomireAcidSmoke,
            graphicsIndex: 0);
    }

    /// <summary>Selects the body instruction list for the skeleton's rising pose.</summary>
    private static void SelectCrocomireRisingInstruction(RoomEnemySlot body)
    {
        ushort pointer = body.Position.Y.Whole < 248
            ? CrocomireInstructionProgramDefinitions.MeltingTwoTopFourRows
            : body.Position.Y.Whole < 264
                ? CrocomireInstructionProgramDefinitions.MeltingTwoTopThreeRows
                : body.Position.Y.Whole < 280
                    ? CrocomireInstructionProgramDefinitions.MeltingTwoTopTwoRows
                    : CrocomireInstructionProgramDefinitions.MeltingTwoTopRow;
        InstallCrocomireInstructionList(body, pointer);
    }

    /// <summary>Completes the acid sink and hands control to the skeleton river sequence.</summary>
    private void RunCrocomireFinalSink(CrocomireEnemyState state, SamusState? samus)
    {
        SelectCrocomireRisingInstruction(state.Body);
        SpawnCrocomireAcidSmoke(state, samus);
        RunCrocomireSink(state);
        if (state.DeathSequenceIndex != CrocomireDeathPhase.WaitForSamusAtWall)
            return;

        LastCrocomireMusicRequest = new CrocomireMusicRequest(
            MusicCommand.SelectTrack(6),
            MusicCommandDelay.EightFrames);
        state.DeathSequenceIndex = CrocomireDeathPhase.RiverSkeletonDetour;
        InstallCrocomireInstructionList(
            state.Body,
            CrocomireInstructionProgramDefinitions.SkeletonFlowingDownRiver);
        _room.Services.RoomScroll.Write(4, RoomScrollState.Blue);
        _room.Services.RoomScroll.Write(5, RoomScrollState.Blue);
        if (state.Tongue is { } tongue)
            tongue.Properties = tongue.Properties.With(EnemyProperties.Deleted);
        PublishCrocomirePlm(new Position16<RoomBlocks>(0x4e, 0x03), PlmHeaderId.ClearCrocomireInvisibleWall);
        _zebetiteBehavior.CameraDistanceIndex = CameraDistanceMode.NormalTracking; // $A4:90D8
        RequireCrocomireDeath().TargetHeightOrSkeletonTileIndex = 0;
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
        PublishCrocomirePlm(new Position16<RoomBlocks>(0x30, 0x03), PlmHeaderId.CreateCrocomireInvisibleWall);
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
        ushort target = unchecked((ushort)definition.TargetYOffset);
        if (definition.IsTerminator)
        {
            death.RumbleYOffset = 0x8080;
            state.DeathPhaseDelay = NativeCountdown.Start(0x0080);
            (_room.Artwork?.CrocomireColors ?? throw new InvalidOperationException(
                "Crocomire wall spikes require installed colors.")).ApplyWallSpikes(_presentation._cgram!);
            state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
            return;
        }

        if (death.RumbleYOffset == target)
        {
            if (unchecked((short)target) < 0)
            {
                if (!death.RumbleCooldown.IsZero)
                {
                    death.RumbleCooldown = death.RumbleCooldown.Decrement().Remaining;
                    state.RumbleTableOffset -= 2;
                    LastCrocomireSoundEffect = 0x002b;
                    return;
                }

                death.RumbleCooldown = NativeCountdown.FromRaw(definition.Cooldown);
                death.RumbleDelta = definition.Delta;
            }
            state.RumbleTableOffset = definition.NextTargetOffset;
            return;
        }

        death.RumbleYOffset = unchecked((short)(death.RumbleYOffset - target)) >= 0
            ? unchecked((ushort)(death.RumbleYOffset - death.RumbleDelta))
            : unchecked((ushort)(death.RumbleYOffset + death.RumbleDelta));
    }

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
        state.ReactionTimer = 0;
        state.ProjectileCounter = 0;
        PublishCrocomirePlm(new Position16<RoomBlocks>(0x20, 0x03), PlmHeaderId.ClearCrocomireInvisibleWall);
        PublishCrocomirePlm(new Position16<RoomBlocks>(0x1e, 0x03), PlmHeaderId.CreateCrocomireInvisibleWall);
        PublishCrocomirePlm(new Position16<RoomBlocks>(0x70, 0x0b), PlmHeaderId.ClearCrocomireBridge);
        LastCrocomireSoundEffect = 0x0029;
        InstallCrocomireInstructionList(
            body,
            CrocomireInstructionProgramDefinitions.SkeletonFallsApart);
        body.PaletteIndex = 0;
        (_room.Artwork?.CrocomireColors ?? throw new InvalidOperationException(
            "Crocomire skeleton requires installed colors.")).ApplySkeletonArm(_presentation._cgram!);

        foreach (RoomEnemyProjectileSlot projectile in _projectiles.EnemyProjectiles)
            projectile.Clear();
        SpawnCrocomireSpikeWallPieces();
        LastCrocomireSoundEffect = 0x0030;
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

    /// <summary>Holds the broken-wall pose before launching the skeleton arc.</summary>
    private void RunCrocomireWallBreakDelay(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        if (unchecked((short)(state.Body.Position.X.Whole - 224)) < 0)
        {
            (state.ReactionTimer, state.ProjectileCounter) = CalculateCrocomireVelocity(
                state.ReactionTimer,
                state.ProjectileCounter,
                0x00008000,
                2);
            state.Body.Position = state.Body.Position.WithX(CalculateCrocomirePosition(
                state.Body.Position.X,
                state.ReactionTimer,
                state.ProjectileCounter));
        }

        if (death.RumbleCooldown.IsZero)
            return;
        death.RumbleCooldown = death.RumbleCooldown.Decrement().Remaining;
        if (!death.RumbleCooldown.IsZero)
            return;
        state.ReactionTimer = 0;
        InstallCrocomireInstructionList(
            state.Body,
            CrocomireInstructionProgramDefinitions.SkeletonFalling);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

    /// <summary>Moves the skeleton through its airborne arc after the wall break.</summary>
    private void RunCrocomireSkeletonArc(CrocomireEnemyState state)
    {
        (state.ReactionTimer, state.ProjectileCounter) = CalculateCrocomireVelocity(
            state.ReactionTimer,
            state.ProjectileCounter,
            0x00000800,
            5);
        state.Body.Position = state.Body.Position.WithY(CalculateCrocomirePosition(
            state.Body.Position.Y,
            fraction: 0xe000,
            whole: 0));
        state.Body.Position = state.Body.Position.WithX(CalculateCrocomirePosition(
            state.Body.Position.X,
            state.ReactionTimer,
            state.ProjectileCounter));
        if (unchecked((short)(state.Body.Position.X.Whole - 576)) < 0)
            return;

        LastCrocomireSoundEffect = 0x0025;
        if (state.Tongue is { } tongue)
            tongue.PaletteIndex = state.Body.PaletteIndex;
        InstallCrocomireInstructionList(
            state.Body,
            CrocomireInstructionProgramDefinitions.SkeletonFallsApart);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

    /// <summary>Finishes the skeleton collapse and scatters sixteen independently chosen enemy drops.</summary>
    private void RunCrocomireSkeletonCollapse(CrocomireEnemyState state)
    {
        if (unchecked((short)(
                state.Body.CurrentInstruction -
                CrocomireInstructionProgramDefinitions.SkeletonStable)) < 0)
        {
            (state.ReactionTimer, state.ProjectileCounter) = CalculateCrocomireVelocity(
                state.ReactionTimer,
                state.ProjectileCounter,
                0x00001000,
                6);
            return;
        }

        InstallCrocomireInstructionList(
            state.Body,
            CrocomireInstructionProgramDefinitions.Dead);
        state.Body.Position += new Offset16<RoomPixels>(64, 21);
        state.Body.Radius = new(40, 28);
        PublishCrocomirePlm(new Position16<RoomBlocks>(0x30, 0x03), PlmHeaderId.ClearCrocomireInvisibleWall);
        LastCrocomireDropRequest = new CrocomireDropRequest();

        // $A0:B995 emits sixteen independent $F337 projectiles across Crocomire's
        // authored arena rectangle. These are not a single pickup at the corpse position:
        // each actor advances RNG again while selecting its own drop from header $DDBF.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.Crocomire,
            count: 16,
            xBase: 576,
            xMask: 0x007f,
            yBase: 96,
            yMask: 0x3f00);
        RequireCrocomireDeath().ItemDropRequested = true;
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

    /// <summary>Restores arena scroll and camera tracking after the skeleton sequence.</summary>
    private void FinishCrocomireArenaScrolls(CrocomireEnemyState state)
    {
        for (int index = 0; index < 4; index++)
            _room.Services.RoomScroll.Write(index, RoomScrollState.Blue);
        PublishCrocomirePlm(new Position16<RoomBlocks>(0x1e, 0x03), PlmHeaderId.ClearCrocomireInvisibleWall);
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
        int entry = death.TargetHeightOrSkeletonTileIndex >> 1;
        var artwork = _room.Artwork?.CrocomireSkeleton ?? throw new InvalidDataException(
            "Installed Crocomire artwork is missing its skeleton characters.");
        if (!CrocomireSkeletonTransferDefinitions.TryGet(entry, out
                CrocomireSkeletonTransferDefinition frame))
            return;
        // The native OBSEL base contributes $6000 words. Only character
        // pixels are editable; the transfer order and VRAM position are not.
        new EnemyVram(_presentation._vram!).CrocomireSkeletonTiles.Load(artwork.Chunk(entry).Span,
            frame.DestinationOffset * sizeof(ushort));
        death.TargetHeightOrSkeletonTileIndex += 2;
    }

    /// <summary>Adds a 16.16 delta to split velocity and clamps its whole word without discarding the fraction.</summary>
    private static (ushort Fraction, ushort Whole) CalculateCrocomireVelocity(
        ushort fraction,
        ushort whole,
        uint delta,
        ushort maximumWhole)
    {
        uint fixedValue = (SnesSixteenSixteen.FromWhole((uint)whole)) | fraction;
        fixedValue = unchecked(fixedValue + delta);
        if (SnesFixedPosition.WholeOf(unchecked((int)fixedValue)) >= maximumWhole)
            fixedValue = (SnesSixteenSixteen.FractionOf(fixedValue)) | (SnesSixteenSixteen.FromWhole((uint)maximumWhole));
        return ((ushort)fixedValue, SnesFixedPosition.WholeOf(unchecked((int)fixedValue)));
    }

    /// <summary>Integrates split fixed-point position with native carry between fractional and whole words.</summary>
    private static SnesFixedPosition CalculateCrocomirePosition(
        SnesFixedPosition position,
        ushort fraction,
        ushort whole)
    {
        uint fixedPosition = position.RawValue;
        uint delta = (SnesSixteenSixteen.FromWhole((uint)whole)) | fraction;
        fixedPosition = unchecked(fixedPosition + delta);
        return SnesFixedPosition.FromRaw(fixedPosition);
    }
    /// <summary>
    /// Dispatches the 27 named $A4 Crocomire callback identities in
    /// <see cref="CrocomireInstruction"/>. The pinned NTSC J/U v1.0
    /// body programs produce every key. Distinct movement, wall,
    /// sound, shake, fight, and dust effects retain their authored
    /// control order; the bounded dust-offset subrule is documented
    /// in the opcode catalog. Non-Crocomire slots and words outside the
    /// closed set return false so the general interpreter can report an
    /// untranslated Crocomire opcode after its other dispatchers.
    /// </summary>
    private bool TryProcessCrocomireInstruction(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level,
        ushort opcode,
        ref ushort cursor,
        ushort cameraX)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Crocomire)
            return false;

        if (!Enum.IsDefined((CrocomireInstruction)opcode))
            return false;

        CrocomireEnemyState state = RequireCrocomire(slot);
        ushort next = unchecked((ushort)(cursor + 2));
        CrocomireInstruction instruction = (CrocomireInstruction)opcode;
        switch (instruction)
        {
            case CrocomireInstruction.FightAI:
                cursor = RunCrocomireFightInstruction(state, samus, next);
                return true;

            case CrocomireInstruction.MaybeStartProjectileAttack:
                if (ReadCrocomireRandom() is ushort attackRandom &&
                    unchecked((short)((attackRandom & 0x0fff) - 0x0400)) < 0)
                {
                    state.FightFunction = CrocomireFightFunction.ProjectileAttack;
                    state.ProjectileCounter = 0;
                    next = CrocomireInstructionProgramDefinitions.ProjectileAttack;
                }
                cursor = next;
                return true;

            case CrocomireInstruction.QueueCrySFX:
                LastCrocomireSoundEffect = 0x0074;
                cursor = next;
                return true;
            case CrocomireInstruction.QueueBigExplosionSFX:
                LastCrocomireSoundEffect = 0x0025;
                cursor = next;
                return true;
            case CrocomireInstruction.QueueSkeletonCollapseSFX:
                LastCrocomireSoundEffect = 0x0075;
                cursor = next;
                return true;
            case CrocomireInstruction.ShakeScreen:
                _earthquake.EarthquakeType = 4;
                _earthquake.EarthquakeTimer = NativeCountdown.Start(5);
                LastCrocomireSoundEffect = 0x0076;
                cursor = next;
                return true;

            case CrocomireInstruction.MoveLeft4Pixels:
                RequireCrocomireLevel(level);
                if ((state.FightFlags & 0x0800) == 0)
                    MoveCrocomire(slot, level!, -4);
                cursor = next;
                return true;
            case CrocomireInstruction.MoveLeft4Pixels_SpawnBigDustCloud:
            case CrocomireInstruction.MoveLeft4Pixels_SpawnBigDustCloud_dup:
                SpawnCrocomireRandomFootDust(state);
                RequireCrocomireLevel(level);
                if ((state.FightFlags & 0x0800) == 0)
                    MoveCrocomire(slot, level!, -4);
                cursor = next;
                return true;
            case CrocomireInstruction.MoveLeft_SpawnCloud_HandleSpikeWall:
                RequireCrocomireLevel(level);
                if (MoveCrocomire(slot, level!, -4))
                {
                    state.FightFunction = CrocomireFightFunction.BackingOffSpikeWall;
                    next = CrocomireInstructionProgramDefinitions.BackOffFromSpikeWall;
                }
                else
                {
                    ushort random = ReadCrocomireRandom();
                    int baseOffset = unchecked((short)(random - 0x0800)) >= 0 ? -32 : 32;
                    SpawnCrocomireDust(state, baseOffset + (random & 0x000f));
                }
                cursor = next;
                return true;
            case CrocomireInstruction.MoveRight4PixelsIfOnScreen:
                RequireCrocomireLevel(level);
                if (unchecked((short)(
                        slot.Position.X.Whole - slot.Radius.X - 260 - cameraX)) < 0)
                {
                    MoveCrocomire(slot, level!, 4);
                }
                cursor = next;
                return true;
            case CrocomireInstruction.MoveRight4Pixels:
                RequireCrocomireLevel(level);
                MoveCrocomire(slot, level!, 4);
                cursor = next;
                return true;
            case CrocomireInstruction.MoveRight4PixelsIfOnScreen_SpawnCloud:
                SpawnCrocomireRandomFootDust(state);
                RequireCrocomireLevel(level);
                if (unchecked((short)(
                        slot.Position.X.Whole - slot.Radius.X - 260 - cameraX)) < 0)
                {
                    MoveCrocomire(slot, level!, 4);
                }
                cursor = next;
                return true;
            case CrocomireInstruction.MoveRight4Pixels_SpawnBigDustCloud:
                SpawnCrocomireRandomFootDust(state);
                RequireCrocomireLevel(level);
                MoveCrocomire(slot, level!, 4);
                cursor = next;
                return true;
            case CrocomireInstruction.SpawnBigDustCloudProjectile_Negative20:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_0:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_Negative10:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_10:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_0_dup:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_8:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_10_dup:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_18:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_20:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_28:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_30:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_38:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_40:
                SpawnCrocomireDust(state, CrocomireDustOffset(instruction));
                cursor = next;
                return true;

            default:
                throw new InvalidOperationException($"Undefined {nameof(CrocomireInstruction)} {opcode:X4}.");
        }
    }

    /// <summary>Signed X offsets loaded by the thirteen $A4:9A9B+5*i dust-projectile stubs.</summary>
    private static int CrocomireDustOffset(CrocomireInstruction instruction) => instruction switch
    {
        CrocomireInstruction.SpawnBigDustCloudProjectile_Negative20 => -32,
        CrocomireInstruction.SpawnBigDustCloudProjectile_0 => 0,
        CrocomireInstruction.SpawnBigDustCloudProjectile_Negative10 => -16,
        CrocomireInstruction.SpawnBigDustCloudProjectile_10 => 16,
        CrocomireInstruction.SpawnBigDustCloudProjectile_0_dup => 0,
        CrocomireInstruction.SpawnBigDustCloudProjectile_8 => 8,
        CrocomireInstruction.SpawnBigDustCloudProjectile_10_dup => 16,
        CrocomireInstruction.SpawnBigDustCloudProjectile_18 => 24,
        CrocomireInstruction.SpawnBigDustCloudProjectile_20 => 32,
        CrocomireInstruction.SpawnBigDustCloudProjectile_28 => 40,
        CrocomireInstruction.SpawnBigDustCloudProjectile_30 => 48,
        CrocomireInstruction.SpawnBigDustCloudProjectile_38 => 56,
        CrocomireInstruction.SpawnBigDustCloudProjectile_40 => 64,
        _ => throw new InvalidOperationException($"{instruction} is not a Crocomire dust-projectile instruction."),
    };

    /// <summary>Ports $A4:86B3-$8A39, including the shipped but normally unused states.</summary>
    private ushort RunCrocomireFightInstruction(
        CrocomireEnemyState state,
        SamusState? samus,
        ushort next)
    {
        switch (state.FightFunction)
        {
            case CrocomireFightFunction.ResetAnimation:
                state.Body.InstructionTimer = NativeCountdown.Start(1);
                return CrocomireInstructionProgramDefinitions.Initial;

            case CrocomireFightFunction.StepForward:
                state.FightFunction = CrocomireFightFunction.Sleeping;
                return CrocomireInstructionProgramDefinitions.StepForward;

            case CrocomireFightFunction.Sleeping:
                if (samus is not null && EnemyMath.WrappedMagnitude(unchecked((ushort)(
                        state.Body.Position.X.Whole - samus.Position.X.Whole))) < 224)
                {
                    state.FightFlags |= 0x8000;
                    state.FightFunction = CrocomireFightFunction.WaitingForFirstDamage;
                    return CrocomireInstructionProgramDefinitions.WaitForFirstSecondDamage;
                }
                return next;

            case CrocomireFightFunction.SteppingForward:
                if ((state.FightFlags & 0x0800) != 0)
                {
                    state.FightFlags &= 0xf7ff;
                    if (state.StepCounter != 0)
                    {
                        state.FightFunction = CrocomireFightFunction.SteppingBack;
                        return CrocomireInstructionProgramDefinitions.StepBack;
                    }
                }
                if (unchecked((short)(
                        state.Body.Position.X.Whole - CrocomireSpikeWallThreshold)) < 0)
                {
                    state.FightFunction = CrocomireFightFunction.NearSpikeWallCharge;
                    return CrocomireInstructionProgramDefinitions.NearSpikeWallCharge;
                }
                return unchecked((short)(
                        next - CrocomireInstructionProgramDefinitions.SteppingBack)) >= 0
                    ? CrocomireInstructionProgramDefinitions.StepForward
                    : next;

            case CrocomireFightFunction.ProjectileAttack:
                if ((state.FightFlags & 0x0800) != 0)
                {
                    state.FightFlags &= 0xf7ff;
                    state.FightFunction = CrocomireFightFunction.SteppingBack;
                    return CrocomireInstructionProgramDefinitions.StepBack;
                }
                if (unchecked((short)(state.ProjectileCounter - 18)) < 0)
                {
                    state.ProjectileCounter = unchecked((ushort)(state.ProjectileCounter + 2));
                    SpawnCrocomireProjectile(state.Body, state.ProjectileCounter);
                    LastCrocomireSoundEffect = 0x001c;
                    return next;
                }
                state.FightFunction = CrocomireFightFunction.SteppingForward;
                return CrocomireInstructionProgramDefinitions.StepForwardAfterDelay;

            case CrocomireFightFunction.NearSpikeWallCharge:
                if ((state.FightFlags & 0x0800) != 0)
                {
                    state.FightFlags &= 0xf7ff;
                    state.FightFunction = CrocomireFightFunction.SteppingBack;
                    return CrocomireInstructionProgramDefinitions.StepBack;
                }
                return next;

            case CrocomireFightFunction.SteppingBack:
                if (state.StepCounter != 0)
                    state.StepCounter = NativeWordCounter.Decrement(state.StepCounter).Value;
                if (state.StepCounter != 0)
                    return CrocomireInstructionProgramDefinitions.SteppingBack;
                state.FightFunction = CrocomireFightFunction.SteppingForward;
                return CrocomireInstructionProgramDefinitions.StepForward;

            case CrocomireFightFunction.BackingOffSpikeWall:
                if (unchecked((short)(
                        state.Body.Position.X.Whole - CrocomireSpikeWallThreshold)) >= 0)
                {
                    state.FightFunction = CrocomireFightFunction.SteppingForward;
                    return CrocomireInstructionProgramDefinitions.StepForward;
                }
                return next;

            case CrocomireFightFunction.RoarAndStepForwardUnused:
                state.FightFunction = CrocomireFightFunction.SteppingForward;
                return CrocomireInstructionProgramDefinitions.Roar;

            case CrocomireFightFunction.WaitingForFirstDamage:
                return RunCrocomireWaitingForDamage(
                    state,
                    next,
                    CrocomireFightFunction.WaitingForSecondDamage);

            case CrocomireFightFunction.WaitingForSecondDamage:
            case CrocomireFightFunction.WaitingForSecondDamageUnused:
                return RunCrocomireWaitingForDamage(
                    state,
                    next,
                    CrocomireFightFunction.SteppingBack);

            case CrocomireFightFunction.PowerBombCharge:
                state.StepCounter = NativeWordCounter.Decrement(state.StepCounter).Value;
                if (unchecked((short)(state.StepCounter - 2)) < 0)
                {
                    state.StepCounter = 0;
                    state.FightFunction = CrocomireFightFunction.SteppingForward;
                    return CrocomireInstructionProgramDefinitions.StepForward;
                }
                return next;

            case CrocomireFightFunction.NearSpikeWallChargeUnused:
                if ((state.FightFlags & 0x0800) != 0)
                {
                    state.FightFlags = unchecked((ushort)((state.FightFlags & 0x1f00) | 0xa000));
                    state.StepCounter = 1;
                    state.ReactionTimer = 10;
                    state.FightFunction = CrocomireFightFunction.SteppingBack;
                    LastCrocomireSoundEffect = 0x0054;
                    return next;
                }
                state.FightFunction = CrocomireFightFunction.NearSpikeWallCharge;
                return CrocomireInstructionProgramDefinitions.RoarCloseMouth;

            case CrocomireFightFunction.ResetAnimationUnused:
                state.Body.InstructionTimer = NativeCountdown.Start(1);
                state.FightFlags |= 0x0200;
                state.StepCounter = 32;
                state.FightFunction = CrocomireFightFunction.ChooseAttackUnused;
                return CrocomireInstructionProgramDefinitions.Initial;

            case CrocomireFightFunction.ChooseAttackUnused:
                if ((state.FightFlags & 0x0100) != 0)
                {
                    state.Body.InstructionTimer = NativeCountdown.Start(1);
                    state.StepCounter = 16;
                    state.FightFunction = CrocomireFightFunction.MoveUntilSamusUnused;
                    return CrocomireInstructionProgramDefinitions.Initial;
                }
                state.FightFunction = CrocomireFightFunction.StepForwardUnused;
                return CrocomireInstructionProgramDefinitions.UnusedChargeForwardOneStep;

            case CrocomireFightFunction.StepForwardUnused:
                state.Body.InstructionTimer = NativeCountdown.Start(1);
                if (state.StepCounter == 0)
                {
                    state.FightFlags |= 0x2000;
                    state.FightFunction = CrocomireFightFunction.MoveClawsUnused;
                    return CrocomireInstructionProgramDefinitions.StepForward;
                }
                return CrocomireInstructionProgramDefinitions.Initial;

            case CrocomireFightFunction.MoveUntilSamusUnused:
                if (unchecked((short)(state.Body.Position.X.Whole - 672)) < 0)
                {
                    state.FightFunction = CrocomireFightFunction.MoveClawsUnused;
                    state.StepCounter = 3;
                    return CrocomireInstructionProgramDefinitions.StepForward;
                }
                if ((state.FightFlags & 0x4000) == 0)
                {
                    state.FightFunction = CrocomireFightFunction.StepForwardVariantUnused;
                    state.FightFlags &= 0xfbff;
                    return CrocomireInstructionProgramDefinitions.MovingClaws;
                }
                state.StepCounter = 5;
                state.ProjectileCounter = (ushort)state.FightFunction;
                state.FightFunction = (CrocomireFightFunction)0x2a;
                return CrocomireInstructionProgramDefinitions.MovingClaws;

            case CrocomireFightFunction.MoveClawsUnused:
                if (state.StepCounter == 0 || (state.StepCounter = NativeWordCounter.Decrement(state.StepCounter).Value) == 0)
                {
                    state.FightFunction = CrocomireFightFunction.MovingClawsUnused;
                    state.FightFlags &= 0xfbff;
                    return CrocomireInstructionProgramDefinitions.StepForward;
                }
                state.FightFunction = CrocomireFightFunction.MoveClawsUnused;
                if (state.Tongue is not null)
                    state.Tongue.VariableD = 0;
                state.FightFlags |= 0x0400;
                return CrocomireInstructionProgramDefinitions.MovingClaws;

            case CrocomireFightFunction.StepForwardVariantUnused:
                if ((state.FightFlags & 0x2000) == 0)
                    state.FightFlags &= 0xfcff;
                state.FightFunction = CrocomireFightFunction.MovingClawsUnused;
                return CrocomireInstructionProgramDefinitions.StepForward;

            case CrocomireFightFunction.MovingClawsUnused:
                if (state.StepCounter == 0)
                {
                    state.FightFlags &= 0xbfff;
                    state.Body.InstructionTimer = NativeCountdown.Start(1);
                    state.FightFunction = (CrocomireFightFunction)state.ProjectileCounter;
                    return CrocomireInstructionProgramDefinitions.MovingClaws;
                }
                if ((state.FightFlags & 0x4000) != 0)
                {
                    state.StepCounter = NativeWordCounter.Decrement(state.StepCounter).Value;
                    LastCrocomireSoundEffect = 0x003b;
                    return CrocomireInstructionProgramDefinitions.MovingClaws;
                }
                state.FightFlags &= 0xbfff;
                state.FightFunction = CrocomireFightFunction.SteppingBack;
                return next;

            default:
                throw new InvalidDataException(
                    $"Crocomire fight function ${((ushort)state.FightFunction):X2} is not translated.");
        }
    }

    /// <summary>Holds the damage-reaction instruction until its native timer permits the next step.</summary>
    private static ushort RunCrocomireWaitingForDamage(
        CrocomireEnemyState state,
        ushort next,
        CrocomireFightFunction damagedDestination)
    {
        if ((state.FightFlags & 0x0800) != 0)
        {
            state.FightFlags &= 0xf7ff;
            state.FightFunction = damagedDestination;
            return CrocomireInstructionProgramDefinitions.StepBack;
        }
        return unchecked((short)(
                next - CrocomireInstructionProgramDefinitions.RoarCloseMouthLoop)) >= 0
            ? CrocomireInstructionProgramDefinitions.Roar
            : next;
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
        int offset = random & 0x001f;
        if (unchecked((short)(random - 0x1000)) >= 0)
            offset = -offset;
        SpawnCrocomireDust(state, offset);
    }

    /// <summary>Places a foot dust puff at the body offset with an additional three-bit RNG jitter.</summary>
    private void SpawnCrocomireDust(CrocomireEnemyState state, int xOffset)
    {
        ushort random = ReadCrocomireRandom();
        _projectiles.SpawnRoomGraphicsDustExplosion(
            new(unchecked((ushort)(state.Body.Position.X.Whole + xOffset + (random & 7))), unchecked((ushort)(state.Body.Position.Y.Whole + state.Body.Radius.Y - 16))),
            animationIndex: 0x15);
    }

    /// <summary>Rejects death and melting operations when the arena collision level is unavailable.</summary>
    private static void RequireCrocomireLevel(RoomLevelData? level)
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
        StartCrocomireMeltingTilemap(state, CrocomireInstructionProgramDefinitions.MeltingOneTopRow);

        if (state.Tongue is { } tongue)
        {
            InstallCrocomireInstructionList(
                tongue,
                CrocomireTongueInstructionProgramDefinitions.Melting);
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
        WriteCrocomireMeltingTilemap(tilemap, clearedWords: 0x200);
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
        StartCrocomireMeltingTilemap(state, CrocomireInstructionProgramDefinitions.MeltingTwoTopRow);
        // $A4:940E-941D clears the first $800 bytes.
        WriteCrocomireMeltingTilemap(tilemap, clearedWords: 0x400);
    }

    /// <summary>Requires a complete installed melting map before phase, actor, scratch, or VRAM state changes.</summary>
    internal ReadOnlySpan<ushort> RequireCrocomireMeltingTilemap(CrocomireMeltingTilemapAddress tilemapAddress) =>
        (_room.Artwork?.CrocomireMelting ?? throw new InvalidDataException(
            "Crocomire melting requires installed tilemap artwork.")).Tilemap(tilemapAddress);

    /// <summary>Starts the selected body melting list and synchronizes actor state with the new BG2 map.</summary>
    private void StartCrocomireMeltingTilemap(CrocomireEnemyState state, ushort bodyInstructionList)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        death.PixelsToErasePerColumn = 48;
        death.TargetHeightOrSkeletonTileIndex = 48;
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
        InstallCrocomireInstructionList(state.Body, bodyInstructionList);
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
        death.DistortionStep = 0x0100;
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
        ushort destinationWord = upload.DestinationWord;
        byte sourceBank = upload.SourceBank;
        ushort source = upload.SourceWord;
        if (sourceBank != 0x7e)
        {
            throw new InvalidDataException(
                $"Crocomire melting VRAM record uses unexpected source bank ${sourceBank:X2}.");
        }

        int sourceOffset = unchecked((ushort)(source - 0x4000));
        if (sourceOffset < 0 || sourceOffset + byteCount > death.MeltingGraphics.Length)
            throw new InvalidDataException("Crocomire melting VRAM record exceeds the scratch image.");
        _presentation._vram!.LoadBytes(destinationWord * 2, death.MeltingGraphics.Slice(sourceOffset, byteCount));
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
        death.RumbleYOffset = NativeWordCounter.Decrement(death.RumbleYOffset).Value;
        state.Body.Position = state.Body.Position.WithWholeX(unchecked((ushort)(
            death.BodyXBeforeMelting + ((death.RumbleYOffset & 2) != 0 ? 4 : 0))));

        if (!EraseNextCrocomireMeltingColumn())
        {
            CompleteCrocomireMeltingDissolve(state);
            return;
        }

        // $A4:95D5 refreshes the BG2 scroll, which also moves the tongue onto the body.
        UpdateCrocomireBg2Scroll(state, includeVerticalPosition: true);
        ushort nextAdjustedY = unchecked((ushort)(death.AdjustedDestinationY - 3));
        if (unchecked((short)(death.AdjustedDestinationY - 19)) < 0)
        {
            if (unchecked((short)(death.DistortionStep - 0x5000)) >= 0)
            {
                CompleteCrocomireMeltingDissolve(state);
                return;
            }
            nextAdjustedY = 16;
        }
        death.AdjustedDestinationY = nextAdjustedY;
        death.DistortionStep = unchecked((ushort)Math.Min(
            death.DistortionStep + 0x0180,
            0x5000));
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
                death.MeltingColumnCursor = unchecked((ushort)cursor);
                return true;
            }
            xColumn = CrocomireMeltingDefinitions.SelectColumn(cursor);
            if (heights[xColumn] < death.TargetHeightOrSkeletonTileIndex)
                break;
            cursor++;
        }
        death.MeltingColumnCursor = unchecked((ushort)cursor);

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
        ushort adjustedY = death.AdjustedDestinationY;
        ushort currentY = adjustedY;
        uint fraction = 0;
        while (line < output.Length &&
               unchecked((short)(adjustedY - death.MaximumAdjustedDestinationY)) < 0)
        {
            output[line++] = unchecked((ushort)(
                CrocomireBg2Scroll.Y + adjustedY - currentY));
            uint sum = fraction + death.DistortionStep;
            bool carry = sum > 0xffff;
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
        state.ReactionTimer = 0;
        state.ProjectileCounter = 0;
        state.VerticalAcceleration = 0x0800;
        ClearCrocomireBg2WorkingTilemap();
        TransferCrocomireBg2Words(0, 1024);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }
    /// <summary>Ports <c>EprojInit_CrocomireProjectile</c> at $86:9023.</summary>
    internal void SpawnCrocomireProjectile(RoomEnemySlot body, ushort spawnParameter)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CrocomireProjectile,
            graphicsIndex: EnemyPaletteBits.Palette5);
        projectile.Velocity = new(new SnesSignedEightEight(0xfe00), new SnesSignedEightEight(1));
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(body.Position.X.Whole - 32)), unchecked((ushort)(body.Position.Y.Whole - 16))));
        projectile.GeneralTimer = 0;
        projectile.Position = projectile.Position.WithFractionX(0).WithFractionY(0);
        projectile.DirectionParameter = spawnParameter;
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
        ushort cursor = death.BridgeFragmentCursor;
        if (unchecked((short)(cursor - 22)) >= 0)
            return;

        // The second SpawnEprojWithGfx argument is the same unaligned 0,2,...,20 cursor.
        // Its copied graphics word is immaterial: the definition initializer immediately
        // replaces it with $0400. Preserve the observable X table and cursor progression.
        death.BridgeFragmentCursor = unchecked((ushort)(cursor + 2));
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CrocomireBridgeCrumbling,
            graphicsIndex: 0);
        projectile.Position = projectile.Position.WithWholeX(CrocomireBridgeFragmentDefinitions.XPosition(cursor));
        projectile.Position = projectile.Position.WithWholeY(187).WithFractionX(0).WithFractionY(0);
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(0));
        // $86:92A5 reads RandomNumberSeed; it does not advance it.
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)((_room.Random.Read() & 0x003f) + 64))));
        projectile.GraphicsIndex = 0x0400;
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
            int yTableIndex = projectile.SlotIndex - 10;
            if ((uint)yTableIndex >= 8)
            {
                throw new InvalidDataException(
                    $"Crocomire spike wall occupied unexpected projectile slot {projectile.SlotIndex}.");
            }

            projectile.Position = projectile.Position.WithWhole(new(528, unchecked((ushort)(0x0038 + yTableIndex * 0x0010))));
            projectile.Velocity = new(new SnesSignedEightEight(0), new SnesSignedEightEight(0xfffb));
            projectile.Variable0 = 0;
            projectile.Variable1 = 0x8800;
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
        velocity = unchecked(velocity + 0x00003000u);
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(SnesSixteenSixteen.WholeOf(velocity)))));
        projectile.Variable1 = unchecked((ushort)velocity);
        uint position = unchecked(projectile.Position.Y.RawValue);
        position = unchecked(position + velocity);
        projectile.Position = projectile.Position.WithY(SnesFixedPosition.FromRaw(unchecked((uint)position)));

        if (projectile.Position.Y.Whole < 0x00a8)
            return;

        ushort impactX = projectile.Position.X.Whole;
        ushort impactY = projectile.Position.Y.Whole;
        int nativeIndex = projectile.SlotIndex * 2;
        projectile.Clear();
        if ((nativeIndex & 2) == 0)
            LastCrocomireSoundEffect = 0x0029;
        _projectiles.SpawnRoomGraphicsDustExplosion(new(impactX, impactY), animationIndex: 0x0015);
        LastCrocomireSoundEffect = 0x0025;
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
            (projectile.Velocity.Y.RawValue + 24) & 0x3fff))));
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
        LastCrocomireDropRequest = null;
        _crocomirePlmRequests.Clear();
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastCrocomireSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }

    /// <summary>Queues this frame's published music requests through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacyMusic(EnemyAudioQueue audio)
    {
        audio.QueueLegacyMusic(LastCrocomireMusicRequest?.Command, LastCrocomireMusicRequest?.Delay);
    }


    /// <summary>Registers the tongue's native no-op AI while its instruction list follows the body.</summary>
    internal static void RegisterCrocomireTongueMainAi(EnemyAiTable table)
    {
        table.RegisterMain(EnemyAiRoutine.MainAI_CrocomireTongue, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.CrocomireTongue, (slot, frame, nmiFrameCounter8) =>
        {
            // $A4:F6BB is a literal RTL. The tongue's bank-$A4 instruction list and
            // extended map position the component relative to Crocomire's body.
        });
    }


    /// <summary>Registers impact actors that allocate a drop before their delete instruction runs.</summary>
    internal void RegisterCrocomireDropProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterInstruction(EnemyProjectileInstruction.SpawnEnemyDropsWithCrocomireChances, null,
            (RoomEnemyProjectileSlot projectile, SamusState? samus, Position16<RoomPixels> camera, ushort word, ref ushort cursor) =>
            {
                // Allocate before the following goto/delete frees the impact actor.
                // There is no inline operand; the next word remains an instruction.
                _deathsAndDrops.SpawnEnemyDropFromEnemyHeader(projectile.Position.Whole, EnemyDefinitionId.Crocomire);
                cursor = unchecked((ushort)(cursor + 2));
                return EnemyProjectileInstructionOutcome.Continue;
            });
    }
}

using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The Kraid species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class KraidBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly EnemyFrameResultBuilder _frameResults;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Kraid, EnemyDefinitionId.KraidArm, EnemyDefinitionId.KraidFoot, EnemyDefinitionId.KraidLintBottom, EnemyDefinitionId.KraidLintMiddle, EnemyDefinitionId.KraidLintTop, EnemyDefinitionId.KraidNail, EnemyDefinitionId.KraidNailBad];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new KraidEntity(this, slot, common);

    /// <summary>Room scratch memory, services, and tilemap state used throughout the Kraid fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves nail and foot movement against room terrain.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Owns rocks, lint, and other fight projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes arena shake during growth, death, and heavy movement.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues Kraid's encounter sound effects.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Spawns room sprite objects for Kraid's visual effects.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;
    /// <summary>Provides the native body and seven part slots.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Applies Kraid palettes, background tiles, and OAM presentation.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Checks mouth, body, and part collisions against Samus and shots.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Applies contact damage, death state, and resulting drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Coordinates Zebetite behavior registered alongside Kraid's room actors.</summary>
    private readonly ZebetiteBehavior _zebetiteBehavior;

    /// <summary>Binds Kraid's room systems and shared actor services.</summary>
    internal KraidBehavior(KraidBehaviorDependencies dependencies)
    {
        _frameResults = dependencies.FrameResults;
        _zebetiteBehavior = dependencies.ZebetiteBehavior;
        _room = dependencies.Room;
        _terrain = dependencies.Terrain;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _audio = dependencies.Audio;
        _spriteObjects = dependencies.SpriteObjects;
        _slotTable = dependencies.SlotTable;
        _presentation = dependencies.Presentation;
        _collision = dependencies.Collision;
        _deathsAndDrops = dependencies.DeathsAndDrops;
    }

    /// <summary>The native maximum timer the still lint and fingernail parts wait behind before their first frame.</summary>
    private const ushort KraidStillPartTimer = 32767;

    /// <summary>Most recent Kraid-private sound request in the current enemy frame.</summary>
    public KraidSoundRequest? LastKraidSoundEffect { get; private set; }

    /// <summary>Creates slot-zero encounter state and restores the defeated room when the area-boss flag is set.</summary>
    private void InitializeKraidBody(RoomEnemySlot body)
    {
        if (body.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Kraid body requires native slot zero, not slot {body.SlotIndex}.");
        }

        Kraid = new KraidEnemyState();
        KraidEnemyState state = Kraid;
        RegisterKraidPart(body, state);
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
        {
            // `$A7:A959` installs the dead-room BG palette before the shared part initializer
            // marks every physical actor invisible/deleted/non-interactive. Its two calls
            // immediately before that initializer are not cosmetic bookkeeping: they
            // restore the defeated arena on every room load so the broken ceiling and
            // removed floor spikes cannot return when the player exits and re-enters.
            LoadKraidColorBand(KraidPaletteSource.RoomBackdrop, _presentation.BossPalettes.KraidRoomBackdrop);
            state.WorkingTop = state.WorkingBottom = KraidBodyPage.Blank;
            state.BackgroundTilemapsPrepared = true;
            UploadKraidRoomBackgroundTiles();
            _frameResults.KraidPlms.AddRange(KraidPlmDefinitions.DefeatedRoom);
            // The body is the owner of `$A7:C715-$C815`; deleting it here skips the two
            // BG2 clears and four standard-BG3 restoration DMAs. The other seven physical
            // records take their own dead initializer and remain deleted as on cartridge.
            state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathClearTopTilemap;
            return;
        }

        // $A7:A9E4-A9E7.
        _zebetiteBehavior.CameraDistanceIndex = CameraDistanceMode.BossTracking;
        ApplyKraidScrolls(grown: false);
        state.EjectionLimit = new Position16<RoomPixels>(0, 324);
        state.InitialHealth = body.Health;

        // `$A7:AAC6` constructs the private WRAM tilemap which subsequent rise, head,
        // growth, and death functions upload in independently timed slices.
        InitializeKraidBackground(state);
        state.HurtFrame = 0;
        state.HurtFrameTimer = NativeCountdown.Start(0);

        body.Position = body.Position.WithWhole(new(176, 592));
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        state.Parts[body.SlotIndex].Function = KraidAiFunction.RestrictSamusToFirstScreen;
        state.Parts[body.SlotIndex].FunctionTimer = 300;
        state.Parts[body.SlotIndex].HeadTimer = 64;
        state.Parts[0].NextFunction = KraidAiFunction.RaiseKraidThroughFloor;

        // Background palette three's target colors are written at CGRAM palette eleven in
        // native target-palette storage. The software renderer exposes that buffer directly.
        LoadKraidColorBand(KraidPaletteSource.InitialTarget, _presentation.BossPalettes.KraidSprites);
        _earthquake.EarthquakeType = 5;
    }

    /// <summary>Switches the arena scrolls between the small and grown boss layouts.</summary>
    private void ApplyKraidScrolls(bool grown)
    {
        for (int index = 0; index < KraidCameraDefinitions.ScreenCount; index++)
            _room.Services.RoomScroll.Write(index, grown
                ? KraidCameraDefinitions.GrownScroll(index)
                : KraidCameraDefinitions.InitialScroll(index));
    }

    /// <summary>Installs the arm part in its native slot with the rising-or-sinking program.</summary>
    private void InitializeKraidArm(RoomEnemySlot arm)
    {
        KraidEnemyState state = RequireKraidState(arm);
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
        {
            MarkKraidPartDead(arm);
            return;
        }
        EnsureKraidSlot(arm, 1, "arm");
        RegisterKraidPart(arm, state);
        arm.PaletteIndex = _slotTable.Slots[0].PaletteIndex;
        state.Parts[arm.SlotIndex].Function = KraidAiFunction.NoOperation;
        SetKraidPartProgram(arm, KraidPartProgram.ArmRisingOrSinking);
        arm.InstructionTimer = NativeCountdown.Start(1);
        state.Parts[arm.SlotIndex].NextFunction = KraidAiFunction.NoOperation;
    }

    /// <summary>Initializes one lint part and verifies its expected physical slot.</summary>
    private void InitializeKraidLint(RoomEnemySlot lint, int expectedSlot)
    {
        KraidEnemyState state = RequireKraidState(lint);
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
        {
            MarkKraidPartDead(lint);
            return;
        }
        EnsureKraidSlot(lint, expectedSlot, "lint");
        RegisterKraidPart(lint, state);
        lint.PaletteIndex = _slotTable.Slots[0].PaletteIndex;
        lint.InstructionTimer = NativeCountdown.Start(KraidStillPartTimer);
        SetKraidPartProgram(lint, KraidPartProgram.LintInitial);
        lint.Frame = EnemyFrameId.KraidLint00;
        state.Parts[lint.SlotIndex].Function = KraidAiFunction.LintInactive;
        state.Parts[lint.SlotIndex].LintXOffset = expectedSlot == 2 ? (ushort)0 : unchecked((ushort)-16);
    }

    /// <summary>Installs a foot part with its initial animation and shared body state.</summary>
    private void InitializeKraidFoot(RoomEnemySlot foot)
    {
        KraidEnemyState state = RequireKraidState(foot);
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
        {
            MarkKraidPartDead(foot);
            return;
        }
        EnsureKraidSlot(foot, 5, "foot");
        RegisterKraidPart(foot, state);
        foot.PaletteIndex = _slotTable.Slots[0].PaletteIndex;
        SetKraidPartProgram(foot, KraidPartProgram.FootInitial);
        foot.InstructionTimer = NativeCountdown.Start(1);
        state.Parts[foot.SlotIndex].Function = KraidAiFunction.NoOperation;
        state.Parts[foot.SlotIndex].NextFunction = 0;
    }

    /// <summary>
    /// Ports <c>InitAI_KraidNail_Common</c> ($A7:BCF2). Unlike the other parts it has no
    /// dead-room branch: in a defeated Kraid's room the fingernails initialize and run,
    /// deleting themselves only once slot zero's health reads below one ($A7:BD34).
    /// </summary>
    internal void InitializeKraidNail(RoomEnemySlot nail, int expectedSlot)
    {
        KraidEnemyState state = RequireKraidState(nail);
        EnsureKraidSlot(nail, expectedSlot, "fingernail");
        RegisterKraidPart(nail, state);
        nail.PaletteIndex = _slotTable.Slots[0].PaletteIndex;
        KraidPartState nailPart = state.Parts[nail.SlotIndex];
        nailPart.NailVelocity = nailPart.NailVelocity.WithX(nailPart.NailVelocity.X.WithFraction(40));
        nail.Properties = nail.Properties.With(EnemyProperties.Invisible);
        nail.InstructionTimer = NativeCountdown.Start(KraidStillPartTimer);
        SetKraidPartProgram(nail, KraidPartProgram.NailLoop);
        nail.Frame = EnemyFrameId.KraidNail0;
        state.Parts[nail.SlotIndex].NextFunction = KraidAiFunction.FingernailInitialize;
        state.Parts[nail.SlotIndex].Function = KraidAiFunction.HandleFunctionTimer;
        state.Parts[nail.SlotIndex].FunctionTimer = 64;
    }

    /// <summary>Rejects a body part loaded outside its cartridge-defined slot index.</summary>
    private static void EnsureKraidSlot(RoomEnemySlot slot, int expectedSlot, string part)
    {
        if (slot.SlotIndex != expectedSlot)
        {
            throw new InvalidDataException(
                $"Kraid {part} requires native slot {expectedSlot}, not {slot.SlotIndex}.");
        }
    }

    /// <summary>Ports `$A7:A943`: preserve raw upper flags while installing `$0700`.</summary>
    private static void MarkKraidPartDead(RoomEnemySlot slot) =>
        slot.Properties = slot.Properties.Replace(
            EnemyProperties.SolidToSamus |
                EnemyProperties.ProcessInstructions |
                EnemyProperties.ProcessOffScreen |
                EnemyProperties.IgnoreSamusCollision |
                EnemyProperties.Deleted |
                EnemyProperties.Invisible,
            EnemyProperties.IgnoreSamusCollision |
                EnemyProperties.Deleted |
                EnemyProperties.Invisible);

    /// <summary>
    /// Ports <c>MainAI_Kraid</c> ($A7:AC21): mouth-projectile collision, palette handling,
    /// body-projectile collision and body-vs-Samus collision precede the function. A
    /// killing mouth hit therefore reaches palette handling, which zeroes the hurt frame,
    /// before the death initializer runs in this same frame.
    /// </summary>
    internal void RunKraidBodyMain(
        RoomEnemySlot body,
        SamusState? samus,
        Position16<RoomPixels> camera,
        VramWriteQueue? vramWriteQueue,
        SamusProjectileSystem? samusProjectiles,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        KraidEnemyState state = RequireKraidState(body);
        KraidShotSlots? shots = samusProjectiles is not null && sharedProjectiles is not null
            ? new KraidShotSlots(samusProjectiles, sharedProjectiles)
            : null;
        if (shots is { } mouthShots)
            ResolveKraidMouthProjectileHits(body, state, mouthShots);
        RunKraidPaletteHandling(body, state);
        if (shots is { } bodyShots)
            ResolveKraidBodyProjectileHits(body, state, bodyShots);
        if (samus is not null)
            ResolveKraidBodyContact(body, state, samus);
        // `$A7:AC21` makes BG2 follow Kraid rather than the room. X radius is the native
        // body's right-edge origin adjustment; 152 is the authored vertical anchor.
        state.Bg2Scroll = new Position16<Bg2Pixels>(
            unchecked((ushort)(camera.X - body.Position.X.Whole + body.Radius.X)),
            unchecked((ushort)(camera.Y - body.Position.Y.Whole + 152)));
        KraidAiFunction function = state.Parts[body.SlotIndex].Function;
        if (function is >= KraidAiFunction.RestrictSamusToFirstScreen and
            <= KraidAiFunction.RaiseBody)
        {
            RunKraidRiseFunction(body, state, samus);
            return;
        }
        RunKraidCombatFunction(body, state, vramWriteQueue, camera.X);
    }

    /// <summary>Advances the active body palette program and health-dependent colors.</summary>
    private void RunKraidPaletteHandling(RoomEnemySlot body, KraidEnemyState state)
    {
        if (body.Health == 0)
        {
            state.HurtFrame = 0;
            UpdateKraidHealthPalettes(body, state);
            return;
        }
        if (state.HurtFrame == 0)
            return;
        state.HurtFrameTimer = state.HurtFrameTimer.Decrement().Remaining;
        if (state.HurtFrameTimer.IsZero)
        {
            state.HurtFrameTimer = NativeCountdown.Start(2);
            state.StepHurtFrameDown();
            UpdateKraidHealthPalettes(body, state);
        }
    }

    /// <summary>Updates Kraid body and background colors from the current health band.</summary>
    internal void UpdateKraidHealthPalettes(RoomEnemySlot body, KraidEnemyState state)
    {
        int thresholdWordOffset = 14;
        if ((state.HurtFrame & 1) != 0)
        {
            thresholdWordOffset = -2;
        }
        else
        {
            while (thresholdWordOffset != 0 &&
                unchecked((short)(
                    body.Health - state.HealthEighthThreshold(thresholdWordOffset / 2))) < 0)
            {
                thresholdWordOffset -= 2;
            }
        }
        int sourceColor = 8 * (thresholdWordOffset + 2);
        for (int color = 0; color < 16; color++)
        {
            _presentation.BossPalettes.KraidBody.Write(
                color,
                ReadKraidColor(KraidPaletteSource.Health, sourceColor + color));
            _presentation.BossPalettes.KraidSecondary.Write(
                color,
                ReadKraidColor(KraidPaletteSource.Secondary, sourceColor + color));
        }
    }

    /// <summary>Binds Kraid's body and mouth reactions to Samus bomb contacts.</summary>
    internal void RegisterKraidBombReactions(EnemySharedReactions table)
    {
        table.AddBombSpecial(c => c.Enemy.EnemyDefinitionPointer is EnemyDefinitionId.KraidArm or EnemyDefinitionId.KraidFoot,
            c =>
            {
                if (c.SelectedShotAi is not (EnemyInteractionCallback.KraidNoOpShot or EnemyInteractionCallback.KraidArmShot) ||
                    c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.KraidFoot &&
                        c.SelectedShotAi != EnemyInteractionCallback.KraidNoOpShot)
                {
                    throw new InvalidDataException(
                        $"Kraid part ${(int)c.Enemy.EnemyDefinitionPointer:X4} selected " +
                        $"normal-bomb shot AI {c.SelectedShotAi?.ToString() ?? "(unnamed)"}.");
                }

                // `$A7:94B6` creates a room-graphics dust actor at the selected physical projectile coordinates,
                // then returns to the multibox walker that has already installed direction bit `$10`. `$94B5` is the
                // adjacent literal RTL used by protected arm frames and every foot rectangle.
                if (c.SelectedShotAi == EnemyInteractionCallback.KraidArmShot)
                    SpawnKraidArmShotExplosion(c.Bomb.PackedType.Raw, c.Bomb.Position.Whole);
            });
    }

    /// <summary>Registers the rock projectile initializers and per-frame handlers.</summary>
    internal static void RegisterKraidProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunKraidRockPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.KraidRocks);

        table.RegisterPreInstruction(context =>
        {
            RunKraidCeilingRockPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.KraidCeilingRocks);
    }

    /// <summary>Ports Kraid's initialization AI.</summary>
    internal void RunKraidInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeKraidBody(slot);
    }

    /// <summary>Ports KraidArm's initialization AI.</summary>
    internal void RunKraidArmInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeKraidArm(slot);
    }

    /// <summary>Ports KraidLintTop's initialization AI.</summary>
    internal void RunKraidLintTopInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeKraidLint(slot, expectedSlot: 2);
    }

    /// <summary>Ports KraidLintMiddle's initialization AI.</summary>
    internal void RunKraidLintMiddleInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeKraidLint(slot, expectedSlot: 3);
    }

    /// <summary>Ports KraidLintBottom's initialization AI.</summary>
    internal void RunKraidLintBottomInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeKraidLint(slot, expectedSlot: 4);
    }

    /// <summary>Ports KraidFoot's initialization AI.</summary>
    internal void RunKraidFootInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeKraidFoot(slot);
    }

    /// <summary>Ports KraidNail's initialization AI.</summary>
    internal void RunKraidNailInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeKraidNail(slot, expectedSlot: 6);
    }

    /// <summary>Ports KraidNailBad's initialization AI.</summary>
    internal void RunKraidNailBadInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeKraidNail(slot, expectedSlot: 7);
    }

    /// <summary>Ports Kraid's main AI.</summary>
    internal void RunKraidMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunKraidBodyMain(slot, frame.Samus, frame.Camera, frame.VramWriteQueue, frame.SamusProjectiles, frame.SharedProjectiles);
    }

    /// <summary>Ports KraidArm's main AI.</summary>
    internal void RunKraidArmMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunKraidArmMain(slot, frame.Camera.Y);
    }

    /// <summary>Ports KraidLintTop's main AI.</summary>
    internal void RunKraidLintTopMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunKraidLintMain(slot, frame.Samus);
    }

    /// <summary>Ports KraidFoot's main AI.</summary>
    internal void RunKraidFootMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunKraidFootMain(slot, frame.Camera.Y);
    }

    /// <summary>Ports KraidNail's main AI.</summary>
    internal void RunKraidNailMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunKraidNailMain(slot, frame.Level);
    }

    /// <summary>Registers the spat rock's typed burst and the rock programs' cues.</summary>
    internal static void RegisterKraidRockAnimations(EnemyProjectileHandlerTable table)
    {
        table.RegisterShotAnimation(RoomEnemyProjectileKind.KraidSpitRock,
            () => new KraidRockAnimation(KraidRockProgram.SpitRockShot));
        table.RegisterCue<KraidRockCue>((context, cue) =>
        {
            RoomEnemyProjectileSlot projectile = context.Projectile;
            switch (cue)
            {
                case KraidRockCue.UsePaletteZero:
                    projectile.PreInstruction = EnemyProjectilePreInstruction.KraidRockSpit_UsePalette0;
                    return;
                case KraidRockCue.Delete:
                    projectile.Clear();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(KraidRockCue)} {cue}.");
            }
        });
    }

    /// <summary>Advances Kraid's typed animation.</summary>
    internal void AdvanceKraidTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        AdvanceKraidPartAnimation(slot);
    }

    /// <summary>Installs <paramref name="program"/> as the part's program, as a native list-pointer store does; the caller sets the timer.</summary>
    private void SetKraidPartProgram(RoomEnemySlot part, KraidPartProgram program) =>
        RequireKraidState(part).Parts[part.SlotIndex].Cursor = AnimationProgramCursor<KraidPartProgram>.Start(program);

    /// <summary>
    /// The program a part's stored cursor belongs to. A cursor resting on a fall-through already belongs to the next
    /// program, as the native instruction pointer then points into it.
    /// </summary>
    private KraidPartProgram KraidPartResidentProgram(RoomEnemySlot part) =>
        KraidAnimations.Programs.ResidentProgram(RequireKraidState(part).Parts[part.SlotIndex].Cursor);

    /// <summary>
    /// Runs a Kraid part's program where the enemy instruction interpreter would: the frame expires on the update
    /// its native timer would, and a newly shown frame raises the new-frame flag.
    /// </summary>
    internal void AdvanceKraidPartAnimation(RoomEnemySlot part)
    {
        if (!EnemyProgramAnimation.Expire(part))
            return;
        KraidPartState partState = RequireKraidState(part).Parts[part.SlotIndex];
        AnimationProgramCursor<KraidPartProgram> cursor = partState.Cursor;
        EnemyProgramAnimation.Run(part, KraidAnimations.Programs, ref cursor, KraidPartFrameDefinitions.ShownFrames.Of,
            (cue, continuation) => ApplyKraidPartCue(cue));
        partState.Cursor = cursor;
    }

    /// <summary>Carries out one cue of a Kraid part program.</summary>
    /// <returns>The slow arm program when the speed cue switches the arm; otherwise null.</returns>
    private KraidPartProgram? ApplyKraidPartCue(KraidPartCue cue)
    {
        RoomEnemySlot body = _slotTable.Slots[0];
        switch (cue)
        {
            case KraidPartCue.RaiseBody:
                body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 1)));
                return null;
            case KraidPartCue.LowerBodyAndShake:
                body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 1)));
                _earthquake.EarthquakeType = 1;
                _earthquake.EarthquakeTimer = NativeCountdown.Start(10);
                return null;
            case KraidPartCue.StepSound:
                LastKraidSoundEffect = new KraidSoundRequest(KraidSoundDefinitions.Footstep);
                return null;
            case KraidPartCue.StepLeft:
                body.Position = body.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole - 3)));
                return null;
            case KraidPartCue.StepRight:
                body.Position = body.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole + 3)));
                return null;
            case KraidPartCue.SlowArmBelowHalfHealth:
                return SelectKraidArmSpeedProgram();
            default:
                throw new InvalidOperationException($"Undefined {nameof(KraidPartCue)} {cue}.");
        }
    }

    /// <summary>Binds shot callbacks for Kraid's mouth, body, arm, and nail surfaces.</summary>
    internal void RegisterKraidShotReactions(EnemySharedReactions table)
    {
        // Kraid's arm and foot are admitted by their literal no-op header callback, but their authored hitboxes
        // select the arm's private callback.
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer is EnemyDefinitionId.KraidArm or EnemyDefinitionId.KraidFoot &&
                                                     EnemyCollision.IsLiteralNoOpShotAi(enemy.Definition.Bank, EnemyCollision.ShotAi(enemy)))
        {
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (!c.UsesExtendedHitboxes ||
                    c.HitboxShotAi is not (EnemyInteractionCallback.KraidNoOpShot or EnemyInteractionCallback.KraidArmShot) ||
                    c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.KraidFoot &&
                        c.HitboxShotAi != EnemyInteractionCallback.KraidNoOpShot)
                {
                    throw new InvalidDataException(
                        $"Kraid part ${(int)c.Enemy.EnemyDefinitionPointer:X4} selected " +
                        $"shot AI {c.HitboxShotAi?.ToString() ?? "(unnamed)"}.");
                }

                c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                if (c.HitboxShotAi == EnemyInteractionCallback.KraidArmShot)
                    SpawnKraidArmShotExplosion(c.ProjectileType.Raw, c.Projectile.Position.Whole);
                return true;
            },
        });
    }

    /// <summary>Binds Samus contact handling for Kraid's body and parts.</summary>
    internal void RegisterKraidTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidArm &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.KraidArmTouch,
            context =>
            {
                // `$A7:9490-$94A4` deliberately addresses physical enemy slot four,
                // rather than searching for a particular lint definition. The encounter's
                // eight-record layout makes that the bottom lint; forcing `$B89B` here
                // causes it to fire immediately after the arm launches Samus up and right.
                // Native writes only the whole displacement words, so preserve both
                // fractional words exactly as they were before contact.
                _ = RequireKraidState(context.Slot);
                context.Samus.Kinematics.ExtraDisplacement = context.Samus.Kinematics.ExtraDisplacement.WithWhole(new(4, -8));
                Part(_slotTable.Slots[4]).Function = KraidAiFunction.LintFire;
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, skipDeathAnimation: true);
            });
    }
    /// <summary>
    /// Ports the working-map construction performed by <c>$A7:AAC6</c>: the installed body composes once and is kept
    /// beside the state, which records only that the working map now holds the body (#1352).
    /// </summary>
    internal void InitializeKraidBackground(KraidEnemyState state)
    {
        _ = WorkingMap();
        state.WorkingTop = state.WorkingBottom = KraidBodyPage.Body;
        state.BackgroundTilemapsPrepared = true;
        state.OwnsBg2Tilemap = true;
    }

    /// <summary>Shows the upper working page exactly as <c>$A7:C874</c> uploads it, covering any head drawn over it.</summary>
    internal static void TransferKraidTopTilemap(KraidEnemyState state)
    {
        state.DisplayedBody = state.DisplayedBody with { Top = state.WorkingTop, Head = null };
        state.TopTilemapUploadCount++;
    }

    /// <summary>Shows the lower working page exactly as <c>$A7:C8B6</c> uploads it.</summary>
    internal static void TransferKraidBottomTilemap(KraidEnemyState state)
    {
        state.DisplayedBody = state.DisplayedBody with { Bottom = state.WorkingBottom };
        state.BottomTilemapUploadCount++;
    }

    /// <summary>
    /// Draws the head pose Kraid's private eight-byte instruction record at <c>$A7:AF3D</c> selected over the upper
    /// page. <c>$A7:AF5D</c> sends it straight to the display; mirroring it into the working map would change later
    /// growth and sink uploads, which is not native behavior.
    /// </summary>
    internal static void TransferKraidHeadTilemap(KraidEnemyState state, KraidHeadFrame frame)
    {
        state.DisplayedBody = state.DisplayedBody with { Head = frame };
        state.HeadTilemapUploadCount++;
    }

    /// <summary>
    /// The body tilemap Kraid shows while he owns BG2, at his BG2 scroll (#1352); null when he does not. Resolved from
    /// his installed artwork once per distinct body frame.
    /// </summary>
    internal BossBg2Layer? CaptureBodyLayer()
    {
        if (Kraid is not { OwnsBg2Tilemap: true } state)
            return null;
        if (bodyTilemap is not { } cached || cached.Frame != state.DisplayedBody)
        {
            KraidBackgroundArtwork art = RequireBackgroundArtwork();
            bodyTilemap = cached = (state.DisplayedBody, KraidBodyTilemap.Resolve(state.DisplayedBody, WorkingMap(), art));
        }
        return new BossBg2Layer(cached.Tilemap, state.Bg2Scroll);
    }

    /// <summary>The last body frame resolved, and its tilemap.</summary>
    [NonSerialized]
    private (KraidBodyFrame Frame, BossBg2Tilemap Tilemap)? bodyTilemap;

    /// <summary>Kraid's working map composed from his installed artwork, and that artwork.</summary>
    [NonSerialized]
    private (KraidBackgroundArtwork Art, ushort[] Words)? workingMap;

    /// <summary>The body the installed artwork composes; a rebound installation recomposes it.</summary>
    private ushort[] WorkingMap()
    {
        KraidBackgroundArtwork art = RequireBackgroundArtwork();
        if (workingMap is { } composed && ReferenceEquals(composed.Art, art))
            return composed.Words;
        ushort[] words = KraidBodyTilemap.ComposeWorkingMap(art);
        workingMap = (art, words);
        bodyTilemap = null;
        return words;
    }

    /// <summary>Kraid's installed BG2 artwork.</summary>
    private KraidBackgroundArtwork RequireBackgroundArtwork() =>
        _room.Artwork?.KraidBackground ?? throw new InvalidOperationException("Kraid BG2 requires installed enemy artwork.");

    /// <summary>Binds the fixed 4-bpp room backdrop characters at their native VRAM word (#1352).</summary>
    internal void UploadKraidRoomBackgroundTiles()
    {
        KraidBackgroundArtwork art = _room.Artwork?.KraidBackground
            ?? throw new InvalidDataException("Installed enemy artwork has no Kraid background characters.");
        EnemyVram.BindKraidRoomBackgroundTiles(_presentation.Backgrounds, KraidRoomBackgroundIdentity, art.RoomBackgroundTiles.Transfer.Span);
    }

    /// <summary>The room backdrop characters Kraid reveals.</summary>
    private static readonly BgArtworkIdentity KraidRoomBackgroundIdentity = new("Kraid room background characters");

    /// <summary>
    /// Ports <c>$A7:AD3A</c>'s priority-bit pass before second phase. It runs only on the composed body; a defeated
    /// room's blank map never reaches growth.
    /// </summary>
    private static void SetKraidBg2Priority(KraidEnemyState state)
    {
        state.WorkingTop = WithPriority(state.WorkingTop);
        state.WorkingBottom = WithPriority(state.WorkingBottom);
        state.Bg2PriorityBitsSet = true;

        static KraidBodyPage WithPriority(KraidBodyPage page) => page == KraidBodyPage.Body
            ? KraidBodyPage.PriorityBody
            : throw new InvalidOperationException($"Kraid's priority pass reached a working page holding {page}.");
    }

    /// <summary>
    /// Restores the ordinary-room BG2 base after the body has sunk: the room's BG2 shows again (#1352). Native clears
    /// the two visible 32x32 pages of the ordinary map on consecutive frames; they hold the blank tile the library
    /// background filled, and the one-frame strip of body between the two clears is not reproduced.
    /// </summary>
    private static void ClearKraidTopTilemapForDeath(KraidEnemyState state)
    {
        state.WorkingTop = KraidBodyPage.Blank;
        state.OwnsBg2Tilemap = false;
        state.TopTilemapUploadCount++;
    }

    /// <summary>Clears the second half of the restored ordinary 32x32 BG2 map.</summary>
    private static void ClearKraidBottomTilemapForDeath(KraidEnemyState state) => state.BottomTilemapUploadCount++;

    /// <summary>
    /// Ports Kraid arm helper <c>SpawnExplosionProjectile</c> at `$A7:B0CB`, called by the
    /// arm hitbox callback at `$A7:94B6`. Super Missiles select native dust animation `$1D`;
    /// beams, missiles, and family-$0500 normal bombs select `$06`. The effect uses the
    /// shared room-graphics projectile allocator and queues sound-library-one effect `$3D`.
    /// </summary>
    private void SpawnKraidArmShotExplosion(
        ushort projectileType,
        Position16<RoomPixels> position)
    {
        // Family bit 1 ($0200) is set for super missiles and power bombs.
        MiscDustAnimation animationIndex = (projectileType & 512) != 0
            ? MiscDustAnimation.BigExplosion
            : MiscDustAnimation.DudShotTinyExplosion;
        _projectiles.SpawnRoomGraphicsDustExplosion(position, animationIndex);
        _audio.QueueEnemySound(SoundEffectLibrary1Sounds.DudShot, maximumQueued: 6);
    }

    /// <summary>
    /// The counter-indexed shot view both passes use. The cartridge starts at the count
    /// itself, not count - 1, and does not inspect active-slot sentinels; count five
    /// therefore includes the first physical bomb.
    /// </summary>
    [RoomScopeBoundary("A per-call input, built for one call and never kept.")]
    internal readonly struct KraidShotSlots
    {
        /// <summary>Five physical Samus shot slots indexed by the native shot counter.</summary>
        private readonly SamusProjectileSystem _projectiles;
        /// <summary>Shared bomb slot reached when the native count includes a sixth projectile.</summary>
        private readonly SamusBombProjectileSystem _sharedProjectiles;

        /// <summary>Captures the shot count and validates its bounded native slot domain.</summary>
        public KraidShotSlots(SamusProjectileSystem projectiles, SamusBombProjectileSystem sharedProjectiles)
        {
            _projectiles = projectiles;
            _sharedProjectiles = sharedProjectiles;
            LastSlot = projectiles.SlotTable.ProjectileCounter;
            if (LastSlot > SamusProjectileSystem.SlotCount)
                throw new InvalidDataException($"Kraid projectile counter {LastSlot} exceeds the bounded native slot domain.");
        }

        /// <summary>Inclusive last index selected by the native projectile counter.</summary>
        public int LastSlot { get; }

        /// <summary>Reads the selected shot or shared bomb's position, radius, type, and damage.</summary>
        public KraidCollisionShot Read(int index)
        {
            if (index < SamusProjectileSystem.SlotCount)
            {
                SamusProjectileSlot shot = _projectiles.Slots[index];
                return new(shot.Position.Whole, shot.Radius, shot.PackedType.Raw, shot.Damage);
            }
            SamusBombProjectileSlot bomb = _sharedProjectiles.Slots[0];
            return new(bomb.Position.Whole, bomb.Radius, bomb.PackedType.Raw, bomb.Damage);
        }

        /// <summary>Marks the selected shot or bomb as having collided with Kraid.</summary>
        public void MarkCollision(int index)
        {
            if (index < SamusProjectileSystem.SlotCount)
            {
                SamusProjectileSlot shot = _projectiles.Slots[index];
                shot.Direction = shot.PackedDirection.WithCollisionLifecycleState();
            }
            else
            {
                SamusBombProjectileSlot bomb = _sharedProjectiles.Slots[0];
                bomb.Direction = bomb.PackedDirection.WithCollisionLifecycleState();
            }
        }
    }

    /// <summary>
    /// Both passes return immediately once Kraid has begun sinking ($A7:AFAD/$B182).
    /// </summary>
    private bool KraidSinkingSkipsProjectileCollision(RoomEnemySlot body) =>
        Part(body).Function >= KraidAiFunction.DeathSink;

    /// <summary>
    /// Ports <c>KraidsMouth_vs_Projectile_CollisionHandling</c> ($A7:AFAA). Projectile
    /// removal is deferred to the projectile pre-instruction.
    /// </summary>
    internal int ResolveKraidMouthProjectileHits(RoomEnemySlot body, KraidEnemyState state, KraidShotSlots shots)
    {
        if (KraidSinkingSkipsProjectileCollision(body))
            return 0;

        int lastSlot = shots.LastSlot;
        int mouthHits = 0;
        KraidMouthHitbox? innerMouth = lastSlot == 0 ? null : KraidHeadAnimations.InnerMouthAt(state.HeadProgram);
        if (innerMouth is { } inner)
        {
            KraidCollisionScratch scratch = LoadKraidCollisionScratch(body, inner);
            for (int index = lastSlot; index >= 0; index--)
            {
                KraidCollisionShot shot = shots.Read(index);
                SamusProjectileTypeWord type = shot.Type;
                if (!scratch.OverlapsMouth(shot) ||
                    (type.Family == SamusProjectileFamily.Beam && !type.IsChargedBeam))
                    continue;

                if (type.Family == SamusProjectileFamily.Beam)
                    state.MouthFlags |= KraidMouthFlags.Hit;
                // The common shot callback reuses DP $12/$14 for type/vulnerability.
                // In particular, $8200 left by a Super Missile changes the next shot's
                // signed CMP; rebuilding a clean rectangle here doubles close-range damage.
                scratch.Bottom = shot.Type.Raw;
                scratch.Top = body.Definition.VulnerabilityPointer != 0
                    ? body.Definition.VulnerabilityPointer : EnemyVulnerabilityDefinitions.DefaultPointer;
                // A vulnerability entry's multiplier is its low nibble for charged beams and its low seven bits otherwise.
                int multiplier = type.Family switch
                {
                    SamusProjectileFamily.Beam => EnemyVulnerabilityDefinitions.Read(
                        scratch.Top, EnemyVulnerabilityDefinitions.ChargedBeamOffset) & 15,
                    SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile or
                    SamusProjectileFamily.Bomb or SamusProjectileFamily.PowerBomb =>
                        EnemyCollision.ReadProjectileVulnerability(body, type) & 127,
                    SamusProjectileFamily.BeamExplosion or SamusProjectileFamily.MissileExplosion => 0,
                    _ => throw new InvalidOperationException($"Undefined SamusProjectileFamily {type.Family}."),
                };
                int damage = unchecked((ushort)((shot.Damage >> 1) * multiplier));
                if (damage != 0)
                {
                    body.Health = damage >= body.Health
                        ? 0
                        : body.Health - damage;
                    body.FlashTimer = NativeCountdown.Start(body.HurtFlashFrames + 8);
                    body.AiStates = body.AiStates | EnemyAiStates.Hurt;
                    if (type.HasAnyBeam(SamusBeamFlags.Plasma))
                        body.InvincibilityTimer = NativeCountdown.Start(16);
                }
                else
                {
                    RoomSpriteObjectSlot? dud = _spriteObjects.SpawnRoomSpriteObject(
                        shot.Position, RoomSpriteObjectKind.EnemyProjectileDud, graphicsIndex: 0);
                    // Create_Sprite_Object returns its native word-array index, twice the slot, in $12 only on success.
                    scratch.Bottom = dud is { } created ? checked((ushort)(created.SlotIndex * sizeof(ushort))) : shot.Position.X;
                    scratch.Top = shot.Position.Y;
                    scratch.Left = (ushort)RoomSpriteObjectKind.EnemyProjectileDud;
                    _audio.QueueEnemySound(SoundEffectLibrary1Sounds.DudShot, maximumQueued: 3);
                }
                shots.MarkCollision(index);
                mouthHits++;
            }
        }

        if (mouthHits != 0)
        {
            state.HurtFrame = 6;
            state.HurtFrameTimer = NativeCountdown.Start(2);
            if ((state.MouthFlags & KraidMouthFlags.ReopenArmed) != 0)
                state.MouthFlags |= KraidMouthFlags.ReopenPending;
            if (body.Health < 1 &&
                state.Parts[body.SlotIndex].Function < KraidAiFunction.DeathInitialize)
                BeginKraidDeath(body, state);
        }

        return mouthHits;
    }

    /// <summary>
    /// Ports <c>KraidBody_vs_Projectile_CollisionHandling</c> ($A7:B181), the independent
    /// outer mouth/body pass.
    /// </summary>
    internal int ResolveKraidBodyProjectileHits(RoomEnemySlot body, KraidEnemyState state, KraidShotSlots shots)
    {
        if (KraidSinkingSkipsProjectileCollision(body))
            return 0;

        state.MouthFlags &= ~KraidMouthFlags.Hit;
        int lastSlot = shots.LastSlot;
        int bodyHits = 0;
        if (lastSlot != 0)
        {
            KraidMouthHitbox outerMouth = KraidHeadAnimations.OuterMouthAt(state.HeadProgram);
            KraidCollisionScratch scratch = LoadKraidCollisionScratch(body, outerMouth);
            for (int index = lastSlot; index >= 0; index--)
            {
                KraidCollisionShot shot = shots.Read(index);
                if (!scratch.OverlapsBody(body, shot))
                    continue;
                SpawnKraidArmShotExplosion(shot.Type.Raw, shot.Position);
                scratch.Bottom = shot.Position.X;
                scratch.Top = shot.Position.Y;
                shots.MarkCollision(index);
                if (shot.Type.IsChargedBeam)
                    state.MouthFlags |= KraidMouthFlags.Hit;
                bodyHits++;
            }
        }

        if (bodyHits != 0 && state.Parts[body.SlotIndex].Function == KraidAiFunction.MainloopThinking)
        {
            state.Parts[body.SlotIndex].Function = KraidAiFunction.InitializeEyeGlow;
            if ((state.MouthFlags & KraidMouthFlags.Hit) != 0)
            {
                state.MouthFlags |= KraidMouthFlags.ReopenArmed;
                state.ArmMouthReopens();
            }
        }
        return bodyHits;
    }

    /// <summary>Converts a mouth shape into wrapped world-space collision bounds.</summary>
    internal static KraidCollisionScratch LoadKraidCollisionScratch(RoomEnemySlot body, KraidMouthHitbox shape) =>
        LoadKraidCollisionScratch(body, KraidMouthHitboxDefinitions.ResolveCollision(shape));

    /// <summary>Places a mouth shape's left, top and bottom edges at Kraid's body position, wrapping as the cartridge does.</summary>
    internal static KraidCollisionScratch LoadKraidCollisionScratch(
        RoomEnemySlot body, (short Left, short Top, short Bottom) shape)
    {
        (short left, short top, short bottom) = shape;
        return new()
        {
            Left = unchecked((ushort)(body.Position.X.Whole + left)),
            Top = unchecked((ushort)(body.Position.Y.Whole + top)),
            Bottom = unchecked((ushort)(body.Position.Y.Whole + bottom)),
        };
    }

    /// <summary>Projectile geometry and attack data read by Kraid's mouth and body collision passes.</summary>
    /// <param name="Position">The projectile's whole-pixel room position.</param>
    /// <param name="Radius">The projectile's collision half-extents.</param>
    /// <param name="Type">The projectile's type: its family, beams and charge.</param>
    /// <param name="Damage">The projectile's damage before Kraid's vulnerability multiplier; a copy of the slot's non-negative damage.</param>
    internal readonly record struct KraidCollisionShot(Position16<RoomPixels> Position, Extent16<RoomPixels> Radius, SamusProjectileTypeWord Type, int Damage);

    /// <summary>
    /// The three bounds Kraid's mouth and body collision passes compare shots against. Between shots the damage and
    /// dud paths overwrite them with the shot's type, the vulnerability table and the dud sprite's index, and the next
    /// shot's wrapped signed comparisons read those leftovers: a Super Missile's type moves the next shot's mouth
    /// bound, which doubles close-range damage. Each bound therefore stays a sixteen-bit wrapping value that can hold
    /// any of those leftovers.
    /// </summary>
    internal struct KraidCollisionScratch
    {
        /// <summary>Lower vertical mouth bound in wrapped room coordinates, or the leftover the damage path stored over it.</summary>
        public ushort Bottom; // cartridge-encoding: allow(Aliasing) - also receives the previous shot's type and the dud sprite index, and the next shot's signed compare reads that leftover
        /// <summary>Upper vertical mouth bound in wrapped room coordinates, or the leftover the damage path stored over it.</summary>
        public ushort Top; // cartridge-encoding: allow(Aliasing) - also receives the vulnerability table and the dud sprite's Y, and the next shot's signed compare reads that leftover
        /// <summary>Left horizontal mouth bound in wrapped room coordinates, or the leftover the dud path stored over it.</summary>
        public ushort Left; // cartridge-encoding: allow(Aliasing) - also receives the dud sprite's kind, and the next shot's signed compare reads that leftover

        /// <summary>Tests the mouth using native signed results of wrapped coordinate subtraction.</summary>
        public readonly bool OverlapsMouth(KraidCollisionShot shot) =>
            unchecked((short)(shot.Position.Y - shot.Radius.Y - 1 - Bottom)) < 0 &&
            unchecked((short)(shot.Position.Y + shot.Radius.Y - Top)) >= 0 &&
            unchecked((short)(shot.Position.X + shot.Radius.X - Left)) >= 0;

        /// <summary>Tests the body contour against one shot and updates scratch limits as required.</summary>
        public bool OverlapsBody(RoomEnemySlot body, KraidCollisionShot shot)
        {
            if (unchecked((short)(shot.Position.Y - shot.Radius.Y - 1 - Bottom)) < 0)
                return unchecked((short)(shot.Position.Y + shot.Radius.Y - Top)) >= 0 &&
                    unchecked((short)(shot.Position.X + shot.Radius.X - Left)) >= 0;

            Bottom = unchecked((ushort)(shot.Position.X + shot.Radius.X));
            short relativeY = unchecked((short)(shot.Position.Y - body.Position.Y.Whole));
            return unchecked((short)(body.Position.X.Whole + KraidBodyContour.LeftEdge(relativeY) - Bottom)) < 0;
        }
    }

    /// <summary>Switches the body from combat to its scripted death function.</summary>
    private void BeginKraidDeath(RoomEnemySlot body, KraidEnemyState state)
    {
        state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathInitialize;
        state.ClearMouthReactions();
        for (int slot = 0; slot < 6; slot++)
        {
            // Native property `$0400` disables the ordinary touch list while the private
            // death state owns body movement and breakup effects.
            // Nails are separate projectile-like actors and delete themselves from HP zero.
            // Keeping this six-slot bound preserves that exact division.
            _slotTable.Slots[slot].Properties = _slotTable.Slots[slot].Properties.With(
                EnemyProperties.IgnoreSamusCollision);
        }
    }
    /// <summary>Separates immutable RGB5 artwork from Kraid's native palette timing.</summary>
    internal Bgr555 ReadKraidColor(KraidPaletteSource source, int index)
    {
        if ((uint)index >= KraidPaletteRomData.ColorCount(source))
            throw new ArgumentOutOfRangeException(nameof(index));
        KraidColorCatalog colors = _room.Artwork?.KraidColors ?? throw new InvalidDataException(
            "Installed enemy artwork has no Kraid RGB5 palette catalog.");
        return colors.Resolve(source, index);
    }

    /// <summary>Copies one Kraid palette source band to its CGRAM destination.</summary>
    internal void LoadKraidColorBand(KraidPaletteSource source, CgramPalette destination)
    {
        for (int color = 0; color < KraidPaletteRomData.BandColors; color++)
            destination.Write(color, ReadKraidColor(source, color));
    }
    /// <summary>Dispatches the active first- or second-phase body combat function.</summary>
    internal void RunKraidCombatFunction(
        RoomEnemySlot body,
        KraidEnemyState state,
        VramWriteQueue? vramWriteQueue,
        ushort cameraX)
    {
        switch (state.Parts[body.SlotIndex].Function)
        {
            case KraidAiFunction.MainloopThinking:
                if (!state.ThinkingTimer.IsZero && (state.ThinkingTimer = state.ThinkingTimer.Decrement().Remaining).IsZero)
                {
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.MainAttackWithMouthOpen;
                    state.HeadProgram = KraidHeadAnimations.RoarAfterEntryFrame;
                    state.Parts[body.SlotIndex].HeadTimer = KraidHeadAnimations.RoarEntryDuration;
                }
                return;

            case KraidAiFunction.MainAttackWithMouthOpen:
                RunKraidMouthOpenAttack(body, state);
                return;

            case KraidAiFunction.MouthOpenReaction:
                RunKraidMouthOpenReaction(body, state);
                return;

            case KraidAiFunction.InitializeEyeGlow:
                InitializeKraidEyeGlow(body, state);
                return;

            case KraidAiFunction.GlowEye:
                GlowKraidEye(body, state);
                return;

            case KraidAiFunction.UnglowEye:
                UnglowKraidEye(body, state);
                return;

            case KraidAiFunction.HandleFunctionTimer:
                TickKraidFunctionTimer(state.Parts[0]);
                return;

            case KraidAiFunction.ProcessHeadInstructionAndTimer:
                _ = ProcessKraidHeadInstruction(body, state);
                TickKraidFunctionTimer(state.Parts[0]);
                return;

            case KraidAiFunction.GrowReleaseCamera:
            case KraidAiFunction.GrowBreakCeilingPlatforms:
            case KraidAiFunction.GrowSetBg2Priority:
            case KraidAiFunction.GrowFinishBg2Update:
            case KraidAiFunction.GrowDrawRoomBackground:
            case KraidAiFunction.GrowFadeInRoomBackground:
                RunKraidGrowthFunction(body, state);
                return;

            case KraidAiFunction.SecondPhaseThinking:
                RunKraidSecondPhaseThinking(body, state);
                return;

            case KraidAiFunction.DeathInitialize:
            case KraidAiFunction.DeathFadeOut:
            case KraidAiFunction.DeathUpdateTopTilemap:
            case KraidAiFunction.DeathUpdateBottomTilemap:
            case KraidAiFunction.DeathSink:
            case KraidAiFunction.DeathClearTopTilemap:
            case KraidAiFunction.DeathClearBottomTilemap:
            case KraidAiFunction.DeathLoadBg3Quarter1:
            case KraidAiFunction.DeathLoadBg3Quarter2:
            case KraidAiFunction.DeathLoadBg3Quarter3:
            case KraidAiFunction.DeathLoadBg3Quarter4:
            case KraidAiFunction.DeathFadeInBackground:
            case KraidAiFunction.DeathFinishedWasAlive:
            case KraidAiFunction.DeathFinishedWasDead:
                RunKraidDeathFunction(body, state, vramWriteQueue, cameraX);
                return;

            default:
                throw new InvalidDataException(
                    $"Kraid body function {state.Parts[body.SlotIndex].Function} is not translated.");
        }
    }

    /// <summary>Restores the small-body thinking state after an attack.</summary>
    private void SetupKraidFirstPhaseThinking(RoomEnemySlot body, KraidEnemyState state)
    {
        state.Parts[body.SlotIndex].Function = KraidAiFunction.MainloopThinking;
        _slotTable.Slots[2].Position = _slotTable.Slots[2].Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 20)));
        _slotTable.Slots[3].Position = _slotTable.Slots[3].Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 46)));
        _slotTable.Slots[4].Position = _slotTable.Slots[4].Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 112)));
        state.ThinkingTimer = NativeCountdown.FromRaw(ReadKraidThinkingTimer());
    }

    /// <summary>Restores the grown-body thinking state after an attack.</summary>
    private void SetupKraidSecondPhaseThinking(RoomEnemySlot body, KraidEnemyState state)
    {
        state.Parts[body.SlotIndex].Function = KraidAiFunction.SecondPhaseThinking;
        _slotTable.Slots[2].Position = _slotTable.Slots[2].Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 20)));
        _slotTable.Slots[3].Position = _slotTable.Slots[3].Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 46)));
        _slotTable.Slots[4].Position = _slotTable.Slots[4].Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 112)));
        state.ThinkingTimer = NativeCountdown.FromRaw(ReadKraidThinkingTimer());
    }

    /// <summary>Counts down the grown body's think delay before opening the mouth for its next attack.</summary>
    internal static void RunKraidSecondPhaseThinking(RoomEnemySlot body, KraidEnemyState state)
    {
        if (!state.ThinkingTimer.IsZero && (state.ThinkingTimer = state.ThinkingTimer.Decrement().Remaining).IsZero)
        {
            state.Parts[body.SlotIndex].Function = KraidAiFunction.MouthOpenReaction;
            state.HeadProgram = KraidHeadAnimations.RoarAfterEntryFrame;
            state.Parts[body.SlotIndex].HeadTimer = KraidHeadAnimations.RoarEntryDuration;
        }
    }

    /// <summary>Advances the open-mouth attack and its head instruction timing.</summary>
    private void RunKraidMouthOpenAttack(RoomEnemySlot body, KraidEnemyState state)
    {
        ushort result = ProcessKraidHeadInstruction(body, state);
        if (result == ushort.MaxValue)
        {
            SetupKraidFirstPhaseThinking(body, state);
            state.Parts[body.SlotIndex].HeadTimer = 90;

            // Bits 2 and 8-15 are populated by body/mouth projectile handling. Preserve
            // the cartridge arithmetic now so adding collision cannot change sequencing.
            if ((state.MouthFlags & KraidMouthFlags.ReopenPending) != 0)
            {
                state.RemoveOneMouthReopen();
                if (state.PendingMouthReopens != 0)
                {
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.HandleFunctionTimer;
                    state.Parts[body.SlotIndex].FunctionTimer = 64;
                    state.Parts[0].NextFunction = KraidAiFunction.InitializeEyeGlow;
                    state.Unknown2 = 2;
                    return;
                }
            }
            state.ClearMouthReactions();
            return;
        }

        // The open-mouth tilemap remains installed for 64 frames. Retail emits a rock on
        // every sixteenth timer value, including the entry frame after `$AF3D` reloads it.
        if (state.DisplayedHeadFrame is { MouthStage: KraidHeadFrame.FullyOpenMouthStage } &&
            state.Parts[body.SlotIndex].HeadTimer % 16 == 0)
        {
            state.SpitRockRequestCount++;
            if (SpawnKraidSpitRock(body))
                state.SpawnedSpitRockCount++;
            LastKraidSoundEffect = new KraidSoundRequest(KraidSoundDefinitions.RockSpit);
        }
    }

    /// <summary>
    /// Counts the displayed head frame down and runs the next head command when it expires. A cleared timer holds the
    /// head (the result is zero); otherwise the result is the remaining time before this update's decrement, or the
    /// executed command's result when the decrement reached zero.
    /// </summary>
    internal ushort ProcessKraidHeadInstruction(RoomEnemySlot body, KraidEnemyState state)
    {
        NativeCountdown headTimer = NativeCountdown.FromRaw(checked((ushort)state.Parts[body.SlotIndex].HeadTimer));
        if (headTimer.IsZero)
            return 0;
        NativeWordCounterStep step = headTimer.Decrement();
        state.Parts[body.SlotIndex].HeadTimer = step.Remaining.RawValue;
        return step.IsZero ? ExecuteKraidHeadInstruction(body, state) : headTimer.RawValue;
    }

    /// <summary>
    /// Runs Kraid's head program to its next pose: cries on the way are announced, a pose installs its duration as the
    /// head timer and uploads its image (the result is one), and a program's end leaves the cursor there (the result is
    /// <see cref="ushort.MaxValue"/>).
    /// </summary>
    internal ushort ExecuteKraidHeadInstruction(RoomEnemySlot body, KraidEnemyState state)
    {
        AnimationProgramCursor<KraidHeadProgram> cursor = state.HeadProgram ?? throw new InvalidDataException(
            "Kraid's head program has not been started.");
        NativeCountdown unusedLoopCounter = default;
        AnimationProgramAdvance<KraidHeadFrame> advance = KraidHeadAnimations.Programs.AdvanceTo(
            ref cursor, ref unusedLoopCounter, (cue, _) =>
            {
                PublishKraidHeadCue(cue, state);
                return null;
            });
        state.HeadProgram = cursor;
        switch (advance.Kind)
        {
            case AnimationProgramAdvanceKind.Hold:
                return ushort.MaxValue;

            case AnimationProgramAdvanceKind.Show:
                state.Parts[body.SlotIndex].HeadTimer = checked((ushort)advance.Duration);
                state.DisplayedHeadFrame = advance.Frame;
                TransferKraidHeadTilemap(state, advance.Frame);
                return 1;

            case AnimationProgramAdvanceKind.Wait:
            case AnimationProgramAdvanceKind.Stop:
                throw new InvalidOperationException($"Kraid's head programs never {advance.Kind}.");

            default:
                throw new InvalidOperationException($"Undefined {nameof(AnimationProgramAdvanceKind)} {advance.Kind}.");
        }
    }

    /// <summary>Plays a head program's cry; a roar is also counted.</summary>
    private void PublishKraidHeadCue(KraidHeadCue cue, KraidEnemyState state)
    {
        switch (cue)
        {
            case KraidHeadCue.Roar:
                LastKraidSoundEffect = new KraidSoundRequest(KraidSoundDefinitions.HeadRoar);
                state.RoarRequestCount++;
                return;

            case KraidHeadCue.DyingCry:
                LastKraidSoundEffect = new KraidSoundRequest(KraidSoundDefinitions.HeadDyingCry);
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(KraidHeadCue)} {cue}.");
        }
    }

    /// <summary>Updates the mouth-open reaction after a damaging shot.</summary>
    private void RunKraidMouthOpenReaction(RoomEnemySlot body, KraidEnemyState state)
    {
        if (ProcessKraidHeadInstruction(body, state) != ushort.MaxValue)
            return;
        state.Parts[body.SlotIndex].Function = KraidAiFunction.MainloopThinking;
        state.Parts[body.SlotIndex].HeadTimer = 90;
        if ((state.MouthFlags & KraidMouthFlags.ReopenPending) != 0)
        {
            state.RemoveOneMouthReopen();
            if (state.PendingMouthReopens != 0)
            {
                state.Parts[body.SlotIndex].Function = KraidAiFunction.HandleFunctionTimer;
                state.Parts[body.SlotIndex].FunctionTimer = 64;
                state.Parts[0].NextFunction = KraidAiFunction.InitializeEyeGlow;
                state.Unknown2 = 2;
                return;
            }
        }
        state.ClearMouthReactions();
    }

    /// <summary>Seeds eye-glow palette state after a body hit.</summary>
    internal void InitializeKraidEyeGlow(RoomEnemySlot body, KraidEnemyState state)
    {
        state.Parts[body.SlotIndex].Function = KraidAiFunction.GlowEye;
        state.HeadProgram = KraidHeadAnimations.EyeGlowAfterEntryFrame;
        state.Parts[body.SlotIndex].HeadTimer = KraidHeadAnimations.EyeGlowEntryDuration;
        GlowKraidEye(body, state);
    }

    /// <summary>Steps the eye highlight while the body reacts to a hit.</summary>
    private void GlowKraidEye(RoomEnemySlot body, KraidEnemyState state)
    {
        _ = ProcessKraidHeadInstruction(body, state);
        int completedChannels = 0;
        CgramPalette eyeColors = _presentation.BossPalettes.KraidEye;
        for (int colorIndex = 0; colorIndex < eyeColors.Count; colorIndex++)
        {
            Bgr555 color = eyeColors[colorIndex];
            int red = color.Red;
            int green = color.Green;
            if (red >= 30)
            {
                red = 31;
                completedChannels++;
            }
            else
            {
                red++;
            }
            if (green >= 30)
            {
                green = 31;
                completedChannels++;
            }
            else
            {
                green++;
            }
            eyeColors.Write(colorIndex, new Bgr555(red, green, color.Blue));
        }
        if (completedChannels >= 6)
            state.Parts[body.SlotIndex].Function = KraidAiFunction.UnglowEye;
    }

    /// <summary>Restores the eye palette after the reaction completes.</summary>
    internal void UnglowKraidEye(RoomEnemySlot body, KraidEnemyState state)
    {
        int thresholdWordOffset = 14;
        while (thresholdWordOffset != 0 &&
            unchecked((short)(
                body.Health - state.HealthEighthThreshold(thresholdWordOffset / 2))) < 0)
        {
            thresholdWordOffset -= 2;
        }
        int sourceColor = 8 * (thresholdWordOffset + 2) + 1;
        int changedChannels = 0;
        CgramPalette eyeColors = _presentation.BossPalettes.KraidEye;
        for (int eye = 0; eye < eyeColors.Count; eye++)
        {
            Bgr555 current = eyeColors[eye];
            Bgr555 target = ReadKraidColor(KraidPaletteSource.Health, sourceColor + eye);
            int red = current.Red;
            int green = current.Green;
            // Native decrements toward any differing target and masks each field, so a
            // channel below its target wraps within its five bits without borrowing.
            if (red != target.Red)
            {
                red = (red - 1) & Bgr555.MaxChannel;
                changedChannels++;
            }
            if (green != target.Green)
            {
                green = (green - 1) & Bgr555.MaxChannel;
                changedChannels++;
            }
            eyeColors.Write(eye, new Bgr555(red, green, current.Blue));
        }
        if (changedChannels == 0)
        {
            state.Parts[body.SlotIndex].Function = KraidAiFunction.MouthOpenReaction;
            state.HeadProgram = KraidHeadAnimations.RoarAfterEntryFrame;
            state.Parts[body.SlotIndex].HeadTimer = KraidHeadAnimations.RoarEntryDuration;
        }
    }
    /// <summary>Checks the body hitbox and applies contact damage to Samus.</summary>
    /// <remarks>These collision checks belong to actor AI rather than generic interactive hitboxes. The body's
    /// permanent intangible flag and lint's no-op touch callback do not suppress these private native paths.</remarks>
    private void ResolveKraidBodyContact(RoomEnemySlot body, KraidEnemyState state, SamusState samus)
    {
        if (state.Parts[body.SlotIndex].Function >= KraidAiFunction.DeathInitialize)
            return;
        short relativeY = unchecked((short)(samus.Position.Y.Whole - body.Position.Y.Whole));
        if (unchecked((short)(body.Position.X.Whole + KraidBodyContour.LeftEdge(relativeY) -
            samus.Position.X.Whole - samus.Kinematics.Radius.X)) >= 0)
            return;
        if (unchecked((short)(samus.Position.X.Whole - KraidContactDefinitions.MinimumBodyEjectionX)) >= 0)
            samus.Position -= new Offset16<RoomPixels>(unchecked((short)KraidContactDefinitions.BodyEjection), 0);
        Position16<RoomPixels> ejected = samus.Position.Whole -
            new Offset16<RoomPixels>(0, unchecked((short)KraidContactDefinitions.BodyEjection));
        samus.Position = samus.Position.WithWholeY((ejected - state.EjectionLimit).Y < 0
            ? state.EjectionLimit.Y : ejected.Y);
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWhole(new(KraidContactDefinitions.ExtraX, KraidContactDefinitions.ExtraY));
        if (samus.InvincibilityTimer.IsZero)
            _collision.ResolveNormalEnemyTouch(body, samus);
    }

    /// <summary>Checks lint contact against Samus without altering the body phase.</summary>
    private void ResolveKraidLintContact(RoomEnemySlot lint, SamusState samus)
    {
        if (lint.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) || !samus.InvincibilityTimer.IsZero)
            return;
        ushort edge = unchecked((ushort)(lint.Position.X.Whole + KraidContactDefinitions.LintLeadingEdge));
        if (unchecked((short)(samus.Position.X.Whole + samus.Kinematics.Radius.X - edge)) < 0 ||
            unchecked((short)(samus.Position.X.Whole - samus.Kinematics.Radius.X - edge)) >= 0 ||
            unchecked((short)(samus.Position.Y.Whole + samus.Kinematics.Radius.Y -
                lint.Position.Y.Whole - KraidContactDefinitions.LintTop)) < 0 ||
            unchecked((short)(samus.Position.Y.Whole - samus.Kinematics.Radius.Y -
                lint.Position.Y.Whole - KraidContactDefinitions.LintBottom)) >= 0)
            return;
        ushort push = unchecked((ushort)(~(samus.Kinematics.Radius.X + KraidContactDefinitions.LintPush) +
            samus.Kinematics.ExtraDisplacement.X.WholePixels));
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeX(unchecked((short)(
            unchecked((short)(push - KraidContactDefinitions.LintPush)) < 0 ? push : KraidContactDefinitions.LintPush)));
        _collision.ResolveNormalEnemyTouch(lint, samus);
        lint.Properties = lint.Properties.With(EnemyProperties.IgnoreSamusCollision);
    }
    /// <summary>Dispatches the staged sinking, palette, and room-repair death sequence.</summary>
    private void RunKraidDeathFunction(
        RoomEnemySlot body,
        KraidEnemyState state,
        VramWriteQueue? vramWriteQueue,
        ushort cameraX)
    {
        switch (state.Parts[body.SlotIndex].Function)
        {
            case KraidAiFunction.DeathInitialize:
                InitializeKraidDeath(body, state);
                return;

            case KraidAiFunction.DeathFadeOut:
                _ = ProcessKraidHeadInstruction(body, state);
                if (AdvanceKraidRoomBackgroundFade(fadeToBlack: true))
                {
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathUpdateTopTilemap;
                    state.HurtFrameTimer = NativeCountdown.Start(1);
                    state.HurtFrame = 1;
                    TransferKraidTopTilemap(state);
                }
                return;

            case KraidAiFunction.DeathUpdateTopTilemap:
                _ = ProcessKraidHeadInstruction(body, state);
                state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathUpdateBottomTilemap;
                for (int lintSlot = 2; lintSlot <= 4; lintSlot++)
                {
                    state.Parts[_slotTable.Slots[lintSlot].SlotIndex].Function = KraidAiFunction.AlignPartToKraid;
                    state.Parts[_slotTable.Slots[lintSlot].SlotIndex].FunctionTimer = KraidStillPartTimer;
                }
                TransferKraidTopTilemap(state);
                return;

            case KraidAiFunction.DeathUpdateBottomTilemap:
                _ = ProcessKraidHeadInstruction(body, state);
                state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathSink;
                state.DeathSoundTimer = NativeCountdown.Start(43);
                body.Properties = body.Properties.With(EnemyProperties.SolidToSamus);
                _earthquake.EarthquakeType = 1;
                _earthquake.EarthquakeTimer = NativeCountdown.Start(256);
                SetKraidPartProgram(_slotTable.Slots[1], KraidPartProgram.ArmRisingOrSinking);
                _slotTable.Slots[1].InstructionTimer = NativeCountdown.Start(1);
                SetKraidPartProgram(_slotTable.Slots[5], KraidPartProgram.FootInitial);
                _slotTable.Slots[5].InstructionTimer = NativeCountdown.Start(1);
                state.Parts[_slotTable.Slots[5].SlotIndex].Function = KraidAiFunction.NoOperation;
                TransferKraidBottomTilemap(state);
                return;

            case KraidAiFunction.DeathSink:
                RunKraidDeathSink(body, state);
                return;

            case KraidAiFunction.DeathClearTopTilemap:
                state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathClearBottomTilemap;
                ClearKraidTopTilemapForDeath(state);
                return;
            case KraidAiFunction.DeathClearBottomTilemap:
                state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathLoadBg3Quarter1;
                ClearKraidBottomTilemapForDeath(state);
                return;
            case KraidAiFunction.DeathLoadBg3Quarter1:
                AdvanceKraidDeathBg3Transfer(
                    body,
                    state,
                    transferIndex: 0,
                    KraidAiFunction.DeathLoadBg3Quarter2,
                    vramWriteQueue);
                return;
            case KraidAiFunction.DeathLoadBg3Quarter2:
                AdvanceKraidDeathBg3Transfer(
                    body,
                    state,
                    transferIndex: 1,
                    KraidAiFunction.DeathLoadBg3Quarter3,
                    vramWriteQueue);
                return;
            case KraidAiFunction.DeathLoadBg3Quarter3:
                AdvanceKraidDeathBg3Transfer(
                    body,
                    state,
                    transferIndex: 2,
                    KraidAiFunction.DeathLoadBg3Quarter4,
                    vramWriteQueue);
                return;
            case KraidAiFunction.DeathLoadBg3Quarter4:
                AdvanceKraidDeathBg3Transfer(
                    body,
                    state,
                    transferIndex: 3,
                    KraidAiFunction.DeathFadeInBackground,
                    vramWriteQueue);
                return;
            case KraidAiFunction.DeathFadeInBackground:
                if (!AdvanceKraidRoomBackgroundFade(fadeToBlack: false))
                    return;
                state.MusicRequest = MusicCommand.SelectTrack(3);
                if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
                {
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathFinishedWasDead;
                }
                else
                {
                    _room.Services.Bosses.SetDefeated(BossBits.AreaBoss);
                    state.BossDefeatPersisted = true;
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathFinishedWasAlive;
                }
                state.DeathSequenceComplete = true;
                return;
            case KraidAiFunction.DeathFinishedWasAlive:
                MarkKraidDeadUnlessDeadAtLeftEdge(body, cameraX);
                return;
            case KraidAiFunction.DeathFinishedWasDead:
                MarkKraidDeadUnlessDeadAtLeftEdge(body, cameraX);
                return;
            default:
                throw new InvalidOperationException($"Kraid body function {state.Parts[body.SlotIndex].Function} is not a death function.");
        }
    }

    /// <summary>
    /// Ports the shared body of <c>$A7:C843</c>/<c>$A7:C851</c>: a living Kraid, or a dead
    /// one whose camera has left the room's left edge, gets <c>SetEnemyPropertiesToDead</c>.
    /// </summary>
    private void MarkKraidDeadUnlessDeadAtLeftEdge(RoomEnemySlot body, ushort cameraX)
    {
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss) && cameraX == 0)
            return;
        MarkKraidPartDead(body);
    }

    /// <summary>Starts the death animation and prepares its room mutation schedule.</summary>
    internal void InitializeKraidDeath(RoomEnemySlot body, KraidEnemyState state)
    {
        if (state.HurtFrame != 0)
            return;
        for (int color = 0; color < 16; color++)
            _presentation.BossPalettes.KraidBody.Write(
                color,
                ReadKraidColor(KraidPaletteSource.DeathArm, color));
        SetKraidPartProgram(_slotTable.Slots[1], KraidPartProgram.ArmDyingOrPreparingToLunge);
        _slotTable.Slots[1].InstructionTimer = NativeCountdown.Start(1);
        state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathFadeOut;
        state.HeadProgram = KraidHeadAnimations.DeathAfterEntryFrame;
        state.Parts[body.SlotIndex].HeadTimer = KraidHeadAnimations.DeathEntryDuration;
        // $A7:C3A8-$C3E8 clear the fingernails' respawn bit, then run EnemyDeath for the
        // nails, then the three lints. A still holds each slot's native index, which
        // EnemyDeath treats as explosion type zero; every part leaves a death explosion.
        foreach (int slot in new[] { 6, 7 })
            _slotTable.Slots[slot].Properties = _slotTable.Slots[slot].Properties.Without(EnemyProperties.RespawnIfKilled);
        foreach (int slot in new[] { 6, 7, 2, 3, 4 })
            _deathsAndDrops.StartGenericEnemyDeath(_slotTable.Slots[slot], deathAnimation: _slotTable.Slots[slot].NativeIndex);
        // The live sequence crumbles successive blocks while Kraid fades/sinks.
        // It must not substitute the instantaneous already-defeated-room clear.
        _frameResults.KraidPlms.Add(KraidPlmDefinitions.LiveDeathSpikes);
    }

    /// <summary>Moves the body downward through the authored sink sequence.</summary>
    private void RunKraidDeathSink(RoomEnemySlot body, KraidEnemyState state)
    {
        _ = ProcessKraidHeadInstruction(body, state);
        state.DeathSoundTimer = state.DeathSoundTimer.Decrement().Remaining;
        if (state.DeathSoundTimer.IsZero)
        {
            LastKraidSoundEffect = new KraidSoundRequest(KraidSoundDefinitions.RockSpit);
            state.DeathSoundTimer = NativeCountdown.Start(30);
        }
        ProcessKraidSinkTable(body, state);
        body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 1)));
        if (unchecked((short)(body.Position.Y.Whole - 608)) < 0)
            return;

        body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        EnemyPropertyWord armProperties = _slotTable.Slots[1].Properties.With(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Deleted);
        _slotTable.Slots[1].Properties = armProperties;
        EnemyPropertyWord otherProperties = armProperties.Replace(
            EnemyProperties.SolidToSamus |
                EnemyProperties.ProcessInstructions |
                EnemyProperties.ProcessOffScreen,
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Deleted);
        for (int slot = 2; slot <= 5; slot++)
            _slotTable.Slots[slot].Properties = otherProperties;
        state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathClearTopTilemap;
        // $A7:C594 returns the camera to distance index zero before the death drop.
        _zebetiteBehavior.CameraDistanceIndex = CameraDistanceMode.NormalTracking;

        // $A0:B8EE fills the same sixteen-projectile pool used by ordinary deaths. Each
        // pickup gets a random position in Kraid's 256x64 floor strip and independently
        // rolls header $E2BF's six-byte chance table.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.Kraid,
            count: 16,
            xBase: 128,
            xMask: byte.MaxValue,
            yBase: 352,
            yMask: SnesLittleEndian.Word(0, 63));
        state.DeathDropRequestCount = 16;
        // $A7:C59B continues into DrawKraidsRoomBackground, which zeroes the shared numerator.
        _presentation.GradualColorChange.Reset();
    }

    /// <summary>Consumes native sink-table entries as the body descends.</summary>
    internal void ProcessKraidSinkTable(RoomEnemySlot body, KraidEnemyState state)
    {
        if (KraidSinkSchedule.CallbackAt(body.Position.Y.Whole) is KraidSinkCallback function)
        {
            state.SinkTableEventCount++;
            // Every eight-pixel sinking row invokes its cartridge callback, including the
            // deliberately empty RTS used by rows whose only job is the BG2 strip upload.
            // Do not merge that address with the adjacent $C6A7 crumble routine: they are
            // distinct legal indirect-JSR targets in the retail table.
            if (KraidPlmDefinitions.ForSinkCallback(function) is { } request)
            {
                // Each sinking rock starts at the left pixel edge of the removed block.
                _ = SpawnKraidCeilingRock((ushort)(request.Block.X * 16));
                _frameResults.KraidPlms.Add(request);
            }
            return;
        }
    }

    /// <summary>Restores standard BG3 tiles through staged DMA after the body dies.</summary>
    internal void AdvanceKraidDeathBg3Transfer(
        RoomEnemySlot body,
        KraidEnemyState state,
        int transferIndex,
        KraidAiFunction next,
        VramWriteQueue? vramWriteQueue)
    {
        if ((uint)transferIndex >= KraidBackgroundRomData.StandardBg3TransferCount)
            throw new ArgumentOutOfRangeException(nameof(transferIndex));

        int destinationWord = transferIndex * (KraidBackgroundRomData.StandardBg3TransferBytes / 2);

        // `$A7:C777-$C7EF` deliberately spreads the $1000-byte restoration over four
        // enemy frames. The runtime path appends the same four native seven-byte DMA
        // records so NMI owns visibility. Isolated enemy audits have no NMI/queue owner;
        // executing that one frame's transfer directly preserves the identical bytes and
        // keeps their per-frame sequencing observable without fabricating a host queue.
        // The restored characters are background artwork bound at the NMI (#1352).
        HudTileAtlas hud = _presentation.HudTileArtwork ?? throw new InvalidDataException(
            "Installed Kraid death restoration has no standard BG3 artwork.");
        if (vramWriteQueue is not null)
            vramWriteQueue.EnqueueBackgroundAsset(
                KraidBackgroundRomData.StandardBg3AssetForQuarter(transferIndex),
                KraidBackgroundRomData.StandardBg3TransferBytes, EnemyVram.KraidBg3RestoreDma(destinationWord));
        else
            EnemyVram.BindKraidBg3Restore(_presentation.Backgrounds, new BgArtworkIdentity($"standard BG3 characters quarter {transferIndex}"),
                destinationWord, hud.KraidRestoreQuarter(transferIndex).Span);

        state.DeathBg3TransferCount++;
        state.Parts[body.SlotIndex].Function = next;
    }
    /// <summary>Starts growth only after the small-form transition requirements are met.</summary>
    internal bool TryBeginKraidGrowth(RoomEnemySlot body, KraidEnemyState state)
    {
        if (body.Health >= state.HealthEighthThreshold(6))
            return false;

        state.Parts[body.SlotIndex].Function = KraidAiFunction.ProcessHeadInstructionAndTimer;
        state.Parts[body.SlotIndex].FunctionTimer = 180;
        state.Parts[0].NextFunction = KraidAiFunction.GrowReleaseCamera;

        KraidHeadGrowthResume resume = KraidHeadAnimations.ReadsAt(state.HeadProgram).Growth;
        state.HeadProgram = KraidHeadAnimations.GrowthResumeCursor(resume);
        state.Parts[body.SlotIndex].HeadTimer = checked((ushort)KraidHeadAnimations.GrowthResumeDelay(resume));
        _earthquake.EarthquakeType = 4;
        _earthquake.EarthquakeTimer = NativeCountdown.Start(340);

        RoomEnemySlot foot = _slotTable.Slots[5];
        SetKraidPartProgram(foot, KraidPartProgram.FootInitial);
        foot.InstructionTimer = NativeCountdown.Start(1);
        state.Parts[foot.SlotIndex].Function = KraidAiFunction.NoOperation;
        RoomEnemySlot arm = _slotTable.Slots[1];
        SetKraidPartProgram(arm, KraidPartProgram.ArmNormal);
        arm.InstructionTimer = NativeCountdown.Start(1);
        for (int lintSlot = 2; lintSlot <= 4; lintSlot++)
            _slotTable.Slots[lintSlot].Properties = _slotTable.Slots[lintSlot].Properties.With(EnemyProperties.Invisible);
        arm.Properties = arm.Properties.With(EnemyProperties.IgnoreSamusCollision);
        return true;
    }

    /// <summary>Dispatches rise, scroll, tilemap, and palette stages of growth.</summary>
    internal void RunKraidGrowthFunction(RoomEnemySlot body, KraidEnemyState state)
    {
        switch (state.Parts[body.SlotIndex].Function)
        {
            case KraidAiFunction.GrowReleaseCamera:
                ApplyKraidScrolls(grown: true);
                state.Parts[body.SlotIndex].Function = KraidAiFunction.GrowBreakCeilingPlatforms;
                state.CameraReleasedForSecondPhase = true;
                state.EjectionLimit = new Position16<RoomPixels>(0, 164);
                return;

            case KraidAiFunction.GrowBreakCeilingPlatforms:
                if ((body.FrameCounter & 7) == 0)
                    RequestKraidRisingRock(body, state);
                body.Position = body.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole +
                    ((body.Position.Y.Whole & 2) == 0 ? 1 : -1))));
                body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 1)));
                if ((body.Position.Y.Whole & 3) == 0 && state.Parts[body.SlotIndex].CeilingRockOffset < 18)
                {
                    int ceilingIndex = state.Parts[body.SlotIndex].CeilingRockOffset / 2;
                    ushort rockX = KraidCeilingRockPositionDefinitions.AtByteOffset(checked((ushort)state.Parts[body.SlotIndex].CeilingRockOffset));
                    if (SpawnKraidCeilingRock(rockX))
                        state.CeilingRockSpawnCount++;
                    _frameResults.KraidPlms.Add(KraidPlmDefinitions.GrowthCeiling[ceilingIndex]);
                    state.Parts[body.SlotIndex].CeilingRockOffset = unchecked((ushort)(state.Parts[body.SlotIndex].CeilingRockOffset + 2));
                }
                if (unchecked((short)(body.Position.Y.Whole - 296)) < 0)
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.GrowSetBg2Priority;
                return;

            case KraidAiFunction.GrowSetBg2Priority:
                SetKraidBg2Priority(state);
                _slotTable.Slots[1].Properties = _slotTable.Slots[1].Properties.Without(
                    EnemyProperties.IgnoreSamusCollision);
                state.Parts[body.SlotIndex].Function = KraidAiFunction.GrowFinishBg2Update;
                TransferKraidTopTilemap(state);
                return;

            case KraidAiFunction.GrowFinishBg2Update:
                state.Parts[body.SlotIndex].Function = KraidAiFunction.GrowDrawRoomBackground;
                RoomEnemySlot foot = _slotTable.Slots[5];
                SetKraidPartProgram(foot, KraidPartProgram.FootNeutral);
                foot.InstructionTimer = NativeCountdown.Start(1);
                for (int lintSlot = 2; lintSlot <= 4; lintSlot++)
                {
                    SetKraidPartProgram(_slotTable.Slots[lintSlot], KraidPartProgram.LintPostGrowth);
                    _slotTable.Slots[lintSlot].Frame = KraidPartFrameDefinitions.ShownFrames.Of(new KraidPartFrame(KraidPartFrameKind.Lint, 1));
                }
                TransferKraidBottomTilemap(state);
                return;

            case KraidAiFunction.GrowDrawRoomBackground:
                state.Parts[body.SlotIndex].Function = KraidAiFunction.GrowFadeInRoomBackground;
                state.Parts[body.SlotIndex].CeilingRockOffset = 0;
                // $A7:ADB0 DrawKraidsRoomBackground zeroes the shared numerator.
                _presentation.GradualColorChange.Reset();
                // `$A7:AD9A` queues this character upload on the same frame that it
                // initializes palette-six fading. Without it, BG1's authored repeating
                // room-background blocks decode an uninitialized character as transparent,
                // exposing Kraid BG2's `$0338` filler rectangle above his head.
                UploadKraidRoomBackgroundTiles();
                return;

            case KraidAiFunction.GrowFadeInRoomBackground:
                if (!AdvanceKraidRoomBackgroundFade(fadeToBlack: false))
                    return;
                FinishKraidGrowth(body, state);
                return;
            default:
                throw new InvalidOperationException($"Kraid body function {state.Parts[body.SlotIndex].Function} is not a growth function.");
        }
    }

    /// <summary>Steps room background colors toward their grown-form target.</summary>
    internal bool AdvanceKraidRoomBackgroundFade(
        bool fadeToBlack)
    {
        int step = _presentation.GradualColorChange.Numerator;
        if (step <= 13)
        {
            for (int color = 0; color < 16; color++)
            {
                Bgr555 current = _presentation.BossPalettes.KraidRoomBackdrop[color];
                Bgr555 target = fadeToBlack
                    ? Bgr555.Black
                    : ReadKraidColor(KraidPaletteSource.RoomBackdrop, color);
                _presentation.BossPalettes.KraidRoomBackdrop.Write(color, TransitionKraidColor(step, current, target));
            }
            _presentation.GradualColorChange.Advance();
            return false;
        }
        _presentation.GradualColorChange.Reset();
        return true;
    }

    /// <summary>Interpolates one RGB5 color toward its growth-phase target.</summary>
    private static Bgr555 TransitionKraidColor(int step, Bgr555 current, Bgr555 target) =>
        current.Zip(target, (_, from, to) => TransitionKraidComponent(step, from, to));

    /// <summary>Moves one color channel toward its target under the native step rule.</summary>
    private static int TransitionKraidComponent(int step, int current, int target)
    {
        if (step == 0)
            return current;
        if (step == 13)
            return target;
        int denominator = 13 - step;
        int fixedDelta = Math.Abs(target - current) * 256 / denominator;
        int signedDelta = target < current ? -fixedDelta : fixedDelta;
        return SnesEightEight.WholeOf(signedDelta + SnesEightEight.FromWhole(current));
    }

    /// <summary>Installs the grown body pose, room scrolls, and second-phase behavior.</summary>
    internal void FinishKraidGrowth(RoomEnemySlot body, KraidEnemyState state)
    {
        SetupKraidSecondPhaseThinking(body, state);

        foreach (KraidLintPart part in Enum.GetValues<KraidLintPart>())
        {
            int slot = (int)part;
            RoomEnemySlot lint = _slotTable.Slots[slot];
            state.Parts[lint.SlotIndex].FunctionTimer = KraidLintInitializationDefinitions.InitialDelay(part);
            state.Parts[lint.SlotIndex].Function = KraidAiFunction.AlignPartToKraid;
            state.Parts[slot].NextFunction = KraidAiFunction.LintProduce;
            state.Parts[lint.SlotIndex].LintTravel = 0;
        }
        for (int nailIndex = 0; nailIndex < 2; nailIndex++)
        {
            RoomEnemySlot nail = _slotTable.Slots[6 + nailIndex];
            state.Parts[6 + nailIndex].NextFunction = KraidAiFunction.FingernailInitialize;
            state.Parts[nail.SlotIndex].Function = KraidAiFunction.HandleFunctionTimer;
            state.Parts[nail.SlotIndex].FunctionTimer = unchecked((ushort)(64 + nailIndex * 64));
        }
        state.HeadProgram = KraidHeadAnimations.RoarAfterEntryFrame;
        state.WalkTarget = new Position16<RoomPixels>(288, 0);
        RoomEnemySlot foot = _slotTable.Slots[5];
        state.Parts[foot.SlotIndex].Function = KraidAiFunction.FootSecondPhaseWalkToStart;
        SetKraidPartProgram(foot, KraidPartProgram.FootWalkingBackward);
        foot.InstructionTimer = NativeCountdown.Start(1);
    }
    /// <summary>
    /// Ports <c>$A7:8A8F</c>: below half health the arm switches to its slow swing, unless its stored program is
    /// already the slow one or a later program.
    /// </summary>
    private KraidPartProgram? SelectKraidArmSpeedProgram()
    {
        RoomEnemySlot body = _slotTable.Slots[0];
        RoomEnemySlot arm = _slotTable.Slots[1];
        int halfHealth = RequireKraidState(body).HealthEighthThreshold(3);
        return body.Health < halfHealth && KraidPartResidentProgram(arm) < KraidPartProgram.ArmSlow
            ? KraidPartProgram.ArmSlow
            : null;
    }

    /// <summary>Dispatches a nail's active flight or rebound function.</summary>
    private void RunKraidNailMain(RoomEnemySlot nail, RoomLevelData? level)
    {
        KraidEnemyState state = RequireKraidState(nail);
        if (_slotTable.Slots[0].Health == 0)
        {
            nail.Properties = nail.Properties.With(
                EnemyProperties.Deleted | EnemyProperties.Invisible);
            return;
        }

        KraidPartState part = state.Parts[nail.SlotIndex];
        switch (state.Parts[nail.SlotIndex].Function)
        {
            case KraidAiFunction.HandleFunctionTimer:
                TickKraidFunctionTimer(part);
                return;

            case KraidAiFunction.FingernailInitialize:
                InitializeKraidNailFlight(nail, part);
                return;

            case KraidAiFunction.FingernailWaitForLint:
                if (unchecked((short)(_slotTable.Slots[2].Position.X.Whole - 256)) >= 0)
                {
                    state.Parts[nail.SlotIndex].Function = part.NextFunction;
                    nail.Properties = nail.Properties.Without(
                        EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
                }
                return;

            case KraidAiFunction.FingernailFire:
                if (level is null)
                {
                    throw new InvalidOperationException(
                        "Active Kraid fingernail movement requires room collision data.");
                }
                TickKraidNailFlight(nail, level);
                return;

            default:
                throw new InvalidDataException(
                    $"Kraid fingernail function {state.Parts[nail.SlotIndex].Function} is not translated.");
        }
    }

    /// <summary>Seeds a released nail's velocity, instruction list, and collision state.</summary>
    internal void InitializeKraidNailFlight(RoomEnemySlot nail, KraidPartState part)
    {
        // The pair coordinates its next launch through the other actor's previous
        // velocity and spawn flag. Read that actor before overwriting this one's state.
        int siblingIndex = nail.SlotIndex == 6 ? 7 : 6;
        RoomEnemySlot sibling = _slotTable.Slots[siblingIndex];
        KraidPartState siblingPart = RequireKraidState(nail).Parts[siblingIndex];
        int random = _room.Random.Read();
        (ushort xFraction, ushort xWhole, ushort yFraction, ushort yWhole) =
            KraidNailLaunchDefinitions.FromSiblingVelocity(siblingPart.NailVelocity.Y.WholeWord);
        part.NailVelocity = FixedVector<RoomPixels>.FromWords(xWhole, xFraction, yWhole, yFraction);
        part.NailLaunched = true;
        nail.Properties = nail.Properties.Without(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
        nail.InstructionTimer = NativeCountdown.Start(1);
        SetKraidPartProgram(nail, KraidPartProgram.NailLoop);
        part.Function = KraidAiFunction.FingernailFire;

        if ((random & 1) == 0 || siblingPart.AlternateSpawn)
        {
            part.AlternateSpawn = false;
            RoomEnemySlot body = _slotTable.Slots[0];
            nail.Position = nail.Position.WithWhole(new(unchecked((ushort)(
                (body.Position.X.Whole - body.Radius.X - nail.Radius.X) & ~15)), unchecked((ushort)(_slotTable.Slots[1].Position.Y.Whole + 128))));
            return;
        }

        part.AlternateSpawn = true;
        nail.Position = nail.Position.WithWhole(new(50, 240));
        part.NailVelocity = FixedVector<RoomPixels>.FromWords(1, 0, 0, 0);
        part.Function = KraidAiFunction.FingernailWaitForLint;
        part.NextFunction = KraidAiFunction.FingernailFire;
        nail.Properties = nail.Properties.With(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
    }

    /// <summary>Advances nail position and terrain response for one enemy update.</summary>
    internal void TickKraidNailFlight(RoomEnemySlot nail, RoomLevelData level)
    {
        KraidPartState part = Part(nail);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, nail, part.NailVelocity.X.RawValue))
            part.NailVelocity = part.NailVelocity.WithX(ReflectKraidNailHorizontalVelocity(part.NailVelocity.X));
        else if (KraidBodyContourReflectsNail(nail))
            part.NailVelocity = part.NailVelocity.WithX(ReflectKraidNailHorizontalVelocity(part.NailVelocity.X));

        if (_terrain.MoveEnemyVertically(level, nail, part.NailVelocity.Y.RawValue))
            part.NailVelocity = part.NailVelocity.WithY(SnesSignedSixteenSixteen.FromRaw(unchecked(-part.NailVelocity.Y.RawValue)));
    }

    /// <summary>Checks whether a nail crosses the body contour that reverses its flight.</summary>
    internal bool KraidBodyContourReflectsNail(RoomEnemySlot nail)
    {
        RoomEnemySlot body = _slotTable.Slots[0];
        ushort contourX = unchecked((ushort)(
            body.Position.X.Whole + KraidNailContourDefinitions.LeftOffset(
                unchecked((ushort)(nail.Position.Y.Whole - body.Position.Y.Whole)))));
        return unchecked((short)(nail.Position.X.Whole + nail.Radius.X - contourX)) >= 0 &&
            Part(nail).NailVelocity.X.WholePixels >= 0;
    }

    /// <summary>
    /// Native horizontal wall/contour reflections at $A7:BE9E and BEE2 negate
    /// each word independently. Do not propagate fractional borrow as a normal
    /// fixed-point negation would; the resulting nonzero-fraction quirk is native.
    /// </summary>
    private static SnesSignedSixteenSixteen ReflectKraidNailHorizontalVelocity(SnesSignedSixteenSixteen velocity) =>
        SnesSignedSixteenSixteen.FromParts(unchecked((short)-velocity.WholeWord), unchecked((ushort)-velocity.Fraction));

    /// <summary>Keeps the arm aligned to the body and advances its animation.</summary>
    private void RunKraidArmMain(RoomEnemySlot arm, ushort cameraY)
    {
        KraidEnemyState state = RequireKraidState(arm);
        RoomEnemySlot body = _slotTable.Slots[0];
        arm.Position = arm.Position.WithWhole(new(body.Position.X.Whole, unchecked((ushort)(body.Position.Y.Whole - 44))));

        // `$B7D3-$B7F0` hides only the arm when its anchor leaves the 224-line gameplay
        // viewport. This is actor visibility, not BG priority, so preserve property $0100.
        bool onScreen = unchecked((short)(arm.Position.Y.Whole - cameraY)) >= 0 &&
            unchecked((short)(arm.Position.Y.Whole - cameraY - 224)) < 0;
        arm.Properties = onScreen
            ? arm.Properties.Without(EnemyProperties.Invisible)
            : arm.Properties.With(EnemyProperties.Invisible);

        // While mouth reopens are pending, incrementing the timer here cancels the common
        // interpreter's later decrement and freezes the selected arm map.
        if (state.PendingMouthReopens != 0)
            arm.InstructionTimer = NativeCountdown.FromRaw(unchecked((ushort)(arm.InstructionTimer.RawValue + 1)));
    }

    /// <summary>Advances a lint part through idle, charge, fire, and reattachment states.</summary>
    internal void RunKraidLintMain(RoomEnemySlot lint, SamusState? samus)
    {
        KraidEnemyState state = RequireKraidState(lint);
        if (samus is not null)
            ResolveKraidLintContact(lint, samus);
        lint.InstructionTimer = NativeCountdown.Start(KraidStillPartTimer);
        KraidPartState part = state.Parts[lint.SlotIndex];
        switch (state.Parts[lint.SlotIndex].Function)
        {
            case KraidAiFunction.LintInactive:
                return;
            case KraidAiFunction.AlignPartToKraid:
                AlignKraidPart(lint, part);
                return;
            case KraidAiFunction.LintProduce:
                ProduceKraidLint(lint);
                return;
            case KraidAiFunction.LintCharge:
                ChargeKraidLint(lint);
                return;
            case KraidAiFunction.LintFire:
                FireKraidLint(lint, part, samus);
                return;
            default:
                throw new InvalidDataException(
                    $"Kraid lint slot {lint.SlotIndex} function {state.Parts[lint.SlotIndex].Function} " +
                    "is not translated.");
        }
    }

    /// <summary>Places a physical part at its body-relative offset.</summary>
    internal void AlignKraidPart(RoomEnemySlot partSlot, KraidPartState part)
    {
        partSlot.Position = partSlot.Position.WithWholeX(unchecked((ushort)(_slotTable.Slots[0].Position.X.Whole - partSlot.Radius.X)));
        TickKraidFunctionTimer(part);
    }

    /// <summary>Starts a lint projectile from the attached part.</summary>
    private void ProduceKraidLint(RoomEnemySlot lint)
    {
        lint.Properties = lint.Properties.Without(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
        lint.Position = lint.Position.WithWholeX(unchecked((ushort)(
            Part(lint).LintXOffset + _slotTable.Slots[0].Position.X.Whole - Part(lint).LintTravel)));
        Part(lint).LintTravel = unchecked((ushort)(Part(lint).LintTravel + 1));
        if (unchecked((short)(Part(lint).LintTravel - 32)) >= 0)
        {
            Part(lint).Function = KraidAiFunction.LintCharge;
            Part(lint).FunctionTimer = 30;
        }
    }

    /// <summary>Advances the visible lint charge before firing.</summary>
    private void ChargeKraidLint(RoomEnemySlot lint)
    {
        lint.PaletteIndex = (Part(lint).FunctionTimer & 1) != 0 ? (ushort)3584 : (ushort)0;
        lint.Position = lint.Position.WithWholeX(unchecked((ushort)(
            Part(lint).LintXOffset + _slotTable.Slots[0].Position.X.Whole - Part(lint).LintTravel)));
        NativeWordCounterStep charge = NativeCountdown.FromRaw(checked((ushort)Part(lint).FunctionTimer)).Decrement();
        Part(lint).FunctionTimer = charge.Remaining.RawValue;
        if (charge.IsZero)
        {
            Part(lint).Function = KraidAiFunction.LintFire;
            LastKraidSoundEffect = new KraidSoundRequest(KraidSoundDefinitions.LintFire);
        }
    }

    /// <summary>Launches a lint projectile toward Samus from the part's position.</summary>
    private static void FireKraidLint(RoomEnemySlot lint, KraidPartState part, SamusState? samus)
    {
        AddSignedKraidHorizontalDisplacement(lint, KraidPlatformMovement.FiringDisplacement);
        if (unchecked((short)(lint.Position.X.Whole - 56)) < 0)
            lint.Properties = lint.Properties.With(EnemyProperties.IgnoreSamusCollision);
        if (unchecked((short)(lint.Position.X.Whole - 32)) < 0)
        {
            lint.Properties = lint.Properties.With(EnemyProperties.Invisible);
            part.Function = KraidAiFunction.AlignPartToKraid;
            part.FunctionTimer = 300;
            part.NextFunction = KraidAiFunction.LintProduce;
            part.LintTravel = 0;
        }

        // Native checks support after moving and even on the frame that hides the lint.
        // Publish carry for Samus's later movement dispatcher, rather than teleporting
        // her here. The clamp changes only the whole word; retain the fractional borrow.
        if (samus is not null && EnemyCollision.IsEnemyTouchingSamusFromBelow(lint, samus))
        {
            SnesSignedSixteenSixteen carry = samus.Kinematics.ExtraDisplacement.X +
                SnesSignedSixteenSixteen.FromRaw(KraidPlatformMovement.FiringDisplacement);
            short whole = carry.WholePixels;
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(carry.WithWholePixels(
                unchecked((short)(whole - KraidPlatformMovement.MinimumCarryWholePixels)) < 0
                    ? (short)KraidPlatformMovement.MinimumCarryWholePixels : whole));
        }
    }

    /// <summary>Dispatches the foot's lunge, retreat, and grown-form walking functions.</summary>
    internal void RunKraidFootMain(RoomEnemySlot foot, ushort cameraY)
    {
        KraidEnemyState state = RequireKraidState(foot);
        RoomEnemySlot body = _slotTable.Slots[0];
        foot.Position = foot.Position.WithWhole(new(body.Position.X.Whole, unchecked((ushort)(body.Position.Y.Whole + 100))));
        bool onScreen = unchecked((short)(foot.Position.Y.Whole - cameraY)) >= 0 &&
            unchecked((short)(foot.Position.Y.Whole - 224 - cameraY)) < 0;
        foot.Properties = onScreen
            ? foot.Properties.Without(EnemyProperties.Invisible)
            : foot.Properties.With(EnemyProperties.Invisible);

        KraidPartState part = state.Parts[5];
        switch (state.Parts[foot.SlotIndex].Function)
        {
            case KraidAiFunction.NoOperation:
                return;
            case KraidAiFunction.FootFirstPhaseThinking:
                if (TryBeginKraidGrowth(body, state))
                    return;
                TickKraidFunctionTimer(part);
                return;
            case KraidAiFunction.FootPrepareFirstPhaseLunge:
                PrepareKraidFirstPhaseLunge(body, foot);
                return;
            case KraidAiFunction.FootFirstPhaseLunge:
                RunKraidFirstPhaseLunge(body, foot, part);
                return;
            case KraidAiFunction.DecrementFunctionTimerAndStartWalk:
                TickKraidFootTransitionTimer(foot, part);
                return;
            case KraidAiFunction.HandleFunctionTimer:
                TickKraidFunctionTimer(part);
                return;
            case KraidAiFunction.FootFirstPhaseRetreat:
                RunKraidFirstPhaseRetreat(body, foot, part);
                return;
            case KraidAiFunction.FootSecondPhaseWalkToStart:
                RunKraidSecondPhaseWalkToStart(body, foot, part);
                return;
            case KraidAiFunction.FootSecondPhaseInitialize:
                SetKraidWalkingRight(foot, part, targetX: 352, thinkTimer: 180);
                return;
            case KraidAiFunction.FootSecondPhaseThinking:
                RunKraidSecondPhaseThinking(body, foot, part);
                return;
            case KraidAiFunction.FootSecondPhaseWalkingRight:
                RunKraidSecondPhaseWalkingRight(body, foot);
                return;
            case KraidAiFunction.FootSecondPhaseWalkingLeft:
                RunKraidSecondPhaseWalkingLeft(body, foot);
                return;
            default:
                throw new InvalidDataException(
                    $"Kraid foot function {state.Parts[foot.SlotIndex].Function} is not translated.");
        }
    }

    /// <summary>Waits for the arm before starting the small-form foot lunge, or begins growth.</summary>
    private void PrepareKraidFirstPhaseLunge(RoomEnemySlot body, RoomEnemySlot foot)
    {
        if (TryBeginKraidGrowth(body, RequireKraidState(body)))
            return;
        RoomEnemySlot arm = _slotTable.Slots[1];
        if (KraidPartResidentProgram(arm) < KraidPartProgram.ArmNormalPause)
            return;
        SetKraidPartProgram(arm, KraidPartProgram.ArmDyingOrPreparingToLunge);
        arm.InstructionTimer = NativeCountdown.Start(1);
        SetKraidPartProgram(foot, KraidPartProgram.FootLungeForward);
        foot.InstructionTimer = NativeCountdown.Start(1);
        Part(foot).Function = KraidAiFunction.FootFirstPhaseLunge;
        Part(foot).FunctionTimer = 0;
    }

    /// <summary>Moves the small-form foot and body through the forward lunge.</summary>
    private void RunKraidFirstPhaseLunge(
        RoomEnemySlot body,
        RoomEnemySlot foot,
        KraidPartState part)
    {
        if (TryBeginKraidGrowth(body, RequireKraidState(body)))
            return;
        if (unchecked((short)(body.Position.X.Whole - 92)) < 0)
            body.Position = body.Position.WithWholeX(92);
        if (KraidPartResidentProgram(foot) != KraidPartProgram.FootLungeForwardFinished)
            return;
        if (body.Position.X.Whole == 92)
        {
            part.NextFunction = KraidAiFunction.FootFirstPhaseRetreat;
            Part(foot).Function = KraidAiFunction.DecrementFunctionTimerAndStartWalk;
            Part(foot).FunctionTimer = 1;
            SetKraidPartProgram(foot, KraidPartProgram.FootNeutral);
        }
        else
        {
            SetKraidPartProgram(foot, KraidPartProgram.FootLungeForward);
        }
        foot.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Counts down a foot phase and installs its next function at zero.</summary>
    private void TickKraidFootTransitionTimer(RoomEnemySlot foot, KraidPartState part)
    {
        NativeCountdown phaseTimer = NativeCountdown.FromRaw(checked((ushort)Part(foot).FunctionTimer));
        if (phaseTimer.IsZero)
            return;
        NativeWordCounterStep step = phaseTimer.Decrement();
        Part(foot).FunctionTimer = step.Remaining.RawValue;
        if (step.IsZero)
        {
            Part(foot).Function = part.NextFunction;
            SetKraidPartProgram(foot, KraidPartProgram.FootWalkingBackward);
            foot.InstructionTimer = NativeCountdown.Start(1);
        }
    }

    /// <summary>Returns the foot and body after the small-form lunge.</summary>
    private void RunKraidFirstPhaseRetreat(
        RoomEnemySlot body,
        RoomEnemySlot foot,
        KraidPartState part)
    {
        if (TryBeginKraidGrowth(body, RequireKraidState(body)))
            return;
        if (unchecked((short)(body.Position.X.Whole - 176)) >= 0)
            body.Position = body.Position.WithWholeX(176);
        if (KraidPartResidentProgram(foot) < KraidPartProgram.FootWalkingBackwardLoop)
            return;
        if (body.Position.X.Whole == 176)
        {
            RoomEnemySlot arm = _slotTable.Slots[1];
            SetKraidPartProgram(arm, KraidPartProgram.ArmNormal);
            arm.InstructionTimer = NativeCountdown.Start(1);
            SetKraidPartProgram(foot, KraidPartProgram.FootNeutral);
            foot.InstructionTimer = NativeCountdown.Start(1);
            Part(foot).Function = KraidAiFunction.FootFirstPhaseThinking;
            Part(foot).FunctionTimer = 300;
            part.NextFunction = KraidAiFunction.FootPrepareFirstPhaseLunge;
        }
        else
        {
            SetKraidPartProgram(foot, KraidPartProgram.FootWalkingBackward);
            foot.InstructionTimer = NativeCountdown.Start(1);
        }
    }

    /// <summary>Positions the grown-form foot before its walking cycle begins.</summary>
    private void RunKraidSecondPhaseWalkToStart(
        RoomEnemySlot body,
        RoomEnemySlot foot,
        KraidPartState part)
    {
        Position16<RoomPixels> target = RequireKraidState(body).WalkTarget;
        if (target.X != body.Position.X.Whole)
        {
            if ((target - body.Position.Whole).X >= 0)
                return;
            body.Position = body.Position.WithWholeX(target.X);
        }
        if (KraidPartResidentProgram(foot) < KraidPartProgram.FootWalkingBackwardLoop)
            return;
        Part(foot).Function = KraidAiFunction.HandleFunctionTimer;
        Part(foot).FunctionTimer = 180;
        part.NextFunction = KraidAiFunction.FootSecondPhaseInitialize;
        SetKraidPartProgram(foot, KraidPartProgram.FootNeutral);
        foot.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Chooses the grown foot's next walking target and direction after its think timer ends.</summary>
    internal void RunKraidSecondPhaseThinking(
        RoomEnemySlot body,
        RoomEnemySlot foot,
        KraidPartState part)
    {
        part.ThinkTimer = part.ThinkTimer.Decrement().Remaining;
        if (!part.ThinkTimer.IsZero)
            return;

        (ushort targetX, ushort thinkTimer) = KraidMovementChoices.Select(body.Position.X.Whole, ReadKraidRandomNumber());
        if (unchecked((short)(targetX - body.Position.X.Whole)) >= 0)
            SetKraidWalkingRight(foot, part, targetX, thinkTimer);
        else
            SetKraidWalkingLeft(foot, part, targetX, thinkTimer);
    }

    /// <summary>Stores a rightward target and starts the foot's walking animation.</summary>
    private void SetKraidWalkingRight(
        RoomEnemySlot foot,
        KraidPartState part,
        ushort targetX,
        ushort thinkTimer)
    {
        RequireKraidState(foot).WalkTarget = new Position16<RoomPixels>(targetX, 0);
        part.ThinkTimer = NativeCountdown.FromRaw(thinkTimer);
        Part(foot).Function = KraidAiFunction.FootSecondPhaseWalkingRight;
        SetKraidPartProgram(foot, KraidPartProgram.FootWalkingBackward);
        foot.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Stores a leftward target and starts the foot's walking animation.</summary>
    private void SetKraidWalkingLeft(
        RoomEnemySlot foot,
        KraidPartState part,
        ushort targetX,
        ushort thinkTimer)
    {
        RequireKraidState(foot).WalkTarget = new Position16<RoomPixels>(targetX, 0);
        part.ThinkTimer = NativeCountdown.FromRaw(thinkTimer);
        Part(foot).Function = KraidAiFunction.FootSecondPhaseWalkingLeft;
        SetKraidPartProgram(foot, KraidPartProgram.FootWalkingForward);
        foot.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Moves the body and foot toward the selected rightward target.</summary>
    private void RunKraidSecondPhaseWalkingRight(RoomEnemySlot body, RoomEnemySlot foot)
    {
        Position16<RoomPixels> target = RequireKraidState(body).WalkTarget;
        if (target.X != body.Position.X.Whole)
        {
            if ((target - body.Position.Whole).X >= 0)
                return;
            body.Position = body.Position.WithWholeX(target.X);
        }
        if (KraidPartResidentProgram(foot) >= KraidPartProgram.FootWalkingBackwardLoop)
        {
            Part(foot).Function = KraidAiFunction.FootSecondPhaseThinking;
            SetKraidPartProgram(foot, KraidPartProgram.FootNeutral);
            foot.InstructionTimer = NativeCountdown.Start(1);
        }
    }

    /// <summary>Moves the body and foot toward the selected leftward target.</summary>
    private void RunKraidSecondPhaseWalkingLeft(RoomEnemySlot body, RoomEnemySlot foot)
    {
        Position16<RoomPixels> target = RequireKraidState(body).WalkTarget;
        if ((target - body.Position.Whole).X < 0)
        {
            if (KraidPartResidentProgram(foot) == KraidPartProgram.FootWalkingForwardFinished)
            {
                SetKraidPartProgram(foot, KraidPartProgram.FootWalkingForward);
                foot.InstructionTimer = NativeCountdown.Start(1);
            }
            return;
        }
        body.Position = body.Position.WithWholeX(target.X);
        if (KraidPartResidentProgram(foot) == KraidPartProgram.FootWalkingForwardFinished)
        {
            Part(foot).Function = KraidAiFunction.FootSecondPhaseThinking;
            SetKraidPartProgram(foot, KraidPartProgram.FootNeutral);
            foot.InstructionTimer = NativeCountdown.Start(1);
        }
    }

    /// <summary>Adds native signed horizontal motion to the body position with word wraparound.</summary>
    private static void AddSignedKraidHorizontalDisplacement(
        RoomEnemySlot slot,
        int displacement)
    {
        uint position = slot.Position.X.RawValue;
        position = unchecked(position + (uint)displacement);
        slot.Position = slot.Position.WithX(SnesFixedPosition.FromRaw(unchecked((uint)position)));
    }
    /// <summary>Emits a mouth rock when a projectile slot is available.</summary>
    internal bool SpawnKraidSpitRock(RoomEnemySlot body)
    {
        RoomEnemyProjectileSlot? rock = _projectiles.AllocateEnemyProjectile();
        if (rock is null)
            return false;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            rock,
            RoomEnemyProjectileKind.KraidSpitRock,
            graphicsIndex: EnemyPaletteBits.Palette3);
        rock.StartAnimation(new KraidRockAnimation(KraidRockProgram.SharedPose));
        int random = ReadKraidRandomNumber();
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(body.Position.X.Whole + 16)), unchecked((ushort)(body.Position.Y.Whole - 96))));
        rock.Velocity = rock.Velocity.WithX(new SnesSignedEightEight(KraidRockLaunchDefinitions.FromRandom(random)));
        rock.Velocity = rock.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(-1024));
        rock.GraphicsIndex = EnemyPaletteBits.Palette3;
        return true;
    }

    /// <summary>Requests a rock from the floor during the growth sequence.</summary>
    internal void RequestKraidRisingRock(RoomEnemySlot body, KraidEnemyState state)
    {
        state.RiseRockSpawnRequestCount++;
        int random = ReadKraidRandomNumber();
        RoomEnemyProjectileKind kind = (random & 16) != 0
            ? RoomEnemyProjectileKind.KraidRisingRockRight
            : RoomEnemyProjectileKind.KraidRisingRockLeft;
        RoomEnemyProjectileSlot? rock = _projectiles.AllocateEnemyProjectile();
        if (rock is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(rock, kind, graphicsIndex: EnemyPaletteBits.Palette3);
        rock.StartAnimation(new KraidRockAnimation(kind == RoomEnemyProjectileKind.KraidRisingRockRight
            ? KraidRockProgram.RisingRightPose
            : KraidRockProgram.SharedPose));
        // $86:9D17 tests bit zero: clear complements the offset to the left.
        int randomOffset = random & 63;
        if ((random & 1) == 0)
            randomOffset = unchecked((short)~randomOffset);
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(body.Position.X.Whole + randomOffset)), 432));
        rock.Velocity = new(new SnesSignedEightEight(unchecked((ushort)(random & 1008))), SnesSignedEightEight.FromSignedRaw(-1280));
        rock.GraphicsIndex = EnemyPaletteBits.Palette3;
        state.SpawnedRiseRockCount++;
        LastKraidSoundEffect = new KraidSoundRequest(KraidSoundDefinitions.RockSpit);
    }

    /// <summary>Emits a ceiling rock at the scripted horizontal position when a slot is free.</summary>
    internal bool SpawnKraidCeilingRock(ushort xPosition)
    {
        RoomEnemyProjectileSlot? rock = _projectiles.AllocateEnemyProjectile();
        if (rock is null)
            return false;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            rock,
            RoomEnemyProjectileKind.KraidCeilingRock,
            graphicsIndex: EnemyPaletteBits.Palette3);
        rock.StartAnimation(new KraidRockAnimation(KraidRockProgram.SharedPose));
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(xPosition, 312));
        rock.Velocity = rock.Velocity.WithX(new SnesSignedEightEight(0));
        rock.Velocity = rock.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)((ReadKraidRandomNumber() & 63) + 64))));
        rock.GraphicsIndex = EnemyPaletteBits.Palette3;
        return true;
    }

    /// <summary>Updates a spit rock's flight and removes it at its native boundary.</summary>
    internal static void RunKraidRockPreInstruction(
        RoomEnemyProjectileSlot rock,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(rock, level, horizontal: true) ||
            EnemyProjectileEngine.MoveProjectileAxis(rock, level, horizontal: false))
        {
            rock.Clear();
            return;
        }

        short horizontal = unchecked((short)(rock.Velocity.X.RawValue + 8));
        if (horizontal >= 256)
            horizontal = -256;
        rock.Velocity = rock.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)horizontal)));
        rock.Velocity = rock.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(rock.Velocity.Y.RawValue + 64))));
    }

    /// <summary>Updates a falling ceiling rock and its contact with room terrain.</summary>
    internal static void RunKraidCeilingRockPreInstruction(
        RoomEnemyProjectileSlot rock,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(rock, level, horizontal: false))
        {
            rock.Clear();
            return;
        }
        rock.Velocity = rock.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)((rock.Velocity.Y.RawValue + 24) & 16383))));
    }

    /// <summary>Consumes one value from the room RNG for Kraid's next decision.</summary>
    private int ReadKraidRandomNumber() =>
        _room.Random.Read();
    /// <summary>Dispatches the authored vertical rise and pause functions.</summary>
    internal void RunKraidRiseFunction(
        RoomEnemySlot body,
        KraidEnemyState state,
        SamusState? samus)
    {
        KraidAiFunction function = state.Parts[body.SlotIndex].Function;
        switch (function)
        {
            case KraidAiFunction.RestrictSamusToFirstScreen:
                RestrictSamusToKraidFirstScreen(samus);
                TickKraidFunctionTimer(state.Parts[0]);
                return;

            case KraidAiFunction.RaiseKraidThroughFloor:
                RestrictSamusToKraidFirstScreen(samus);
                state.Parts[body.SlotIndex].Function = KraidAiFunction.RaiseLoadBottomTilemap;
                TransferKraidTopTilemap(state);
                return;

            case KraidAiFunction.RaiseLoadBottomTilemap:
                RestrictSamusToKraidFirstScreen(samus);
                state.Parts[body.SlotIndex].Function = KraidAiFunction.RaiseRocksEvery16Frames;
                state.Parts[body.SlotIndex].RisingRockTimer = 120;
                _earthquake.EarthquakeTimer = NativeCountdown.Start(496);
                state.MusicRequest = MusicCommand.SelectTrack(5);
                TransferKraidBottomTilemap(state);
                return;

            case KraidAiFunction.RaiseRocksEvery16Frames:
                RestrictSamusToKraidFirstScreen(samus);
                NativeWordCounterStep slowRockStep = NativeCountdown.FromRaw(checked((ushort)state.Parts[body.SlotIndex].RisingRockTimer)).Decrement();
                state.Parts[body.SlotIndex].RisingRockTimer = slowRockStep.Remaining.RawValue;
                if (slowRockStep.IsZero)
                {
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.RaiseRocksEvery8Frames;
                    state.Parts[body.SlotIndex].RisingRockTimer = 96;
                }
                else if ((state.Parts[body.SlotIndex].RisingRockTimer & 15) == 0)
                {
                    RequestKraidRisingRock(body, state);
                }
                return;

            case KraidAiFunction.RaiseRocksEvery8Frames:
                RestrictSamusToKraidFirstScreen(samus);
                NativeWordCounterStep fastRockStep = NativeCountdown.FromRaw(checked((ushort)state.Parts[body.SlotIndex].RisingRockTimer)).Decrement();
                state.Parts[body.SlotIndex].RisingRockTimer = fastRockStep.Remaining.RawValue;
                if (fastRockStep.IsZero)
                {
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.RaiseBody;
                    state.Parts[body.SlotIndex].FunctionTimer = 288;
                }
                else if ((state.Parts[body.SlotIndex].RisingRockTimer & 7) == 0)
                {
                    RequestKraidRisingRock(body, state);
                }
                return;

            case KraidAiFunction.RaiseBody:
                RestrictSamusToKraidFirstScreen(samus);
                if ((_earthquake.EarthquakeTimer.RawValue & 5) == 0)
                    RequestKraidRisingRock(body, state);
                body.Position = body.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole +
                    ((body.Position.Y.Whole & 2) == 0 ? -1 : 1))));
                AddSignedKraidVerticalDisplacement(body, -32768);
                if (unchecked((short)(body.Position.Y.Whole - 457)) < 0)
                {
                    body.Position = body.Position.WithWholeX(176);
                    // `$C97B` does not leave execution at setup routine `$ADE9`.
                    // That JSR immediately installs `$AEA4`, seeds the random thinking
                    // timer and selects roar list entry `$96DA` for the first attack.
                    state.HeadProgram = KraidHeadAnimations.RoarAfterEntryFrame;
                    SetupKraidFirstPhaseThinking(body, state);

                    // The foot owns first-phase lunges independently of the body thinker.
                    // Its 300-frame delay starts on the exact rise-completion frame.
                    RoomEnemySlot foot = _slotTable.Slots[5];
                    state.Parts[foot.SlotIndex].Function = KraidAiFunction.FootFirstPhaseThinking;
                    state.Parts[foot.SlotIndex].FunctionTimer = 300;
                    state.Parts[5].NextFunction = KraidAiFunction.FootPrepareFirstPhaseLunge;
                    SetKraidPartProgram(_slotTable.Slots[1], KraidPartProgram.ArmNormal);
                    _slotTable.Slots[1].InstructionTimer = NativeCountdown.Start(1);
                }
                return;

            default:
                throw new InvalidDataException(
                    $"Kraid body function {state.Parts[body.SlotIndex].Function} is not translated.");
        }
    }

    /// <summary>
    /// Ports <c>RestrictSamusXPositionToFirstScreen</c> ($A7:C9EE). It writes the integer X
    /// and previous X words only, so the camera's $90:95A0 compare sees Samus's subsequent
    /// movement away from $100 as a scroll.
    /// </summary>
    private static void RestrictSamusToKraidFirstScreen(SamusState? samus)
    {
        if (samus is not null && unchecked((short)(samus.Position.X.Whole - 256)) >= 0)
        {
            samus.Position = samus.Position.WithWholeX(256);
            samus.WritePreviousXPosition(256);
        }
    }

    /// <summary>Counts a Kraid function timer down and installs its next state at zero.</summary>
    private static void TickKraidFunctionTimer(KraidPartState part)
    {
        NativeCountdown functionTimer = NativeCountdown.FromRaw(checked((ushort)part.FunctionTimer));
        if (functionTimer.IsZero)
            return;
        NativeWordCounterStep step = functionTimer.Decrement();
        part.FunctionTimer = step.Remaining.RawValue;
        if (step.IsZero)
            part.Function = part.NextFunction;
    }

    /// <summary>Adds native signed vertical motion to the body position with fixed-point wraparound.</summary>
    private static void AddSignedKraidVerticalDisplacement(
        RoomEnemySlot body,
        int displacement)
    {
        uint position = body.Position.Y.RawValue;
        position = unchecked(position + (uint)displacement);
        body.Position = body.Position.WithY(SnesFixedPosition.FromRaw(unchecked((uint)position)));
    }

    /// <summary>Derives a nonzero multiple-of-64 thinking delay from the room RNG.</summary>
    private ushort ReadKraidThinkingTimer()
    {
        int random = _room.Random.Read();
        int value = random & 7;
        if (value == 0)
            value = 2;
        return unchecked((ushort)(value << 6));
    }
    /// <summary>Active typed state when the loaded room owns retail Kraid slot zero.</summary>
    public KraidEnemyState? Kraid { get; internal set; }

    /// <summary>
    /// Kraid's room state, created by the body's initializer and kept for the room. Parts
    /// read slot zero's words directly, so they keep running after the dead-room body is
    /// deleted, as the fingernails do.
    /// </summary>
    private KraidEnemyState RequireKraidState(RoomEnemySlot slot)
    {
        if (Kraid is null)
        {
            throw new InvalidOperationException(
                $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} requires Kraid's body to have initialized this room.");
        }
        return Kraid;
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        // Legacy publications are momentary like their native QueueSfx/QueueMusic calls; clearing them at the
        // EnemyMain frame boundary keeps a paused or frozen actor from replaying its previous request.
        if (Kraid is not null)
            Kraid.MusicRequest = null;
        LastKraidSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastKraidSoundEffect is { } kraidSound)
            audio.QueueEnemySound(kraidSound.SoundEffect, maximumQueued: 6);
    }

    /// <summary>Queues this frame's published music requests through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacyMusic(EnemyAudioQueue audio)
    {
        audio.QueueLegacyMusic(Kraid?.MusicRequest, MusicCommandDelay.EightFrames);
    }

    /// <summary>Binds nail touch callbacks and their shared contact-damage path.</summary>
    internal void RegisterKraidNailTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidNail &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.KraidNailTouch ||
                               slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidNailBad &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.KraidBadNailTouch,
            context =>
            {
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
                // The callback passes the common routine's remaining-health accumulator
                // to EnemyDeath; its bounded animation selector normalizes values >= 5.
                if (context.Slot.EnemyDefinitionPointer != 0)
                    _deathsAndDrops.StartGenericEnemyDeath(context.Slot, checked((ushort)context.Slot.Health));
            });
    }

    /// <summary>Registers the shared pre-instruction used by spit rocks.</summary>
    internal static void RegisterKraidRockSpitPreInstruction(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            context.Projectile.GraphicsIndex = 0;
        }, EnemyProjectilePreInstruction.KraidRockSpit_UsePalette0);
    }

    /// <summary>The callback of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindKraidFootHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        if (!(enemy.EnemyDefinitionPointer == EnemyDefinitionId.KraidFoot &&
            enemy.Definition.Bank == KraidFootCollisionDefinitions.Bank &&
            KraidFootCollisionDefinitions.TryGetComponents(
            EnemyFrames.Spritemap(enemy.Frame), out var footComponents)))
            return null;

        // Kraid's foot art is replaceable, but each native extended frame
        // still places two physical components against one shared hitbox list.
        foreach (KraidFootCollisionComponent component in footComponents)
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (KraidFootCollisionHitbox hitbox in component.Hitboxes)
            {
                ushort left = unchecked((ushort)(componentOrigin.X + hitbox.Left));
                ushort top = unchecked((ushort)(componentOrigin.Y + hitbox.Top));
                ushort right = unchecked((ushort)(componentOrigin.X + hitbox.Right));
                ushort bottom = unchecked((ushort)(componentOrigin.Y + hitbox.Bottom));
                if (!probe.Overlaps(left, top, right, bottom))
                    continue;
                callback = probe.SelectShotCallback
                    ? KraidHitboxCallbackCodec.ToCallback(hitbox.Shot)
                    : KraidHitboxCallbackCodec.ToCallback(hitbox.Touch);
                return true;
            }
        }
        return false;
    
    }

    /// <summary>The callback of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindKraidArmHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        if (!(enemy.EnemyDefinitionPointer == EnemyDefinitionId.KraidArm &&
            enemy.Definition.Bank == KraidArmCollisionDefinitions.Bank &&
            KraidArmCollisionDefinitions.TryGetComponents(
            EnemyFrames.Spritemap(enemy.Frame), out var kraidComponents)))
            return null;

        // Kraid's arm art is editable, but the native extended spritemap
        // also carries a separate, ordered physical component stream.
        // Read that stream from the immutable cartridge definition catalog.
        foreach (KraidArmCollisionComponent component in kraidComponents)
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            ushort componentX = componentOrigin.X;
            ushort componentY = componentOrigin.Y;
            foreach (KraidArmCollisionHitbox hitbox in component.Hitboxes)
            {
                ushort left = unchecked((ushort)(componentX + hitbox.Left));
                ushort top = unchecked((ushort)(componentY + hitbox.Top));
                ushort right = unchecked((ushort)(componentX + hitbox.Right));
                ushort bottom = unchecked((ushort)(componentY + hitbox.Bottom));
                if (!probe.Overlaps(left, top, right, bottom))
                    continue;

                callback = probe.SelectShotCallback
                    ? KraidHitboxCallbackCodec.ToCallback(hitbox.Shot)
                    : KraidHitboxCallbackCodec.ToCallback(hitbox.Touch);
                return true;
            }
        }
        return false;
    
    }

    /// <summary>The state of the Kraid part in <paramref name="slot"/>.</summary>
    private KraidPartState Part(RoomEnemySlot slot) => RequireKraidState(slot).Parts[slot.SlotIndex];

    /// <summary>Installs a part's state with its slot, so a kill empties its record words.</summary>
    private static void RegisterKraidPart(RoomEnemySlot slot, KraidEnemyState state)
    {
        KraidPartState part = state.Parts[slot.SlotIndex];
        slot.SetSpeciesState(part, part.ClearWithRecord);
    }
}

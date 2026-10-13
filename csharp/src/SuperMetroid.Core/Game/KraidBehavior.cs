using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Kraid species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class KraidBehavior : IEnemySpecies
{
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

    /// <summary>Native rising-or-sinking arm list selected at room initialization.</summary>
    private const ushort KraidInitialArmInstruction =
        KraidArmInstructionProgramDefinitions.RisingOrSinking;
    /// <summary>Native initial lint animation list for each lint part.</summary>
    private const ushort KraidInitialLintInstruction =
        KraidLintInstructionLists.InitialLint;
    /// <summary>Native initial foot animation list before lunge or walking.</summary>
    private const ushort KraidInitialFootInstruction =
        KraidFootInstructionProgramDefinitions.Initial;

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
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
        {
            // `$A7:A959` installs the dead-room BG palette before the shared part initializer
            // marks every physical actor invisible/deleted/non-interactive. Its two calls
            // immediately before that initializer are not cosmetic bookkeeping: they
            // restore the defeated arena on every room load so the broken ceiling and
            // removed floor spikes cannot return when the player exits and re-enters.
            LoadKraidColorBand(KraidPaletteSource.RoomBackdrop, _presentation.BossPalettes.KraidRoomBackdrop);
            state.BackgroundTilemapWords.AsSpan().Fill(KraidBackgroundRomData.BlankTile);
            state.BackgroundTilemapsPrepared = true;
            UploadKraidRoomBackgroundTiles();
            _kraidPlmRequests.AddRange(KraidPlmDefinitions.DefeatedRoom);
            // The body is the owner of `$A7:C715-$C815`; deleting it here skips the two
            // BG2 clears and four standard-BG3 restoration DMAs. The other seven physical
            // records take their own dead initializer and remain deleted as on cartridge.
            body.VariableA = (ushort)KraidAiFunction.DeathClearTopTilemap;
            return;
        }

        // $A7:A9E4-A9E7.
        _zebetiteBehavior.CameraDistanceIndex = CameraDistanceMode.BossTracking;
        ApplyKraidScrolls(grown: false);
        state.MinimumYPositionForEjection = 324;
        state.InitialHealth = body.Health;

        // `$A7:AAC6` constructs the private WRAM tilemap which subsequent rise, head,
        // growth, and death functions upload in independently timed slices.
        InitializeKraidBackground(state);
        state.HurtFrame = 0;
        state.HurtFrameTimer = NativeCountdown.Start(0);

        body.Position = body.Position.WithWhole(new(176, 592));
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        body.VariableA = (ushort)KraidAiFunction.RestrictSamusToFirstScreen;
        body.VariableF = 300;
        body.VariableC = 64;
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
        arm.PaletteIndex = _slotTable._slots[0].PaletteIndex;
        arm.VariableA = (ushort)KraidAiFunction.NoOperation;
        arm.CurrentInstruction = KraidInitialArmInstruction;
        arm.InstructionTimer = NativeCountdown.Start(1);
        arm.VariableB = 0;
        state.Parts[arm.SlotIndex].NextFunction = KraidAiFunction.NoOperation;
    }

    /// <summary>Initializes one lint part and verifies its expected physical slot.</summary>
    private void InitializeKraidLint(RoomEnemySlot lint, int expectedSlot)
    {
        _ = RequireKraidState(lint);
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
        {
            MarkKraidPartDead(lint);
            return;
        }
        EnsureKraidSlot(lint, expectedSlot, "lint");
        lint.PaletteIndex = _slotTable._slots[0].PaletteIndex;
        lint.InstructionTimer = NativeCountdown.Start(0x7fff);
        lint.CurrentInstruction = KraidInitialLintInstruction;
        lint.SpritemapPointer = KraidLintVisualDefinitions.InitialFrame;
        lint.VariableA = (ushort)KraidAiFunction.LintInactive;
        lint.VariableC = expectedSlot == 2 ? (ushort)0 : (ushort)0xfff0;
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
        foot.PaletteIndex = _slotTable._slots[0].PaletteIndex;
        foot.CurrentInstruction = KraidInitialFootInstruction;
        foot.InstructionTimer = NativeCountdown.Start(1);
        foot.VariableA = (ushort)KraidAiFunction.NoOperation;
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
        nail.PaletteIndex = _slotTable._slots[0].PaletteIndex;
        nail.VariableB = 40;
        nail.Properties = nail.Properties.With(EnemyProperties.Invisible);
        nail.InstructionTimer = NativeCountdown.Start(0x7fff);
        nail.CurrentInstruction = KraidNailInstructionProgramDefinitions.Loop;
        nail.SpritemapPointer = KraidVisualDefinitions.InitialNailFrame;
        state.Parts[nail.SlotIndex].NextFunction = KraidAiFunction.FingernailInitialize;
        nail.VariableA = (ushort)KraidAiFunction.HandleFunctionTimer;
        nail.VariableF = 64;
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
        KraidAiFunction function = (KraidAiFunction)body.VariableA;
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
            state.HurtFrame = NativeWordCounter.Decrement(state.HurtFrame).Value;
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
    internal void RegisterKraidBombReactions(EnemyAiTable table)
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
                        $"normal-bomb shot AI $A7:{c.SelectedShotAiPointer:X4}.");
                }

                // `$A7:94B6` creates a room-graphics dust actor at the selected physical projectile coordinates,
                // then returns to the multibox walker that has already installed direction bit `$10`. `$94B5` is the
                // adjacent literal RTL used by protected arm frames and every foot rectangle.
                if (c.SelectedShotAi == EnemyInteractionCallback.KraidArmShot)
                    SpawnKraidArmShotExplosion(c.Bomb.Type, c.Bomb.Position.Whole);
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

    /// <summary>Binds initializers for Kraid's body, arm, lint, foot, and nail records.</summary>
    internal void RegisterKraidInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Kraid, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Kraid, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeKraidBody(slot);
        });

        table.RegisterInitialization(EnemyAiRoutine.InitAI_KraidArm, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidArm, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeKraidArm(slot);
        });

        table.RegisterInitialization(EnemyAiRoutine.InitAI_KraidLintTop, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidLintTop, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeKraidLint(slot, expectedSlot: 2);
        });

        table.RegisterInitialization(EnemyAiRoutine.InitAI_KraidLintMiddle, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidLintMiddle, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeKraidLint(slot, expectedSlot: 3);
        });

        table.RegisterInitialization(EnemyAiRoutine.InitAI_KraidLintBottom, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidLintBottom, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeKraidLint(slot, expectedSlot: 4);
        });

        table.RegisterInitialization(EnemyAiRoutine.InitAI_KraidFoot, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidFoot, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeKraidFoot(slot);
        });

        table.RegisterInitialization(EnemyAiRoutine.InitAI_KraidNail, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidNail, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeKraidNail(slot, expectedSlot: 6);
        });

        table.RegisterInitialization(EnemyAiRoutine.InitAI_KraidNailBad, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidNailBad, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeKraidNail(slot, expectedSlot: 7);
        });
    }

    /// <summary>Binds frame updates for Kraid's body and physical parts.</summary>
    internal void RegisterKraidMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Kraid, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Kraid, (slot, frame, nmiFrameCounter8) =>
        {
            RunKraidBodyMain(slot, frame.Samus, frame.Camera, frame.VramWriteQueue, frame.SamusProjectiles, frame.SharedProjectiles);
        });

        table.RegisterMain(EnemyAiRoutine.MainAI_KraidArm, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidArm, (slot, frame, nmiFrameCounter8) =>
        {
            RunKraidArmMain(slot, frame.Camera.Y);
        });

        EnemyMainAiHandler shared107 = (slot, frame, nmiFrameCounter8) =>
        {
            RunKraidLintMain(slot, frame.Samus);
        };

        table.RegisterMain(EnemyAiRoutine.MainAI_KraidLintTop, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidLintTop, shared107);

        table.RegisterMain(EnemyAiRoutine.MainAI_KraidLintMiddle, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidLintMiddle, shared107);

        table.RegisterMain(EnemyAiRoutine.MainAI_KraidLintBottom, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidLintBottom, shared107);

        table.RegisterMain(EnemyAiRoutine.MainAI_KraidFoot, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidFoot, (slot, frame, nmiFrameCounter8) =>
        {
            RunKraidFootMain(slot, frame.Camera.Y);
        });

        EnemyMainAiHandler shared112 = (slot, frame, nmiFrameCounter8) =>
        {
            RunKraidNailMain(slot, frame.Level);
        };

        table.RegisterMain(EnemyAiRoutine.MainAI_KraidNail, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidNail, shared112);

        table.RegisterMain(EnemyAiRoutine.MainAI_KraidNailBad, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidNailBad, shared112);
    }

    /// <summary>Registers Kraid-specific enemy instruction commands and their arguments.</summary>
    internal void RegisterKraidOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessKraidArmInstruction(slot, word, ref cursor);
        });

        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessKraidFootInstruction(slot, context.Level, word, ref cursor);
        });
    }

    /// <summary>Binds shot callbacks for Kraid's mouth, body, arm, and nail surfaces.</summary>
    internal void RegisterKraidShotReactions(EnemyAiTable table)
    {
        // Kraid's arm and foot are admitted by their literal no-op header callback, but their authored hitboxes
        // select the arm's private callback.
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer is EnemyDefinitionId.KraidArm or EnemyDefinitionId.KraidFoot &&
                                                     EnemyCollision.IsLiteralNoOpEnemyAi(enemy.Definition.Bank, enemy.Definition.ShotAiPointer))
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
                        $"shot AI $A7:{c.HitboxShotAiPointer:X4}.");
                }

                c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                if (c.HitboxShotAi == EnemyInteractionCallback.KraidArmShot)
                    SpawnKraidArmShotExplosion(c.ProjectileType, c.Projectile.Position.Whole);
                return true;
            },
        });
    }

    /// <summary>Binds Samus contact handling for Kraid's body and parts.</summary>
    internal void RegisterKraidTouch(EnemyAiTable table)
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
                _slotTable._slots[4].VariableA = (ushort)KraidAiFunction.LintFire;
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, skipDeathAnimation: true);
            });
    }
    /// <summary>Ports the working-map construction performed by <c>$A7:AAC6</c>.</summary>
    internal void InitializeKraidBackground(KraidEnemyState state)
    {
        KraidBackgroundArtwork art = _room.Artwork?.KraidBackground ?? throw new InvalidOperationException(
            "Kraid BG2 requires installed enemy artwork.");
        byte[] upper = art.Upper.Transfer.ToArray();
        byte[] lower = art.Lower.Transfer.ToArray();
        if (upper.Length != KraidBackgroundRomData.DecompressedTilemapBytes ||
            lower.Length != KraidBackgroundRomData.DecompressedTilemapBytes)
        {
            throw new InvalidDataException(
                $"Kraid BG2 sources must each decompress to " +
                $"${KraidBackgroundRomData.DecompressedTilemapBytes:X} bytes; " +
                $"upper=${upper.Length:X}, lower=${lower.Length:X}.");
        }

        Span<ushort> working = state.BackgroundTilemapWords;
        // The lower stream is decompressed directly over `$7E:2000-$2FFF`, which is also
        // Kraid's working tilemap. `$A7:AB19` then copies only its first $600 bytes upward
        // to `$2800` before the upper stream replaces `$2000-$27FF`. The untouched
        // `$2E00-$2FFF` tail therefore remains lower-stream data. Priority is cleared only
        // in the copied regions, not that untouched tail. Clearing the managed buffer
        // first left those eight rows as tile word zero; once growth raised BG2 and
        // enabled priority, character zero became a conspicuous repeated rectangle above
        // Kraid's head.
        for (int word = 0; word < KraidBackgroundRomData.WorkingTilemapWords; word++)
            working[word] = ReadLittleEndianWord(lower, word);
        for (int word = 0; word < KraidBackgroundRomData.LowerSourceCopyWords; word++)
        {
            working[KraidBackgroundRomData.WorkingLowerHalfWord + word] =
                WithoutKraidBg2Priority(working[word]);
        }
        for (int word = 0; word < KraidBackgroundRomData.VisiblePageWords; word++)
            working[word] = WithoutKraidBg2Priority(ReadLittleEndianWord(upper, word));
        working.Slice(KraidBackgroundRomData.BlankRowWorkingWord, 32)
            .Fill(KraidBackgroundRomData.BlankTile);

        state.BackgroundTilemapsPrepared = true;
        state.OwnsBg2Tilemap = true;
    }

    /// <summary>Uploads the upper 32x32 page exactly as <c>$A7:C874</c> requests.</summary>
    internal void TransferKraidTopTilemap(KraidEnemyState state)
    {
        new EnemyVram(_presentation._vram!).KraidTopPage.Write(
            state.BackgroundTilemapWords.AsSpan(0, KraidBackgroundRomData.VisiblePageWords));
        state.TopTilemapUploadCount++;
    }

    /// <summary>Uploads the lower 32x32 page exactly as <c>$A7:C8B6</c> requests.</summary>
    internal void TransferKraidBottomTilemap(KraidEnemyState state)
    {
        new EnemyVram(_presentation._vram!).KraidBottomPage.Write(
            state.BackgroundTilemapWords.AsSpan(
                KraidBackgroundRomData.WorkingLowerHalfWord,
                KraidBackgroundRomData.VisiblePageWords));
        state.BottomTilemapUploadCount++;
    }

    /// <summary>
    /// Installs the 704-byte head tilemap selected by Kraid's private eight-byte
    /// instruction record at <c>$A7:AF3D</c>.
    /// </summary>
    internal void TransferKraidHeadTilemap(KraidEnemyState state, ushort sourcePointer)
    {
        ReadOnlySpan<ushort> source;
        if (sourcePointer >= KraidBackgroundRomData.HeadRomWindowStart)
        {
            KraidBackgroundArtwork art = _room.Artwork?.KraidBackground
                ?? throw new InvalidDataException("Installed enemy artwork has no Kraid BG2 tilemaps.");
            source = art.HeadWords(sourcePointer);
        }
        else
        {
            // Supported native low-half pointers alias live WRAM, not immutable
            // head art. Preserve that exceptional cartridge behavior exactly.
            var alias = new ushort[KraidBackgroundRomData.HeadTilemapWords];
            var lowBank = new KraidLowBankAliasMemory(_room.Bus!);
            for (int word = 0; word < KraidBackgroundRomData.HeadTilemapWords; word++)
                alias[word] = lowBank.HeadTilemapWord(sourcePointer, word);
            source = alias;
        }
        // $A7:AF5D queues this source directly to VRAM. Mirroring it into the
        // body's WRAM map changes later growth/sink uploads and is not native behavior.
        new EnemyVram(_presentation._vram!).KraidTopPage.Write(source);
        state.HeadTilemapUploadCount++;
    }

    /// <summary>Uploads the fixed 4-bpp room backdrop characters at the native VRAM word.</summary>
    internal void UploadKraidRoomBackgroundTiles()
    {
        KraidBackgroundArtwork art = _room.Artwork?.KraidBackground
            ?? throw new InvalidDataException("Installed enemy artwork has no Kraid background characters.");
        art.RoomBackgroundTiles.LoadTo(new EnemyVram(_presentation._vram!).KraidRoomBackgroundTiles);
    }

    /// <summary>Ports <c>$A7:AD3A</c>'s priority-bit pass before second phase.</summary>
    private static void SetKraidBg2Priority(KraidEnemyState state)
    {
        Span<ushort> working = state.BackgroundTilemapWords;
        for (int word = 0; word < working.Length; word++)
            working[word] |= KraidBackgroundRomData.PriorityBit;
        state.Bg2PriorityBitsSet = true;
    }

    /// <summary>
    /// Restores the ordinary-room BG2 base used after the body has sunk. Native clears
    /// the two visible 32x32 pages on consecutive frames before reloading BG3 graphics.
    /// </summary>
    private void ClearKraidTopTilemapForDeath(KraidEnemyState state)
    {
        state.BackgroundTilemapWords.AsSpan(0, KraidBackgroundRomData.VisiblePageWords)
            .Fill(KraidBackgroundRomData.BlankTile);
        new GameplayVram(_presentation._vram!).Bg2Tilemap.Write(
            state.BackgroundTilemapWords.AsSpan(0, KraidBackgroundRomData.VisiblePageWords / 2));
        state.OwnsBg2Tilemap = false;
        state.TopTilemapUploadCount++;
    }

    /// <summary>Clears the second half of the restored ordinary 32x32 BG2 map.</summary>
    private void ClearKraidBottomTilemapForDeath(KraidEnemyState state)
    {
        new GameplayVram(_presentation._vram!).Bg2Tilemap.Write(
            state.BackgroundTilemapWords.AsSpan(0, KraidBackgroundRomData.VisiblePageWords / 2),
            KraidBackgroundRomData.VisiblePageWords / 2);
        state.BottomTilemapUploadCount++;
    }

    /// <summary>Clears the BG2 priority bit from a Kraid background tile word.</summary>
    private static ushort WithoutKraidBg2Priority(ushort word) =>
        unchecked((ushort)(word & ~KraidBackgroundRomData.PriorityBit));

    /// <summary>Reads one indexed little-endian word from an imported byte span.</summary>
    private static ushort ReadLittleEndianWord(ReadOnlySpan<byte> source, int word)
    {
        int offset = checked(word * 2);
        return unchecked((ushort)(SnesLittleEndian.Word(source[offset], source[offset + 1])));
    }
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
        MiscDustAnimation animationIndex = (projectileType & 0x0200) != 0
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
                return new(shot.Position.Whole, shot.Radius, shot.Type, shot.Damage);
            }
            SamusBombProjectileSlot bomb = _sharedProjectiles.Slots[0];
            return new(bomb.Position.Whole, bomb.Radius, bomb.Type, bomb.Damage);
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
                bomb.Direction = new SamusProjectileDirectionWord(bomb.Direction).WithCollisionLifecycleState();
            }
        }
    }

    /// <summary>
    /// Both passes return immediately once Kraid has begun sinking ($A7:AFAD/$B182).
    /// </summary>
    private static bool KraidSinkingSkipsProjectileCollision(RoomEnemySlot body) =>
        unchecked((short)(body.VariableA - (ushort)KraidAiFunction.DeathSink)) >= 0;

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
        ushort innerMouth = lastSlot == 0 ? ushort.MaxValue :
            KraidHeadInstructionDefinitions.ReadCollisionHitbox(_room.Bus!, body.VariableB, innerMouth: true);
        if (lastSlot != 0 && innerMouth != ushort.MaxValue)
        {
            KraidCollisionScratch scratch = LoadKraidCollisionScratch(body, innerMouth);
            for (int index = lastSlot; index >= 0; index--)
            {
                KraidCollisionShot shot = shots.Read(index);
                SamusProjectileTypeWord type = new(shot.Type);
                if (!scratch.OverlapsMouth(shot) ||
                    (type.Family == SamusProjectileFamily.Beam && !type.IsChargedBeam))
                    continue;

                if (type.Family == SamusProjectileFamily.Beam)
                    state.MouthFlags |= 1;
                // The common shot callback reuses DP $12/$14 for type/vulnerability.
                // In particular, $8200 left by a Super Missile changes the next shot's
                // signed CMP; rebuilding a clean rectangle here doubles close-range damage.
                scratch.Bottom = shot.Type;
                scratch.Top = body.Definition.VulnerabilityPointer != 0
                    ? body.Definition.VulnerabilityPointer : EnemyVulnerabilityDefinitions.DefaultPointer;
                int multiplier = type.Family switch
                {
                    SamusProjectileFamily.Beam => EnemyVulnerabilityDefinitions.Read(
                        scratch.Top, EnemyVulnerabilityDefinitions.ChargedBeamOffset) & 0x0f,
                    SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile or
                    SamusProjectileFamily.Bomb or SamusProjectileFamily.PowerBomb =>
                        EnemyCollision.ReadProjectileVulnerability(body, type) & 0x7f,
                    SamusProjectileFamily.BeamExplosion or SamusProjectileFamily.MissileExplosion => 0,
                    _ => throw new InvalidOperationException($"Undefined SamusProjectileFamily {type.Family}."),
                };
                int damage = unchecked((ushort)((shot.Damage >> 1) * multiplier));
                if (damage != 0)
                {
                    body.Health = damage >= body.Health
                        ? (ushort)0
                        : unchecked((ushort)(body.Health - damage));
                    body.FlashTimer = NativeCountdown.FromRaw(unchecked((ushort)((body.HurtAiTime == 0 ? 4 : body.HurtAiTime) + 8)));
                    body.AiHandlerBits = unchecked((ushort)(body.AiHandlerBits | 2));
                    if ((shot.Type & 8) != 0)
                        body.InvincibilityTimer = NativeCountdown.Start(16);
                }
                else
                {
                    RoomSpriteObjectSlot? dud = _spriteObjects.SpawnRoomSpriteObject(
                        shot.Position, RoomSpriteObjectKind.EnemyProjectileDud, graphicsIndex: 0);
                    // Create_Sprite_Object returns its native index in $12 only on success.
                    scratch.Bottom = dud?.NativeIndex ?? shot.Position.X;
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
            if ((state.MouthFlags & 2) != 0)
                state.MouthFlags |= 4;
            if (unchecked((short)(body.Health - 1)) < 0 &&
                unchecked((short)(body.VariableA - (ushort)KraidAiFunction.DeathInitialize)) < 0)
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

        state.MouthFlags &= 0xfffe;
        int lastSlot = shots.LastSlot;
        int bodyHits = 0;
        if (lastSlot != 0)
        {
            ushort outerMouth = KraidHeadInstructionDefinitions.ReadCollisionHitbox(_room.Bus!, body.VariableB, innerMouth: false);
            KraidCollisionScratch scratch = LoadKraidCollisionScratch(body, outerMouth);
            for (int index = lastSlot; index >= 0; index--)
            {
                KraidCollisionShot shot = shots.Read(index);
                if (!scratch.OverlapsBody(body, shot))
                    continue;
                SpawnKraidArmShotExplosion(shot.Type, shot.Position);
                scratch.Bottom = shot.Position.X;
                scratch.Top = shot.Position.Y;
                shots.MarkCollision(index);
                if ((shot.Type & 0x0010) != 0)
                    state.MouthFlags |= 1;
                bodyHits++;
            }
        }

        if (bodyHits != 0 && body.VariableA == (ushort)KraidAiFunction.MainloopThinking)
        {
            body.VariableA = (ushort)KraidAiFunction.InitializeEyeGlow;
            if ((state.MouthFlags & 1) != 0)
                state.MouthFlags |= 0x0302;
        }
        return bodyHits;
    }

    /// <summary>Converts a mouth hitbox pointer into wrapped world-space collision bounds.</summary>
    internal KraidCollisionScratch LoadKraidCollisionScratch(RoomEnemySlot body, ushort pointer)
    {
        (short left, short top, short bottom) = KraidMouthHitboxes.ResolveCollision(_room.Bus!, pointer);
        return new()
        {
            Left = unchecked((ushort)(body.Position.X.Whole + left)),
            Top = unchecked((ushort)(body.Position.Y.Whole + top)),
            Bottom = unchecked((ushort)(body.Position.Y.Whole + bottom)),
        };
    }

    /// <summary>Projectile geometry and attack data read by Kraid's mouth and body collision passes.</summary>
    internal readonly record struct KraidCollisionShot(Position16<RoomPixels> Position, Extent16<RoomPixels> Radius, ushort Type, ushort Damage);

    /// <summary>Native DP $12/$14/$16 scratch bounds for wrapped mouth and body collision tests.</summary>
    internal struct KraidCollisionScratch
    {
        /// <summary>Lower vertical mouth bound in wrapped room coordinates.</summary>
        public ushort Bottom;
        /// <summary>Upper vertical mouth bound in wrapped room coordinates.</summary>
        public ushort Top;
        /// <summary>Left horizontal mouth bound in wrapped room coordinates.</summary>
        public ushort Left;

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
        body.VariableA = (ushort)KraidAiFunction.DeathInitialize;
        state.MouthFlags = 0;
        for (int slot = 0; slot < 6; slot++)
        {
            // Native property `$0400` disables the ordinary touch list while the private
            // death state owns body movement and breakup effects.
            // Nails are separate projectile-like actors and delete themselves from HP zero.
            // Keeping this six-slot bound preserves that exact division.
            _slotTable._slots[slot].Properties = _slotTable._slots[slot].Properties.With(
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
        switch ((KraidAiFunction)body.VariableA)
        {
            case KraidAiFunction.MainloopThinking:
                if (!state.ThinkingTimer.IsZero && (state.ThinkingTimer = state.ThinkingTimer.Decrement().Remaining).IsZero)
                {
                    body.VariableA = (ushort)KraidAiFunction.MainAttackWithMouthOpen;
                    body.VariableB = KraidHeadInstructionDefinitions.RoarContinuation;
                    body.VariableC = KraidHeadInstructionDefinitions.RoarEntryTimer;
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
                TickKraidFunctionTimer(body, state.Parts[0]);
                return;

            case KraidAiFunction.ProcessHeadInstructionAndTimer:
                _ = ProcessKraidHeadInstruction(body, state);
                TickKraidFunctionTimer(body, state.Parts[0]);
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
                    $"Kraid body function $A7:{body.VariableA:X4} is not translated.");
        }
    }

    /// <summary>Restores the small-body thinking state after an attack.</summary>
    private void SetupKraidFirstPhaseThinking(RoomEnemySlot body, KraidEnemyState state)
    {
        body.VariableA = (ushort)KraidAiFunction.MainloopThinking;
        _slotTable._slots[2].Position = _slotTable._slots[2].Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 20)));
        _slotTable._slots[3].Position = _slotTable._slots[3].Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 46)));
        _slotTable._slots[4].Position = _slotTable._slots[4].Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 112)));
        state.ThinkingTimer = NativeCountdown.FromRaw(ReadKraidThinkingTimer());
    }

    /// <summary>Restores the grown-body thinking state after an attack.</summary>
    private void SetupKraidSecondPhaseThinking(RoomEnemySlot body, KraidEnemyState state)
    {
        body.VariableA = (ushort)KraidAiFunction.SecondPhaseThinking;
        _slotTable._slots[2].Position = _slotTable._slots[2].Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 20)));
        _slotTable._slots[3].Position = _slotTable._slots[3].Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 46)));
        _slotTable._slots[4].Position = _slotTable._slots[4].Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 112)));
        state.ThinkingTimer = NativeCountdown.FromRaw(ReadKraidThinkingTimer());
    }

    /// <summary>Counts down the grown body's think delay before opening the mouth for its next attack.</summary>
    internal static void RunKraidSecondPhaseThinking(RoomEnemySlot body, KraidEnemyState state)
    {
        if (!state.ThinkingTimer.IsZero && (state.ThinkingTimer = state.ThinkingTimer.Decrement().Remaining).IsZero)
        {
            body.VariableA = (ushort)KraidAiFunction.MouthOpenReaction;
            body.VariableB = KraidHeadInstructionDefinitions.RoarContinuation;
            body.VariableC = KraidHeadInstructionDefinitions.RoarEntryTimer;
        }
    }

    /// <summary>Advances the open-mouth attack and its head instruction timing.</summary>
    private void RunKraidMouthOpenAttack(RoomEnemySlot body, KraidEnemyState state)
    {
        ushort result = ProcessKraidHeadInstruction(body, state);
        if (result == ushort.MaxValue)
        {
            SetupKraidFirstPhaseThinking(body, state);
            body.VariableC = 90;

            // Bits 2 and 8-15 are populated by body/mouth projectile handling. Preserve
            // the cartridge arithmetic now so adding collision cannot change sequencing.
            if ((state.MouthFlags & 4) != 0)
            {
                state.MouthFlags = unchecked((ushort)(state.MouthFlags - 0x0100));
                if ((SnesLittleEndian.HighByteInPlace(state.MouthFlags)) != 0)
                {
                    body.VariableA = (ushort)KraidAiFunction.HandleFunctionTimer;
                    body.VariableF = 64;
                    // `$B6BF` (eye glow initialization) is the next collision-driven
                    // function; it remains deliberately unnamed until that slice lands.
                    state.Parts[0].NextFunction = (KraidAiFunction)0xb6bf;
                    state.Unknown2 = 2;
                    return;
                }
            }
            state.MouthFlags = 0;
            return;
        }

        // The open-mouth tilemap remains installed for 64 frames. Retail emits a rock on
        // every sixteenth timer value, including the entry frame after `$AF3D` reloads it.
        if (state.CurrentHeadTilemap == KraidHeadInstructionDefinitions.OpenMouthTilemap &&
            (body.VariableC & 0x000f) == 0)
        {
            state.SpitRockRequestCount++;
            if (SpawnKraidSpitRock(body))
                state.SpawnedSpitRockCount++;
            LastKraidSoundEffect = new KraidSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x001e));
        }
    }

    /// <summary>Reads and advances the current compiled head instruction word.</summary>
    internal ushort ProcessKraidHeadInstruction(RoomEnemySlot body, KraidEnemyState state)
    {
        ushort result = body.VariableC;
        if (result == 0)
            return 0;
        body.VariableC = unchecked((ushort)(body.VariableC - 1));
        return result == 1 ? ExecuteKraidHeadInstruction(body, state) : result;
    }

    /// <summary>Applies a Kraid head command and returns its next list position.</summary>
    internal ushort ExecuteKraidHeadInstruction(RoomEnemySlot body, KraidEnemyState state)
    {
        for (int commandCount = 0; commandCount < 16; commandCount++)
        {
            ushort cursor = body.VariableB;
            KraidHeadInstructionDefinition instruction =
                KraidHeadInstructionDefinitions.Resolve(cursor);
            switch (instruction.Kind)
            {
                case KraidHeadInstructionKind.Terminate:
                    return ushort.MaxValue;

                case KraidHeadInstructionKind.RoarSound:
                case KraidHeadInstructionKind.DyingSound:
                    LastKraidSoundEffect = new KraidSoundRequest(
                        SoundEffectId.FromCartridge(
                            SoundEffectLibrary.Library2,
                            instruction.SoundId));
                    if (instruction.Kind == KraidHeadInstructionKind.RoarSound)
                        state.RoarRequestCount++;
                    body.VariableB = unchecked((ushort)(cursor + 2));
                    continue;

                case KraidHeadInstructionKind.Frame:
                    body.VariableC = instruction.Duration;
                    state.CurrentHeadTilemap = instruction.Tilemap;
                    state.VulnerableMouthHitbox = instruction.VulnerableHitbox;
                    state.InvulnerableMouthHitbox = instruction.InvulnerableHitbox;
                    body.VariableB = unchecked((ushort)(cursor + 8));
                    TransferKraidHeadTilemap(state, state.CurrentHeadTilemap);
                    return 1;

                default:
                    throw new InvalidDataException(
                        $"Kraid head instruction kind {instruction.Kind} is not translated.");
            }
        }
        throw new InvalidDataException("Kraid head instruction stream exceeded its command guard.");
    }

    /// <summary>Updates the mouth-open reaction after a damaging shot.</summary>
    private void RunKraidMouthOpenReaction(RoomEnemySlot body, KraidEnemyState state)
    {
        if (ProcessKraidHeadInstruction(body, state) != ushort.MaxValue)
            return;
        body.VariableA = (ushort)KraidAiFunction.MainloopThinking;
        body.VariableC = 90;
        if ((state.MouthFlags & 4) != 0)
        {
            state.MouthFlags = unchecked((ushort)(state.MouthFlags - 0x0100));
            if ((SnesLittleEndian.HighByteInPlace(state.MouthFlags)) != 0)
            {
                body.VariableA = (ushort)KraidAiFunction.HandleFunctionTimer;
                body.VariableF = 64;
                state.Parts[0].NextFunction = KraidAiFunction.InitializeEyeGlow;
                state.Unknown2 = 2;
                return;
            }
        }
        state.MouthFlags = 0;
    }

    /// <summary>Seeds eye-glow palette state after a body hit.</summary>
    internal void InitializeKraidEyeGlow(RoomEnemySlot body, KraidEnemyState state)
    {
        body.VariableA = (ushort)KraidAiFunction.GlowEye;
        body.VariableB = KraidHeadInstructionDefinitions.EyeGlowContinuation;
        body.VariableC = KraidHeadInstructionDefinitions.EyeGlowEntryTimer;
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
            body.VariableA = (ushort)KraidAiFunction.UnglowEye;
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
            body.VariableA = (ushort)KraidAiFunction.MouthOpenReaction;
            body.VariableB = KraidHeadInstructionDefinitions.RoarContinuation;
            body.VariableC = KraidHeadInstructionDefinitions.RoarEntryTimer;
        }
    }
    // These collision checks belong to actor AI rather than generic interactive hitboxes.
    // The body's permanent intangible flag and lint's no-op touch callback do not
    // suppress these private native paths.
    /// <summary>Checks the body hitbox and applies contact damage to Samus.</summary>
    private void ResolveKraidBodyContact(RoomEnemySlot body, KraidEnemyState state, SamusState samus)
    {
        if (unchecked((short)(body.VariableA - (ushort)KraidAiFunction.DeathInitialize)) >= 0)
            return;
        short relativeY = unchecked((short)(samus.Position.Y.Whole - body.Position.Y.Whole));
        if (unchecked((short)(body.Position.X.Whole + KraidBodyContour.LeftEdge(relativeY) -
            samus.Position.X.Whole - samus.Kinematics.Radius.X)) >= 0)
            return;
        if (unchecked((short)(samus.Position.X.Whole - KraidContactDefinitions.MinimumBodyEjectionX)) >= 0)
            samus.Position -= new Offset16<RoomPixels>(unchecked((short)KraidContactDefinitions.BodyEjection), 0);
        ushort nextY = unchecked((ushort)(samus.Position.Y.Whole - KraidContactDefinitions.BodyEjection));
        samus.Position = samus.Position.WithWholeY(unchecked((short)(nextY - state.MinimumYPositionForEjection)) < 0
            ? state.MinimumYPositionForEjection : nextY);
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
        switch ((KraidAiFunction)body.VariableA)
        {
            case KraidAiFunction.DeathInitialize:
                InitializeKraidDeath(body, state);
                return;

            case KraidAiFunction.DeathFadeOut:
                _ = ProcessKraidHeadInstruction(body, state);
                if (AdvanceKraidRoomBackgroundFade(fadeToBlack: true))
                {
                    body.VariableA = (ushort)KraidAiFunction.DeathUpdateTopTilemap;
                    state.HurtFrameTimer = NativeCountdown.Start(1);
                    state.HurtFrame = 1;
                    TransferKraidTopTilemap(state);
                }
                return;

            case KraidAiFunction.DeathUpdateTopTilemap:
                _ = ProcessKraidHeadInstruction(body, state);
                body.VariableA = (ushort)KraidAiFunction.DeathUpdateBottomTilemap;
                for (int lintSlot = 2; lintSlot <= 4; lintSlot++)
                {
                    _slotTable._slots[lintSlot].VariableA = (ushort)KraidAiFunction.AlignPartToKraid;
                    _slotTable._slots[lintSlot].VariableF = 0x7fff;
                }
                TransferKraidTopTilemap(state);
                return;

            case KraidAiFunction.DeathUpdateBottomTilemap:
                _ = ProcessKraidHeadInstruction(body, state);
                body.VariableA = (ushort)KraidAiFunction.DeathSink;
                state.DeathSoundTimer = NativeCountdown.Start(43);
                body.Properties = body.Properties.With(EnemyProperties.SolidToSamus);
                _earthquake.EarthquakeType = 1;
                _earthquake.EarthquakeTimer = NativeCountdown.Start(256);
                _slotTable._slots[1].CurrentInstruction = KraidInitialArmInstruction;
                _slotTable._slots[1].InstructionTimer = NativeCountdown.Start(1);
                _slotTable._slots[5].CurrentInstruction = KraidInitialFootInstruction;
                _slotTable._slots[5].InstructionTimer = NativeCountdown.Start(1);
                _slotTable._slots[5].VariableA = (ushort)KraidAiFunction.NoOperation;
                TransferKraidBottomTilemap(state);
                return;

            case KraidAiFunction.DeathSink:
                RunKraidDeathSink(body, state);
                return;

            case KraidAiFunction.DeathClearTopTilemap:
                body.VariableA = (ushort)KraidAiFunction.DeathClearBottomTilemap;
                ClearKraidTopTilemapForDeath(state);
                return;
            case KraidAiFunction.DeathClearBottomTilemap:
                body.VariableA = (ushort)KraidAiFunction.DeathLoadBg3Quarter1;
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
                    body.VariableA = (ushort)KraidAiFunction.DeathFinishedWasDead;
                }
                else
                {
                    _room.Services.Bosses.SetDefeated(BossBits.AreaBoss);
                    state.BossDefeatPersisted = true;
                    body.VariableA = (ushort)KraidAiFunction.DeathFinishedWasAlive;
                }
                state.DeathSequenceComplete = true;
                return;
            case KraidAiFunction.DeathFinishedWasAlive:
                MarkKraidDeadUnlessDeadAtLeftEdge(body, cameraX);
                return;
            case KraidAiFunction.DeathFinishedWasDead:
                MarkKraidDeadUnlessDeadAtLeftEdge(body, cameraX);
                // $A7:C85E forces BG1 column streaming to treat every column as new.
                state.Layer1XBlockResetRequested = true;
                return;
            default:
                throw new InvalidOperationException($"Kraid body function $A7:{body.VariableA:X4} is not a death function.");
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
        _slotTable._slots[1].CurrentInstruction = KraidArmRetractedInstruction;
        _slotTable._slots[1].InstructionTimer = NativeCountdown.Start(1);
        body.VariableA = (ushort)KraidAiFunction.DeathFadeOut;
        body.VariableB = KraidHeadInstructionDefinitions.DeathContinuation;
        body.VariableC = KraidHeadInstructionDefinitions.DeathEntryTimer;
        // $A7:C3A8-$C3E8 clear the fingernails' respawn bit, then run EnemyDeath for the
        // nails, then the three lints. A still holds each slot's native index, which
        // EnemyDeath treats as explosion type zero; every part leaves a death explosion.
        foreach (int slot in new[] { 6, 7 })
            _slotTable._slots[slot].Properties = _slotTable._slots[slot].Properties.Without(EnemyProperties.RespawnIfKilled);
        foreach (int slot in new[] { 6, 7, 2, 3, 4 })
            _deathsAndDrops.StartGenericEnemyDeath(_slotTable._slots[slot], deathAnimation: _slotTable._slots[slot].NativeIndex);
        // The live sequence crumbles successive blocks while Kraid fades/sinks.
        // It must not substitute the instantaneous already-defeated-room clear.
        _kraidPlmRequests.Add(KraidPlmDefinitions.LiveDeathSpikes);
    }

    /// <summary>Moves the body downward through the authored sink sequence.</summary>
    private void RunKraidDeathSink(RoomEnemySlot body, KraidEnemyState state)
    {
        _ = ProcessKraidHeadInstruction(body, state);
        state.DeathSoundTimer = state.DeathSoundTimer.Decrement().Remaining;
        if (state.DeathSoundTimer.IsZero)
        {
            LastKraidSoundEffect = new KraidSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x001e));
            state.DeathSoundTimer = NativeCountdown.Start(30);
        }
        ProcessKraidSinkTable(body, state);
        body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 1)));
        if (unchecked((short)(body.Position.Y.Whole - 608)) < 0)
            return;

        body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        ushort armProperties = _slotTable._slots[1].Properties.With(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Deleted);
        _slotTable._slots[1].Properties = armProperties;
        ushort otherProperties = armProperties.Replace(
            EnemyProperties.SolidToSamus |
                EnemyProperties.ProcessInstructions |
                EnemyProperties.ProcessOffScreen,
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Deleted);
        for (int slot = 2; slot <= 5; slot++)
            _slotTable._slots[slot].Properties = otherProperties;
        body.VariableA = (ushort)KraidAiFunction.DeathClearTopTilemap;
        // $A7:C594 returns the camera to distance index zero before the death drop.
        _zebetiteBehavior.CameraDistanceIndex = CameraDistanceMode.NormalTracking;

        // $A0:B8EE fills the same sixteen-projectile pool used by ordinary deaths. Each
        // pickup gets a random position in Kraid's 256x64 floor strip and independently
        // rolls header $E2BF's six-byte chance table.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.Kraid,
            count: 16,
            xBase: 128,
            xMask: 0x00ff,
            yBase: 352,
            yMask: 0x3f00);
        state.DeathDropRequestCount = 16;
        // $A7:C59B continues into DrawKraidsRoomBackground, which zeroes the shared numerator.
        _presentation.GradualColorChange.Numerator = 0;
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
                _kraidPlmRequests.Add(request);
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
        HudTileAtlas hud = _presentation.HudTileArtwork ?? throw new InvalidDataException(
            "Installed Kraid death restoration has no standard BG3 artwork.");
        if (vramWriteQueue is not null)
            vramWriteQueue.EnqueueAsset(
                KraidBackgroundRomData.StandardBg3AssetForQuarter(transferIndex),
                KraidBackgroundRomData.StandardBg3TransferBytes, EnemyVram.KraidBg3RestoreDma(destinationWord));
        else
            new EnemyVram(_presentation._vram!).KraidBg3Restore.Load(hud.KraidRestoreQuarter(transferIndex).Span,
                destinationWord * sizeof(ushort));

        state.DeathBg3TransferCount++;
        body.VariableA = (ushort)next;
    }
    /// <summary>Starts growth only after the small-form transition requirements are met.</summary>
    internal bool TryBeginKraidGrowth(RoomEnemySlot body, KraidEnemyState state)
    {
        if (unchecked((short)(body.Health - state.HealthEighthThreshold(6))) >= 0)
            return false;

        body.VariableA = (ushort)KraidAiFunction.ProcessHeadInstructionAndTimer;
        body.VariableF = 180;
        state.Parts[0].NextFunction = KraidAiFunction.GrowReleaseCamera;

        ushort selectionWord = KraidHeadInstructionDefinitions.ReadGrowthSelectionWord(
            _room.Bus!, body.VariableB);
        KraidHeadResumeDefinition resume =
            KraidHeadInstructionDefinitions.GrowthResume(selectionWord);
        body.VariableB = resume.Pointer;
        body.VariableC = resume.Timer;
        _earthquake.EarthquakeType = 4;
        _earthquake.EarthquakeTimer = NativeCountdown.Start(340);

        RoomEnemySlot foot = _slotTable._slots[5];
        foot.CurrentInstruction = KraidInitialFootInstruction;
        foot.InstructionTimer = NativeCountdown.Start(1);
        foot.VariableA = (ushort)KraidAiFunction.NoOperation;
        RoomEnemySlot arm = _slotTable._slots[1];
        arm.CurrentInstruction = KraidArmNormalInstruction;
        arm.InstructionTimer = NativeCountdown.Start(1);
        for (int lintSlot = 2; lintSlot <= 4; lintSlot++)
            _slotTable._slots[lintSlot].Properties = _slotTable._slots[lintSlot].Properties.With(EnemyProperties.Invisible);
        arm.Properties = arm.Properties.With(EnemyProperties.IgnoreSamusCollision);
        return true;
    }

    /// <summary>Dispatches rise, scroll, tilemap, and palette stages of growth.</summary>
    internal void RunKraidGrowthFunction(RoomEnemySlot body, KraidEnemyState state)
    {
        switch ((KraidAiFunction)body.VariableA)
        {
            case KraidAiFunction.GrowReleaseCamera:
                ApplyKraidScrolls(grown: true);
                body.VariableA = (ushort)KraidAiFunction.GrowBreakCeilingPlatforms;
                state.CameraReleasedForSecondPhase = true;
                state.MinimumYPositionForEjection = 164;
                return;

            case KraidAiFunction.GrowBreakCeilingPlatforms:
                if ((body.FrameCounter & 7) == 0)
                    RequestKraidRisingRock(body, state);
                body.Position = body.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole +
                    ((body.Position.Y.Whole & 2) == 0 ? 1 : -1))));
                body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 1)));
                if ((body.Position.Y.Whole & 3) == 0 && body.VariableF < 18)
                {
                    int ceilingIndex = body.VariableF / 2;
                    ushort rockX = KraidCeilingRockPositions.AtByteOffset(body.VariableF);
                    if (SpawnKraidCeilingRock(rockX))
                        state.CeilingRockSpawnCount++;
                    _kraidPlmRequests.Add(KraidPlmDefinitions.GrowthCeiling[ceilingIndex]);
                    body.VariableF = unchecked((ushort)(body.VariableF + 2));
                }
                if (unchecked((short)(body.Position.Y.Whole - 296)) < 0)
                    body.VariableA = (ushort)KraidAiFunction.GrowSetBg2Priority;
                return;

            case KraidAiFunction.GrowSetBg2Priority:
                SetKraidBg2Priority(state);
                _slotTable._slots[1].Properties = _slotTable._slots[1].Properties.Without(
                    EnemyProperties.IgnoreSamusCollision);
                body.VariableA = (ushort)KraidAiFunction.GrowFinishBg2Update;
                TransferKraidTopTilemap(state);
                return;

            case KraidAiFunction.GrowFinishBg2Update:
                body.VariableA = (ushort)KraidAiFunction.GrowDrawRoomBackground;
                RoomEnemySlot foot = _slotTable._slots[5];
                foot.CurrentInstruction = KraidFootInstructionProgramDefinitions.Neutral;
                foot.InstructionTimer = NativeCountdown.Start(1);
                for (int lintSlot = 2; lintSlot <= 4; lintSlot++)
                {
                    _slotTable._slots[lintSlot].CurrentInstruction =
                        KraidLintInstructionLists.Ilist_8B04;
                    _slotTable._slots[lintSlot].SpritemapPointer = 0x8c6c;
                }
                TransferKraidBottomTilemap(state);
                return;

            case KraidAiFunction.GrowDrawRoomBackground:
                body.VariableA = (ushort)KraidAiFunction.GrowFadeInRoomBackground;
                body.VariableE = 0;
                body.VariableF = 0;
                // $A7:ADB0 DrawKraidsRoomBackground zeroes the shared numerator.
                _presentation.GradualColorChange.Numerator = 0;
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
                throw new InvalidOperationException($"Kraid body function $A7:{body.VariableA:X4} is not a growth function.");
        }
    }

    /// <summary>Steps room background colors toward their grown-form target.</summary>
    internal bool AdvanceKraidRoomBackgroundFade(
        bool fadeToBlack)
    {
        ushort step = _presentation.GradualColorChange.Numerator;
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
            _presentation.GradualColorChange.Numerator = unchecked((ushort)(step + 1));
            return false;
        }
        _presentation.GradualColorChange.Numerator = 0;
        return true;
    }

    /// <summary>Interpolates one RGB5 color toward its growth-phase target.</summary>
    private static Bgr555 TransitionKraidColor(ushort step, Bgr555 current, Bgr555 target) =>
        current.Zip(target, (_, from, to) => TransitionKraidComponent(step, from, to));

    /// <summary>Moves one color channel toward its target under the native step rule.</summary>
    private static int TransitionKraidComponent(ushort step, int current, int target)
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
            RoomEnemySlot lint = _slotTable._slots[slot];
            lint.VariableF = KraidLintInitializationDefinitions.InitialDelay(part);
            lint.VariableA = (ushort)KraidAiFunction.AlignPartToKraid;
            state.Parts[slot].NextFunction = KraidAiFunction.LintProduce;
            lint.VariableB = 0;
        }
        for (int nailIndex = 0; nailIndex < 2; nailIndex++)
        {
            RoomEnemySlot nail = _slotTable._slots[6 + nailIndex];
            state.Parts[6 + nailIndex].NextFunction = KraidAiFunction.FingernailInitialize;
            nail.VariableA = (ushort)KraidAiFunction.HandleFunctionTimer;
            nail.VariableF = unchecked((ushort)(64 + nailIndex * 64));
        }
        _slotTable._slots[1].VariableC = 1;
        body.VariableB = KraidHeadInstructionDefinitions.RoarContinuation;
        state.TargetX = 288;
        RoomEnemySlot foot = _slotTable._slots[5];
        foot.VariableA = (ushort)KraidAiFunction.FootSecondPhaseWalkToStart;
        foot.CurrentInstruction = KraidFootWalkBackInstruction;
        foot.InstructionTimer = NativeCountdown.Start(1);
    }
    /// <summary>Executes Kraid's arm's private animation instruction.</summary>
    private bool TryProcessKraidArmInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.KraidArm ||
            !Enum.IsDefined((KraidArmInstruction)word))
            return false;

        switch ((KraidArmInstruction)word)
        {
            case KraidArmInstruction.SlowArmIfLessThanHalfHealth:
                cursor = SelectKraidArmSpeedInstruction(cursor);
                return true;
            default:
                throw new InvalidOperationException(
                    $"Kraid arm does not own instruction ${word:X4}.");
        }
    }

    /// <summary>Executes Kraid's foot's private animation instructions.</summary>
    private bool TryProcessKraidFootInstruction(RoomEnemySlot slot, RoomLevelData? level, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.KraidFoot ||
            !Enum.IsDefined((KraidFootInstruction)word))
            return false;

        ProcessKraidFootInstruction((KraidFootInstruction)word, level);
        cursor = unchecked((ushort)(cursor + 2));
        return true;
    }

    /// <summary>Selects the arm motion list reached by the current instruction cursor.</summary>
    private ushort SelectKraidArmSpeedInstruction(ushort cursor)
    {
        RoomEnemySlot body = _slotTable._slots[0];
        RoomEnemySlot arm = _slotTable._slots[1];
        ushort halfHealth = RequireKraidState(body).HealthEighthThreshold(3);
        if (unchecked((short)(body.Health - halfHealth)) < 0 &&
            unchecked((short)(arm.CurrentInstruction -
                KraidArmInstructionProgramDefinitions.Slow)) < 0)
        {
            return KraidArmInstructionProgramDefinitions.Slow;
        }
        return unchecked((ushort)(cursor + 2));
    }

    /// <summary>Applies a compiled foot command, including movement and terrain-dependent effects.</summary>
    private void ProcessKraidFootInstruction(KraidFootInstruction instruction, RoomLevelData? level)
    {
        RoomEnemySlot body = _slotTable._slots[0];
        switch (instruction)
        {
            case KraidFootInstruction.Instruction_Kraid_NOP_A7B633:
                return;
            case KraidFootInstruction.Instruction_Kraid_DecrementYPosition:
                body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 1)));
                return;
            case KraidFootInstruction.Instruction_Kraid_IncrementYPosition_SetScreenShaking:
                body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 1)));
                _earthquake.EarthquakeType = 1;
                _earthquake.EarthquakeTimer = NativeCountdown.Start(10);
                return;
            case KraidFootInstruction.Instruction_Kraid_QueueSFX76_Lib2_Max6:
                LastKraidSoundEffect = new KraidSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0076));
                return;
            case KraidFootInstruction.Instruction_Kraid_XPositionMinus3:
            case KraidFootInstruction.Instruction_Kraid_XPositionMinus3_duplicate:
                body.Position = body.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole - 3)));
                return;
            case KraidFootInstruction.Instruction_Kraid_XPositionPlus3:
                body.Position = body.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole + 3)));
                return;
            case KraidFootInstruction.UNUSED_Instruction_Kraid_MoveRight_A7B683:
                if (level is null)
                    throw new InvalidOperationException(
                        "Kraid's move-right foot instruction requires room collision data.");
                KraidEnemyState state = RequireKraidState(body);
                bool shouldMove = unchecked((short)(body.Position.X.Whole - 320)) < 0;
                if (!shouldMove)
                {
                    state.TargetX = NativeWordCounter.Decrement(state.TargetX).Value;
                    shouldMove = state.TargetX == 0;
                }
                if (shouldMove && _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                        level, body, 4 << 16))
                {
                    _earthquake.EarthquakeType = 0;
                    _earthquake.EarthquakeTimer = NativeCountdown.Start(7);
                    _slotTable._slots[5].Position = _slotTable._slots[5].Position.WithWholeX(body.Position.X.Whole);
                }
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(KraidFootInstruction)} {(int)instruction:X4}.");
        }
    }
    /// <summary>Dispatches a nail's active flight or rebound function.</summary>
    private void RunKraidNailMain(RoomEnemySlot nail, RoomLevelData? level)
    {
        KraidEnemyState state = RequireKraidState(nail);
        if (_slotTable._slots[0].Health == 0)
        {
            nail.Properties = nail.Properties.With(
                EnemyProperties.Deleted | EnemyProperties.Invisible);
            return;
        }

        KraidPartState part = state.Parts[nail.SlotIndex];
        switch ((KraidAiFunction)nail.VariableA)
        {
            case KraidAiFunction.HandleFunctionTimer:
                TickKraidFunctionTimer(nail, part);
                return;

            case KraidAiFunction.FingernailInitialize:
                InitializeKraidNailFlight(nail, part);
                return;

            case KraidAiFunction.FingernailWaitForLint:
                if (unchecked((short)(_slotTable._slots[2].Position.X.Whole - 256)) >= 0)
                {
                    nail.VariableA = (ushort)part.NextFunction;
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
                    $"Kraid fingernail function $A7:{nail.VariableA:X4} is not translated.");
        }
    }

    /// <summary>Seeds a released nail's velocity, instruction list, and collision state.</summary>
    internal void InitializeKraidNailFlight(RoomEnemySlot nail, KraidPartState part)
    {
        // The pair coordinates its next launch through the other actor's previous
        // velocity and spawn flag. Read that actor before overwriting this one's state.
        int siblingIndex = nail.SlotIndex == 6 ? 7 : 6;
        RoomEnemySlot sibling = _slotTable._slots[siblingIndex];
        KraidPartState siblingPart = RequireKraidState(nail).Parts[siblingIndex];
        ushort random = _room.Random.Read();
        (nail.VariableB, nail.VariableC, nail.VariableD, nail.VariableE) =
            KraidNailLaunchDefinitions.FromSiblingVelocity(sibling.VariableE);
        nail.Parameter1 = 1;
        nail.Properties = nail.Properties.Without(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
        nail.InstructionTimer = NativeCountdown.Start(1);
        nail.CurrentInstruction = KraidNailInstructionProgramDefinitions.Loop;
        nail.VariableA = (ushort)KraidAiFunction.FingernailFire;

        if ((random & 1) == 0 || siblingPart.AlternateSpawnFlag == 1)
        {
            part.AlternateSpawnFlag = 0;
            RoomEnemySlot body = _slotTable._slots[0];
            nail.Position = nail.Position.WithWhole(new(unchecked((ushort)(
                (body.Position.X.Whole - body.Radius.X - nail.Radius.X) & 0xfff0)), unchecked((ushort)(_slotTable._slots[1].Position.Y.Whole + 128))));
            return;
        }

        part.AlternateSpawnFlag = 1;
        nail.Position = nail.Position.WithWhole(new(50, 240));
        nail.VariableB = 0;
        nail.VariableC = 1;
        nail.VariableD = 0;
        nail.VariableE = 0;
        nail.VariableA = (ushort)KraidAiFunction.FingernailWaitForLint;
        part.NextFunction = KraidAiFunction.FingernailFire;
        nail.Properties = nail.Properties.With(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
    }

    /// <summary>Advances nail position and terrain response for one enemy update.</summary>
    internal void TickKraidNailFlight(RoomEnemySlot nail, RoomLevelData level)
    {
        int horizontal = CombineKraidVelocity(nail.VariableB, nail.VariableC);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, nail, horizontal))
        {
            (nail.VariableB, nail.VariableC) =
                ReflectKraidNailHorizontalVelocity(nail.VariableB, nail.VariableC);
        }
        else if (KraidBodyContourReflectsNail(nail))
        {
            (nail.VariableB, nail.VariableC) =
                ReflectKraidNailHorizontalVelocity(nail.VariableB, nail.VariableC);
        }

        int vertical = CombineKraidVelocity(nail.VariableD, nail.VariableE);
        if (_terrain.MoveEnemyVertically(level, nail, vertical))
        {
            (nail.VariableD, nail.VariableE) =
                NegateKraidVelocity(nail.VariableD, nail.VariableE);
        }
    }

    /// <summary>Checks whether a nail crosses the body contour that reverses its flight.</summary>
    internal bool KraidBodyContourReflectsNail(RoomEnemySlot nail)
    {
        RoomEnemySlot body = _slotTable._slots[0];
        ushort contourX = unchecked((ushort)(
            body.Position.X.Whole + KraidNailContour.LeftOffset(
                unchecked((ushort)(nail.Position.Y.Whole - body.Position.Y.Whole)))));
        return unchecked((short)(nail.Position.X.Whole + nail.Radius.X - contourX)) >= 0 &&
            unchecked((short)nail.VariableC) >= 0;
    }

    /// <summary>Combines the native low and high velocity words as signed fixed-point motion.</summary>
    private static int CombineKraidVelocity(ushort low, ushort high) =>
        unchecked((int)(SnesSixteenSixteen.Compose(high, low)));

    /// <summary>
    /// Native horizontal wall/contour reflections at $A7:BE9E and BEE2 negate
    /// each word independently. Do not propagate fractional borrow as a normal
    /// fixed-point negation would; the resulting nonzero-fraction quirk is native.
    /// </summary>
    private static (ushort Low, ushort High) ReflectKraidNailHorizontalVelocity(ushort low, ushort high) =>
        (unchecked((ushort)-low), unchecked((ushort)-high));

    /// <summary>Negates a two-word native velocity while preserving wraparound.</summary>
    private static (ushort Low, ushort High) NegateKraidVelocity(ushort low, ushort high)
    {
        int negated = unchecked(-CombineKraidVelocity(low, high));
        return (
            unchecked((ushort)negated),
            SnesFixedPosition.WholeOf(unchecked((int)((uint)negated))));
    }
    /// <summary>Normal arm list selected while the body remains in combat.</summary>
    private const ushort KraidArmNormalInstruction =
        KraidArmInstructionProgramDefinitions.Normal;
    /// <summary>Retracted arm list used during death and before the first-phase lunge.</summary>
    private const ushort KraidArmRetractedInstruction =
        KraidArmInstructionProgramDefinitions.DyingOrPreparingToLunge;
    /// <summary>Neutral foot list selected between authored movement phases.</summary>
    private const ushort KraidFootNeutralInstruction =
        KraidFootInstructionProgramDefinitions.Neutral;
    /// <summary>Foot list that carries the first-phase forward lunge.</summary>
    private const ushort KraidFootLungeInstruction =
        KraidFootInstructionProgramDefinitions.LungeForward;
    /// <summary>Foot list selected when the forward lunge reaches its endpoint.</summary>
    private const ushort KraidFootLungeFinishedInstruction =
        KraidFootInstructionProgramDefinitions.LungeForwardFinished;
    /// <summary>Initial foot list for walking backward after a lunge.</summary>
    private const ushort KraidFootWalkBackInstruction =
        KraidFootInstructionProgramDefinitions.WalkingBackward;
    /// <summary>Repeating backward-walk foot list until the target position is reached.</summary>
    private const ushort KraidFootWalkBackLoopInstruction =
        KraidFootInstructionProgramDefinitions.WalkingBackwardLoop;

    /// <summary>Keeps the arm aligned to the body and advances its animation.</summary>
    private void RunKraidArmMain(RoomEnemySlot arm, ushort cameraY)
    {
        KraidEnemyState state = RequireKraidState(arm);
        RoomEnemySlot body = _slotTable._slots[0];
        arm.Position = arm.Position.WithWhole(new(body.Position.X.Whole, unchecked((ushort)(body.Position.Y.Whole - 44))));

        // `$B7D3-$B7F0` hides only the arm when its anchor leaves the 224-line gameplay
        // viewport. This is actor visibility, not BG priority, so preserve property $0100.
        bool onScreen = unchecked((short)(arm.Position.Y.Whole - cameraY)) >= 0 &&
            unchecked((short)(arm.Position.Y.Whole - cameraY - 224)) < 0;
        arm.Properties = onScreen
            ? arm.Properties.Without(EnemyProperties.Invisible)
            : arm.Properties.With(EnemyProperties.Invisible);

        // Mouth reopen attempts are encoded in the high byte. Incrementing the timer here
        // cancels the common interpreter's later decrement and freezes the selected arm map.
        if ((SnesLittleEndian.HighByteInPlace(state.MouthFlags)) != 0)
            arm.InstructionTimer = NativeCountdown.FromRaw(unchecked((ushort)(arm.InstructionTimer.RawValue + 1)));
    }

    /// <summary>Advances a lint part through idle, charge, fire, and reattachment states.</summary>
    internal void RunKraidLintMain(RoomEnemySlot lint, SamusState? samus)
    {
        KraidEnemyState state = RequireKraidState(lint);
        if (samus is not null)
            ResolveKraidLintContact(lint, samus);
        lint.InstructionTimer = NativeCountdown.Start(0x7fff);
        KraidPartState part = state.Parts[lint.SlotIndex];
        switch ((KraidAiFunction)lint.VariableA)
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
                    $"Kraid lint slot {lint.SlotIndex} function $A7:{lint.VariableA:X4} " +
                    "is not translated.");
        }
    }

    /// <summary>Places a physical part at its body-relative offset.</summary>
    internal void AlignKraidPart(RoomEnemySlot partSlot, KraidPartState part)
    {
        partSlot.Position = partSlot.Position.WithWholeX(unchecked((ushort)(_slotTable._slots[0].Position.X.Whole - partSlot.Radius.X)));
        TickKraidFunctionTimer(partSlot, part);
    }

    /// <summary>Starts a lint projectile from the attached part.</summary>
    private void ProduceKraidLint(RoomEnemySlot lint)
    {
        lint.Properties = lint.Properties.Without(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
        lint.Position = lint.Position.WithWholeX(unchecked((ushort)(
            lint.VariableC + _slotTable._slots[0].Position.X.Whole - lint.VariableB)));
        lint.VariableB = unchecked((ushort)(lint.VariableB + 1));
        if (unchecked((short)(lint.VariableB - 32)) >= 0)
        {
            lint.VariableA = (ushort)KraidAiFunction.LintCharge;
            lint.VariableF = 30;
        }
    }

    /// <summary>Advances the visible lint charge before firing.</summary>
    private void ChargeKraidLint(RoomEnemySlot lint)
    {
        lint.PaletteIndex = (lint.VariableF & 1) != 0 ? (ushort)3584 : (ushort)0;
        lint.Position = lint.Position.WithWholeX(unchecked((ushort)(
            lint.VariableC + _slotTable._slots[0].Position.X.Whole - lint.VariableB)));
        ushort oldTimer = lint.VariableF;
        lint.VariableF = unchecked((ushort)(lint.VariableF - 1));
        if (oldTimer == 1)
        {
            lint.VariableA = (ushort)KraidAiFunction.LintFire;
            LastKraidSoundEffect = new KraidSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x001f));
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
            lint.VariableA = (ushort)KraidAiFunction.AlignPartToKraid;
            lint.VariableF = 300;
            part.NextFunction = KraidAiFunction.LintProduce;
            lint.VariableB = 0;
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
        RoomEnemySlot body = _slotTable._slots[0];
        foot.Position = foot.Position.WithWhole(new(body.Position.X.Whole, unchecked((ushort)(body.Position.Y.Whole + 100))));
        bool onScreen = unchecked((short)(foot.Position.Y.Whole - cameraY)) >= 0 &&
            unchecked((short)(foot.Position.Y.Whole - 224 - cameraY)) < 0;
        foot.Properties = onScreen
            ? foot.Properties.Without(EnemyProperties.Invisible)
            : foot.Properties.With(EnemyProperties.Invisible);

        KraidPartState part = state.Parts[5];
        switch ((KraidAiFunction)foot.VariableA)
        {
            case KraidAiFunction.NoOperation:
                return;
            case KraidAiFunction.FootFirstPhaseThinking:
                if (TryBeginKraidGrowth(body, state))
                    return;
                TickKraidFunctionTimer(foot, part);
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
                TickKraidFunctionTimer(foot, part);
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
                    $"Kraid foot function $A7:{foot.VariableA:X4} is not translated.");
        }
    }

    /// <summary>Waits for the arm before starting the small-form foot lunge, or begins growth.</summary>
    private void PrepareKraidFirstPhaseLunge(RoomEnemySlot body, RoomEnemySlot foot)
    {
        if (TryBeginKraidGrowth(body, RequireKraidState(body)))
            return;
        RoomEnemySlot arm = _slotTable._slots[1];
        if (unchecked((short)(arm.CurrentInstruction -
            KraidArmInstructionProgramDefinitions.NormalPause)) < 0)
            return;
        arm.CurrentInstruction = KraidArmRetractedInstruction;
        arm.InstructionTimer = NativeCountdown.Start(1);
        foot.CurrentInstruction = KraidFootLungeInstruction;
        foot.InstructionTimer = NativeCountdown.Start(1);
        foot.VariableA = (ushort)KraidAiFunction.FootFirstPhaseLunge;
        foot.VariableF = 0;
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
        if (foot.CurrentInstruction != KraidFootLungeFinishedInstruction)
            return;
        if (body.Position.X.Whole == 92)
        {
            part.NextFunction = KraidAiFunction.FootFirstPhaseRetreat;
            foot.VariableA = (ushort)KraidAiFunction.DecrementFunctionTimerAndStartWalk;
            foot.VariableF = 1;
            foot.CurrentInstruction = KraidFootNeutralInstruction;
        }
        else
        {
            foot.CurrentInstruction = KraidFootLungeInstruction;
        }
        foot.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Counts down a foot phase and installs its next function at zero.</summary>
    private static void TickKraidFootTransitionTimer(RoomEnemySlot foot, KraidPartState part)
    {
        if (foot.VariableF == 0)
            return;
        ushort oldTimer = foot.VariableF;
        foot.VariableF = unchecked((ushort)(foot.VariableF - 1));
        if (oldTimer == 1)
        {
            foot.VariableA = (ushort)part.NextFunction;
            foot.CurrentInstruction = KraidFootWalkBackInstruction;
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
        if (unchecked((short)(foot.CurrentInstruction - KraidFootWalkBackLoopInstruction)) < 0)
            return;
        if (body.Position.X.Whole == 176)
        {
            RoomEnemySlot arm = _slotTable._slots[1];
            arm.CurrentInstruction = KraidArmNormalInstruction;
            arm.InstructionTimer = NativeCountdown.Start(1);
            foot.CurrentInstruction = KraidFootNeutralInstruction;
            foot.InstructionTimer = NativeCountdown.Start(1);
            foot.VariableA = (ushort)KraidAiFunction.FootFirstPhaseThinking;
            foot.VariableF = 300;
            part.NextFunction = KraidAiFunction.FootPrepareFirstPhaseLunge;
        }
        else
        {
            foot.CurrentInstruction = KraidFootWalkBackInstruction;
            foot.InstructionTimer = NativeCountdown.Start(1);
        }
    }

    /// <summary>Positions the grown-form foot before its walking cycle begins.</summary>
    private void RunKraidSecondPhaseWalkToStart(
        RoomEnemySlot body,
        RoomEnemySlot foot,
        KraidPartState part)
    {
        ushort targetX = RequireKraidState(body).TargetX;
        if (targetX != body.Position.X.Whole)
        {
            if (unchecked((short)(targetX - body.Position.X.Whole)) >= 0)
                return;
            body.Position = body.Position.WithWholeX(targetX);
        }
        if (unchecked((short)(foot.CurrentInstruction - KraidFootWalkBackLoopInstruction)) < 0)
            return;
        foot.VariableA = (ushort)KraidAiFunction.HandleFunctionTimer;
        foot.VariableF = 180;
        part.NextFunction = KraidAiFunction.FootSecondPhaseInitialize;
        foot.CurrentInstruction = KraidFootNeutralInstruction;
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
        RequireKraidState(foot).TargetX = targetX;
        part.ThinkTimer = NativeCountdown.FromRaw(thinkTimer);
        foot.VariableA = (ushort)KraidAiFunction.FootSecondPhaseWalkingRight;
        foot.CurrentInstruction = KraidFootWalkBackInstruction;
        foot.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Stores a leftward target and starts the foot's walking animation.</summary>
    private void SetKraidWalkingLeft(
        RoomEnemySlot foot,
        KraidPartState part,
        ushort targetX,
        ushort thinkTimer)
    {
        RequireKraidState(foot).TargetX = targetX;
        part.ThinkTimer = NativeCountdown.FromRaw(thinkTimer);
        foot.VariableA = (ushort)KraidAiFunction.FootSecondPhaseWalkingLeft;
        foot.CurrentInstruction = KraidFootInstructionProgramDefinitions.WalkingForward;
        foot.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Moves the body and foot toward the selected rightward target.</summary>
    private void RunKraidSecondPhaseWalkingRight(RoomEnemySlot body, RoomEnemySlot foot)
    {
        ushort targetX = RequireKraidState(body).TargetX;
        if (targetX != body.Position.X.Whole)
        {
            if (unchecked((short)(targetX - body.Position.X.Whole)) >= 0)
                return;
            body.Position = body.Position.WithWholeX(targetX);
        }
        if (unchecked((short)(foot.CurrentInstruction - KraidFootWalkBackLoopInstruction)) >= 0)
        {
            foot.VariableA = (ushort)KraidAiFunction.FootSecondPhaseThinking;
            foot.CurrentInstruction = KraidFootNeutralInstruction;
            foot.InstructionTimer = NativeCountdown.Start(1);
        }
    }

    /// <summary>Moves the body and foot toward the selected leftward target.</summary>
    private void RunKraidSecondPhaseWalkingLeft(RoomEnemySlot body, RoomEnemySlot foot)
    {
        ushort targetX = RequireKraidState(body).TargetX;
        if (unchecked((short)(targetX - body.Position.X.Whole)) < 0)
        {
            if (foot.CurrentInstruction ==
                KraidFootInstructionProgramDefinitions.WalkingForwardFinished)
            {
                foot.CurrentInstruction =
                    KraidFootInstructionProgramDefinitions.WalkingForward;
                foot.InstructionTimer = NativeCountdown.Start(1);
            }
            return;
        }
        body.Position = body.Position.WithWholeX(targetX);
        if (foot.CurrentInstruction ==
            KraidFootInstructionProgramDefinitions.WalkingForwardFinished)
        {
            foot.VariableA = (ushort)KraidAiFunction.FootSecondPhaseThinking;
            foot.CurrentInstruction = KraidFootNeutralInstruction;
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
            graphicsIndex: 0x0600);
        ushort random = ReadKraidRandomNumber();
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(body.Position.X.Whole + 16)), unchecked((ushort)(body.Position.Y.Whole - 96))));
        rock.Velocity = rock.Velocity.WithX(new SnesSignedEightEight(KraidRockLaunchDefinitions.FromRandom(random)));
        rock.Velocity = rock.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-0x0400)));
        rock.GraphicsIndex = 0x0600;
        return true;
    }

    /// <summary>Requests a rock from the floor during the growth sequence.</summary>
    internal void RequestKraidRisingRock(RoomEnemySlot body, KraidEnemyState state)
    {
        state.RiseRockSpawnRequestCount++;
        ushort random = ReadKraidRandomNumber();
        RoomEnemyProjectileKind kind = (random & 0x0010) != 0
            ? RoomEnemyProjectileKind.KraidRisingRockRight
            : RoomEnemyProjectileKind.KraidRisingRockLeft;
        RoomEnemyProjectileSlot? rock = _projectiles.AllocateEnemyProjectile();
        if (rock is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(rock, kind, graphicsIndex: 0x0600);
        // $86:9D17 tests bit zero: clear complements the offset to the left.
        int randomOffset = random & 0x003f;
        if ((random & 1) == 0)
            randomOffset = unchecked((short)~randomOffset);
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(body.Position.X.Whole + randomOffset)), 432));
        rock.Velocity = new(new SnesSignedEightEight(unchecked((ushort)(random & 0x03f0))), new SnesSignedEightEight(unchecked((ushort)-0x0500)));
        rock.GraphicsIndex = 0x0600;
        state.SpawnedRiseRockCount++;
        LastKraidSoundEffect = new KraidSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x001e));
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
            graphicsIndex: 0x0600);
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(xPosition, 312));
        rock.Velocity = rock.Velocity.WithX(new SnesSignedEightEight(0));
        rock.Velocity = rock.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)((ReadKraidRandomNumber() & 0x003f) + 0x0040))));
        rock.GraphicsIndex = 0x0600;
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
        if (horizontal >= 0x0100)
            horizontal = -0x0100;
        rock.Velocity = rock.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)horizontal)));
        rock.Velocity = rock.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(rock.Velocity.Y.RawValue + 0x0040))));
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
        rock.Velocity = rock.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)((rock.Velocity.Y.RawValue + 0x0018) & 0x3fff))));
    }

    /// <summary>Consumes one value from the room RNG for Kraid's next decision.</summary>
    private ushort ReadKraidRandomNumber() =>
        _room.Random.Read();
    /// <summary>Dispatches the authored vertical rise and pause functions.</summary>
    internal void RunKraidRiseFunction(
        RoomEnemySlot body,
        KraidEnemyState state,
        SamusState? samus)
    {
        KraidAiFunction function = (KraidAiFunction)body.VariableA;
        switch (function)
        {
            case KraidAiFunction.RestrictSamusToFirstScreen:
                RestrictSamusToKraidFirstScreen(samus);
                TickKraidFunctionTimer(body, state.Parts[0]);
                return;

            case KraidAiFunction.RaiseKraidThroughFloor:
                RestrictSamusToKraidFirstScreen(samus);
                body.VariableA = (ushort)KraidAiFunction.RaiseLoadBottomTilemap;
                TransferKraidTopTilemap(state);
                return;

            case KraidAiFunction.RaiseLoadBottomTilemap:
                RestrictSamusToKraidFirstScreen(samus);
                body.VariableA = (ushort)KraidAiFunction.RaiseRocksEvery16Frames;
                body.VariableF = 120;
                _earthquake.EarthquakeTimer = NativeCountdown.Start(496);
                state.MusicRequest = MusicCommand.SelectTrack(5);
                TransferKraidBottomTilemap(state);
                return;

            case KraidAiFunction.RaiseRocksEvery16Frames:
                RestrictSamusToKraidFirstScreen(samus);
                body.VariableF = unchecked((ushort)(body.VariableF - 1));
                if (body.VariableF == 0)
                {
                    body.VariableA = (ushort)KraidAiFunction.RaiseRocksEvery8Frames;
                    body.VariableF = 96;
                }
                else if ((body.VariableF & 0x000f) == 0)
                {
                    RequestKraidRisingRock(body, state);
                }
                return;

            case KraidAiFunction.RaiseRocksEvery8Frames:
                RestrictSamusToKraidFirstScreen(samus);
                body.VariableF = unchecked((ushort)(body.VariableF - 1));
                if (body.VariableF == 0)
                {
                    body.VariableA = (ushort)KraidAiFunction.RaiseBody;
                    body.VariableF = 288;
                }
                else if ((body.VariableF & 7) == 0)
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
                AddSignedKraidVerticalDisplacement(body, -0x8000);
                if (unchecked((short)(body.Position.Y.Whole - 457)) < 0)
                {
                    body.Position = body.Position.WithWholeX(176);
                    // `$C97B` does not leave execution at setup routine `$ADE9`.
                    // That JSR immediately installs `$AEA4`, seeds the random thinking
                    // timer and selects roar list entry `$96DA` for the first attack.
                    body.VariableB = KraidHeadInstructionDefinitions.RoarContinuation;
                    SetupKraidFirstPhaseThinking(body, state);

                    // The foot owns first-phase lunges independently of the body thinker.
                    // Its 300-frame delay starts on the exact rise-completion frame.
                    RoomEnemySlot foot = _slotTable._slots[5];
                    foot.VariableA = (ushort)KraidAiFunction.FootFirstPhaseThinking;
                    foot.VariableF = 300;
                    state.Parts[5].NextFunction = KraidAiFunction.FootPrepareFirstPhaseLunge;
                    _slotTable._slots[1].CurrentInstruction =
                        KraidArmInstructionProgramDefinitions.Normal;
                    _slotTable._slots[1].InstructionTimer = NativeCountdown.Start(1);
                }
                return;

            default:
                throw new InvalidDataException(
                    $"Kraid body function $A7:{body.VariableA:X4} is not translated.");
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
    private static void TickKraidFunctionTimer(
        RoomEnemySlot slot,
        KraidPartState part)
    {
        if (slot.VariableF == 0)
            return;
        slot.VariableF--;
        if (slot.VariableF == 0)
            slot.VariableA = (ushort)part.NextFunction;
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
        ushort random = _room.Random.Read();
        int value = random & 7;
        if (value == 0)
            value = 2;
        return unchecked((ushort)(value << 6));
    }
    /// <summary>Room block mutations requested by Kraid during the current enemy frame.</summary>
    internal readonly List<KraidPlmRequest> _kraidPlmRequests = [];

    /// <summary>Active typed state when the loaded room owns retail Kraid slot zero.</summary>
    public KraidEnemyState? Kraid { get; internal set; }

    /// <summary>Hardcoded Kraid room mutations published during the current enemy frame.</summary>
    public IReadOnlyList<KraidPlmRequest> KraidPlmRequests => _kraidPlmRequests;

    /// <summary>Discards prior room Kraid state and pending hardcoded PLM requests.</summary>
    internal void ResetKraidRoomState()
    {
        Kraid = null;
        _kraidPlmRequests.Clear();
    }

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
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        // Legacy publications are momentary like their native QueueSfx/QueueMusic calls; clearing them at the
        // EnemyMain frame boundary keeps a paused or frozen actor from replaying its previous request.
        if (Kraid is not null)
            Kraid.MusicRequest = null;
        ResetKraidRoomState();
        LastKraidSoundEffect = null;
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        // Legacy publications are momentary like their native QueueSfx/QueueMusic calls; clearing them at the
        // EnemyMain frame boundary keeps a paused or frozen actor from replaying its previous request.
        if (Kraid is not null)
            Kraid.MusicRequest = null;
        LastKraidSoundEffect = null;
        _kraidPlmRequests.Clear();
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
    internal void RegisterKraidNailTouch(EnemyAiTable table)
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
                    _deathsAndDrops.StartGenericEnemyDeath(context.Slot, context.Slot.Health);
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
}

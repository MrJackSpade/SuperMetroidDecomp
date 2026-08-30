using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Untouched-ROM audit of Draygon's retail four-record load and the complete 1,488-frame
/// opening. It intentionally stops at the first combat swoop seam; later slices extend this
/// same encounter instead of manufacturing boss records or forcing private function words.
/// </summary>
internal static class DraygonAudit
{
    private const ushort RoomPointer = 0xda60;
    private const ushort PopulationPointer = 0xd314;
    private static readonly ushort[] ExpectedDefinitions = [0xde3f, 0xde7f, 0xdebf, 0xdeff];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        if (room.WidthInScreens != 2 || room.HeightInScreens != 2 ||
            room.AreaIndex != 4 || room.State.EnemyPopulationPointer != PopulationPointer)
        {
            throw new InvalidDataException(
                $"Draygon room mismatch: {room.WidthInScreens}x{room.HeightInScreens}, " +
                $"area={room.AreaIndex}, population=$A1:{room.State.EnemyPopulationPointer:X4}.");
        }

        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x1234);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            XPosition = 0x0100,
            // Remain above the firing lane for this no-hit route. This is a legitimate
            // player position, not an enemy-state override, and lets the same audit inspect
            // complete projectile flight before the later grab/damage slice takes contact.
            YPosition = 0x0040,
            Pose = SamusState.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus,
            isAreaBossDefeated: () => false);

        DraygonEnemyState state = enemies.Draygon ??
            throw new InvalidDataException("Draygon room did not allocate typed encounter state.");
        if (enemies.EnemyCount != 4 || state.Eye is null || state.Tail is null || state.Arms is null)
            throw new InvalidDataException("Draygon's four physical records were not linked.");
        for (int slotIndex = 0; slotIndex < ExpectedDefinitions.Length; slotIndex++)
        {
            if (enemies.Slots[slotIndex].EnemyDefinitionPointer != ExpectedDefinitions[slotIndex])
            {
                throw new InvalidDataException(
                    $"Draygon slot {slotIndex} loaded ${enemies.Slots[slotIndex].EnemyDefinitionPointer:X4}, " +
                    $"expected ${ExpectedDefinitions[slotIndex]:X4}.");
            }
        }

        RoomEnemySlot body = state.Body;
        if (body.XPosition != 0xffb0 || body.YPosition != 0xffb0 || body.Health != 6000 ||
            body.CurrentInstruction != 0x9889 || state.Eye.CurrentInstruction != 0x9944 ||
            state.Tail.CurrentInstruction != 0x99fc || state.Arms.CurrentInstruction != 0x97e7 ||
            body.PaletteIndex != 0x0e00 || state.Tail.PaletteIndex != 0x0e00 ||
            state.Arms.PaletteIndex != 0x0e00 || state.Arms.Layer != 2 ||
            state.Function != DraygonAiFunction.IntroInitialDelay ||
            state.Bg2TilemapSize != 0x0400 || !state.BackgroundTilemapPrepared ||
            vram.ReadWord(0x4800) != 0x0338 || vram.ReadWord(0x4fff) != 0x0338 ||
            !state.MinimapDisabledAndBossTilesExplored || !state.BottomUnusedTurretDisabled)
        {
            throw new InvalidDataException(
                $"Draygon initialization mismatch: body=({body.XPosition:X4},{body.YPosition:X4}) " +
                $"hp={body.Health}, lists={body.CurrentInstruction:X4}/{state.Eye.CurrentInstruction:X4}/" +
                $"{state.Tail.CurrentInstruction:X4}/{state.Arms.CurrentInstruction:X4}, " +
                $"function=$A5:{(ushort)state.Function:X4}, BG={state.BackgroundTilemapPrepared}.");
        }

        var functions = new HashSet<DraygonAiFunction>();
        var bodyMaps = new HashSet<ushort>();
        var eyeMaps = new HashSet<ushort>();
        var tailMaps = new HashSet<ushort>();
        var armMaps = new HashSet<ushort>();
        var tailDisplacements = new HashSet<(ushort X, ushort Y)>();
        var movedEvirSlots = new HashSet<int>();
        var priorEvirPositions = new Dictionary<int, (ushort X, ushort Y)>();
        var turretSlots = new HashSet<int>();
        var movedTurretSlots = new HashSet<int>();
        var priorTurretPositions = new Dictionary<int, (ushort X, ushort Y)>();
        var goopSlots = new HashSet<int>();
        var movedGoopSlots = new HashSet<int>();
        var goopPreInstructions = new HashSet<ushort>();
        var priorGoopPositions = new Dictionary<int, (ushort X, ushort Y)>();
        var goopYPositions = new HashSet<ushort>();
        bool sawFourLiveEvirs = false;
        bool allPartsStayedAttached = true;
        bool sawRightFacing = false;
        bool sawLeftFacing = false;
        bool sawExactOpeningTurretCadence = false;
        ushort maximumVisibleX = 0;
        ushort maximumVisibleY = 0;
        int frame;
        for (frame = 0; frame < 8000; frame++)
        {
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData,
                nmiFrameCounter8: unchecked((byte)frame));

            functions.Add(state.Function);
            sawRightFacing |= state.FacingRight;
            sawLeftFacing |= !state.FacingRight &&
                state.Function is DraygonAiFunction.SwoopLeftDescending or
                    DraygonAiFunction.SwoopLeftApex or DraygonAiFunction.SwoopLeftAscending;
            bodyMaps.Add(body.SpritemapPointer);
            eyeMaps.Add(state.Eye.SpritemapPointer);
            tailMaps.Add(state.Tail.SpritemapPointer);
            armMaps.Add(state.Arms.SpritemapPointer);
            tailDisplacements.Add(
                (state.BodyGraphicsXDisplacement, state.BodyGraphicsYDisplacement));
            if (body.XPosition < 0x8000)
                maximumVisibleX = Math.Max(maximumVisibleX, body.XPosition);
            if (body.YPosition < 0x8000)
                maximumVisibleY = Math.Max(maximumVisibleY, body.YPosition);
            allPartsStayedAttached &= state.Eye.XPosition == body.XPosition &&
                state.Eye.YPosition == body.YPosition &&
                state.Tail.XPosition == body.XPosition && state.Tail.YPosition == body.YPosition &&
                state.Arms.XPosition == body.XPosition && state.Arms.YPosition == body.YPosition;

            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
            {
                if (projectile.Kind != RoomEnemyProjectileKind.DraygonWallTurret)
                    continue;
                turretSlots.Add(projectile.SlotIndex);
                if (priorTurretPositions.TryGetValue(
                        projectile.SlotIndex,
                        out (ushort X, ushort Y) prior) &&
                    (prior.X != projectile.XPosition || prior.Y != projectile.YPosition))
                {
                    movedTurretSlots.Add(projectile.SlotIndex);
                }
                priorTurretPositions[projectile.SlotIndex] =
                    (projectile.XPosition, projectile.YPosition);
            }
            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
            {
                if (projectile.Kind != RoomEnemyProjectileKind.DraygonGoop)
                    continue;
                goopSlots.Add(projectile.SlotIndex);
                goopPreInstructions.Add(projectile.PreInstruction);
                if (priorGoopPositions.TryGetValue(
                        projectile.SlotIndex,
                        out (ushort X, ushort Y) prior) &&
                    (prior.X != projectile.XPosition || prior.Y != projectile.YPosition))
                {
                    movedGoopSlots.Add(projectile.SlotIndex);
                }
                priorGoopPositions[projectile.SlotIndex] =
                    (projectile.XPosition, projectile.YPosition);
            }
            if (state.Function is DraygonAiFunction.GoopRight or DraygonAiFunction.GoopRightTail or
                DraygonAiFunction.GoopRightRecovery or DraygonAiFunction.GoopLeft or
                DraygonAiFunction.GoopLeftTail or DraygonAiFunction.GoopLeftRecovery)
            {
                goopYPositions.Add(body.YPosition);
            }

            int liveEvirs = 0;
            foreach (RoomSpriteObjectSlot sprite in enemies.RoomSpriteObjects)
            {
                if (!sprite.IsActive || sprite.Kind != RoomSpriteObjectKind.DraygonIntroEvir)
                    continue;
                liveEvirs++;
                if (priorEvirPositions.TryGetValue(
                        sprite.SlotIndex,
                        out (ushort X, ushort Y) prior) &&
                    (prior.X != sprite.XPosition || prior.Y != sprite.YPosition))
                {
                    movedEvirSlots.Add(sprite.SlotIndex);
                }
                priorEvirPositions[sprite.SlotIndex] = (sprite.XPosition, sprite.YPosition);
            }
            sawFourLiveEvirs |= liveEvirs == 4;

            // Exercise the actual extended-spritemap reader on every animation tick. The
            // boss begins off screen, but malformed maps and hitbox pointers still surface.
            enemies.DrawLayers(new OamBuffer(), 0, 0, firstLayer: 0, lastLayer: 7);
            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)frame));

            if (!sawExactOpeningTurretCadence &&
                functions.Contains(DraygonAiFunction.SwoopLeftAscending) &&
                state.Function is DraygonAiFunction.SwoopRightSetup or
                    DraygonAiFunction.GoopRightSetup)
            {
                sawExactOpeningTurretCadence = state.TurretCadenceChecks == frame / 64 + 1;
            }

            bool completedGoopPass = functions.Contains(DraygonAiFunction.GoopRightRecovery) ||
                functions.Contains(DraygonAiFunction.GoopLeftRecovery);
            if (completedGoopPass &&
                state.Function is DraygonAiFunction.SwoopRightSetup or DraygonAiFunction.SwoopLeftSetup)
                break;
        }

        int remainingEvirs = enemies.RoomSpriteObjects.Count(
            sprite => sprite.IsActive && sprite.Kind == RoomSpriteObjectKind.DraygonIntroEvir);
        if (frame >= 8000 || !functions.Contains(DraygonAiFunction.IntroInitialDelay) ||
            !functions.Contains(DraygonAiFunction.IntroDance) ||
            !functions.Contains(DraygonAiFunction.SwoopRightSetup) ||
            !functions.Contains(DraygonAiFunction.SwoopRightDescending) ||
            !functions.Contains(DraygonAiFunction.SwoopRightApex) ||
            !functions.Contains(DraygonAiFunction.SwoopRightAscending) ||
            !functions.Contains(DraygonAiFunction.SwoopLeftSetup) ||
            !functions.Contains(DraygonAiFunction.SwoopLeftDescending) ||
            !functions.Contains(DraygonAiFunction.SwoopLeftApex) ||
            !functions.Contains(DraygonAiFunction.SwoopLeftAscending) ||
            !(functions.Contains(DraygonAiFunction.GoopRightSetup) ||
                functions.Contains(DraygonAiFunction.GoopLeftSetup)) ||
            !(functions.Contains(DraygonAiFunction.GoopRight) ||
                functions.Contains(DraygonAiFunction.GoopLeft)) ||
            !(functions.Contains(DraygonAiFunction.GoopRightTail) ||
                functions.Contains(DraygonAiFunction.GoopLeftTail)) ||
            !(functions.Contains(DraygonAiFunction.GoopRightRecovery) ||
                functions.Contains(DraygonAiFunction.GoopLeftRecovery)) ||
            state.Function is not (DraygonAiFunction.SwoopRightSetup or DraygonAiFunction.SwoopLeftSetup) ||
            !state.IntroEvirGraphicsLoaded || state.IntroEvirsSpawned != 4 ||
            !sawFourLiveEvirs || movedEvirSlots.Count != 4 || remainingEvirs != 0 ||
            state.IntroDanceFrames != 0x04d0 || !sawExactOpeningTurretCadence ||
            state.LeftSideResetXPosition != 0xffb0 || state.RightSideResetXPosition != 0x0250 ||
            state.ResetYPosition != 0xffb0 || state.SwoopYAcceleration != 0x0018 ||
            state.SwoopPathEntryCount < 32 || maximumVisibleX < 0x0250 ||
            maximumVisibleY < 0x0140 || state.BreathBubblesSpawned < 2 ||
            !sawRightFacing || !sawLeftFacing ||
            state.WallTurretsSpawned == 0 || turretSlots.Count == 0 || movedTurretSlots.Count == 0 ||
            state.GoopProjectilesSpawned == 0 || goopSlots.Count == 0 ||
            movedGoopSlots.Count == 0 || !goopPreInstructions.Contains(0x8e0f) ||
            goopYPositions.Count < 16 || state.LastSoundLibrary2 != 0x004c ||
            !allPartsStayedAttached || bodyMaps.Count < 1 || eyeMaps.Count < 1 ||
            tailMaps.Count < 8 || armMaps.Count < 6 || tailDisplacements.Count < 7)
        {
            throw new InvalidDataException(
                $"Draygon opening mismatch after {frame} frames: function=$A5:{(ushort)state.Function:X4}, " +
                $"functions={functions.Count}, Evir spawn/move/live={state.IntroEvirsSpawned}/" +
                $"{movedEvirSlots.Count}/{remainingEvirs}, dance={state.IntroDanceFrames}, " +
                $"turret cadence={state.TurretCadenceChecks}, reset=({state.LeftSideResetXPosition:X4}," +
                $"{state.ResetYPosition:X4})/right={state.RightSideResetXPosition:X4}, " +
                $"swoop entries/max=({state.SwoopPathEntryCount},{maximumVisibleX:X4}," +
                $"{maximumVisibleY:X4}), bubbles={state.BreathBubblesSpawned}, " +
                $"turrets={state.WallTurretsSpawned}/{turretSlots.Count}/{movedTurretSlots.Count}, " +
                $"goop={state.GoopProjectilesSpawned}/{goopSlots.Count}/{movedGoopSlots.Count}, " +
                $"goop states/Y={goopPreInstructions.Count}/{goopYPositions.Count}, " +
                $"maps={bodyMaps.Count}/{eyeMaps.Count}/{tailMaps.Count}/{armMaps.Count}, " +
                $"tail displacements={tailDisplacements.Count}, attached={allPartsStayedAttached}.");
        }

        Console.WriteLine(
            $"Draygon audit completed the opening and first natural goop pass after {frame + 1} frames: retail " +
            "2x2 room/four-part population, full BG2 clear, independent body/eye/tail/arms " +
            "animation, Evir tile upload, four physical intro dancers, 1,232 dance ticks, " +
            "eye tracking, tail graphics displacement, exact turret RNG cadence, table-driven " +
            "descent/apex/ascent, physical aimed wall-turret shots, breath bubbles, and attached " +
            "multipart coordinates, followed by cosine-path goop approach/fire/exit and physical " +
            "destroyable goop projectiles.");
        return 0;
    }
}

using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed lifecycle audit for the Shitroid encounter and all ten companion corpses.
/// </summary>
internal static class ShitroidAudit
{
    private const ushort RoomPointer = 0xdcb1;
    private const ushort StatePointer = 0xdcc3;
    private const int PopulationBank = 0xa10000;
    private const ushort EncounterCameraX = 0x0200;

    private static readonly PopulationExpectation[] ExpectedPopulation =
    [
        new(0xeebf, 0, 0, 0xef37, 0xefc5, 0xf789, 0xf842, 0xefba),
        new(0xed7f, 0, 0, 0xd7b6, 0xd8db, 0xdd44, 0xdd1d, 0xd8cc),
        new(0xed7f, 2, 0, 0xd7b6, 0xd8db, 0xdd44, 0xdd1d, 0xd8cc),
        new(0xedff, 0, 0, 0xd849, 0xd8db, 0xdcf8, 0xdcf8, 0xdced),
        new(0xedff, 2, 0, 0xd849, 0xd8db, 0xdcf8, 0xdcf8, 0xdced),
        new(0xedff, 4, 0, 0xd849, 0xd8db, 0xdcf8, 0xdcf8, 0xdced),
        new(0xee3f, 0, 0, 0xd876, 0xd8db, 0xdd08, 0xdd08, 0xdcfd),
        new(0xee3f, 2, 0, 0xd876, 0xd8db, 0xdd08, 0xdd08, 0xdcfd),
        new(0xee7f, 0, 0, 0xd89f, 0xd8db, 0xdd18, 0xdd18, 0xdd0d),
        new(0xee7f, 2, 0, 0xd89f, 0xd8db, 0xdd18, 0xdd18, 0xdd0d),
        new(0xee7f, 4, 0, 0xd89f, 0xd8db, 0xdd18, 0xdd18, 0xdd0d),
    ];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer) with
        {
            State = CartridgeRoomState.Load(bus, StatePointer),
        };
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        VerifyPopulationAndHeaders(bus, room);

        // Contact while palette stage is below eight is the only damaging sidehopper path.
        // Exercise it on a disposable load so knockback state cannot contaminate the long
        // cutscene lifecycle checked immediately afterward.
        LoadedShitroidEncounter early = Load(bus, room, assets);
        RoomEnemySlot earlyVictim = early.Enemies.Slots[1];
        early.Enemies.Slots[0].Properties = unchecked((ushort)(
            early.Enemies.Slots[0].Properties | 0x0400));
        Step(bus, assets.LevelData, early, frame: 0, cameraX: 513);
        early.Samus.XPosition = earlyVictim.XPosition;
        early.Samus.YPosition = earlyVictim.YPosition;
        ushort earlyHealth = early.Samus.Health;
        bool earlyContact = early.Enemies.ResolveOrdinarySamusContact(
            early.Samus,
            controllerInput: 0,
            assets.LevelData);
        if (!earlyContact || early.Samus.Health >= earlyHealth)
        {
            throw new InvalidDataException(
                $"Live sidehopper contact did not damage Samus: contact={earlyContact}, " +
                $"health={earlyHealth}->{early.Samus.Health}.");
        }

        LoadedShitroidEncounter loaded = Load(bus, room, assets);
        RoomEnemySlot shitroidSlot = loaded.Enemies.Slots[0];
        ShitroidEnemyState shitroid = loaded.Enemies.Shitroid ??
            throw new InvalidDataException("Shitroid load produced no typed state.");
        DeadSidehopperEnemyState victim = loaded.Enemies.DeadSidehoppers[1] ??
            throw new InvalidDataException("Shitroid victim produced no typed state.");
        DeadSidehopperEnemyState alternateSidehopper = loaded.Enemies.DeadSidehoppers[2] ??
            throw new InvalidDataException("Alternate sidehopper produced no typed state.");
        VerifyInitialization(bus, loaded, shitroid, victim, alternateSidehopper);

        var visitedShitroidStates = new HashSet<ShitroidAiFunction>();
        bool sawCloseWall = false;
        bool sawOpenWall = false;
        bool sawEntranceMusic = false;
        StepAndRecord(
            bus,
            assets.LevelData,
            loaded,
            frame: 0,
            visitedShitroidStates,
            ref sawCloseWall,
            ref sawOpenWall,
            ref sawEntranceMusic);
        if (shitroid.Function != ShitroidAiFunction.BeginEntranceDelay ||
            loaded.Enemies.RequestedShitroidCameraX != EncounterCameraX ||
            loaded.Enemies.ShitroidPlmRequests.Count != 2 ||
            loaded.Enemies.ShitroidPlmRequests.Any(request => request.Header != 0xb767) ||
            victim.Function != DeadSidehopperAiFunction.NoOperation)
        {
            throw new InvalidDataException(
                $"First Shitroid frame mismatch: function={shitroid.Function}, " +
                $"camera={loaded.Enemies.RequestedShitroidCameraX}, " +
                $"walls={string.Join(',', loaded.Enemies.ShitroidPlmRequests)}, " +
                $"victim={victim.Function}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawLayers(oam, EncounterCameraX, cameraY: 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Shitroid encounter emitted no first-frame OBJ pieces.");

        int frameCounter = 1;
        while (shitroid.Function != ShitroidAiFunction.HoverNearSamus && frameCounter < 4000)
        {
            StepAndRecord(
                bus,
                assets.LevelData,
                loaded,
                frameCounter++,
                visitedShitroidStates,
                ref sawCloseWall,
                ref sawOpenWall,
                ref sawEntranceMusic);
        }
        if (shitroid.Function != ShitroidAiFunction.HoverNearSamus ||
            victim.PaletteStage != 8 ||
            victim.Function != DeadSidehopperAiFunction.WaitForSamusCollision ||
            victim.Slot.YRadius != 12 ||
            (victim.Slot.Properties & 0x8000) == 0 ||
            !sawCloseWall || !sawOpenWall || !sawEntranceMusic)
        {
            throw new InvalidDataException(
                $"Shitroid feeding lifecycle incomplete after {frameCounter} frames: " +
                $"function={shitroid.Function}, corpse stage/function/radius/properties=" +
                $"{victim.PaletteStage}/{victim.Function}/{victim.Slot.YRadius}/" +
                $"${victim.Slot.Properties:X4}, walls={sawCloseWall}/{sawOpenWall}, " +
                $"music={sawEntranceMusic}.");
        }

        // The private shot callback consumes the common projectile prelude but changes only
        // Shitroid's recoil velocity. Health remains the main routine's forced $7FFF.
        ushort healthBeforeShot = shitroidSlot.Health;
        ushort xVelocityBeforeShot = shitroid.XVelocity;
        ushort yVelocityBeforeShot = shitroid.YVelocity;
        ArmProjectile(loaded.Shots.Slots[0], shitroidSlot);
        int shitroidShotHits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            loaded.Shots,
            loaded.SharedProjectiles,
            loaded.Samus);
        if (shitroidShotHits != 1 || shitroidSlot.Health != healthBeforeShot ||
            (loaded.Shots.Slots[0].Direction & 0x0010) == 0 ||
            (shitroid.XVelocity == xVelocityBeforeShot &&
             shitroid.YVelocity == yVelocityBeforeShot))
        {
            throw new InvalidDataException(
                $"Shitroid shot mismatch: hits={shitroidShotHits}, " +
                $"health={healthBeforeShot}->{shitroidSlot.Health}, velocity=" +
                $"${xVelocityBeforeShot:X4}/${yVelocityBeforeShot:X4}->" +
                $"${shitroid.XVelocity:X4}/${shitroid.YVelocity:X4}.");
        }

        // One contact changes hover to pursuit. On the next frame, placing Samus exactly at
        // the latch target makes `$F789`'s authored exact-arrival path begin draining.
        PositionSamusAtShitroidLatchTarget(loaded.Samus, shitroidSlot);
        loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0, assets.LevelData);
        if (shitroid.Function != ShitroidAiFunction.ChaseSamus)
            throw new InvalidDataException($"Shitroid touch selected {shitroid.Function}, not chase.");
        StepAndRecord(
            bus,
            assets.LevelData,
            loaded,
            frameCounter++,
            visitedShitroidStates,
            ref sawCloseWall,
            ref sawOpenWall,
            ref sawEntranceMusic);
        PositionSamusAtShitroidLatchTarget(loaded.Samus, shitroidSlot);
        loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0, assets.LevelData);
        if (shitroid.Function != ShitroidAiFunction.BeginDrainingSamus)
            throw new InvalidDataException($"Shitroid latch selected {shitroid.Function}.");

        ushort preDrainHealth = loaded.Samus.Health;
        while (shitroid.Function is not ShitroidAiFunction.BeginPostDrainPause and
               not ShitroidAiFunction.PostDrainPause && frameCounter < 6000)
        {
            StepAndRecord(
                bus,
                assets.LevelData,
                loaded,
                frameCounter++,
                visitedShitroidStates,
                ref sawCloseWall,
                ref sawOpenWall,
                ref sawEntranceMusic);
        }
        if (loaded.Samus.Health != 1 || loaded.Samus.Health >= preDrainHealth ||
            loaded.SharedProjectiles.BombCounter != 0 ||
            loaded.Samus.SpecialSuperPaletteFlags != 0)
        {
            throw new InvalidDataException(
                $"Shitroid drain terminal mismatch: health={preDrainHealth}->" +
                $"{loaded.Samus.Health}, bombs={loaded.SharedProjectiles.BombCounter}, " +
                $"palette=${loaded.Samus.SpecialSuperPaletteFlags:X4}.");
        }

        while (shitroid.Function != ShitroidAiFunction.ReleasedFollow && frameCounter < 8000)
        {
            StepAndRecord(
                bus,
                assets.LevelData,
                loaded,
                frameCounter++,
                visitedShitroidStates,
                ref sawCloseWall,
                ref sawOpenWall,
                ref sawEntranceMusic);
        }
        if (shitroid.Function != ShitroidAiFunction.ReleasedFollow || shitroidSlot.Parameter2 != 1)
        {
            throw new InvalidDataException(
                $"Shitroid never released Samus: {shitroid.Function}, " +
                $"parameter2=${shitroidSlot.Parameter2:X4}.");
        }

        // The definition contains `$EFBA`, but its retail vulnerability byte rejects the
        // power bomb before private dispatch. Preserve that outer bank-$A0 admission rule;
        // merely having a callback pointer must not make the explosion effective.
        ushort propertiesBeforePowerBomb = shitroidSlot.Properties;
        int shitroidPowerReactions = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            shitroidSlot.XPosition,
            shitroidSlot.YPosition,
            explosionRadius: 32,
            loaded.Samus);
        if (shitroidPowerReactions != 0 ||
            shitroid.Function != ShitroidAiFunction.ReleasedFollow ||
            shitroidSlot.Properties != propertiesBeforePowerBomb)
        {
            throw new InvalidDataException(
                $"Shitroid released power-bomb mismatch: reactions={shitroidPowerReactions}, " +
                $"function={shitroid.Function}, properties=${shitroidSlot.Properties:X4}.");
        }

        // A beam/missile hit is admitted and function 26 changes ReleasedFollow to
        // BeginExit. Unlike `$EFBA`, `$F842` does not tail-call main, so Exit appears on the
        // following scheduled frame rather than inside the collision callback.
        ArmProjectile(loaded.Shots.Slots[0], shitroidSlot);
        int releasedShotHits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            loaded.Shots,
            loaded.SharedProjectiles,
            loaded.Samus);
        if (releasedShotHits != 1 || shitroid.Function != ShitroidAiFunction.BeginExit)
        {
            throw new InvalidDataException(
                $"Released Shitroid shot mismatch: hits={releasedShotHits}, " +
                $"function={shitroid.Function}.");
        }
        StepAndRecord(
            bus,
            assets.LevelData,
            loaded,
            frameCounter++,
            visitedShitroidStates,
            ref sawCloseWall,
            ref sawOpenWall,
            ref sawEntranceMusic);
        if (shitroid.Function != ShitroidAiFunction.Exit)
            throw new InvalidDataException($"Shitroid exit setup selected {shitroid.Function}.");

        while (shitroid.Function != ShitroidAiFunction.Dormant && frameCounter < 9000)
        {
            StepAndRecord(
                bus,
                assets.LevelData,
                loaded,
                frameCounter++,
                visitedShitroidStates,
                ref sawCloseWall,
                ref sawOpenWall,
                ref sawEntranceMusic);
        }
        if (shitroid.Function != ShitroidAiFunction.Dormant ||
            (shitroidSlot.Properties & 0x2100) != 0)
        {
            throw new InvalidDataException(
                $"Shitroid exit incomplete: {shitroid.Function}, " +
                $"properties=${shitroidSlot.Properties:X4}.");
        }

        VerifySidehopperDecomposition(
            bus,
            assets.LevelData,
            loaded,
            victim,
            alternateSidehopper,
            ref frameCounter);
        VerifyOtherCorpseDecomposition(bus, assets.LevelData, loaded, ref frameCounter);

        ShitroidAiFunction[] requiredVisibleStates =
        [
            ShitroidAiFunction.BeginEntranceDelay,
            ShitroidAiFunction.EntranceDelay,
            ShitroidAiFunction.FlyToSidehopper,
            ShitroidAiFunction.ChaseSidehopper,
            ShitroidAiFunction.AttachToSidehopper,
            ShitroidAiFunction.DrainSidehopper,
            ShitroidAiFunction.RiseAfterFeeding,
            ShitroidAiFunction.HoverNearSamus,
            ShitroidAiFunction.ChaseSamus,
            ShitroidAiFunction.DrainSamus,
            ShitroidAiFunction.PostDrainPause,
            ShitroidAiFunction.RiseAfterDrainingSamus,
            ShitroidAiFunction.FlyLeft,
            ShitroidAiFunction.FlyRight,
            ShitroidAiFunction.HoldSamusBeforeRelease,
            ShitroidAiFunction.ReleasedFollow,
            ShitroidAiFunction.Exit,
            ShitroidAiFunction.Dormant,
        ];
        ShitroidAiFunction[] missing = requiredVisibleStates
            .Where(state => !visitedShitroidStates.Contains(state))
            .ToArray();
        if (missing.Length != 0)
            throw new InvalidDataException($"Shitroid lifecycle missed states: {string.Join(',', missing)}.");

        Console.WriteLine(
            $"Shitroid audit passed in {frameCounter} actor frames: the exact eleven-entry " +
            "retail population loaded; live sidehopper damage, Shitroid entrance/feed/" +
            "palette/chase/latch/drain/release/recoil/power-bomb immunity/exit, both wall " +
            "requests, all ten corpse graphics variants, solid contact, touch/shot " +
            "callbacks and retail power-bomb immunity, " +
            "shared row scheduling, dust, VRAM transfers, and terminal states completed.");
        return 0;
    }

    private static void VerifyPopulationAndHeaders(ISnesAddressSpace bus, CartridgeRoomHeader room)
    {
        ushort cursor = room.State.EnemyPopulationPointer;
        for (int slotIndex = 0; slotIndex < ExpectedPopulation.Length; slotIndex++)
        {
            PopulationExpectation expected = ExpectedPopulation[slotIndex];
            ushort definitionPointer = ReadWord(bus, PopulationBank | cursor);
            ushort parameter1 = ReadWord(bus, PopulationBank | unchecked((ushort)(cursor + 12)));
            ushort parameter2 = ReadWord(bus, PopulationBank | unchecked((ushort)(cursor + 14)));
            RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, definitionPointer);
            if (definitionPointer != expected.DefinitionPointer ||
                parameter1 != expected.Parameter1 || parameter2 != expected.Parameter2 ||
                definition.InitializationAiPointer != expected.InitializationAi ||
                definition.MainAiPointer != expected.MainAi ||
                definition.TouchAiPointer != expected.TouchAi ||
                definition.ShotAiPointer != expected.ShotAi ||
                definition.PowerBombReactionPointer != expected.PowerBombAi)
            {
                throw new InvalidDataException(
                    $"Shitroid population slot {slotIndex} mismatch: def/params=" +
                    $"${definitionPointer:X4}/${parameter1:X4}/${parameter2:X4}, " +
                    $"init/main=${definition.InitializationAiPointer:X4}/" +
                    $"${definition.MainAiPointer:X4}, callbacks=" +
                    $"${definition.TouchAiPointer:X4}/${definition.ShotAiPointer:X4}/" +
                    $"${definition.PowerBombReactionPointer:X4}.");
            }
            cursor = unchecked((ushort)(cursor + 16));
        }
        if (ReadWord(bus, PopulationBank | cursor) != 0xffff)
            throw new InvalidDataException("Shitroid population has unexpected trailing actors.");
    }

    private static void VerifyInitialization(
        ISnesAddressSpace bus,
        LoadedShitroidEncounter loaded,
        ShitroidEnemyState shitroid,
        DeadSidehopperEnemyState victim,
        DeadSidehopperEnemyState alternate)
    {
        RoomEnemySlot slot = shitroid.Slot;
        if (loaded.Enemies.EnemyCount != ExpectedPopulation.Length ||
            slot.CurrentInstruction != 0xf90e || slot.PaletteIndex != 0x0400 ||
            (slot.Properties & 0x3000) != 0x3000 ||
            shitroid.Function != ShitroidAiFunction.WaitForCamera ||
            shitroid.XVelocity != 0 || shitroid.YVelocity != 0 ||
            shitroid.PaletteDelay != 10)
        {
            throw new InvalidDataException(
                $"Shitroid initialization mismatch: count={loaded.Enemies.EnemyCount}, " +
                $"list/palette/properties=${slot.CurrentInstruction:X4}/" +
                $"${slot.PaletteIndex:X4}/${slot.Properties:X4}, function={shitroid.Function}, " +
                $"velocity=${shitroid.XVelocity:X4}/${shitroid.YVelocity:X4}, " +
                $"delay={shitroid.PaletteDelay}.");
        }
        VerifyPaletteRange(bus, shitroid.TargetPalette.Span, 0x90, 0xa9f8c6);
        VerifyPaletteRange(bus, shitroid.TargetPalette.Span, 0xa0, 0xa9f8e6);
        VerifyPaletteRange(bus, shitroid.TargetPalette.Span, 0xf0, 0xa9f8a6);

        if (victim.ConfigurationPointer != 0xdd68 || victim.CopyFunction != 0xe4f5 ||
            victim.MoveFunction != 0xe468 || victim.EntryCount != 40 ||
            victim.Function != DeadSidehopperAiFunction.AliveWaitForCamera ||
            victim.HorizontalVelocity != 96 || victim.VerticalVelocity != 256 ||
            victim.Slot.XPosition != 488 || victim.Slot.YPosition != 184 ||
            victim.Slot.YRadius != 21 || victim.Slot.PaletteIndex != 0x0200 ||
            alternate.ConfigurationPointer != 0xdd78 || alternate.CopyFunction != 0xe5f6 ||
            alternate.MoveFunction != 0xe564 || alternate.EntryCount != 40 ||
            alternate.PaletteStage != 0xffff ||
            alternate.Function != DeadSidehopperAiFunction.WaitForSamusCollision ||
            alternate.Slot.PaletteIndex != 0x0e00)
        {
            throw new InvalidDataException(
                $"Sidehopper initialization mismatch: victim config/callbacks/entries=" +
                $"${victim.ConfigurationPointer:X4}/${victim.CopyFunction:X4}/" +
                $"${victim.MoveFunction:X4}/{victim.EntryCount}, state={victim.Function}, " +
                $"alternate=${alternate.ConfigurationPointer:X4}/" +
                $"${alternate.CopyFunction:X4}/${alternate.MoveFunction:X4}/" +
                $"{alternate.EntryCount}, stage/state={alternate.PaletteStage}/" +
                $"{alternate.Function}.");
        }

        foreach (DeadSidehopperEnemyState state in loaded.Enemies.DeadSidehoppers
                     .Where(state => state is not null).Cast<DeadSidehopperEnemyState>())
            VerifyRotTableEndpoints(bus, state.TablePointer, state.EntryCount);

        foreach (DeadTourianCorpseEnemyState state in loaded.Enemies.DeadTourianCorpses
                     .Where(state => state is not null).Cast<DeadTourianCorpseEnemyState>())
        {
            ushort expectedRows = state.Species == DeadTourianCorpseSpecies.Skree
                ? (ushort)32
                : (ushort)16;
            if (state.EntryCount != expectedRows || state.VariantIndex != state.Slot.Parameter1 / 2)
            {
                throw new InvalidDataException(
                    $"Dead {state.Species} variant/row mismatch: " +
                    $"{state.VariantIndex}/{state.EntryCount}.");
            }
            VerifyRotTableEndpoints(bus, state.TablePointer, state.EntryCount);
            foreach (RoomEnemySystem.DeadTourianCorpseGraphicsCopy copy in
                     state.Variant.InitialGraphicsCopies)
                VerifyGraphicsCopy(bus, copy.SourceOffset, copy.DestinationOffset, copy.Length);
        }
        VerifyGraphicsCopy(bus, sourceOffset: 0x0040, destinationOffset: 0x0040, length: 0x0060);
        VerifyGraphicsCopy(bus, sourceOffset: 0x0120, destinationOffset: 0x0320, length: 0x0040);
    }

    private static void VerifySidehopperDecomposition(
        SuperMetroidAddressSpace bus,
        RoomLevelData level,
        LoadedShitroidEncounter loaded,
        DeadSidehopperEnemyState victim,
        DeadSidehopperEnemyState alternate,
        ref int frameCounter)
    {
        ArmProjectile(loaded.Shots.Slots[0], victim.Slot);
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            loaded.Shots,
            loaded.SharedProjectiles,
            loaded.Samus);
        if (hits != 1 || victim.Function != DeadSidehopperAiFunction.Rotting)
            throw new InvalidDataException($"Sidehopper corpse shot did not start rot: {hits}/{victim.Function}.");
        bool sawTransfers = RunUntil(
            bus,
            level,
            loaded,
            ref frameCounter,
            2000,
            () => victim.Function == DeadSidehopperAiFunction.WaitForSamusCollision &&
                victim.FinishedEntryCount == victim.EntryCount);
        if (!sawTransfers || victim.DustSpawnCount != victim.EntryCount ||
            victim.LastFinishedEntryIndex != victim.EntryCount - 1)
        {
            throw new InvalidDataException(
                $"Sidehopper shot decomposition mismatch: finished/dust=" +
                $"{victim.FinishedEntryCount}/{victim.DustSpawnCount}, " +
                $"last={victim.LastFinishedEntryIndex}, transfers={sawTransfers}.");
        }

        loaded.Samus.Kinematics.RecordSolidEnemyCollision(
            SamusCollisionDirection.Down,
            alternate.Slot.NativeIndex);
        ushort alternateCameraX = unchecked((ushort)Math.Max(
            0,
            alternate.Slot.XPosition - 128));
        Step(bus, level, loaded, frameCounter++, alternateCameraX);
        if (alternate.Function != DeadSidehopperAiFunction.PreRotDelay)
            throw new InvalidDataException($"Alternate sidehopper solid contact selected {alternate.Function}.");
        for (int delay = 0; delay < 16; delay++)
            Step(bus, level, loaded, frameCounter++, alternateCameraX);
        if (alternate.Function != DeadSidehopperAiFunction.Rotting)
            throw new InvalidDataException($"Alternate sidehopper delay selected {alternate.Function}.");
        sawTransfers = RunUntil(
            bus,
            level,
            loaded,
            ref frameCounter,
            2000,
            () => alternate.Function == DeadSidehopperAiFunction.WaitForSamusCollision &&
                alternate.FinishedEntryCount == alternate.EntryCount,
            alternateCameraX);
        if (!sawTransfers || alternate.DustSpawnCount != alternate.EntryCount)
            throw new InvalidDataException("Alternate sidehopper decomposition did not complete.");
    }

    private static void VerifyOtherCorpseDecomposition(
        SuperMetroidAddressSpace bus,
        RoomLevelData level,
        LoadedShitroidEncounter loaded,
        ref int frameCounter)
    {
        DeadTourianCorpseEnemyState[] corpses = loaded.Enemies.DeadTourianCorpses
            .Where(state => state is not null)
            .Cast<DeadTourianCorpseEnemyState>()
            .ToArray();
        DeadTourianCorpseEnemyState touchTarget = corpses.First(state =>
            state.Species == DeadTourianCorpseSpecies.Ripper);
        ushort[] savedProperties = loaded.Enemies.Slots.Select(slot => slot.Properties).ToArray();
        foreach (RoomEnemySlot slot in loaded.Enemies.Slots)
        {
            if (!ReferenceEquals(slot, touchTarget.Slot) && slot.EnemyDefinitionPointer != 0)
                slot.Properties = unchecked((ushort)(slot.Properties | 0x0400));
        }
        // Enemy/Samus collision only considers the active enemy list assembled by the
        // preceding enemy frame.  Centre that frame on the authored corpse rather than
        // accidentally depending on the Shitroid encounter's default camera window.
        Step(bus, level, loaded, frameCounter++, CameraXFor(touchTarget.Slot));
        loaded.Samus.XPosition = touchTarget.Slot.XPosition;
        loaded.Samus.YPosition = touchTarget.Slot.YPosition;
        ushort healthBeforeTouch = loaded.Samus.Health;
        bool touched = loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0, level);
        if (!touched || touchTarget.Slot.VariableA != 0xdae6 ||
            loaded.Samus.Health != healthBeforeTouch)
        {
            throw new InvalidDataException(
                $"Dead Ripper touch mismatch: touched={touched}, " +
                $"function=${touchTarget.Slot.VariableA:X4}, " +
                $"health={healthBeforeTouch}->{loaded.Samus.Health}.");
        }
        for (int index = 0; index < savedProperties.Length; index++)
        {
            if (!ReferenceEquals(loaded.Enemies.Slots[index], touchTarget.Slot))
                loaded.Enemies.Slots[index].Properties = savedProperties[index];
        }
        Step(bus, level, loaded, frameCounter++);

        DeadTourianCorpseEnemyState powerTarget = corpses.First(state =>
            state.Species == DeadTourianCorpseSpecies.Skree);
        int powerReactions = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            powerTarget.Slot.XPosition,
            powerTarget.Slot.YPosition,
            explosionRadius: 12,
            loaded.Samus);
        // The header contains private power callback $DD0D, but its retail vulnerability
        // table $F12E has a zero power-bomb byte. Bank-$A0 therefore rejects the explosion
        // before dispatch, and the corpse must remain in its wait state. Keeping this check
        // here prevents a tempting but inaccurate direct invocation of an unreachable path.
        if (powerReactions != 0 || powerTarget.Slot.VariableA != 0xda6e)
        {
            throw new InvalidDataException(
                $"Dead Skree power-bomb immunity mismatch: reactions={powerReactions}, " +
                $"function=${powerTarget.Slot.VariableA:X4}.");
        }

        foreach (DeadTourianCorpseEnemyState state in corpses)
        {
            if (state.Slot.VariableA == state.Profile.RottingFunction)
                continue;

            // Projectile collision uses the same active list as Samus contact.  Each
            // corpse is deliberately tested at its ROM-authored position, so rebuild
            // that list around the individual target before firing the synthetic shot.
            Step(bus, level, loaded, frameCounter++, CameraXFor(state.Slot));
            ArmProjectile(loaded.Shots.Slots[0], state.Slot);
            int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
                bus,
                loaded.Shots,
                loaded.SharedProjectiles,
                loaded.Samus);
            if (hits == 0 || state.Slot.VariableA != state.Profile.RottingFunction)
            {
                throw new InvalidDataException(
                    $"Dead {state.Species} variant {state.VariantIndex} shot did not start rot.");
            }
        }

        bool sawTransfers = RunUntil(
            bus,
            level,
            loaded,
            ref frameCounter,
            2000,
            () => corpses.All(state =>
                state.Slot.VariableA == 0xda63 &&
                state.FinishedEntryCount == state.EntryCount));
        foreach (DeadTourianCorpseEnemyState state in corpses)
        {
            if (state.DustSpawnCount != state.EntryCount ||
                state.LastFinishedEntryIndex != state.EntryCount - 1)
            {
                throw new InvalidDataException(
                    $"Dead {state.Species} variant {state.VariantIndex} decomposition " +
                    $"finished/dust/last={state.FinishedEntryCount}/" +
                    $"{state.DustSpawnCount}/{state.LastFinishedEntryIndex}.");
            }
        }
        if (!sawTransfers)
            throw new InvalidDataException("Dead Tourian corpse set emitted no VRAM transfers.");
    }

    private static LoadedShitroidEncounter Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 0x0260,
            YPosition = 0x00a0,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        var random = new Bank80SystemState();
        var scrollBytes = new byte[Math.Max(4, room.WidthInScreens * room.HeightInScreens)];
        var enemies = new RoomEnemySystem();
        var vram = new SnesVram();
        var cgram = new SnesCgram();
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
            isAreaBossDefeated: () => false,
            hasEvent: _ => false,
            setEvent: _ => { },
            clearEvent: _ => { },
            isAreaMiniBossDefeated: () => false,
            setAreaMiniBossDefeated: () => { },
            isAreaTorizoDefeated: () => false,
            setAreaTorizoDefeated: () => { },
            isRoomPlmPresent: _ => false,
            setSamusControlsEnabled: _ => { },
            setRoomScrollByte: (index, value) =>
            {
                if ((uint)index < (uint)scrollBytes.Length)
                    scrollBytes[index] = value;
            },
            setAreaBossDefeated: () => { },
            incrementMotherBrainGlassRoomArgument: () => { },
            readRoomScrollByte: index =>
                (uint)index < (uint)scrollBytes.Length ? scrollBytes[index] : (byte)0,
            setMotherBrainLayerBlendingDefaultConfig: _ => { },
            setMotherBrainBg2Scroll: (_, _) => { },
            cameraX: EncounterCameraX);
        return new LoadedShitroidEncounter(
            enemies,
            samus,
            new SamusProjectileSystem(),
            new SamusBombProjectileSystem(),
            vram,
            new VramWriteQueue(),
            scrollBytes);
    }

    private static void Step(
        SuperMetroidAddressSpace bus,
        RoomLevelData level,
        LoadedShitroidEncounter loaded,
        int frame,
        ushort cameraX = EncounterCameraX)
    {
        loaded.VramWrites.DrainTo(loaded.Vram, bus);
        loaded.Enemies.StepFrame(
            cameraX,
            cameraY: 0,
            timeIsFrozen: false,
            loaded.Samus,
            level: level,
            samusProjectiles: loaded.Shots,
            nmiFrameCounter8: unchecked((byte)frame),
            sharedProjectiles: loaded.SharedProjectiles,
            vramWriteQueue: loaded.VramWrites);
    }

    private static void StepAndRecord(
        SuperMetroidAddressSpace bus,
        RoomLevelData level,
        LoadedShitroidEncounter loaded,
        int frame,
        ISet<ShitroidAiFunction> visited,
        ref bool sawCloseWall,
        ref bool sawOpenWall,
        ref bool sawEntranceMusic)
    {
        ShitroidEnemyState state = loaded.Enemies.Shitroid!;
        visited.Add(state.Function);
        Step(bus, level, loaded, frame);
        visited.Add(state.Function);
        sawCloseWall |= loaded.Enemies.ShitroidPlmRequests.Any(request => request.Header == 0xb767);
        sawOpenWall |= loaded.Enemies.ShitroidPlmRequests.Any(request => request.Header == 0xb763);
        sawEntranceMusic |= loaded.Enemies.LastShitroidMusicRequest is { Track: 5, DelayFrames: 8 };
    }

    private static bool RunUntil(
        SuperMetroidAddressSpace bus,
        RoomLevelData level,
        LoadedShitroidEncounter loaded,
        ref int frameCounter,
        int maximumFrames,
        Func<bool> completed,
        ushort cameraX = EncounterCameraX)
    {
        bool sawTransfers = false;
        int endFrame = frameCounter + maximumFrames;
        while (!completed() && frameCounter < endFrame)
        {
            Step(bus, level, loaded, frameCounter++, cameraX);
            sawTransfers |= loaded.Enemies.LastDeadSidehopperVramTransfers.Count != 0;
        }
        if (!completed())
            throw new InvalidDataException($"Corpse lifecycle exceeded {maximumFrames} frames.");
        return sawTransfers;
    }

    private static void ArmProjectile(SamusProjectileSlot projectile, RoomEnemySlot target)
    {
        projectile.ClearFields();
        projectile.Type = (ushort)SamusProjectileFamily.Missile;
        projectile.Damage = 100;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 8;
        projectile.YRadius = 8;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static ushort CameraXFor(RoomEnemySlot slot) =>
        unchecked((ushort)Math.Max(0, slot.XPosition - 128));

    private static void PositionSamusAtShitroidLatchTarget(SamusState samus, RoomEnemySlot shitroid)
    {
        samus.XPosition = shitroid.XPosition;
        samus.YPosition = unchecked((ushort)(shitroid.YPosition + 32));
    }

    private static void VerifyPaletteRange(
        ISnesAddressSpace bus,
        ReadOnlySpan<ushort> palette,
        int destinationColor,
        int sourceAddress)
    {
        for (int color = 0; color < 16; color++)
        {
            ushort expected = ReadWord(bus, sourceAddress + color * 2);
            if (palette[destinationColor + color] != expected)
            {
                throw new InvalidDataException(
                    $"Shitroid target palette color ${destinationColor + color:X2} " +
                    $"is ${palette[destinationColor + color]:X4}, expected ${expected:X4}.");
            }
        }
    }

    private static void VerifyRotTableEndpoints(
        ISnesAddressSpace bus,
        ushort tablePointer,
        ushort entryCount)
    {
        CorpseRottingTableEntry first = CorpseRottingTableProcessor.ReadEntry(
            bus,
            0x7e0000 | tablePointer,
            entryCount,
            0);
        CorpseRottingTableEntry last = CorpseRottingTableProcessor.ReadEntry(
            bus,
            0x7e0000 | tablePointer,
            entryCount,
            entryCount - 1);
        if (first.YOffset != entryCount - 1 || first.Timer != 0 ||
            last.YOffset != 0 || last.Timer != 2 * (entryCount - 1))
        {
            throw new InvalidDataException(
                $"Corpse table ${tablePointer:X4} endpoints mismatch: {first}/{last}.");
        }
    }

    private static void VerifyGraphicsCopy(
        ISnesAddressSpace bus,
        int sourceOffset,
        int destinationOffset,
        int length)
    {
        for (int byteIndex = 0; byteIndex < length; byteIndex++)
        {
            byte expected = bus.ReadByte(0xb7c000 + sourceOffset + byteIndex);
            byte actual = bus.ReadByte(0x7e2000 + destinationOffset + byteIndex);
            if (actual != expected)
            {
                throw new InvalidDataException(
                    $"Dead-monster graphics copy ${sourceOffset:X4}->${destinationOffset:X4} " +
                    $"diverged at ${byteIndex:X3}: ${actual:X2} != ${expected:X2}.");
            }
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct PopulationExpectation(
        ushort DefinitionPointer,
        ushort Parameter1,
        ushort Parameter2,
        ushort InitializationAi,
        ushort MainAi,
        ushort TouchAi,
        ushort ShotAi,
        ushort PowerBombAi);

    private sealed record LoadedShitroidEncounter(
        RoomEnemySystem Enemies,
        SamusState Samus,
        SamusProjectileSystem Shots,
        SamusBombProjectileSystem SharedProjectiles,
        SnesVram Vram,
        VramWriteQueue VramWrites,
        byte[] ScrollBytes);
}

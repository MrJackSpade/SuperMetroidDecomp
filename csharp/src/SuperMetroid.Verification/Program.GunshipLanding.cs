using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Drives the three ordinary enemy slots used by the post-Ceres gunship from the
    /// special station-eighteen initializer through descent, bounce, pad animation waits,
    /// Samus lift, and the final ordinary-control handoff.
    /// </summary>
    static void VerifyPostCeresGunshipLanding(bool entrySoundOnly = false)
    {
        const EnemyDefinitionId topDefinition = EnemyDefinitionId.ShipTop;
        const EnemyDefinitionId bottomDefinition = EnemyDefinitionId.ShipBottomEntrance;
        const ushort populationPointer = 0x9200;
        const ushort tilesetPointer = 0x9200;

        var bus = new TestAddressSpace();
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        bool zebesTimebombSet = false;

        SeedPostCeresGunshipCartridge(bus, topDefinition, bottomDefinition, populationPointer, tilesetPointer);

        var samus = new SamusState
        {
            // Load station eighteen places Samus at $007C before the initializer moves
            // the top to SamusY-17. The ordinary station-zero deck coordinate $0440 is
            // produced only after the complete descent and lift sequence.
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x0480, 0x0080)),
            InputLocked = true,
        };
        var enemies = new RoomEnemySystem();
        samus.TileTransfers.BindArtwork(
            RepositoryInstallation.SamusBody);
        enemies.Load(new RoomEnemyLoadRequest(new GunshipMotionFixtureGuard(bus), populationPointer, tilesetPointer, vram, cgram, new StandaloneRoomEnemyServices(() => 0) { Events = new FixtureEventFlags { HasEvent = eventNumber =>
                zebesTimebombSet && eventNumber == EventNumber.ZebesTimebombSet } }) { Samus = samus, GunshipScenario = GunshipLoadScenario.EscapingCeres });

        RoomEnemySlot top = enemies.Slots[0];
        RoomEnemySlot bottom = enemies.Slots[1];
        RoomEnemySlot pad = enemies.Slots[2];
        AssertEqual(0x006f, top.Position.Y.Whole, "post-Ceres top begins seventeen pixels above Samus");
        AssertEqual(0x0097, bottom.Position.Y.Whole, "post-Ceres bottom begins twenty-three pixels below Samus");
        AssertEqual(0xa80c, top.VariableF, "post-Ceres top selects descent function three");

        int frames = 0;
        bool observedSlowDescent = false;
        bool observedPadOpen = false;
        bool observedPadClose = false;
        bool observedEngineSound = false;
        bool observedLandingClamp = false;
        while (enemies.LastGunshipEvent != GunshipFrameEvent.LandingCompleted && frames < 800)
        {
            ushort previousTopY = top.Position.Y.Whole;
            ushort previousTopSubY = top.Position.Y.Fraction;
            ushort previousFunction = top.VariableF;
            ushort cameraY = samus.Position.Y.Whole > 120
                ? unchecked((ushort)(samus.Position.Y.Whole - 120))
                : (ushort)0;
            enemies.StepFrame(new RoomEnemyFrame { Camera = new(0x0400, cameraY), TimeIsFrozen = false, Samus = samus });
            frames++;
            observedEngineSound |= enemies.SoundRequests.Contains(
                new EnemySoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x4d), MaximumQueued: 6));

            if (top.VariableF == 0xa80c)
            {
                AssertEqual(
                    unchecked((ushort)(top.Position.Y.Whole + 17)),
                    samus.Position.Y.Whole,
                    $"gunship descent rigid Samus whole Y frame {frames}");
                AssertEqual(top.Position.Y.Fraction, samus.Kinematics.Position.Y.Fraction,
                    $"gunship descent rigid Samus sub-Y frame {frames}");
                if (previousTopY >= 0x0300)
                {
                    uint previous = ((uint)previousTopY << 16) | previousTopSubY;
                    uint current = top.Position.Y.RawValue;
                    AssertEqual(0x0002_8000u, current - previous,
                        "gunship descent slows to cartridge 2.8 fixed pixels");
                    observedSlowDescent = true;
                }
            }

            if (previousFunction == 0xa80c && top.VariableF != 0xa80c)
            {
                // $A2:A8B2 clamps whole Y positions only; each hull keeps this call's subpixel sum.
                ushort carriedSubY = unchecked((ushort)(previousTopSubY + 0x8000));
                AssertEqual((ushort)0x045f, top.Position.Y.Whole, "landing clamp top Y");
                AssertEqual(carriedSubY, top.Position.Y.Fraction, "landing clamp keeps top subpixel");
                AssertEqual(carriedSubY, bottom.Position.Y.Fraction, "landing clamp keeps bottom subpixel");
                AssertEqual(carriedSubY, pad.Position.Y.Fraction, "landing clamp keeps pad subpixel");
                observedLandingClamp = true;
            }

            observedPadOpen |= enemies.LastGunshipEvent == GunshipFrameEvent.LandingPadOpened;
            if (enemies.LastGunshipEvent == GunshipFrameEvent.LandingPadOpened)
            {
                // $A2:A91E-$A925 place Samus beside the hatch in both X words, so the
                // following MainScrollingRoutine measures no horizontal movement.
                var checkpoint = FixedPosition<RoomPixels>.FromWords(0x0480, 0x1234, samus.Position.Y.Whole, 0);
                AssertEqual(checkpoint.WithX(checkpoint.X.WithWhole(samus.Position.X.Whole)),
                    samus.ApplyPreviousPositionWrites(checkpoint),
                    "landing writes SamusPreviousXPosition with Samus X");
                AssertTrue(enemies.SoundRequests.Contains(new EnemySoundRequest(
                        SoundEffectLibrary3Sounds.GunshipEntrancePad, MaximumQueued: 6)),
                    "landing queues QueueSound_Lib3_Max6($14)");
                // EnemyMain installs $A5BE, then this same slot's ordinary instruction
                // phase consumes its first four-byte timed frame before StepFrame returns.
                AssertEqual(
                    unchecked((ushort)(
                        GunshipInstructionProgramDefinitions.EntrancePadOpening + 4)),
                    pad.CurrentInstruction,
                    "bounce completion advances the cartridge pad-open list once");
            }
            observedPadClose |= enemies.LastGunshipEvent == GunshipFrameEvent.LandingPadClosed;
            if (enemies.LastGunshipEvent == GunshipFrameEvent.LandingPadClosed)
            {
                AssertEqual(
                    unchecked((ushort)(
                        GunshipInstructionProgramDefinitions.EntrancePadClosing + 4)),
                    pad.CurrentInstruction,
                    "Samus lift advances the cartridge pad-close list once");
            }
        }

        AssertTrue(observedSlowDescent, "gunship crosses native slow-descent threshold");
        AssertTrue(observedLandingClamp, "gunship reaches the native landing clamp");
        AssertTrue(observedPadOpen, "gunship publishes pad-open boundary");
        AssertTrue(observedPadClose, "gunship publishes pad-close boundary");
        AssertTrue(observedEngineSound,
            "gunship bottom timer publishes QueueSfx2_Max6($4D)");
        AssertEqual(GunshipFrameEvent.LandingCompleted, enemies.LastGunshipEvent,
            "gunship completes post-Ceres landing");
        AssertEqual(641, frames, "gunship full landing frame count");
        AssertEqual(0xa9bd, top.VariableF, "gunship installs ordinary idle function");
        AssertTrue(!samus.InputLocked, "gunship completion restores Samus input handler");
        AssertEqual(0x0440, samus.Position.Y.Whole, "gunship raises Samus to deck standing Y");

        var openingSound = new EnemySoundRequest(
            SoundEffectLibrary3Sounds.GunshipEntrancePad, MaximumQueued: 6);
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0x0400, 0x0400), TimeIsFrozen = false, Samus = samus });
        AssertTrue(!enemies.SoundRequests.Contains(openingSound),
            "idle ship does not publish an entrance sound without Down");
        ushort deckX = samus.Position.X.Whole;
        samus.Position = samus.Position.WithWholeX(unchecked((ushort)(top.Position.X.Whole + 16)));
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0x0400, 0x0400), TimeIsFrozen = false, Samus = samus, NewlyPressedControllerInput = 0x0400 });
        AssertTrue(!enemies.SoundRequests.Contains(openingSound),
            "Down outside entrance does not publish an opening sound");
        samus.Position = samus.Position.WithWholeX(deckX);

        // Re-entering after Mother Brain's event takes the native function-17 branch at
        // RestoreSamusInGunship: there is no refill wait, save prompt, or ordinary exit.
        zebesTimebombSet = true;
        var takeoffWrites = new VramWriteQueue();
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0x0400, 0x0400), TimeIsFrozen = false, Samus = samus, NewlyPressedControllerInput = 0x0400, VramWriteQueue = takeoffWrites });
        AssertEqual(GunshipFrameEvent.EntryStarted, enemies.LastGunshipEvent,
            "endgame Down input begins ordinary gunship entry");
        AssertEqual(1, enemies.SoundRequests.Count(request => request == openingSound),
            "entry frame publishes exactly one library-three $14 Max6 sound");
        if (entrySoundOnly)
        {
            enemies.StepFrame(new RoomEnemyFrame { Camera = new(0x0400, 0x0400), TimeIsFrozen = false, Samus = samus, NewlyPressedControllerInput = 0x0400 });
            AssertTrue(!enemies.SoundRequests.Contains(openingSound),
                "opening animation does not republish the entry sound");
            Console.WriteLine("Gunship entry sound: exact entry edge, input/position guards and no repeat verified.");
            return;
        }

        int takeoffFrames = 0;
        bool observedTakeoffStart = false;
        int maximumLiftoffDustActors = 0;
        while (enemies.LastGunshipEvent != GunshipFrameEvent.EscapeTakeoffCompleted &&
               takeoffFrames < 900)
        {
            ushort takeoffCameraY = samus.Position.Y.Whole > 120
                ? unchecked((ushort)(samus.Position.Y.Whole - 120))
                : (ushort)0;
            enemies.StepFrame(new RoomEnemyFrame { Camera = new(0x0400, takeoffCameraY), TimeIsFrozen = false, Samus = samus, VramWriteQueue = takeoffWrites });
            observedTakeoffStart |=
                enemies.LastGunshipEvent == GunshipFrameEvent.EscapeTakeoffStarted;
            maximumLiftoffDustActors = Math.Max(
                maximumLiftoffDustActors,
                enemies.EnemyProjectiles.Count(projectile =>
                    projectile.IsActive &&
                    projectile.Kind == RoomEnemyProjectileKind.GunshipLiftoffDustCloud));
            takeoffFrames++;
        }

        AssertTrue(observedTakeoffStart,
            "event $0E bypasses restoration and publishes takeoff start");
        AssertEqual(GunshipFrameEvent.EscapeTakeoffCompleted, enemies.LastGunshipEvent,
            "accelerating gunship publishes frontend state-$26 boundary");
        AssertEqual(5, takeoffWrites.Entries.Count,
            "takeoff queues all five cartridge dust-cloud tile chunks");
        AssertEqual(6, maximumLiftoffDustActors,
            "takeoff frame 64 spawns all six room-graphics dust actors");
        AssertTrue(top.Position.Y.Whole < 0x0100,
            "state-$26 boundary occurs after top hull passes Y=$0100");

        Console.WriteLine(
            "  Gunship landing: station-18 descent, rigid camera carrier, bounce, pad, " +
            "Samus lift, event-$0E takeoff, and state-$26 handoff agree.");
    }

    [CpuBusBoundary("exact bus test: seeds the constructed cartridge population's $FFFF terminator and its kill-quota byte")]
    private static void WriteGunshipPopulationTerminator(TestAddressSpace bus, int address)
    {
        WriteTestWord(bus, address, 0xffff);
        bus.WriteByte(address + 2, 0);
    }

    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for one gunship enemy population record")]
    private static void WriteGunshipPopulationRecord(
        TestAddressSpace bus,
        int address,
        EnemyDefinitionId definition,
        Position16<RoomPixels> position,
        ushort parameter2)
    {
        WriteTestWord(bus, address, (ushort)definition);
        WriteTestWord(bus, address + 2, position.X);
        WriteTestWord(bus, address + 4, position.Y);
        WriteTestWord(bus, address + 6, 0);
        WriteTestWord(bus, address + 8, 0);
        WriteTestWord(bus, address + 10, 0);
        WriteTestWord(bus, address + 12, 0);
        WriteTestWord(bus, address + 14, parameter2);
    }

    /// <summary>
    /// Seeds the two gunship enemy definitions, their presentation-owned spritemap operands, the
    /// dust-cloud upload table, an empty tileset and the three-slot gunship population.
    /// </summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for the post-Ceres gunship landing enemies")]
    private static void SeedPostCeresGunshipCartridge(
        TestAddressSpace bus,
        EnemyDefinitionId topDefinition,
        EnemyDefinitionId bottomDefinition,
        ushort populationPointer,
        ushort tilesetPointer)
    {
        WriteEnemyDefinition(
            bus,
            topDefinition,
            tileDataSize: 0,
            palettePointer: 0x9000,
            bank: 0xa2,
            tileDataAddress: 0xa28000,
            bossId: 0,
            namePointer: 0,
            fieldSeed: 0x2100);
        SeedEnemyDefinitionWord(bus, topDefinition, 8, 32);
        SeedEnemyDefinitionWord(bus, topDefinition, 10, 16);
        SeedEnemyDefinitionWord(bus, topDefinition, 18, 0xa644);
        SeedEnemyDefinitionWord(bus, topDefinition, 24, 0xa759);

        WriteEnemyDefinition(
            bus,
            bottomDefinition,
            tileDataSize: 0,
            palettePointer: 0x9000,
            bank: 0xa2,
            tileDataAddress: 0xa28000,
            bossId: 0,
            namePointer: 0,
            fieldSeed: 0x2200);
        SeedEnemyDefinitionWord(bus, bottomDefinition, 8, 32);
        SeedEnemyDefinitionWord(bus, bottomDefinition, 10, 16);
        SeedEnemyDefinitionWord(bus, bottomDefinition, 18, 0xa6d2);
        SeedEnemyDefinitionWord(bus, bottomDefinition, 24, 0x804c);

        // Gunship control words are compiled engine data. This synthetic address space
        // supplies only the interleaved spritemap operands that remain presentation-owned.
        for (int index = 0;
             index < GunshipInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            WriteTestWord(
                bus,
                0xa20000 |
                    GunshipInstructionProgramDefinitions.PresentationWordAddress(index),
                0xa700);
        }

        // Function 17 uploads five consecutive $400-byte dust-cloud chunks from bank $94.
        for (int index = 0; index < 5; index++)
        {
            WriteTestWord(bus, 0xa2ac07 + index * 2, unchecked((ushort)(0x8000 + index * 0x0400)));
            WriteTestWord(bus, 0xa2ac11 + index * 2, unchecked((ushort)(0x7600 + index * 0x0200)));
        }

        WriteTestWord(bus, 0xb40000 | tilesetPointer, 0xffff);
        int population = 0xa10000 | populationPointer;
        var gunship = new Position16<RoomPixels>(0x0480, 0x0200);
        WriteGunshipPopulationRecord(bus, population, topDefinition, gunship, parameter2: 0);
        WriteGunshipPopulationRecord(bus, population + 16, bottomDefinition, gunship, parameter2: 0);
        WriteGunshipPopulationRecord(bus, population + 32, bottomDefinition, gunship, parameter2: 1);
        WriteGunshipPopulationTerminator(bus, population + 48);
    }
}

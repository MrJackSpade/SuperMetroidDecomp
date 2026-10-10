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
        const ushort topDefinition = GunshipEnemyDefinitions.Top;
        const ushort bottomDefinition = GunshipEnemyDefinitions.BottomEntrance;
        const ushort populationPointer = 0x9200;
        const ushort tilesetPointer = 0x9200;

        var bus = new TestAddressSpace();
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        bool zebesTimebombSet = false;

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
        WriteWord(bus, 0xa00000 | (topDefinition + 8), 32);
        WriteWord(bus, 0xa00000 | (topDefinition + 10), 16);
        WriteWord(bus, 0xa00000 | (topDefinition + 18), 0xa644);
        WriteWord(bus, 0xa00000 | (topDefinition + 24), 0xa759);

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
        WriteWord(bus, 0xa00000 | (bottomDefinition + 8), 32);
        WriteWord(bus, 0xa00000 | (bottomDefinition + 10), 16);
        WriteWord(bus, 0xa00000 | (bottomDefinition + 18), 0xa6d2);
        WriteWord(bus, 0xa00000 | (bottomDefinition + 24), 0x804c);

        // Gunship control words are compiled engine data. This synthetic address space
        // supplies only the interleaved spritemap operands that remain presentation-owned.
        for (int index = 0;
             index < GunshipInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            WriteWord(
                bus,
                0xa20000 |
                    GunshipInstructionProgramDefinitions.PresentationWordAddress(index),
                0xa700);
        }

        // Function 17 uploads five consecutive $400-byte dust-cloud chunks from bank $94.
        for (int index = 0; index < 5; index++)
        {
            WriteWord(bus, 0xa2ac07 + index * 2, unchecked((ushort)(0x8000 + index * 0x0400)));
            WriteWord(bus, 0xa2ac11 + index * 2, unchecked((ushort)(0x7600 + index * 0x0200)));
        }

        WriteWord(bus, 0xb40000 | tilesetPointer, 0xffff);
        int population = 0xa10000 | populationPointer;
        WriteGunshipPopulationRecord(bus, population, topDefinition, 0x0480, 0x0200, parameter2: 0);
        WriteGunshipPopulationRecord(bus, population + 16, bottomDefinition, 0x0480, 0x0200, parameter2: 0);
        WriteGunshipPopulationRecord(bus, population + 32, bottomDefinition, 0x0480, 0x0200, parameter2: 1);
        WriteWord(bus, population + 48, 0xffff);
        bus.WriteByte(population + 50, 0);

        var samus = new SamusState
        {
            XPosition = 0x0480,
            // Load station eighteen places Samus at $007C before the initializer moves
            // the top to SamusY-17. The ordinary station-zero deck coordinate $0440 is
            // produced only after the complete descent and lift sequence.
            YPosition = 0x0080,
            InputLocked = true,
        };
        var enemies = new RoomEnemySystem();
        samus.TileTransfers.BindArtwork(
            RepositoryInstallation.SamusBody);
        enemies.Load(
            new GunshipMotionDefinitionReadGuard(bus),
            populationPointer,
            tilesetPointer,
            vram,
            cgram,
            () => 0,
            samus: samus,
            hasEvent: eventNumber =>
                zebesTimebombSet && eventNumber == EventNumber.ZebesTimebombSet,
            gunshipLoadScenario: GunshipLoadScenario.EscapingCeres);

        RoomEnemySlot top = enemies.Slots[0];
        RoomEnemySlot bottom = enemies.Slots[1];
        RoomEnemySlot pad = enemies.Slots[2];
        AssertEqual(0x006f, top.YPosition, "post-Ceres top begins seventeen pixels above Samus");
        AssertEqual(0x0097, bottom.YPosition, "post-Ceres bottom begins twenty-three pixels below Samus");
        AssertEqual(0xa80c, top.VariableF, "post-Ceres top selects descent function three");

        int frames = 0;
        bool observedSlowDescent = false;
        bool observedPadOpen = false;
        bool observedPadClose = false;
        bool observedEngineSound = false;
        bool observedLandingClamp = false;
        while (enemies.LastGunshipEvent != GunshipFrameEvent.LandingCompleted && frames < 800)
        {
            ushort previousTopY = top.YPosition;
            ushort previousTopSubY = top.YSubposition;
            ushort previousFunction = top.VariableF;
            ushort cameraY = samus.YPosition > 120
                ? unchecked((ushort)(samus.YPosition - 120))
                : (ushort)0;
            enemies.StepFrame(cameraX: 0x0400, cameraY, timeIsFrozen: false, samus);
            frames++;
            observedEngineSound |= enemies.SoundRequests.Contains(
                new EnemySoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x4d), MaximumQueued: 6));

            if (top.VariableF == 0xa80c)
            {
                AssertEqual(
                    unchecked((ushort)(top.YPosition + 17)),
                    samus.YPosition,
                    $"gunship descent rigid Samus whole Y frame {frames}");
                AssertEqual(top.YSubposition, samus.Kinematics.YSubposition,
                    $"gunship descent rigid Samus sub-Y frame {frames}");
                if (previousTopY >= 0x0300)
                {
                    uint previous = ((uint)previousTopY << 16) | previousTopSubY;
                    uint current = ((uint)top.YPosition << 16) | top.YSubposition;
                    AssertEqual(0x0002_8000u, current - previous,
                        "gunship descent slows to cartridge 2.8 fixed pixels");
                    observedSlowDescent = true;
                }
            }

            if (previousFunction == 0xa80c && top.VariableF != 0xa80c)
            {
                // $A2:A8B2 clamps whole Y positions only; each hull keeps this call's subpixel sum.
                ushort carriedSubY = unchecked((ushort)(previousTopSubY + 0x8000));
                AssertEqual((ushort)0x045f, top.YPosition, "landing clamp top Y");
                AssertEqual(carriedSubY, top.YSubposition, "landing clamp keeps top subpixel");
                AssertEqual(carriedSubY, bottom.YSubposition, "landing clamp keeps bottom subpixel");
                AssertEqual(carriedSubY, pad.YSubposition, "landing clamp keeps pad subpixel");
                observedLandingClamp = true;
            }

            observedPadOpen |= enemies.LastGunshipEvent == GunshipFrameEvent.LandingPadOpened;
            if (enemies.LastGunshipEvent == GunshipFrameEvent.LandingPadOpened)
            {
                // $A2:A91E-$A925 place Samus beside the hatch in both X words, so the
                // following MainScrollingRoutine measures no horizontal movement.
                var checkpoint = new SamusCameraPoint(0x0480, 0x1234, samus.YPosition, 0);
                AssertEqual(checkpoint with { XPosition = samus.XPosition },
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
        AssertEqual(0x0440, samus.YPosition, "gunship raises Samus to deck standing Y");

        var openingSound = new EnemySoundRequest(
            SoundEffectLibrary3Sounds.GunshipEntrancePad, MaximumQueued: 6);
        enemies.StepFrame(0x0400, 0x0400, timeIsFrozen: false, samus);
        AssertTrue(!enemies.SoundRequests.Contains(openingSound),
            "idle ship does not publish an entrance sound without Down");
        ushort deckX = samus.XPosition;
        samus.XPosition = unchecked((ushort)(top.XPosition + 16));
        enemies.StepFrame(0x0400, 0x0400, timeIsFrozen: false, samus,
            newlyPressedControllerInput: 0x0400);
        AssertTrue(!enemies.SoundRequests.Contains(openingSound),
            "Down outside entrance does not publish an opening sound");
        samus.XPosition = deckX;

        // Re-entering after Mother Brain's event takes the native function-17 branch at
        // RestoreSamusInGunship: there is no refill wait, save prompt, or ordinary exit.
        zebesTimebombSet = true;
        var takeoffWrites = new VramWriteQueue();
        enemies.StepFrame(
            cameraX: 0x0400,
            cameraY: 0x0400,
            timeIsFrozen: false,
            samus,
            newlyPressedControllerInput: 0x0400,
            vramWriteQueue: takeoffWrites);
        AssertEqual(GunshipFrameEvent.EntryStarted, enemies.LastGunshipEvent,
            "endgame Down input begins ordinary gunship entry");
        AssertEqual(1, enemies.SoundRequests.Count(request => request == openingSound),
            "entry frame publishes exactly one library-three $14 Max6 sound");
        if (entrySoundOnly)
        {
            enemies.StepFrame(0x0400, 0x0400, timeIsFrozen: false, samus,
                newlyPressedControllerInput: 0x0400);
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
            ushort takeoffCameraY = samus.YPosition > 120
                ? unchecked((ushort)(samus.YPosition - 120))
                : (ushort)0;
            enemies.StepFrame(
                cameraX: 0x0400,
                cameraY: takeoffCameraY,
                timeIsFrozen: false,
                samus,
                vramWriteQueue: takeoffWrites);
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
        AssertTrue(top.YPosition < 0x0100,
            "state-$26 boundary occurs after top hull passes Y=$0100");

        Console.WriteLine(
            "  Gunship landing: station-18 descent, rigid camera carrier, bounce, pad, " +
            "Samus lift, event-$0E takeoff, and state-$26 handoff agree.");
    }

    /// <summary>Writes a synthetic 16-byte room-enemy population record for the gunship landing fixture.</summary>
    /// <param name="bus">Address space receiving the record's little-endian words.</param>
    /// <param name="address">Address of the first word in the population table.</param>
    /// <param name="definition">Enemy header pointer stored in the record.</param>
    /// <param name="x">Initial enemy X position.</param>
    /// <param name="y">Initial enemy Y position.</param>
    /// <param name="parameter2">Second actor-specific spawn parameter stored at record offset +$0E.</param>
    private static void WriteGunshipPopulationRecord(
        TestAddressSpace bus,
        int address,
        ushort definition,
        ushort x,
        ushort y,
        ushort parameter2)
    {
        WriteWord(bus, address, definition);
        WriteWord(bus, address + 2, x);
        WriteWord(bus, address + 4, y);
        WriteWord(bus, address + 6, 0);
        WriteWord(bus, address + 8, 0);
        WriteWord(bus, address + 10, 0);
        WriteWord(bus, address + 12, 0);
        WriteWord(bus, address + 14, parameter2);
    }
}

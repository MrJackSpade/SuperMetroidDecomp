using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Drives the three ordinary enemy slots used by the post-Ceres gunship from the
    /// special station-eighteen initializer through descent, bounce, pad animation waits,
    /// Samus lift, and the final ordinary-control handoff.
    /// </summary>
    static void VerifyPostCeresGunshipLanding()
    {
        const ushort topDefinition = 0xd000;
        const ushort bottomDefinition = 0xd040;
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

        // The exact gunship lists are animation owners, not phase timers. Small looping
        // fixture maps keep ProcessInstructions alive while the main-AI assertions below
        // prove that opening/closing selects the cartridge's $A5BE/$A5EE entry addresses.
        foreach (ushort list in new ushort[] { 0xa616, 0xa61c, 0xa60e, 0xa5be, 0xa5ee })
        {
            WriteWord(bus, 0xa20000 | list, 1);
            WriteWord(bus, (0xa20000 | list) + 2, 0xa700);
            WriteWord(bus, (0xa20000 | list) + 4, 0x80ed);
            WriteWord(bus, (0xa20000 | list) + 6, list);
        }

        // A signed bounce table makes both positive and negative rigid translations
        // observable and has a zero sum, so the post-bounce top returns to Y=$045F.
        short[] bounce = [3, 3, 2, 2, 1, 1, 0, -1, -2, -3, -3, -2, -1, 0, 0, 0, 0];
        for (int index = 0; index < bounce.Length; index++)
            WriteWord(bus, 0xa2a622 + index * 2, unchecked((ushort)bounce[index]));

        // Function 17 uploads five consecutive $400-byte dust-cloud chunks from bank $94.
        for (int index = 0; index < 5; index++)
        {
            WriteWord(bus, 0xa2ac07 + index * 2, unchecked((ushort)(0x8000 + index * 0x0400)));
            WriteWord(bus, 0xa2ac11 + index * 2, unchecked((ushort)(0x7600 + index * 0x0200)));
        }

        // Bobbing begins only after the bounce. Durations and signed deltas are interleaved
        // bytes at the native odd address, so seed all four records explicitly.
        byte[] bob = [2, 1, 2, 0xff, 2, 1, 2, 0xff];
        for (int index = 0; index < bob.Length; index++)
            bus.WriteByte(0xa2a7cf + index, bob[index]);

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
        enemies.Load(
            bus,
            populationPointer,
            tilesetPointer,
            vram,
            cgram,
            () => 0,
            samus: samus,
            hasEvent: eventNumber =>
                zebesTimebombSet && eventNumber == (int)EventNumber.ZebesTimebombSet,
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
        while (enemies.LastGunshipEvent != GunshipFrameEvent.LandingCompleted && frames < 800)
        {
            ushort previousTopY = top.YPosition;
            ushort previousTopSubY = top.YSubposition;
            ushort cameraY = samus.YPosition > 120
                ? unchecked((ushort)(samus.YPosition - 120))
                : (ushort)0;
            enemies.StepFrame(cameraX: 0x0400, cameraY, timeIsFrozen: false, samus);
            frames++;
            observedEngineSound |= enemies.SoundRequests.Contains(
                new EnemySoundRequest(Library: 2, SoundId: 0x4d, MaximumQueued: 6));

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

            observedPadOpen |= enemies.LastGunshipEvent == GunshipFrameEvent.LandingPadOpened;
            if (enemies.LastGunshipEvent == GunshipFrameEvent.LandingPadOpened)
            {
                // EnemyMain installs $A5BE, then this same slot's ordinary instruction
                // phase consumes its first four-byte timed frame before StepFrame returns.
                AssertEqual(0xa5c2, pad.CurrentInstruction,
                    "bounce completion advances the cartridge pad-open list once");
            }
            observedPadClose |= enemies.LastGunshipEvent == GunshipFrameEvent.LandingPadClosed;
            if (enemies.LastGunshipEvent == GunshipFrameEvent.LandingPadClosed)
            {
                AssertEqual(0xa5f2, pad.CurrentInstruction,
                    "Samus lift advances the cartridge pad-close list once");
            }
        }

        AssertTrue(observedSlowDescent, "gunship crosses native slow-descent threshold");
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

        int takeoffFrames = 0;
        bool observedTakeoffStart = false;
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
            takeoffFrames++;
        }

        AssertTrue(observedTakeoffStart,
            "event $0E bypasses restoration and publishes takeoff start");
        AssertEqual(GunshipFrameEvent.EscapeTakeoffCompleted, enemies.LastGunshipEvent,
            "accelerating gunship publishes frontend state-$26 boundary");
        AssertEqual(5, takeoffWrites.Entries.Count,
            "takeoff queues all five cartridge dust-cloud tile chunks");
        AssertTrue(top.YPosition < 0x0100,
            "state-$26 boundary occurs after top hull passes Y=$0100");

        Console.WriteLine(
            "  Gunship landing: station-18 descent, rigid camera carrier, bounce, pad, " +
            "Samus lift, event-$0E takeoff, and state-$26 handoff agree.");
    }

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

using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

/// <summary>Enumerates every retail attract scene before connecting the gameplay dispatcher.</summary>
internal static class AttractDemoDataAudit
{
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        byte[] originalSave = bus.SaveRam.ToArray();
        // Independent counts from the four sentinel-terminated bank-$82 room lists.
        int[] expectedCounts = [6, 6, 6, 5];
        int total = 0;
        for (int set = 0; set < AttractDemoRomData.SetCount; set++)
        {
            int scene = 0;
            while (AttractDemoScene.Read(bus, set, scene) is { } entry)
            {
                if (scene >= expectedCounts[set])
                    throw new InvalidDataException($"Demo set {set} exceeded its retail terminator.");
                if (entry.RoomPointer < 0x8000 || entry.InputObject < 0x8000 || entry.Duration == 0)
                    throw new InvalidDataException($"Demo {set}/{scene} has invalid cartridge data.");
                ushort initializer = RomDataReader.ReadWordFixedBank(bus, 0x910000 | entry.InputObject);
                ushort instructions = RomDataReader.ReadWordFixedBank(bus, (0x910000 | entry.InputObject) + 4);
                Console.WriteLine($"Demo {set}/{scene}: room={entry.RoomPointer:X4} door={entry.DoorPointer:X4} " +
                    $"camera={entry.CameraX:X4},{entry.CameraY:X4} samus={entry.SamusX:X4},{entry.SamusY:X4} " +
                    $"duration={entry.Duration} room-setup={entry.RoomSetupPointer:X4} " +
                    $"samus-setup={entry.SamusSetupPointer:X4} input={entry.InputObject:X4} " +
                    $"initializer={initializer:X4} instructions={instructions:X4}");
                var input = new AttractDemoInput(bus, entry);
                // This is interpreter coverage, not a gameplay trajectory test. A fixed
                // movement type cannot stand in for the real demo's changing actor state.
                for (int frame = 0; frame < entry.Duration; frame++)
                    input.Step(bus, SuperMetroidGameState.PlayingDemo, SamusMovementType.Standing);
                input.Step(bus, SuperMetroidGameState.TransitionFromDemoB, SamusMovementType.Standing);
                if (input.Script.InstructionPointer != 0 || input.Script.Held != 0 || input.Script.NewlyPressed != 0)
                    throw new InvalidDataException($"Demo {set}/{scene} did not clear its input on departure.");
                var runtime = new SuperMetroidRuntime(bus);
                runtime.InitializeAttractDemo(entry);
                if (runtime.ActiveRoom?.Pointer != entry.RoomPointer || runtime.ActiveDoor?.Pointer != entry.DoorPointer ||
                    runtime.Samus!.XPosition != entry.SamusX || runtime.Samus.YPosition != entry.SamusY ||
                    runtime.Camera!.XPosition != entry.CameraX || runtime.Camera.YPosition != entry.CameraY ||
                    runtime.Samus.EquippedItems != entry.Items || runtime.Samus.MaxHealth != entry.Health ||
                    runtime.Samus.EquippedBeams != entry.EquippedBeams)
                    throw new InvalidDataException($"Demo {set}/{scene} runtime did not preserve its cartridge placement/loadout.");
                for (int frame = 0; frame < entry.Duration; frame++)
                {
                    try { runtime.StepFrame(0, advanceGameTime: false); }
                    catch (Exception exception)
                    {
                        throw new InvalidDataException($"Demo playback {set}/{scene} failed at frame {frame}, pose {runtime.Samus.Pose:X2}.", exception);
                    }
                    if (runtime.Controller1.Current != 0 || runtime.Controller1.NewlyPressed != 0)
                        throw new InvalidDataException($"Demo {set}/{scene} leaked scripted input into the player cancellation latch.");
                }
                Console.WriteLine($"  Played {entry.Duration} frames: final Samus={runtime.Samus.XPosition:X4},{runtime.Samus.YPosition:X4}, pose={runtime.Samus.Pose:X2}.");
                if (set == 0 && scene == 0 && runtime.Samus.XPosition == entry.SamusX)
                    throw new InvalidDataException("The initial Landing Site demo never moved Samus horizontally.");
                scene++;
            }
            if (scene != expectedCounts[set])
                throw new InvalidDataException($"Demo set {set}: expected {expectedCounts[set]} scenes, read {scene}.");
            total += scene;
        }
        AttractDemoScene first = AttractDemoScene.Read(bus, 0, 0)!;
        if (first.RoomPointer != 0x91f8 || first.CameraX != 0x400 || first.CameraY != 0x400 ||
            first.SamusX != 0x481 || first.SamusY != 0x440 || first.Duration != 0x4d3 ||
            first.SamusSetupPointer != 0x8a33)
            throw new InvalidDataException("Landing Site demo placement/setup differs from the NTSC cartridge definition.");
        if (!bus.SaveRam.SequenceEqual(originalSave))
            throw new InvalidDataException("Demo playback modified the player's SRAM.");
        Console.WriteLine($"PASS: {total} retail demo scene loads/scripts/runtime durations, all four terminators, unchanged SRAM; title integration remains outstanding.");
        return 0;
    }
}

using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyAttractDemoScene()
    {
        VerifyStockAttractScenes();
        var rom = new byte[SuperMetroidAddressSpace.RetailRomByteCount];
        WriteRomWord(rom, AttractDemoRomData.RoomSetPointers, 0x9000);
        WriteRomWord(rom, AttractDemoRomData.EquipmentSetPointers, 0xa000);
        WriteRomWord(rom, AttractDemoRomData.SamusSetupSetPointers, 0xb000);
        ushort[] room = [0x91f8, 0x8000, 1, 0x100, 0x200, 0x40, 0xffd2, 0x151, 0x8924];
        ushort[] equipment = [0x3105, 10, 5, 2, 399, 0x100f, 0x100b, 0x9000];
        for (int word = 0; word < room.Length; word++) WriteRomWord(rom, 0x829000 + word * 2, room[word]);
        for (int word = 0; word < equipment.Length; word++) WriteRomWord(rom, 0x91a000 + word * 2, equipment[word]);
        WriteRomWord(rom, 0x91b000, 0x8a53);
        WriteRomWord(rom, 0x829012, AttractDemoRomData.EndOfSet);
        WriteRomWord(rom, 0x919000, DemoInputRomData.Routines.NoOp);
        WriteRomWord(rom, 0x919002, DemoInputRomData.Attract.CheckLeave);
        WriteRomWord(rom, 0x919004, 0xc000);
        WriteRomWord(rom, 0x91c000, 5);
        WriteRomWord(rom, 0x91c002, (ushort)SnesButton.Left);
        WriteRomWord(rom, 0x91c004, 0);
        WriteRomWord(rom, DemoInputRomData.BankBase | DemoInputRomData.Attract.DeleteList,
            DemoInputRomData.Instructions.Delete);
        WriteRomWord(rom, DemoInputRomData.BankBase | DemoInputRomData.Attract.ShinesparkContinuation, 7);
        WriteRomWord(rom, (DemoInputRomData.BankBase | DemoInputRomData.Attract.ShinesparkContinuation) + 2,
            (ushort)SnesButton.Right);
        var bus = new SuperMetroidAddressSpace(rom);
        var expected = new AttractDemoScene(0x91f8, 0x8000, 1, 0x100, 0x200, 0x40, -46,
            0x151, 0x8924, 0x8a53, 0x3105, 10, 5, 2, 399, 0x100f, 0x100b, 0x9000);
        if (AttractDemoScene.Read(bus, 0, 0) != expected)
            throw new InvalidDataException("Demo room/equipment/setup tables did not join at the same scene index.");
        if (expected.SamusX != 338 || expected.SamusY != 576)
            throw new InvalidDataException("Demo offsets lost their native X-center/Y-top interpretation.");
        if (AttractDemoScene.Read(bus, 0, 1) is not null)
            throw new InvalidDataException("Demo room sentinel did not terminate the set.");
        var input = new AttractDemoInput(bus, expected);
        input.Step(bus, SuperMetroidGameState.PlayingDemo, SamusMovementType.Standing);
        if (input.Script.Held != (ushort)SnesButton.Left || input.Script.InstructionTimer != 5)
            throw new InvalidDataException("Title demo did not publish its first timed input record.");
        input.Step(bus, SuperMetroidGameState.TransitionFromDemoB, SamusMovementType.Standing);
        if (input.Script.InstructionPointer != 0 || input.Script.Held != 0)
            throw new InvalidDataException("Title demo departure did not delete input in the same handler call.");
        input = new AttractDemoInput(bus, expected);
        input.Script.Redirect(DemoInputRomData.Attract.ShinesparkPreInstruction, 0xc000);
        input.Step(bus, SuperMetroidGameState.PlayingDemo, SamusMovementType.DraygonHeld);
        if (input.Script.Held != (ushort)SnesButton.Left)
            throw new InvalidDataException("Native type-$1A branch should leave the current script intact.");
        input.Step(bus, SuperMetroidGameState.PlayingDemo, SamusMovementType.Standing);
        if (input.Script.Held != (ushort)SnesButton.Right || input.Script.InstructionTimer != 7 ||
            input.Script.PreInstructionPointer != DemoInputRomData.Attract.CheckLeave)
            throw new InvalidDataException("Demo pre-instruction redirect lost its list, timer, or normal callback.");
        Console.WriteLine("  Attract demo data: joined fields, signed placement, and end-of-set sentinel agree.");
        var frontend = new SuperMetroidGame(bus);
        if (frontend.AvailableDemoSetCount() != 3)
            throw new InvalidDataException("An empty save must expose only the three ordinary demo sets.");
        for (int index = 0; index < AttractDemoRomData.CompletionMarker.Length; index++)
            bus.WriteByte(AttractDemoRomData.CompletionMarkerAddress + index, AttractDemoRomData.CompletionMarker[index]);
        if (frontend.AvailableDemoSetCount() != 3)
            throw new InvalidDataException("Completion marker without any valid save incorrectly unlocked set four.");
        new SuperMetroidSaveRam(bus).SaveSlot(0, new SuperMetroidSaveSnapshot());
        if (frontend.AvailableDemoSetCount() != 4)
            throw new InvalidDataException("Valid completed-game save did not unlock the fourth demo set.");
        bus.WriteByte(AttractDemoRomData.CompletionMarkerAddress, 0);
        if (frontend.AvailableDemoSetCount() != 3)
            throw new InvalidDataException("Incomplete-game save incorrectly unlocked set four.");
    }

    private static void VerifyStockAttractScenes()
    {
        var retail = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int[] counts = [6, 6, 6, 5];
        int total = 0;
        for (int set = 0; set < counts.Length; set++)
        {
            for (int scene = 0; scene < counts[set]; scene++)
            {
                AssertEqual(AttractDemoScene.Read(retail, set, scene), StockAttractDemoScenes.Get(set, scene),
                    $"compiled scene {set}/{scene} matches every cartridge setup field");
                total++;
                VerifyCompiledAttractInput(retail, StockAttractDemoScenes.Get(set, scene)!);
            }
            AssertTrue(StockAttractDemoScenes.Get(set, counts[set]) is null &&
                AttractDemoScene.Read(retail, set, counts[set]) is null, "stock set sentinel");
            AssertThrows<ArgumentOutOfRangeException>(() => StockAttractDemoScenes.Get(set, counts[set] + 1),
                "reject scene beyond compiled set");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => StockAttractDemoScenes.Get(-1, 0), "negative demo set");
        AssertThrows<ArgumentOutOfRangeException>(() => StockAttractDemoScenes.Get(0, -1), "negative demo scene");
        Console.WriteLine($"Compiled attract scenes: {total} records and four sentinels match cartridge data.");
    }

    private static void VerifyCompiledAttractInput(ISnesAddressSpace bus, AttractDemoScene scene)
    {
        // Run well beyond each displayed scene: this also checks script tails that ordinary
        // frontend timing does not reach. Separate schedules exercise both native $1A
        // pre-instruction branches and cancellation before/during/after timed records.
        foreach (int leaveFrame in new[] { -1, 0, 19, (int)scene.Duration })
        {
            var reference = new AttractDemoInput(bus, scene);
            var compiled = new AttractDemoInput(scene);
            for (int frame = 0; frame < 6000; frame++)
            {
                var gameState = frame == leaveFrame
                    ? SuperMetroidGameState.TransitionFromDemoB : SuperMetroidGameState.PlayingDemo;
                var movement = frame < 200 ? SamusMovementType.DraygonHeld : SamusMovementType.Standing;
                reference.Step(bus, gameState, movement);
                compiled.StepStock(gameState, movement);
                AssertEqual(Snapshot(reference.Script), Snapshot(compiled.Script),
                    $"compiled attract ${scene.InputObject:X4}, leave {leaveFrame}, frame {frame}");
            }
        }

        static string Snapshot(DemoInputState state) =>
            $"{state.Enabled}/{state.InitializationParameter}/{state.PreInstructionPointer}/" +
            $"{state.InstructionPointer}/{state.InstructionTimer}/{state.Timer}/{state.Held}/" +
            $"{state.NewlyPressed}/{state.PreviousHeld}/{state.PreviousNewlyPressed}/" +
            $"{state.PublishedPreviousHeld}/{state.PublishedPreviousNewlyPressed}";
    }
}

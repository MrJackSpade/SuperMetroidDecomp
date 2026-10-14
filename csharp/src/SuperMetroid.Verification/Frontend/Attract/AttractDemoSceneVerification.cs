using static SuperMetroid.Verification.Samus.Grapple.GrappleDemoTrajectoryVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Attract.AttractInputFixtureVerification;
using static SuperMetroid.Verification.Frontend.FrontendVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Attract;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Saves;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Frontend.Attract;
using SuperMetroid.Verification.Harness;

/// <summary>Checks attract-scene setup records and compiled input scripts against native data and playback.</summary>
internal static class AttractDemoSceneVerification
{
    /// <summary>Exercises joined room/equipment/setup records, script redirects, and completed-save access to demo set four.</summary>
    internal static void VerifyAttractDemoScene()
    {
        Suite(nameof(VerifyStockAttractScenes), () => VerifyStockAttractScenes());
        var rom = new byte[SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.RetailRomByteCount];
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
        WriteRomWord(rom, 0x919002, (ushort)AttractDemoPreInstruction.CheckLeave);
        WriteRomWord(rom, 0x919004, 0xc000);
        WriteRomWord(rom, 0x91c000, 5);
        WriteRomWord(rom, 0x91c002, (ushort)SnesButton.Left);
        WriteRomWord(rom, 0x91c004, 0);
        WriteRomWord(rom, DemoInputRomData.BankBase | DemoInputRomData.Attract.DeleteList,
            (ushort)DemoInputInstruction.Delete);
        WriteRomWord(rom, DemoInputRomData.BankBase | DemoInputRomData.Attract.ShinesparkContinuation, 7);
        WriteRomWord(rom, (DemoInputRomData.BankBase | DemoInputRomData.Attract.ShinesparkContinuation) + 2,
            (ushort)SnesButton.Right);
        var bus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
        var expected = new AttractDemoScene(0x91f8, 0x8000, new(0x100, 0x200), new(-46, 0x40),
            0x151, AttractDemoRoomSetup.NoOp, AttractDemoSamusSetup.StandingRight, 0x3105, 10, 5, 2, 399, 0x100f, 0x100b, 0x9000);
        if (SuperMetroid.TestSupport.VerificationImporters.AttractDemoSceneImporter.Read(bus, 0, 0) != expected)
            throw new InvalidDataException("Demo room/equipment/setup tables did not join at the same scene index.");
        if (expected.SamusPosition != new Position16<RoomPixels>(338, 576))
            throw new InvalidDataException("Demo offsets lost their native X-center/Y-top interpretation.");
        if (SuperMetroid.TestSupport.VerificationImporters.AttractDemoSceneImporter.Read(bus, 0, 1) is not null)
            throw new InvalidDataException("Demo room sentinel did not terminate the set.");
        var input = new ReferenceAttractInput(bus, expected);
        input.Step(bus, SuperMetroidGameState.PlayingDemo, SamusMovementType.Standing);
        if (input.Script.Held != (ushort)SnesButton.Left || input.Script.InstructionTimer.RawValue != 5)
            throw new InvalidDataException("Title demo did not publish its first timed input record.");
        input.Step(bus, SuperMetroidGameState.TransitionFromDemoB, SamusMovementType.Standing);
        if (input.Script.InstructionPointer != 0 || input.Script.Held != 0)
            throw new InvalidDataException("Title demo departure did not delete input in the same handler call.");
        input = new ReferenceAttractInput(bus, expected);
        input.Script.Redirect((ushort)AttractDemoPreInstruction.Shinespark, 0xc000);
        input.Step(bus, SuperMetroidGameState.PlayingDemo, SamusMovementType.DraygonHeld);
        if (input.Script.Held != (ushort)SnesButton.Left)
            throw new InvalidDataException("Native type-$1A branch should leave the current script intact.");
        input.Step(bus, SuperMetroidGameState.PlayingDemo, SamusMovementType.Standing);
        if (input.Script.Held != (ushort)SnesButton.Right || input.Script.InstructionTimer.RawValue != 7 ||
            input.Script.PreInstructionPointer != (ushort)AttractDemoPreInstruction.CheckLeave)
            throw new InvalidDataException("Demo pre-instruction redirect lost its list, timer, or normal callback.");
        Console.WriteLine("  Attract demo data: joined fields, signed placement, and end-of-set sentinel agree.");
        // $80:8261 fixes NumberOfDemoSets once at boot, so each eligibility case boots a session.
        if (new SuperMetroidGame(bus).AvailableDemoSetCount() != 3)
            throw new InvalidDataException("An empty save must expose only the three ordinary demo sets.");
        var saveRam = new SuperMetroidSaveRam(bus, RetailPresentationFixture());
        saveRam.SetGameCompleted(true);
        if (new SuperMetroidGame(bus).AvailableDemoSetCount() != 3)
            throw new InvalidDataException("Completion marker without any valid save incorrectly unlocked set four.");
        saveRam.SaveSlot(0, new SuperMetroidSaveSnapshot());
        saveRam.SetGameCompleted(true);
        if (new SuperMetroidGame(bus).AvailableDemoSetCount() != 4)
            throw new InvalidDataException("Valid completed-game save did not unlock the fourth demo set.");
        saveRam.SetGameCompleted(false);
        if (new SuperMetroidGame(bus).AvailableDemoSetCount() != 3)
            throw new InvalidDataException("Incomplete-game save incorrectly unlocked set four.");
    }

    /// <summary>Matches all four compiled demo sets and their sentinels to retail scene records.</summary>
    internal static void VerifyStockAttractScenes()
    {
        var retail = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int[] counts = [6, 6, 6, 5];
        int total = 0;
        for (int set = 0; set < counts.Length; set++)
        {
            for (int scene = 0; scene < counts[set]; scene++)
            {
                AssertEqual(SuperMetroid.TestSupport.VerificationImporters.AttractDemoSceneImporter.Read(retail, set, scene), StockAttractDemoScenes.Get(set, scene),
                    $"compiled scene {set}/{scene} matches every cartridge setup field");
                total++;
                VerifyCompiledAttractInput(retail, (StockAttractDemoScenes.Get(set, scene) ?? throw new InvalidOperationException("Verification requires StockAttractDemoScenes.Get(set, scene).")));
            }
            AssertTrue(StockAttractDemoScenes.Get(set, counts[set]) is null &&
                SuperMetroid.TestSupport.VerificationImporters.AttractDemoSceneImporter.Read(retail, set, counts[set]) is null, "stock set sentinel");
            AssertThrows<ArgumentOutOfRangeException>(() => StockAttractDemoScenes.Get(set, counts[set] + 1),
                "reject scene beyond compiled set");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => StockAttractDemoScenes.Get(-1, 0), "negative demo set");
        AssertThrows<ArgumentOutOfRangeException>(() => StockAttractDemoScenes.Get(0, -1), "negative demo scene");
        Suite(nameof(VerifyAttractCommandDefinitionsAndPlayback), () => VerifyAttractCommandDefinitionsAndPlayback(retail, counts));
        Console.WriteLine($"Compiled attract scenes: {total} records and four sentinels match cartridge data.");
    }

    /// <summary>Checks all compiled input commands and runs each scene's production input owner for its displayed duration.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for attract demo command definitions and playback")]
    private static void VerifyAttractCommandDefinitionsAndPlayback(
        ISnesAddressSpace retail,
        int[] sceneCounts)
    {

        var commandRoots = new HashSet<ushort>
        {
            DemoInputRomData.Attract.DeleteList,
            DemoInputRomData.Attract.ShinesparkContinuation,
        };
        for (int set = 0; set < sceneCounts.Length; set++)
        {
            for (int sceneIndex = 0; sceneIndex < sceneCounts[set]; sceneIndex++)
            {
                AttractDemoScene scene = (StockAttractDemoScenes.Get(set, sceneIndex) ?? throw new InvalidOperationException("Verification requires StockAttractDemoScenes.Get(set, sceneIndex)."));
                StockAttractInputProgramsCatalog.ObjectDefinition definition =
                    StockAttractInputProgramsCatalog.GetObject((scene ?? throw new InvalidOperationException("Verification requires scene.")).InputObject);
                int objectAddress = DemoInputRomData.BankBase | scene.InputObject;
                AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(objectAddress), definition.Initializer, "native attract initializer");
                AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((objectAddress + 2)), definition.PreInstruction, "native attract callback");
                AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((objectAddress + 4)), definition.Start, "native attract initial command");
                commandRoots.Add(definition.Start);
            }
        }

        var visitedCommands = new HashSet<ushort>();
        int inputCount = 0, deleteCount = 0, gotoCount = 0;
        foreach (ushort root in commandRoots)
        {
            ushort cursor = root;
            while (visitedCommands.Add(cursor))
            {
                StockAttractInputProgramsCatalog.Command command = StockAttractInputProgramsCatalog.GetCommand(cursor);
                int address = DemoInputRomData.BankBase | cursor;
                ushort nativeOperation = CartridgeImportSource.Require(retail).ReadCartridgeWord(address);
                if (command.Kind == StockAttractInputProgramsCatalog.Operation.Input)
                {
                    inputCount++;
                    AssertTrue(nativeOperation < DemoInputRomData.Instructions.OpcodeBit,
                        $"native attract input ${cursor:X4}");
                    AssertEqual(nativeOperation, command.Duration, $"native attract duration ${cursor:X4}");
                    AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((address + 2)), (ushort)command.Held,
                        $"native attract held ${cursor:X4}");
                    AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((address + 4)), (ushort)command.NewlyPressed,
                        $"native attract edge ${cursor:X4}");
                    AssertEqual((ushort)(cursor + DemoInputRomData.Instructions.InputRecordBytes), command.Next,
                        $"calculated attract successor ${cursor:X4}");
                }
                else if (command.Kind == StockAttractInputProgramsCatalog.Operation.Delete)
                {
                    deleteCount++;
                    AssertEqual((ushort)DemoInputInstruction.Delete, nativeOperation,
                        $"native attract deletion ${cursor:X4}");
                    AssertEqual((ushort)0, command.Next, $"terminal attract successor ${cursor:X4}");
                }
                else if (command.Kind == StockAttractInputProgramsCatalog.Operation.Goto)
                {
                    gotoCount++;
                    AssertEqual((ushort)DemoInputInstruction.Goto, nativeOperation,
                        $"native attract jump ${cursor:X4}");
                    AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((address + 2)), command.Next,
                        $"native attract jump destination ${cursor:X4}");
                }
                if (command.Kind == StockAttractInputProgramsCatalog.Operation.Delete)
                    break;
                cursor = command.Next;
            }
        }

        AssertEqual(798, inputCount, "bounded native attract input inventory");
        AssertEqual(24, deleteCount, "bounded native attract deletion inventory");
        AssertEqual(1, gotoCount, "bounded native attract jump inventory");
        Console.WriteLine("Attract command dispatch: 798 native input triples, 24 deletions and one jump match exactly.");

        for (int set = 0; set < sceneCounts.Length; set++)
        {
            for (int sceneIndex = 0; sceneIndex < sceneCounts[set]; sceneIndex++)
            {
                AttractDemoScene scene = (StockAttractDemoScenes.Get(set, sceneIndex) ?? throw new InvalidOperationException("Verification requires StockAttractDemoScenes.Get(set, sceneIndex)."));
                // The production input owner has no address-space capability. Room and
                // presentation initialization are separate from this dispatch contract.
                var input = new AttractDemoInput((scene ?? throw new InvalidOperationException("Verification requires scene.")));
                for (int frame = 0; frame < scene.Duration; frame++)
                    input.StepStock(SuperMetroidGameState.PlayingDemo,
                        frame < 200 ? SamusMovementType.DraygonHeld : SamusMovementType.Standing);
            }
        }
    }

    /// <summary>Compares each scene's compiled script with the native interpreter through long tails and leave-frame schedules.</summary>
    private static void VerifyCompiledAttractInput(ISnesAddressSpace bus, AttractDemoScene scene)
    {
        // Run well beyond each displayed scene: this also checks script tails that ordinary
        // frontend timing does not reach. Separate schedules exercise both native $1A
        // pre-instruction branches and cancellation before/during/after timed records.
        foreach (int leaveFrame in new[] { -1, 0, 19, (int)scene.Duration })
        {
            var reference = new ReferenceAttractInput(bus, scene);
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
            $"{state.InstructionPointer}/{state.InstructionTimer.RawValue}/{state.Timer.RawValue}/{state.Held}/" +
            $"{state.NewlyPressed}/{state.PreviousHeld}/{state.PreviousNewlyPressed}/" +
            $"{state.PublishedPreviousHeld}/{state.PublishedPreviousNewlyPressed}";
    }

    /// <summary>Runs attract input fixtures and the paired grapple demo trajectory check.</summary>
    [VerificationCommand("--attract-input-fixtures")]
    internal static int AttractInputFixturesCommand()
    {
     VerifyAttractDemoScene(); VerifyGrappleDemoTrajectory(); return 0; 
    }

    /// <summary>Runs the retail-backed stock attract-scene check.</summary>
    [VerificationCommand("--stock-attract-scenes")]
    internal static int StockAttractScenesCommand()
    {
        Suite(nameof(VerifyStockAttractScenes), () => VerifyStockAttractScenes());
        return 0;
    }
}

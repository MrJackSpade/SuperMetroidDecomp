using SuperMetroid.Diagnostics.Input;
using SuperMetroid.Verification.Input;
using static SuperMetroid.Verification.Samus.Grapple.GrappleDemoTrajectoryVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Runtime.Attract.AttractInputFixtureVerification;
using static SuperMetroid.Verification.Frontend.FrontendVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Runtime.Attract;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Runtime.Attract;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Progression;

/// <summary>Checks attract-scene setup records and compiled input scripts against native data and playback.</summary>
internal static class AttractDemoSceneVerification
{
    /// <summary>Exercises joined room/equipment/setup records, script redirects, and completed-save access to demo set four.</summary>
    internal static void VerifyAttractDemoScene()
    {
        Suite(nameof(VerifyStockAttractScenes), () => VerifyStockAttractScenes());
        var rom = new byte[SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.RetailRomByteCount];
        WriteRomWord(rom, AttractDemoSceneSources.RoomSetPointers, 0x9000);
        WriteRomWord(rom, AttractDemoSceneSources.EquipmentSetPointers, 0xa000);
        WriteRomWord(rom, AttractDemoSceneSources.SamusSetupSetPointers, 0xb000);
        ushort[] room = [0x91f8, 0x896a, 1, 0x100, 0x200, 0x40, 0xffd2, 0x151, 0x8924];
        ushort inputObject = AttractDemoInputDefinitions.NativeHeaderOf(AttractDemoInputDefinitions.AttractLandingSite);
        ushort[] equipment = [0x3105, 10, 5, 2, 399, 0x100f, 0x100b, inputObject];
        for (int word = 0; word < room.Length; word++) WriteRomWord(rom, 0x829000 + word * 2, room[word]);
        for (int word = 0; word < equipment.Length; word++) WriteRomWord(rom, 0x91a000 + word * 2, equipment[word]);
        WriteRomWord(rom, 0x91b000, 0x8a53);
        WriteRomWord(rom, 0x829012, AttractDemoSceneSources.EndOfSet);
        WriteRomWord(rom, DemoInputRomData.BankBase | inputObject, DemoInputRomData.Routines.NoOp);
        WriteRomWord(rom, (DemoInputRomData.BankBase | inputObject) + 2, (ushort)NativeDemoFrameBehavior.AttractCheckLeave);
        WriteRomWord(rom, (DemoInputRomData.BankBase | inputObject) + 4, 0xc000);
        WriteRomWord(rom, 0x91c000, 5);
        WriteRomWord(rom, 0x91c002, ControllerWordCodec.ToJoypadWord(ControllerButton.Left));
        WriteRomWord(rom, 0x91c004, 0);
        WriteRomWord(rom, DemoInputRomData.BankBase | DemoInputNativeListAddresses.Entry(DemoInputRecordingId.AttractDelete),
            (ushort)DemoInputInstruction.Delete);
        WriteRomWord(rom, DemoInputRomData.BankBase | DemoInputNativeListAddresses.Entry(DemoInputRecordingId.AttractDiagonalShinesparkContinuation), 7);
        WriteRomWord(rom, (DemoInputRomData.BankBase | DemoInputNativeListAddresses.Entry(DemoInputRecordingId.AttractDiagonalShinesparkContinuation)) + 2,
            ControllerWordCodec.ToJoypadWord(ControllerButton.Right));
        var bus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
        var expected = new AttractDemoScene(RoomDefinitions.LandingSite, CrateriaDoorDefinitions.Parlor1, new(0x100, 0x200), new(-46, 0x40),
            0x151, AttractDemoRoomSetup.NoOp, AttractDemoSamusSetup.StandingRight, (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster), 10, 5, 2, 399, SamusBeamFlags.Charge | SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma, SamusBeamFlags.Charge | SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Plasma, AttractDemoInputDefinitions.AttractLandingSite);
        if (SuperMetroid.TestSupport.VerificationImporters.AttractDemoSceneImporter.Read(bus, 0, 0) != expected)
            throw new InvalidDataException("Demo room/equipment/setup tables did not join at the same scene index.");
        if (expected.SamusPosition != new Position16<RoomPixels>(338, 576))
            throw new InvalidDataException("Demo offsets lost their native X-center/Y-top interpretation.");
        if (SuperMetroid.TestSupport.VerificationImporters.AttractDemoSceneImporter.Read(bus, 0, 1) is not null)
            throw new InvalidDataException("Demo room sentinel did not terminate the set.");
        var input = new ReferenceAttractInput(bus, expected);
        input.Step(SuperMetroidGameState.PlayingDemo, SamusMovementType.Standing);
        if (input.Script.Held != ControllerWordCodec.ToJoypadWord(ControllerButton.Left) || input.Script.FrameHold.RawValue != 5)
            throw new InvalidDataException("Title demo did not publish its first timed input record.");
        input.Step(SuperMetroidGameState.RoutingAfterDemo, SamusMovementType.Standing);
        if (input.Script.InstructionPointer != 0 || input.Script.Held != 0)
            throw new InvalidDataException("Title demo departure did not delete input in the same handler call.");
        input = new ReferenceAttractInput(bus, expected);
        input.Script.Redirect((ushort)NativeDemoFrameBehavior.AttractShinespark, 0xc000);
        input.Step(SuperMetroidGameState.PlayingDemo, SamusMovementType.DraygonHeld);
        if (input.Script.Held != ControllerWordCodec.ToJoypadWord(ControllerButton.Left))
            throw new InvalidDataException("Native type-$1A branch should leave the current script intact.");
        input.Step(SuperMetroidGameState.PlayingDemo, SamusMovementType.Standing);
        if (input.Script.Held != ControllerWordCodec.ToJoypadWord(ControllerButton.Right) || input.Script.FrameHold.RawValue != 7 ||
            input.Script.FrameBehaviorPointer != (ushort)NativeDemoFrameBehavior.AttractCheckLeave)
            throw new InvalidDataException("Demo pre-instruction redirect lost its list, timer, or normal callback.");
        Console.WriteLine("  Attract demo data: joined fields, signed placement, and end-of-set sentinel agree.");
        // Boot decides once whether the completion set plays, so each eligibility case boots a session.
        var saveRam = new SaveSlotStore(RetailPresentationFixture());
        if (new SuperMetroidGame(saveRam).Session.DemoPlaylist != StockAttractDemoScenes.OrdinaryPlaylist)
            throw new InvalidDataException("An empty save must expose only the three ordinary demo sets.");
        saveRam.SetGameCompleted(true);
        if (new SuperMetroidGame(saveRam).Session.DemoPlaylist != StockAttractDemoScenes.OrdinaryPlaylist)
            throw new InvalidDataException("Completion marker without any valid save incorrectly unlocked the completion set.");
        saveRam.SaveSlot(0, new SuperMetroidSaveSnapshot());
        saveRam.SetGameCompleted(true);
        if (new SuperMetroidGame(saveRam).Session.DemoPlaylist != StockAttractDemoScenes.CompletedGamePlaylist)
            throw new InvalidDataException("Valid completed-game save did not unlock the completion demo set.");
        saveRam.SetGameCompleted(false);
        if (new SuperMetroidGame(saveRam).Session.DemoPlaylist != StockAttractDemoScenes.OrdinaryPlaylist)
            throw new InvalidDataException("Incomplete-game save incorrectly unlocked the completion set.");
    }

    /// <summary>
    /// Matches all four compiled demo sets, in the order the completed-game playlist plays them, to the retail scene
    /// records, each set ending where the cartridge's does; the ordinary playlist is the same without the completion set.
    /// </summary>
    internal static void VerifyStockAttractScenes()
    {
        var retail = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        IReadOnlyList<AttractDemoSet> sets = StockAttractDemoScenes.CompletedGamePlaylist;
        AssertSequenceEqual(sets.Where(set => set != StockAttractDemoScenes.CompletionSet).ToArray(),
            StockAttractDemoScenes.OrdinaryPlaylist, "the ordinary playlist plays every set but the completion set");
        AssertEqual(StockAttractDemoScenes.CompletionSet, sets[^1], "the completion set plays after the ordinary sets");
        int total = 0;
        for (int set = 0; set < sets.Count; set++)
        {
            IReadOnlyList<AttractDemoScene> scenes = sets[set].Scenes;
            for (int scene = 0; scene < scenes.Count; scene++)
            {
                AssertEqual(SuperMetroid.TestSupport.VerificationImporters.AttractDemoSceneImporter.Read(retail, set, scene), scenes[scene],
                    $"compiled scene {sets[set]}/{scene} matches every cartridge setup field");
                total++;
                VerifyCompiledAttractInput(retail, scenes[scene]);
            }
            AssertTrue(SuperMetroid.TestSupport.VerificationImporters.AttractDemoSceneImporter.Read(retail, set, scenes.Count) is null,
                $"{sets[set]} ends where the cartridge's set ends");
        }
        Suite(nameof(VerifyAttractPlayback), () => VerifyAttractPlayback(sets));
        Console.WriteLine($"Compiled attract scenes: {total} records in {sets.Count} sets match cartridge data and end with it.");
    }

    /// <summary>Runs each scene's production input owner for its displayed duration; <c>--demo-input-programs</c> checks the programs.</summary>
    private static void VerifyAttractPlayback(IReadOnlyList<AttractDemoSet> sets)
    {
        foreach (AttractDemoSet set in sets)
        {
            foreach (AttractDemoScene scene in set.Scenes)
            {
                // The production input owner has no address-space capability. Room and
                // presentation initialization are separate from this dispatch contract.
                var input = new AttractDemoInput(scene);
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
        foreach (int leaveFrame in new[] { -1, 0, 19, scene.Duration })
        {
            var reference = new ReferenceAttractInput(bus, scene);
            var compiled = new AttractDemoInput(scene);
            for (int frame = 0; frame < 6000; frame++)
            {
                var gameState = frame == leaveFrame
                    ? SuperMetroidGameState.RoutingAfterDemo : SuperMetroidGameState.PlayingDemo;
                var movement = frame < 200 ? SamusMovementType.DraygonHeld : SamusMovementType.Standing;
                reference.Step(gameState, movement);
                compiled.StepStock(gameState, movement);
                AssertEqual(reference.Script.Snapshot(), NativeDemoInputReference.Of(compiled.Script, DemoInputNativeListAddresses.WatchAddress(compiled.Watch)),
                    $"compiled attract {scene.Input.Animation}, leave {leaveFrame}, frame {frame}");
            }
        }
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

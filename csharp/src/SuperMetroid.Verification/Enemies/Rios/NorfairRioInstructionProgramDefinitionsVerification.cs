using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Rios;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled Norfair Rio parent and flame instruction programs.</summary>
internal static class NorfairRioInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for Norfair Rio instruction comparison.</summary>
    internal static void VerifyNorfairRioInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyNorfairRioInstructionProgramDefinitions), () => VerifyNorfairRioInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks native words, seven guarded programs, callbacks, visual selectors, and allocation-free reads.</summary>
    internal static void VerifyNorfairRioInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        // Each update shows the next pose; the cues before it place the flames, and a one-shot swoop start ends by
        // telling main AI it has finished.
        (NorfairRioProgram Entry, int Calls, short Offset, bool Signal, string Name)[] programs =
        [
            (NorfairRioProgram.Idle, 5, 8, false, "idle"),
            (NorfairRioProgram.StartDescending, 7, -16, true, "start descending"),
            (NorfairRioProgram.Descending, 5, -12, false, "descending"),
            (NorfairRioProgram.StartAscending, 9, 12, true, "start ascending"),
            (NorfairRioProgram.Ascending, 5, 12, false, "ascending"),
            (NorfairRioProgram.FlamesAscending, 5, 0, false, "ascending flames"),
            (NorfairRioProgram.FlamesDescending, 5, 0, false, "descending flames"),
        ];
        foreach ((NorfairRioProgram entry, int calls, short offset, bool signal, string name) in programs)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, NorfairRioEnemyState state) =
                NewNorfairRioInstructionSystem(entry);
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, SnesButton.None];
            RunNorfairRioInstructionFrames(arguments,
                slot,
                calls);
            AssertEqual(new Offset16<RoomPixels>(0, offset), state.FollowerOffset,
                $"Norfair Rio {name} publishes its final follower offset");
            AssertEqual(signal, state.AnimationSignal,
                $"Norfair Rio {name} animation signal");
        }

        Suite(nameof(VerifyNorfairRioInitializerSelections), () => VerifyNorfairRioInitializerSelections());
        Suite(nameof(NorfairRioAnimationsVerification.VerifyNorfairRioAnimations), () => NorfairRioAnimationsVerification.VerifyNorfairRioAnimations(rom));

        Console.WriteLine(
            "Norfair Rio: all seven parent and flame programs with their flame placements and swoop-start handshakes, " +
            "and the initializer selections pass.");
    }

    /// <summary>Checks that parent and follower initialization choose idle and ascending-flames programs respectively.</summary>
    private static void VerifyNorfairRioInitializerSelections(
)
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);

        RoomEnemySlot parent = enemies.SlotTable.Slots[0];
        parent.Bind(EnemyDefinitionId.Geruta, parent.Definition);
        parent.Bind(parent.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xa2 });
        enemies.NorfairRioBehavior.InitializeNorfairRio(parent);
        AssertEqual(AnimationProgramCursor<NorfairRioProgram>.Start(NorfairRioProgram.Idle),
            enemies.SlotTable.Slots[0].EntityState<NorfairRioEntity, NorfairRioEnemyState>()?.Program,
            "Norfair Rio parent initializer selects idle");

        RoomEnemySlot follower = enemies.SlotTable.Slots[1];
        follower.Bind(EnemyDefinitionId.Geruta, follower.Definition);
        follower.Bind(follower.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xa2 });
        follower.SetSpawnParameter1(0x8000);
        enemies.NorfairRioBehavior.InitializeNorfairRio(follower);
        AssertEqual(AnimationProgramCursor<NorfairRioProgram>.Start(NorfairRioProgram.FlamesAscending),
            enemies.SlotTable.Slots[1].EntityState<NorfairRioEntity, NorfairRioEnemyState>()?.Program,
            "Norfair Rio follower initializer selects ascending flames");
    }

    /// <summary>Seeds a Geruta-backed Norfair Rio actor with an instruction entry and optional installed artwork.</summary>
    internal static (
        RoomEnemySystem Enemies,
        RoomEnemySlot Slot,
        NorfairRioEnemyState State) NewNorfairRioInstructionSystem(
            NorfairRioProgram entry,
            EnemyTileArtworkCatalog? art = null)
    {
        var enemies = new RoomEnemySystem(new EnemySession()) { TileArtwork = art };
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Geruta, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xa2 });
        slot.InstructionTimer = NativeCountdown.Start(1);
        var state = new NorfairRioEnemyState { Program = AnimationProgramCursor<NorfairRioProgram>.Start(entry) };
        slot.SetSpeciesState(state);
        return (enemies, slot, state);
    }

    /// <summary>Processes the selected instruction entry on each requested actor update.</summary>
    internal static void RunNorfairRioInstructionFrames(object?[] arguments,
        RoomEnemySlot slot,
        int count)
    {
        for (int call = 0; call < count; call++)
        {
            slot.InstructionTimer = NativeCountdown.Start(1);
            EnemyInstructionRunner.Process(arguments);
        }
    }

    /// <summary>Runs the retail-cartridge Norfair Rio instruction verification.</summary>
    [VerificationCommand("--norfair-rio-instruction-program-definitions")]
    internal static int NorfairRioInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyNorfairRioInstructionProgramDefinitions), () => VerifyNorfairRioInstructionProgramDefinitions());
        return 0;
    }
}

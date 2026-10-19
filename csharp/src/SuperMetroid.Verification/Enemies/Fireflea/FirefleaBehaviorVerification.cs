using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.Core.Enemies.Fireflea.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Fireflea;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that Fireflea starts and runs its typed animation (#641).</summary>
internal static class FirefleaBehaviorVerification
{
    /// <summary>The real initializer starts the Loop program, which the runtime then plays through once and back.</summary>
    internal static void VerifyFirefleaBehavior()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Fireflea, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = FirefleaFrameToolingDefinitions.Bank });
        slot.SetSpawnParameter1(0);
        slot.SetSpawnParameter2(0);
        enemies.FirefleaBehavior.InitializeFireflea(slot);
        AssertEqual(AnimationProgramCursor<FirefleaProgram>.Start(FirefleaProgram.Loop), (enemies.SlotTable.Slots[0].EntityState<FirefleaEntity, FirefleaEnemyState>() ?? throw new InvalidOperationException("The Fireflea has no state.")).Program,
            "Fireflea initializer animation");
        slot.InstructionTimer = NativeCountdown.Start(1);
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(FirefleaAnimations.Programs, FirefleaProgram.Loop, FirefleaFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(FirefleaAnimations.Programs, FirefleaProgram.Loop)),
            "Fireflea runs its loop and returns through its goto");
        Console.WriteLine("  Fireflea behavior: the initializer starts and runs its loop.");
    }

    /// <summary>
    /// A vertically moving Fireflea spawned four pixels below the room's top edge has radius eight, so its upper extent
    /// wraps to 65532. The production main AI compares the position with both extents by the wrapped spatial difference:
    /// at its spawn Y it is inside both and keeps its speed entry; one pixel above the wrapped upper extent (65531), or at
    /// the lower extent (12), it reverses to the other half of the speed entry.
    /// </summary>
    internal static void VerifyFirefleaVerticalExtentsWrap()
    {
        var frame = new ClockedEnemyAiFrame(
            new RoomEnemyFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false },
            new FrameCounters(0));
        foreach ((ushort y, int expectedSpeedTableIndex) in new (ushort, int)[] { (4, 4), (65531, 0), (12, 0) })
        {
            var enemies = new RoomEnemySystem(new EnemySession());
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Fireflea, slot.Definition);
            slot.SetSpawnParameter1(0);
            slot.SetSpawnParameter2(0);
            slot.Position = slot.Position.WithWhole(new(128, 4));
            enemies.FirefleaBehavior.InitializeFireflea(slot);
            FirefleaEnemyState state = FirefleaBehavior.RequireFirefleaState(slot);
            AssertEqual(new Position16<RoomPixels>(0, 65532), state.MinimumYExtent, "the upper extent wraps past the room's top edge");
            AssertEqual(new Position16<RoomPixels>(0, 12), state.MaximumYExtent, "the lower extent is the spawn Y plus the radius");
            slot.Position = slot.Position.WithWholeY(y);

            FirefleaBehavior.RunFirefleaMainAi(slot, frame);

            AssertEqual(expectedSpeedTableIndex, state.SpeedTableIndex, $"Fireflea at Y {y} keeps or reverses its speed entry");
        }
        Console.WriteLine("  Fireflea vertical extents: positions compare with wrapped extents by the wrapped spatial difference.");
    }

    /// <summary>Runs the Fireflea behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--fireflea-instruction-program-definitions")]
    internal static int FirefleaBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyFirefleaBehavior), () => VerifyFirefleaBehavior());
        Suite(nameof(VerifyFirefleaVerticalExtentsWrap), () => VerifyFirefleaVerticalExtentsWrap());
        Suite(nameof(FirefleaAnimationsVerification.VerifyFirefleaAnimations), () => FirefleaAnimationsVerification.VerifyFirefleaAnimations(rom));
        return 0;
    }
}

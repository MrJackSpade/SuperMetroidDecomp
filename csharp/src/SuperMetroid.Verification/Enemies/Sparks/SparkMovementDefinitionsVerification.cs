using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Sparks;
using SuperMetroid.Core.Enemies.Sparks.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Sparks;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Spark's initial animation/function selections and how its animations run.</summary>
internal static class SparkMovementDefinitionsVerification
{
    /// <summary>
    /// The three authored selectors start the native list and function, and selector three keeps its native function
    /// overread while its list word, the following main-AI function pointer, starts no animation. Flicker-on makes the
    /// Spark tangible and runs into the active loop; flicker-out makes it intangible and holds; both loops run on time.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Spark selector tables")]
    internal static void VerifySparkMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: new TestAddressSpace(), services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().IsAreaBossDefeated = (Func<bool>)(() => true);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Spark, slot.Definition);
        for (ushort selector = 0; selector < 4; selector++)
        {
            ushort nativeList = CartridgeImportSource.Require(rom).ReadCartridgeWord(SparkNativeValues.InstructionListTable + selector * 2);
            var nativeFunction = (SparkEnemyFunction)CartridgeImportSource.Require(rom).ReadCartridgeWord(SparkNativeValues.FunctionTable + selector * 2);
            SparkMovementDefinition definition = SparkMovementDefinitions.InitialState(selector);
            ushort list = definition.Program is { } program
                ? NativeEnemyProgramOracle.EntryOf(SparkNativeListAddresses.Entries, program)
                : (ushort)SparkEnemyFunction.AlwaysActive;
            AssertEqual(nativeList, list, $"Spark list selector {selector}");
            AssertEqual(nativeFunction, definition.Function, $"Spark function selector {selector}");

            slot.SetSpawnParameter1(selector);
            slot.SetSpawnParameter2(5);
            enemies.SparkBehavior.InitializeSpark(slot);
            SparkEnemyState state = enemies.SparkStates[0] ?? throw new InvalidOperationException("Spark has no state.");
            AssertEqual(definition.Program is { } started ? AnimationProgramCursor<SparkProgram>.Start(started) : (AnimationProgramCursor<SparkProgram>?)null,
                state.Program, $"Spark production animation selector {selector}");
            AssertEqual(nativeFunction, state.Function, $"Spark production function selector {selector}");
        }
        foreach (ushort highBits in new ushort[] { 4, 0x0100, 0xffff })
            AssertEqual(SparkMovementDefinitions.InitialState((ushort)(highBits & 3)), SparkMovementDefinitions.InitialState(highBits),
                $"Spark selector mask {highBits:X4}");

        foreach (SparkProgram loop in new[] { SparkProgram.Active, SparkProgram.Emitter })
        {
            (RoomEnemySystem loopSystem, RoomEnemySlot loopSlot, _) = Start(loop, intangible: false);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(SparkAnimations.Programs, loop, SparkFrameDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(loopSlot, EnemyInstructionRunner.LoopUpdates(SparkAnimations.Programs, loop)), $"Spark {loop} loop");
        }
        (RoomEnemySystem onSystem, RoomEnemySlot on, SparkEnemyState onState) = Start(SparkProgram.FlickerOn, intangible: true);
        EnemyInstructionRunner.ShownFrames(on, 1);
        AssertTrue(!on.Properties.HasAny(EnemyProperties.IgnoreSamusCollision), "Spark flicker-on makes the actor tangible before its first flash");
        EnemyInstructionRunner.ShownFrames(on, 40);
        AssertEqual(SparkProgram.Active, onState.Program?.Program, "Spark flicker-on runs into the active loop");
        (RoomEnemySystem outSystem, RoomEnemySlot off, SparkEnemyState offState) = Start(SparkProgram.FlickerOut, intangible: false);
        EnemyInstructionRunner.ShownFrames(off, 40);
        AssertTrue(off.Properties.HasAny(EnemyProperties.IgnoreSamusCollision), "Spark flicker-out makes the actor intangible");
        AssertEqual(SparkProgram.FlickerOut, offState.Program?.Program, "Spark flicker-out holds");
        Console.WriteLine("  Spark: four native selections, the selector-three overread, both loops and both flickers with their collision cues pass.");
    }

    /// <summary>A Spark running <paramref name="program"/> from its first step.</summary>
    private static (RoomEnemySystem, RoomEnemySlot, SparkEnemyState) Start(SparkProgram program, bool intangible)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().IsAreaBossDefeated = (Func<bool>)(() => true);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Spark, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = SparkFrameToolingDefinitions.Bank });
        enemies.SparkBehavior.InitializeSpark(slot);
        SparkEnemyState state = enemies.SparkStates[0] ?? throw new InvalidOperationException("Spark has no state.");
        state.Program = AnimationProgramCursor<SparkProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Properties = EnemyPropertyWord.FromNativeWord((ushort)(intangible ? (ushort)EnemyProperties.IgnoreSamusCollision : (ushort)0));
        return (enemies, slot, state);
    }

    /// <summary>Runs the Spark checks and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--spark-instruction-mechanics")]
    internal static int SparkInstructionMechanicsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifySparkMovementDefinitions), () => VerifySparkMovementDefinitions(rom));
        Suite(nameof(SparkAnimationsVerification.VerifySparkAnimations), () => SparkAnimationsVerification.VerifySparkAnimations(rom));
        return 0;
    }
}

using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Yard;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Yard's crawling, turning, hiding, and airborne instruction entries against bank A3.</summary>
internal static class YardInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for the Yard animation checks.</summary>
    internal static void VerifyYardInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyYardInstructionProgramDefinitions), () => VerifyYardInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Every program runs for 80 forced updates; a hiding Yard's repeat cue keeps re-showing the pose before it, while a
    /// visible Yard with an even random draw runs on into the hidden animation; the program oracle matches bank $A3.
    /// </summary>
    internal static void VerifyYardInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        RoomEnemySystem enemies = CreateYardInstructionSystem(rom, out RoomEnemySlot yard);
        YardEnemyState state = (enemies.SlotTable.Slots[0].Entity as YardEntity)?.State ?? throw new InvalidOperationException("The Yard has no state.");
        foreach (YardProgram program in Enum.GetValues<YardProgram>())
        {
            state.Program = AnimationProgramCursor<YardProgram>.Start(program);
            RunYard(yard, 80);
        }

        // Hiding: the repeat cue resumes at the pose before it however often it runs.
        ushort repeatedPose = YardAnimations.Spritemaps(YardProgram.HidingUpsideUpMovingLeft).Last();
        state.Behavior = YardBehavior.Hiding;
        state.Program = AnimationProgramCursor<YardProgram>.Start(YardProgram.HidingUpsideUpMovingLeft);
        RunYard(yard, 20);
        AssertEqual(YardProgram.HidingUpsideUpMovingLeft, state.Program.Program, "a hiding Yard stays in its hiding animation");
        AssertEqual(repeatedPose, yard.SpritemapPointer, "a hiding Yard keeps re-showing the pose before the repeat cue");

        // Visible, with the fixture's even random draw: the cue runs on into the hidden animation.
        state.Behavior = YardBehavior.NormalCrawling;
        state.Program = AnimationProgramCursor<YardProgram>.Start(YardProgram.HidingUpsideUpMovingLeft);
        RunYard(yard, 3);
        AssertEqual(YardProgram.HiddenUpsideUpMovingLeft, state.Program.Program, "a visible Yard's even draw runs on into the hidden animation");

        Suite(nameof(YardAnimationsVerification.VerifyYardAnimations), () => YardAnimationsVerification.VerifyYardAnimations(rom));
        Console.WriteLine("Yard: all 38 programs run, and the repeat cue holds a hiding Yard and runs a visible one on.");
    }

    /// <summary>Forces the requested number of animation updates.</summary>
    private static void RunYard(RoomEnemySlot yard, int updates)
    {
        for (int update = 0; update < updates; update++)
        {
            yard.InstructionTimer = NativeCountdown.Start(1);
            EnemyInstructionInterpreter.ProcessInstructions(yard, null, null, Position16<RoomPixels>.Zero, (ushort)0);
        }
    }

    /// <summary>Creates a bank A3 Yard slot with deterministic random input and its behavior state installed.</summary>
    internal static RoomEnemySystem CreateYardInstructionSystem(
        ISnesAddressSpace bus,
        out RoomEnemySlot yard)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(bus);
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0);

        yard = enemies.SlotTable.Slots[0];
        yard.Bind(EnemyDefinitionId.Yard, yard.Definition);
        yard.Bind(yard.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xa3 });
        var state = new YardEnemyState(yard) { Program = AnimationProgramCursor<YardProgram>.Start(YardProgram.CrawlingUpsideUpMovingLeft) };
        yard.SetSpeciesState(state);
        return enemies;
    }

    /// <summary>Runs the Yard instruction comparison from the verification command dispatcher.</summary>
    [VerificationCommand("--yard-instruction-program-definitions")]
    internal static int YardInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyYardInstructionProgramDefinitions), () => VerifyYardInstructionProgramDefinitions());
        return 0;
    }
}

using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.TestSupport.MovieReplay.RidleyNativeListAddresses;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// #1287: Ridley's typed body, breakup-fragment and fireball programs against the retail cartridge. Each typed
/// program must match its native list step by step, and the body's facing and low-energy branches must redirect as
/// the native gotos do. The step-by-step walk also yields each typed step's native instruction address, which
/// <see cref="RidleyNativeInstructionPointers"/> uses to compare typed cursors with recorded native pointers.
/// </summary>
internal static class RidleyAnimationsVerification
{
    /// <summary>Every body, breakup and fireball program matches its native list, and the body's branches redirect natively.</summary>
    internal static void VerifyRidleyAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<RidleyBodyProgram>().Length, BodyEntries.Length, "every typed Ridley body program has a native entry");
        for (int index = 0; index < BodyEntries.Length; index++)
            AssertEqual((RidleyBodyProgram)index, BodyEntries[index].Program, "the Ridley body programs are declared in authored order");
        AssertTrue(BodyEntries.Zip(BodyEntries.Skip(1), (first, second) => first.Entry < second.Entry).All(ascends => ascends), "native Ridley body entries ascend");
        AssertEqual(Enum.GetValues<RidleyFireballProgram>().Length, FireballEntries.Length, "every typed fireball program has a native entry");

        var body = RidleyNativeProgramOracle.BodyStepAddresses(cartridge);
        RidleyNativeProgramOracle.BreakupStepAddresses(cartridge);
        var fireball = RidleyNativeProgramOracle.FireballStepAddresses(cartridge);
        AssertEqual(RidleyFireballVisualDefinitions.ProjectileFrameCount, RidleyFireballVisualDefinitions.ProjectileFrames.Count,
            "the fireball catalog lists every frame");

        VerifyFacingBranch();
        VerifyLowEnergyBranch();
        Console.WriteLine($"  Ridley programs: {body.Count} body programs ({body.Values.Sum(steps => steps.Length)} steps), " +
            $"{Enum.GetValues<RidleyBreakupFrame>().Length} breakup programs and {fireball.Count} fireball programs match their native lists; " +
            "the facing and low-energy branches redirect natively.");
    }

    /// <summary>Creates Ridley in slot zero of a fresh enemy system, facing <paramref name="facing"/>.</summary>
    private static NewRidleyResult NewRidley(EnemyDefinitionId definition, RidleyFacing facing)
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var state = new RidleyEnemyState { FacingDirection = facing };
        enemies.RidleyEncounter.Ridley = state;
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(definition, body.Definition);
        return new NewRidleyResult(enemies, state, body);
    }

    /// <summary>Runs one body update, expiring the current frame first.</summary>
    private static void Pass(RoomEnemySlot body, SamusState? samus)
    {
        body.FrameHold = WrappingCountdown.Start(1);
        EnemyInstructionInterpreter.RunsAnimationScript(body, samus, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);
    }

    /// <summary>A right-facing Ridley starting a left-facing entry runs its right-facing program in the same update.</summary>
    private static void VerifyFacingBranch()
    {
        (_, RidleyEnemyState state, RoomEnemySlot body) = NewRidley(EnemyDefinitionId.Ridley, RidleyFacing.Right);
        CeresRidleyFlight.StartRidleyProgram(state, body, RidleyBodyProgram.OpeningRoar);
        Pass(body, samus: null);
        AssertEqual(RidleyBodyProgram.OpeningRoarFacingRight, state.BodyProgram.Program, "a right-facing Ridley roars facing right");
        AssertEqual((RidleyCollisionDefinitions.BodyFrame?)RidleyCollisionDefinitions.BodyFrame.Right,
            RidleyCollisionDefinitions.BodyFrameAt(body.SpritemapPointer), "the right-facing roar shows its first frame at once");
        AssertEqual((ushort)6, body.FrameHold.RawValue, "the right-facing roar's first frame lasts six updates");

        (_, state, body) = NewRidley(EnemyDefinitionId.Ridley, RidleyFacing.Left);
        CeresRidleyFlight.StartRidleyProgram(state, body, RidleyBodyProgram.OpeningRoar);
        Pass(body, samus: null);
        AssertEqual(RidleyBodyProgram.OpeningRoar, state.BodyProgram.Program, "a left-facing Ridley keeps the left-facing roar");
        AssertEqual((RidleyCollisionDefinitions.BodyFrame?)RidleyCollisionDefinitions.BodyFrame.Left, RidleyCollisionDefinitions.BodyFrameAt(body.SpritemapPointer), "the left-facing roar shows its first frame");
    }

    /// <summary>
    /// In Ceres, a Samus under 30 energy cuts the fireballing short to its closing tail and shortens the hover to eight
    /// updates; Lower Norfair Ridley ignores her energy.
    /// </summary>
    private static void VerifyLowEnergyBranch()
    {
        (_, RidleyEnemyState state, RoomEnemySlot body) = NewRidley(EnemyDefinitionId.RidleyCeres, RidleyFacing.Left);
        state.Roaring = true;
        CeresRidleyFlight.StartRidleyProgram(state, body, RidleyBodyProgram.Fireballing);
        Pass(body, new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { Health = 29 });
        AssertEqual(RidleyBodyProgram.FireballingEnd, state.BodyProgram.Program, "low energy in Ceres skips to the fireballing tail");
        AssertEqual((ushort)8, state.FunctionTimer, "low energy shortens the hover to eight updates");
        AssertTrue(!state.Roaring, "the fireballing tail clears the roaring flag");
        AssertEqual((RidleyCollisionDefinitions.BodyFrame?)RidleyCollisionDefinitions.BodyFrame.Left, RidleyCollisionDefinitions.BodyFrameAt(body.SpritemapPointer), "the tail shows the standing pose");

        (_, state, body) = NewRidley(EnemyDefinitionId.RidleyCeres, RidleyFacing.Left);
        CeresRidleyFlight.StartRidleyProgram(state, body, RidleyBodyProgram.Fireballing);
        Pass(body, new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { Health = 30 });
        AssertEqual(RidleyBodyProgram.Fireballing, state.BodyProgram.Program, "thirty energy keeps the full fireballing");

        (_, state, body) = NewRidley(EnemyDefinitionId.Ridley, RidleyFacing.Left);
        CeresRidleyFlight.StartRidleyProgram(state, body, RidleyBodyProgram.Fireballing);
        Pass(body, new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { Health = 1 });
        AssertEqual(RidleyBodyProgram.Fireballing, state.BodyProgram.Program, "Lower Norfair Ridley ignores Samus's energy");
    }

    /// <summary>Runs Ridley's program parity check against the repository cartridge.</summary>
    [VerificationCommand("--ridley-animations")]
    internal static int RidleyAnimationsCommand()
    {
        Suite(nameof(VerifyRidleyAnimations), () => VerifyRidleyAnimations(LoadRepositoryRom()));
        return 0;
    }
}

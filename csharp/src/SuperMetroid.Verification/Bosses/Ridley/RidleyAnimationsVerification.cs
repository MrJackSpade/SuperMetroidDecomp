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
    internal static void VerifyRidleyAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<RidleyBodySegment>().Length, BodyEntries.Length, "every typed Ridley body program has a native entry");
        for (int index = 0; index < BodyEntries.Length; index++)
            AssertEqual((RidleyBodySegment)index, BodyEntries[index].Program, "the Ridley body programs are declared in authored order");
        AssertTrue(BodyEntries.Zip(BodyEntries.Skip(1), (first, second) => first.Entry < second.Entry).All(ascends => ascends), "native Ridley body entries ascend");
        AssertEqual(Enum.GetValues<RidleyFireballSegment>().Length, FireballEntries.Length, "every typed fireball program has a native entry");

        var body = RidleyNativeProgramOracle.BodyLists(cartridge);
        RidleyNativeProgramOracle.CompareBreakupLists(cartridge);
        var fireball = RidleyNativeProgramOracle.FireballLists(cartridge);
        AssertEqual(RidleyFireballVisualDefinitions.ProjectileFrameCount, RidleyFireballVisualDefinitions.ProjectileFrames.Count,
            "the fireball catalog lists every frame");

        VerifyFacingBranch();
        VerifyLowEnergyBranch();
        Console.WriteLine($"  Ridley animations: {body.Count} body segments ({body.Values.Sum(list => list.BeatAddresses.Length)} beats), " +
            $"{Enum.GetValues<RidleyBreakupFrame>().Length} breakup segments and {fireball.Count} fireball segments match their native lists; " +
            "the facing and low-energy branches redirect natively.");
    }

    /// <summary>Creates Ridley in slot zero of a fresh enemy system, facing <paramref name="facing"/>.</summary>
    private static NewRidleyResult NewRidley(EnemyDefinitionId definition, RidleyFacing facing)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var state = new RidleyEnemyState { FacingDirection = facing };
        enemies.RidleyEncounter.Ridley = state;
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(definition, body.Definition);
        return new NewRidleyResult(enemies, state, body);
    }

    /// <summary>Runs one body update, expiring the current frame first.</summary>
    private static void Pass(RoomEnemySlot body, SamusState? samus)
    {
        body.AnimationClock.StepOnNextUpdate();
        EnemyAnimationPlayer.AdvanceAnimation(body, samus, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);
    }

    /// <summary>A right-facing Ridley starting a left-facing entry runs its right-facing program in the same update.</summary>
    private static void VerifyFacingBranch()
    {
        (_, RidleyEnemyState state, RoomEnemySlot body) = NewRidley(EnemyDefinitionId.Ridley, RidleyFacing.Right);
        CeresRidleyFlight.StartRidleyAnimation(state, body, RidleyBodySegment.OpeningRoar);
        Pass(body, samus: null);
        AssertEqual(RidleyAnimations.StartingWith(RidleyBodySegment.OpeningRoarFacingRight), state.BodyPlayback.Animation, "a right-facing Ridley roars facing right");
        AssertEqual((RidleyCollisionDefinitions.BodyFrame?)RidleyCollisionDefinitions.BodyFrame.Right,
            RidleyCollisionDefinitions.BodyFrameAt(body.SpritemapPointer), "the right-facing roar shows its first frame at once");
        AssertEqual((ushort)6, body.AnimationClock.Hold.RawValue, "the right-facing roar's first frame lasts six updates");

        (_, state, body) = NewRidley(EnemyDefinitionId.Ridley, RidleyFacing.Left);
        CeresRidleyFlight.StartRidleyAnimation(state, body, RidleyBodySegment.OpeningRoar);
        Pass(body, samus: null);
        AssertEqual(RidleyAnimations.StartingWith(RidleyBodySegment.OpeningRoar), state.BodyPlayback.Animation, "a left-facing Ridley keeps the left-facing roar");
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
        CeresRidleyFlight.StartRidleyAnimation(state, body, RidleyBodySegment.Fireballing);
        Pass(body, new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Health = 29 });
        AssertEqual(RidleyAnimations.StartingWith(RidleyBodySegment.FireballingEnd), state.BodyPlayback.Animation, "low energy in Ceres skips to the fireballing tail");
        AssertEqual(8, state.FunctionTimer, "low energy shortens the hover to eight updates");
        AssertTrue(!state.Roaring, "the fireballing tail clears the roaring flag");
        AssertEqual((RidleyCollisionDefinitions.BodyFrame?)RidleyCollisionDefinitions.BodyFrame.Left, RidleyCollisionDefinitions.BodyFrameAt(body.SpritemapPointer), "the tail shows the standing pose");

        (_, state, body) = NewRidley(EnemyDefinitionId.RidleyCeres, RidleyFacing.Left);
        CeresRidleyFlight.StartRidleyAnimation(state, body, RidleyBodySegment.Fireballing);
        Pass(body, new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Health = 30 });
        AssertEqual(RidleyAnimations.StartingWith(RidleyBodySegment.Fireballing), state.BodyPlayback.Animation, "thirty energy keeps the full fireballing");

        (_, state, body) = NewRidley(EnemyDefinitionId.Ridley, RidleyFacing.Left);
        CeresRidleyFlight.StartRidleyAnimation(state, body, RidleyBodySegment.Fireballing);
        Pass(body, new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Health = 1 });
        AssertEqual(RidleyAnimations.StartingWith(RidleyBodySegment.Fireballing), state.BodyPlayback.Animation, "Lower Norfair Ridley ignores Samus's energy");
    }

    /// <summary>Runs Ridley's program parity check against the repository cartridge.</summary>
    [VerificationCommand("--ridley-animations")]
    internal static int RidleyAnimationsCommand()
    {
        Suite(nameof(VerifyRidleyAnimations), () => VerifyRidleyAnimations(LoadRepositoryRom()));
        return 0;
    }
}

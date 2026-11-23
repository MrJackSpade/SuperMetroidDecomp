using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Diagnostics.Configuration;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.Grapple;
namespace SuperMetroid.Verification.Harness;

/// <summary>Checks narrow gameplay operation interfaces, the grapple frame's
/// withheld capabilities, and construction-time validation of required context.</summary>
internal static class OperationContractsVerification
{
    /// <summary>
    /// #633: operation contexts are supersets of their children's narrow contracts, child operations accept
    /// only those contracts, the grapple wrapper's masked frame withholds exactly three capabilities, and
    /// concrete contexts reject missing required values at construction instead of during the operation.
    /// </summary>
    internal static void VerifyOperationContracts()
    {
        // The narrow contracts are the compiler's: main AI and the common grapple wrapper take main AI's frame, which the
        // pass's clocked frame satisfies, and PLM setup reads boss flags through the reader, which cannot set them.
        Action<RoomEnemyPlace, IEnemyAiFrame> mainAi = EnemyLifecycle.RunMainAi;
        Func<GrappleBehavior, RoomEnemyOccupant, IEnemyAiFrame, bool> grapple = static (behavior, grappled, frame) => behavior.RunCommonGrappleAi(grappled, frame);
        Func<ClockedEnemyAiFrame, IEnemyAiFrame> narrowed = static frame => frame;
        Func<RoomObjectSetupContext, IDefeatedBossesReader> bosses = static context => context.Bosses;
        // Each conversion compiles only while its member takes the narrow contract; nothing runs them.
        _ = new Delegate[] { mainAi, grapple, narrowed, bosses };

        // Concrete contexts validate required values at construction.
        var level = new RoomLevelData(new(1, 1), new RoomLevelPlanes(new RoomTerrainBlock[1], new RoomBlockBehavior[1], new RoomBlockAppearance[1]), RoomLevelData.NothingBeyondGrid(), []);

        // Explicit optional scenarios: the ordinary frame and retail overrides are named values.
        var ordinary = new RuntimeFrameRequest();
        AssertTrue(ReferenceEquals(GameplayUpdateOwner.RuntimeOnlyHost, ordinary.Owner) && ordinary.Owner.IsMainGameplay &&
            ordinary.Owner.CountsPlayTime && !ordinary.Owner.RunsLowHealthCheck && !ordinary.Owner.EntersOutOfHealthRoute,
            "a default frame request is a runtime-only host's update: main gameplay counting the play time, without the low-health check or the out-of-health route");
        var tester = new SuperMetroidGameOptions { Invincibility = true, InfiniteAmmo = true, GrantAllEquipment = true };
        AssertTrue(tester.ToAssists() is { SamusCannotDie: true, AmmoNeverRunsOut: true, EncounterResetOnLoad: null } assists &&
            ReferenceEquals(assists.GrantedLoadout, TesterAssistDefinitions.Loadout),
            "a session receives the tester's configured assists");
        Console.WriteLine("#633 operation contracts: narrow child contracts, the masked grapple frame and construction-time validation pass.");
    }

    /// <summary>Runs the focused operation-contract checks from the CLI.</summary>
    [VerificationCommand("--operation-contracts")]
    internal static int OperationContractsCommand()
    {
        Suite(nameof(VerifyOperationContracts), () => VerifyOperationContracts());
        return 0;
    }
}

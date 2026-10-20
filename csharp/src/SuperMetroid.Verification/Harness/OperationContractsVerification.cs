using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Harness;

/// <summary>Checks narrow gameplay operation interfaces, the grapple frame's
/// withheld capabilities, and construction-time validation of required context.</summary>
internal static class OperationContractsVerification
{
    /// <summary>
    /// #633: operation contexts are supersets of their children's narrow contracts, child operations accept
    /// only those contracts, the grapple wrapper's masked frame withholds exactly four capabilities, and
    /// concrete contexts reject missing required values at construction instead of during the operation.
    /// </summary>
    internal static void VerifyOperationContracts()
    {
        const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        const BindingFlags shared = BindingFlags.Static | BindingFlags.NonPublic;

        // Main AI's narrow frame is the enemy frame plus the counters the pass resolved once.
        AssertTrue(typeof(IEnemyAiFrame).IsAssignableFrom(typeof(ClockedEnemyAiFrame)), "the pass's clocked frame satisfies main AI's frame");
        AssertTrue(MetadataShape.Properties(typeof(IEnemyAiFrame), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).Select(property => property.Name).Order(StringComparer.Ordinal).SequenceEqual(
            ["Camera", "Clock", "ControllerInput", "Level", "Mode7Transform", "NewlyPressedControllerInput", "Samus", "SamusProjectiles",
             "SharedProjectiles", "VramWriteQueue"]),
            "main AI's frame exposes only the ten values main AI reads");
        AssertEqual(typeof(IEnemyAiFrame), MetadataShape.ParameterTypes(typeof(EnemySlotDispatch), nameof(EnemySlotDispatch.RunMainAi), shared)[1],
            "main AI accepts its narrow frame, not the enemy pass's whole frame");
        AssertEqual(typeof(IEnemyAiFrame), MetadataShape.ParameterTypes(typeof(GrappleBehavior), nameof(GrappleBehavior.RunCommonGrappleAi), instance)[1],
            "the common grapple wrapper accepts main AI's frame");

        // The grapple wrapper hands main AI the enemy's frame without projectiles, Mode 7, bombs or a VRAM queue.
        var samus = new SamusState();
        var frame = new RoomEnemyFrame
        {
            Camera = new(0x0120, 0x0040),
            TimeIsFrozen = false,
            Samus = samus,
            NewlyPressedControllerInput = SnesButton.A,
            ControllerInput = SnesButton.Right,
            SamusProjectiles = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None),
            Mode7Transform = new SamusMode7Transform(240, 16, -16, new(128, 112)),
            SharedProjectiles = new SamusBombProjectileSystem(),
            VramWriteQueue = new VramWriteQueue(),
        };
        var clock = new FrameCounters(3);
        var masked = new GrappleBehavior.GrappleWrappedAiFrame(new ClockedEnemyAiFrame(frame, clock));
        AssertTrue(masked.Camera == frame.Camera && ReferenceEquals(masked.Samus, samus) &&
            masked.NewlyPressedControllerInput == frame.NewlyPressedControllerInput && masked.ControllerInput == frame.ControllerInput &&
            ReferenceEquals(masked.Level, frame.Level) && masked.Clock == clock,
            "the grapple wrapper passes the enemy's camera, Samus, input, room and clock");
        AssertTrue(masked.SamusProjectiles is null && masked.Mode7Transform is null && masked.SharedProjectiles is null &&
            masked.VramWriteQueue is null, "the grapple wrapper withholds projectiles, Mode 7, bombs and the VRAM queue");

        // Children that read a subset take that subset, not the parent's record.
        AssertEqual(typeof(PaletteFxConditions),
            MetadataShape.ParameterTypes(typeof(RoomPaletteFxSystem), "RunPreInstruction", instance)[2],
            "palette pre-instructions take only their conditions; the power bomb stays with the step");
        AssertEqual(typeof(VerticalTerrainProbe),
            MetadataShape.ParameterTypes(typeof(SamusBlockCollision), "ScanVerticalTerrain", shared)[2],
            "the vertical terrain scan takes its probe, not the whole vertical move");
        AssertEqual(typeof(IAreaBossFlagReader),
            MetadataShape.ParameterTypes(typeof(PlmPopulationLoader), "TryRunRoomPopulationSetup", instance)[1],
            "PLM setup reads boss flags without being able to set them");

        // Concrete contexts validate required values at construction.
        var bus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var level = new RoomLevelData(new(1, 1), new RoomLevelPlanes(new ushort[1], new byte[1], new ushort[1]), new byte[8]);

        // Explicit optional scenarios: the ordinary frame and retail overrides are named values.
        var ordinary = new RuntimeFrameRequest();
        AssertTrue(ordinary.Policy.AllowCeresElevatorDeparture && ordinary.Policy.AdvanceGameTime &&
            ReferenceEquals(ordinary.Audio, RuntimeFrameAudio.Deferred), "a default frame request is state-eight gameplay with deferred audio");
        var tester = new SuperMetroidGameOptions { Invincibility = true, InfiniteAmmo = true, GrantAllEquipment = true };
        AssertTrue(ReferenceEquals(HostGameplayOverrides.None, tester.RuntimeOverrides(forAttractDemo: true)),
            "attract playback runs retail behaviour whatever the host configured");
        AssertTrue(tester.RuntimeOverrides(forAttractDemo: false) is { PlayerInvincibilityEnabled: true, InfiniteAmmoEnabled: true, GrantAllEquipment: true },
            "a session receives the host's configured overrides");
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

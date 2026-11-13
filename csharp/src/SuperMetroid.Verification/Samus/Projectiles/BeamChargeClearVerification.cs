using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>
/// Checks that the grapple wall jump clears the beam's charge itself (#1577): the original keeps one charge count, which
/// the wall jump's launch zeroes, so a charge built before it restarts from nothing on the next update.
/// </summary>
internal static class BeamChargeClearVerification
{
    /// <summary>The updates Shoot is held to build a charge before the clear.</summary>
    private const int ChargingUpdates = 40;

    /// <summary>
    /// Builds a charge in the retail runtime, makes the grapple wall jump's launch, runs one more update with Shoot still
    /// held, and compares the charge Samus reads with a run whose charge was also cleared the way a HUD selection clears
    /// it, which reaches the beam's own count.
    /// </summary>
    internal static void VerifyGrappleWallJumpClearsBeamCharge()
    {
        int wallJumpOnly = ChargeAfterWallJump(alsoClearThroughHudSelection: false, out int built);
        int fullyCleared = ChargeAfterWallJump(alsoClearThroughHudSelection: true, out _);
        AssertTrue(built >= ChargingUpdates / 2, $"the fixture builds a charge before the wall jump ({built})");
        AssertEqual(fullyCleared, wallJumpOnly,
            "the update after a grapple wall jump reads a charge restarted from nothing, as after a full charge clear");
        Console.WriteLine($"Grapple wall jump clears the beam charge: a charge of {built} restarts at {wallJumpOnly} on the next update.");
    }

    /// <summary>The charge Samus reads on the update after a grapple wall jump; <paramref name="built"/> is the charge before it.</summary>
    private static int ChargeAfterWallJump(bool alsoClearThroughHudSelection, out int built)
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.LandingSite);
        var level = runtime.Room.LoadedLevelData;
        // A flat clearing isolates the charge from slopes, the gunship and scenery.
        for (int y = 24; y < 36; y++)
        for (int x = 24; x < 40; x++)
        {
            int index = y * level.Size.X + x;
            level.SetTerrain(index, RoomTerrainBlock.Of(0, BlockMirroring.None,
                y >= 32 ? RoomCollisionType.SolidBlock : RoomCollisionType.Air));
            level.SetBehavior(index, RoomBlockBehavior.None);
        }
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.EquippedBeams = SamusBeamFlags.Charge;
        samus.Position = samus.Position.WithWhole(new(512, 490));
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        ControllerButton shoot = runtime.Options.ControllerBindings.Shoot;
        for (int update = 0; update < ChargingUpdates; update++)
            runtime.StepFrame(shoot);
        built = samus.BeamCharge.FlareCounter;

        samus.Pose = SamusPoseId.GrappleWallContactRightPose;
        SamusAerialPoseTransitions.ApplyGrappleWallJump(samus);
        if (alsoClearThroughHudSelection)
        {
            runtime.Projectiles.Firing.CancelChargeForHudSelection();
        }
        runtime.StepFrame(shoot);
        return samus.BeamCharge.FlareCounter;
    }

    /// <summary>Runs the grapple wall jump's charge clear check.</summary>
    [VerificationCommand("--grapple-wall-jump-charge-clear")]
    internal static int GrappleWallJumpClearsBeamChargeCommand()
    {
        Suite(nameof(VerifyGrappleWallJumpClearsBeamCharge), VerifyGrappleWallJumpClearsBeamCharge);
        return 0;
    }
}

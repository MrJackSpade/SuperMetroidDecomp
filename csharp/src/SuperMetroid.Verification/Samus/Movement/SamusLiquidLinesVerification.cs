using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>
/// Confirms that Samus's liquid tests, which compare her top and bottom with a surface plainly (#2528), decide every
/// centre and surface as the coordinate-range comparisons they replace did, her top lying above the room's top among them.
/// </summary>
internal static class SamusLiquidLinesVerification
{
    /// <summary>The live radius of the spinning body whose top the spin's submersion test reads.</summary>
    private const int SpinRadius = 21;

    /// <summary>Water and lava surfaces at, beside and well below the lines a body near the room's top reaches.</summary>
    private static readonly int[] Surfaces = [0, 1, 2, 10, 20, 21, 22, 40, 100, 279, 280];

    /// <summary>Compares the top and bottom tests over every centre from the room's top down 300 pixels.</summary>
    internal static void VerifySamusLiquidLines()
    {
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation,
            new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(),
            new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), InstalledFixtures.SamusArtwork()).InPose(SamusPoseId.SpinJumpRightPose);
        samus.Kinematics.Radius = samus.Kinematics.Radius.WithY(SpinRadius);
        int poseRadius = SamusPoseMetadata.ReadPoseYRadius(samus.Pose);
        int cases = 0;
        for (int centre = 0; centre <= 300; centre++)
        {
            samus.Position = FixedPosition<RoomPixels>.FromWhole(new(64, centre));
            int wrappedTop = CoordinateAxis.Wrap(centre - SpinRadius);
            int wrappedBottom = CoordinateAxis.Wrap(centre + poseRadius - 1);
            foreach (int surface in Surfaces)
            {
                samus.LiquidPhysics.ConfigureWater(surface, ignoresSurface: false, SuperMetroid.Core.Samus.Movement.WaterRoomFx.Water);
                AssertSameAt(CoordinateAxis.Difference(surface, wrappedTop) < 0, samus.LiquidPhysics.IsTopBoundarySubmerged(samus),
                    $"top under water at line {surface}", centre);
                AssertSameAt(CoordinateAxis.Difference(surface, wrappedBottom) < 0 ? SamusLiquidMedium.Water : SamusLiquidMedium.Air,
                    samus.LiquidPhysics.DetermineMovementMedium(samus), $"movement in water at line {surface}", centre);
                samus.LiquidPhysics.ConfigureLavaAcid(surface, SuperMetroid.Core.Samus.Movement.LavaAcidRoomFx.Lava);
                AssertSameAt(CoordinateAxis.Difference(surface, wrappedTop) < 0, samus.LiquidPhysics.IsTopBoundarySubmerged(samus),
                    $"top under lava at line {surface}", centre);
                AssertSameAt(CoordinateAxis.Difference(surface, wrappedBottom) < 0 ? SamusLiquidMedium.LavaOrAcid : SamusLiquidMedium.Air,
                    samus.LiquidPhysics.DetermineMovementMedium(samus), $"movement in lava at line {surface}", centre);
                cases++;
            }
        }

        Console.WriteLine($"Samus liquid lines: {cases} centres and surfaces, tops above the room's among them, decide as before.");
    }
}

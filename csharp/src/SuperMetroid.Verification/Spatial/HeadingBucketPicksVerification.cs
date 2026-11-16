using SuperMetroid.Core.Enemies.Sbug;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Verification.Spatial;

/// <summary>
/// Confirms the #1684 heading picks that replaced bucket numbers against the bucket arithmetic they replace: Shaktool's
/// head orientation and cue direction, Sbug's facings and the grapple rope's pictures, each for every heading it can be
/// given.
/// </summary>
internal static class HeadingBucketPicksVerification
{
    /// <summary>The compass directions clockwise from up, as the replaced bucket numbers counted them in eighths.</summary>
    private static readonly CompassDirection[] OctantDirections =
    [
        CompassDirection.Up, CompassDirection.UpRight, CompassDirection.Right, CompassDirection.DownRight,
        CompassDirection.Down, CompassDirection.DownLeft, CompassDirection.Left, CompassDirection.UpLeft,
    ];

    /// <summary>Runs every heading pick check.</summary>
    internal static void VerifyHeadingBucketPicks()
    {
        VerifyShaktoolOrientations();
        VerifySbugFacings();
        VerifyGrappleRopeOrientations();
        Console.WriteLine("  Heading bucket picks: Shaktool's 65536 orientations and eight cue directions, Sbug's 65536 facings and the rope's 65536 pictures equal the bucket arithmetic.");
    }

    /// <summary>
    /// Shaktool's head orientation for all 65536 midpoints against the replaced bucket, the midpoint eight steps on in
    /// eighths of a turn times 32, and each orientation's cue direction against that bucket read as an angle.
    /// </summary>
    private static void VerifyShaktoolOrientations()
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            RotationAngle midpoint = RotationAngle.FromRaw(raw);
            int bucket = midpoint.TurnedBySteps(8).Sector(8) * 32;
            AssertEqual(OctantDirections[bucket / 32], ShaktoolOrbitDefinitions.Orientations.At(midpoint), $"Shaktool midpoint {raw} orientation");
        }
        for (int octant = 0; octant < OctantDirections.Length; octant++)
            AssertEqual(new TrigonometryAngle(octant * 32), Bearing.Toward(OctantDirections[octant]).ToMirroredTrigonometryAngle(),
                $"Shaktool orientation {OctantDirections[octant]} cue direction");
    }

    /// <summary>Sbug's facing for all 65536 headings against the replaced sector of the heading 48 steps back.</summary>
    private static void VerifySbugFacings()
    {
        CompassDirection[] counterClockwiseFromRight =
        [
            CompassDirection.Right, CompassDirection.UpRight, CompassDirection.Up, CompassDirection.UpLeft,
            CompassDirection.Left, CompassDirection.DownLeft, CompassDirection.Down, CompassDirection.DownRight,
        ];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            RotationAngle heading = RotationAngle.FromRaw(raw);
            AssertEqual(counterClockwiseFromRight[heading.TurnedBySteps(-48).Sector(8)], SbugMovement.FacingOf(heading), $"Sbug heading {raw} facing");
        }
    }

    /// <summary>The rope's pictures for all 65536 grapple angles against the replaced sixty-fourth-of-a-turn count.</summary>
    private static void VerifyGrappleRopeOrientations()
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            RotationAngle angle = RotationAngle.FromRaw(raw);
            int halfTurnSector = angle.Sector(64) % 32;
            int fromUpright = Math.Min(halfTurnSector, 31 - halfTurnSector);
            GrappleRopeOrientation expected = fromUpright < 3 ? GrappleRopeOrientation.Vertical
                : fromUpright < 11 ? GrappleRopeOrientation.Diagonal
                : GrappleRopeOrientation.Horizontal;
            AssertEqual(expected, GrappleRopeOrientations.For(angle), $"grapple angle {raw} rope pictures");
        }
    }
}

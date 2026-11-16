namespace SuperMetroid.Verification.Spatial;

/// <summary>
/// Confirms the #1684 compass-direction picks Botwoon's head and segments face by against the octant arithmetic they
/// replace, for all 256 bearings, and each direction's bearing.
/// </summary>
internal static class CompassDirectionPicksVerification
{
    /// <summary>The compass directions clockwise from up, as the replaced octant numbers counted them.</summary>
    private static readonly CompassDirection[] OctantDirections =
    [
        CompassDirection.Up, CompassDirection.UpRight, CompassDirection.Right, CompassDirection.DownRight,
        CompassDirection.Down, CompassDirection.DownLeft, CompassDirection.Left, CompassDirection.UpLeft,
    ];

    /// <summary>Botwoon's octant and nearest-octant picks for all 256 bearings, and each direction's bearing.</summary>
    internal static void VerifyCompassDirectionPicks()
    {
        for (int units = 0; units < 256; units++)
        {
            var bearing = new Bearing(units);
            AssertEqual(OctantDirections[units >> 5], bearing.DirectionOfEighth(), $"bearing {units} octant direction");
            AssertEqual(OctantDirections[((units + 16) & 255) >> 5], bearing.NearestDirection(), $"bearing {units} nearest direction");
        }
        for (int octant = 0; octant < OctantDirections.Length; octant++)
            AssertEqual(new Bearing(octant * 32), Bearing.Toward(OctantDirections[octant]), $"octant {octant} bearing");
        AssertThrows<ArgumentOutOfRangeException>(() => Bearing.Toward((CompassDirection)99), "undefined compass direction bearing");
        Console.WriteLine("  Compass direction picks: 256 bearings' octant and nearest directions equal the octant arithmetic, and each direction's bearing is its eighths.");
    }
}

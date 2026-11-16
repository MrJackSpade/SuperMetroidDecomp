using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the enemies' one view (#2025) against each cull it replaced: the edge-inclusive view the projectile culls,
/// Fake Kraid, Magdollite, Shutters, the Yapping Maw and the on-screen centre used; the view excluding its far edges
/// that the dust, Skree particle, Magdollite lava and pirate culls used; and the Evir's widened view.
/// </summary>
internal static class EnemyViewVerification
{
    /// <summary>The camera positions checked: the room's corner, an ordinary place, both sides of the half-space and the wrap.</summary>
    private static readonly int[] Cameras = [0, 100, 32_600, 32_768, 65_400];

    /// <summary>The point offsets from the camera checked on each axis: every offset around both edges and the far half-space.</summary>
    private static IEnumerable<int> Offsets() =>
        Enumerable.Range(-700, 1_400).Concat(Enumerable.Range(32_500, 600)).Concat(Enumerable.Range(-33_100, 600));

    /// <summary>
    /// Compares the view's three tests with the replaced inline comparisons, across the room's wrap, at every offset of
    /// <see cref="Offsets"/> on one axis with the other axis inside, and at sampled offsets on both axes together.
    /// </summary>
    internal static void VerifyEnemyView()
    {
        int compared = 0;
        foreach (int camera in Cameras)
        {
            var cameraPosition = new Position16<RoomPixels>(camera, camera);
            int[] offsets = [.. Offsets()];
            foreach (int x in offsets)
            {
                foreach (int y in new[] { 0, 128, 256, 257, -1, x })
                {
                    var point = new Position16<RoomPixels>(CoordinateAxis.Wrap(camera + x), CoordinateAxis.Wrap(camera + y));
                    Compare(point, cameraPosition, $"camera {camera}, offset ({x}, {y})");
                    compared++;
                }
            }
        }
        Console.WriteLine($"  Enemy view: {compared} points agree with every inline cull the view replaced.");
    }

    /// <summary>Asserts the view's tests at <paramref name="point"/> equal the inline comparisons they replaced.</summary>
    private static void Compare(Position16<RoomPixels> point, Position16<RoomPixels> camera, string label)
    {
        // The edge-inclusive cull Cacatac's spike shared with Fune, Namihe and Polyp.
        int right = CoordinateAxis.Wrap(camera.X + 256);
        int bottom = CoordinateAxis.Wrap(camera.Y + 256);
        bool sharedOutside = CoordinateAxis.Difference(point.X, camera.X) < 0 ||
            CoordinateAxis.Difference(right, point.X) < 0 ||
            CoordinateAxis.Difference(point.Y, camera.Y) < 0 ||
            CoordinateAxis.Difference(bottom, point.Y) < 0;
        AssertEqual(!sharedOutside, EnemyView.Contains(point, camera), $"{label}: the shared projectile cull");

        // Botwoon's spit, Fake Kraid's sounds, Shutters' sound and the Yapping Maw.
        bool signedOnScreen = CoordinateAxis.Difference(point.X, camera.X) >= 0 &&
            CoordinateAxis.AsSigned(camera.X + 256 - point.X) >= 0 &&
            CoordinateAxis.Difference(point.Y, camera.Y) >= 0 &&
            CoordinateAxis.AsSigned(camera.Y + 256 - point.Y) >= 0;
        AssertEqual(signedOnScreen, EnemyView.Contains(point, camera), $"{label}: the signed on-screen test");

        // Magdollite's sound and the enemy centre's on-screen test.
        bool precedesOffScreen = CoordinateAxis.Precedes(point.X, camera.X) ||
            CoordinateAxis.Precedes(camera.X + 256, point.X) ||
            CoordinateAxis.Precedes(point.Y, camera.Y) ||
            CoordinateAxis.Precedes(camera.Y + 256, point.Y);
        AssertEqual(!precedesOffScreen, EnemyView.Contains(point, camera), $"{label}: the on-screen centre test");

        // The dust and pirate culls, which already count the far edges as outside.
        bool strictOutside = CoordinateAxis.Difference(point.X, camera.X) < 0 ||
            CoordinateAxis.Difference(point.X, camera.X + 256) >= 0 ||
            CoordinateAxis.Difference(point.Y, camera.Y) < 0 ||
            CoordinateAxis.Difference(point.Y, camera.Y + 256) >= 0;
        AssertEqual(!strictOutside, EnemyView.ContainsBeforeFarEdges(point, camera), $"{label}: the dust cull");

        // Magdollite's lava and the Skree particles, measured from the camera.
        bool unsignedOutside = CoordinateAxis.Wrap(point.X - camera.X) >= 256 || CoordinateAxis.Wrap(point.Y - camera.Y) >= 256;
        AssertEqual(!unsignedOutside, EnemyView.ContainsBeforeFarEdges(point, camera), $"{label}: the lava cull");

        // The Evir's spit, which regenerates once a further screen's width outside the view.
        const int evirMargin = 256;
        bool farOffscreen =
            CoordinateAxis.Precedes(point.X + evirMargin, camera.X) ||
            CoordinateAxis.Precedes(camera.X + 256 + evirMargin, point.X) ||
            CoordinateAxis.Precedes(point.Y + evirMargin, camera.Y) ||
            CoordinateAxis.Precedes(camera.Y + 256 + evirMargin, point.Y);
        AssertEqual(!farOffscreen, EnemyView.ContainsWithMargin(point, camera, evirMargin), $"{label}: the Evir's far test");
    }

    /// <summary>Runs the enemy view check from the verification command line.</summary>
    [VerificationCommand("--enemy-view")]
    internal static int EnemyViewCommand()
    {
        VerifyEnemyView();
        return 0;
    }
}

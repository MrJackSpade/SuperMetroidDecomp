using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>
/// Checks the released soul's exit through the production soul-motion step: the soul is deleted exactly when its Y
/// lands in an odd 256-pixel screen row, including the wrap above the room's top edge.
/// </summary>
internal static class TourianStatueSoulExitVerification
{
    /// <summary>The soul's launch speed: four pixels up per update.</summary>
    private const int RisePixels = 4;

    /// <summary>Steps a soul once from each start height and compares deletion with the screen row it reaches.</summary>
    internal static void VerifyTourianStatueSoulExit()
    {
        // Wrapping above the top edge, staying in the top row, leaving an odd row, entering an odd row, staying in an even row.
        int[] startHeights = [2, 100, 258, 400, 600];
        foreach (int start in startHeights)
        {
            var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
            RoomEnemyProjectileSlot soul = enemies.Projectiles.EnemyProjectiles[0];
            soul.PreInstruction = EnemyProjectilePreInstruction.TourianStatueSoulMotion;
            soul.Position = FixedPosition<RoomPixels>.FromWhole(new Position16<RoomPixels>(132, checked((ushort)start)));
            soul.Velocity = soul.Velocity.WithY(new SignedEightEight(unchecked((ushort)(-RisePixels * 256))));

            AssertTrue(enemies.TourianUnlockBehavior.TryStepTourianUnlockEffect(soul), $"soul motion runs from Y {start}");

            int reached = (start - RisePixels + 65536) % 65536;
            bool oddScreenRow = reached / 256 % 2 == 1;
            AssertEqual(reached, (int)soul.Position.Y.Whole, $"soul rises four pixels from Y {start}");
            AssertEqual(oddScreenRow, soul.Animation is TourianStatueProjectileAnimation { Cursor.Program: TourianStatueProjectileProgram.Delete },
                $"soul from Y {start} is deleted only on reaching an odd screen row");
        }
        Console.WriteLine("Tourian statue soul exit: deletion follows the screen row reached, including the top-edge wrap.");
    }

    /// <summary>Runs the focused soul-exit check.</summary>
    [VerificationCommand("--tourian-statue-soul-exit")]
    internal static int TourianStatueSoulExitCommand()
    {
        VerifyTourianStatueSoulExit();
        return 0;
    }
}

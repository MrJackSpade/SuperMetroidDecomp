using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

/// <summary>Exact first-shot assertions for the recording attached to #1159.</summary>
internal static class ReportedShaftDoorConfirmation
{
    internal static int Run(string recording, string installedRoot, string trace)
    {
        int baselinePlms = -1;
        bool firstShotAccepted = false;
        bool capCleared = false;
        int exit = InstalledInputReplay.Run(recording, installedRoot, 6105, 6124, trace,
            (game, frame) =>
            {
                if (frame < 6105) return;
                var runtime = game.RuntimeForVerification ?? throw new InvalidDataException("Reported door frame lacks runtime.");
                if (runtime.ActiveRoom?.Pointer != 0x96ba)
                    throw new InvalidDataException("Recording is not the reported Climb door fixture.");
                var level = runtime.LevelData ?? throw new InvalidDataException("Reported door frame lacks terrain.");
                if (frame == 6105)
                {
                    baselinePlms = runtime.Plms.ActiveCount;
                    Require(level.GetCollisionBlock(30, 134).CollisionKind == RoomCollisionType.ShootableBlock,
                        "cap begins closed/shootable before the first shot");
                }
                if (frame == 6106)
                {
                    Require(runtime.Projectiles.LastFrameResult.FiredSlot is not null,
                        "first Shoot edge actually produces a projectile");
                    Require(runtime.Projectiles.LastFrameResult.CollisionStartedExplosion,
                        "point-blank first shot reacts before moving beyond the cap");
                    Require(runtime.Plms.ActiveCount == baselinePlms + 1,
                        "first shot allocates the opening actor");
                    firstShotAccepted = true;
                }
                if (frame == 6124)
                {
                    for (int row = 134; row < 138; row++)
                        Require(level.GetCollisionBlock(30, row).CollisionKind == RoomCollisionType.Air,
                            $"the first opening actor clears cap row {row} at the native 18-tick endpoint");
                    capCleared = true;
                }
            });
        Require(firstShotAccepted && capCleared, "both reported first-shot checkpoints were observed");
        Console.WriteLine("#1159: first recorded shot opens all four cap blocks on the native timer; no door collision change justified by this recording.");
        return exit;
    }

    private static void Require(bool condition, string property)
    {
        if (!condition) throw new InvalidDataException("#1159 confirmation failed: " + property);
    }
}

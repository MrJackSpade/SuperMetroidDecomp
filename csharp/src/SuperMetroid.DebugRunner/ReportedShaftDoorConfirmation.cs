using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

/// <summary>Both reported Climb shots, compared with the original CPU fixture for #1159.</summary>
internal static class ReportedShaftDoorConfirmation
{
    internal static int Run(string recording, string installedRoot, string trace)
    {
        int baselinePlms = -1;
        bool firstShotMissObserved = false;
        bool stoppedShotAccepted = false;
        bool capCleared = false;
        int exit = InstalledInputReplay.Run(recording, installedRoot, 5976, 6124, trace,
            (game, frame) =>
            {
                if (frame < 5976) return;
                var runtime = game.RuntimeForVerification ?? throw new InvalidDataException("Reported door frame lacks runtime.");
                if (runtime.ActiveRoom?.Pointer != RoomHeaderPointers.Climb)
                    throw new InvalidDataException("Recording is not the reported Climb door fixture.");
                var level = runtime.LevelData ?? throw new InvalidDataException("Reported door frame lacks terrain.");
                if (frame == 5976)
                {
                    baselinePlms = runtime.Plms.ActiveCount;
                    Require(runtime.Samus is { Pose: SamusPoseIds.MovingRightNormalPose, XPosition: 475 },
                        "the first shot starts from the reported running pose at the wall");
                    Require(level.GetCollisionBlock(30, 134).CollisionKind == RoomCollisionType.ShootableBlock,
                        "cap begins closed/shootable before the first shot");
                }
                if (frame == 5977)
                {
                    Require(runtime.Projectiles.LastFrameResult.FiredSlot is not null,
                        "first Shoot edge actually produces a projectile");
                    Require(runtime.Projectiles.LastFrameResult.CollisionStartedExplosion,
                        "the initial shot explodes against the transition block behind the cap");
                    // The original CPU probe obtains exactly this explosion anchor and
                    // does not allocate a cap actor. Running's longer muzzle offset puts
                    // the leading-radius scan in column 31, beyond the cap in column 30.
                    Require(runtime.Projectiles.LastFiredProjectileSnapshot is { XPosition: 498, YPosition: 2179 },
                        "the first-shot impact anchor matches the original CPU");
                    Require(runtime.Plms.ActiveCount == baselinePlms,
                        "the running shot misses the cap just as on the original CPU");
                    firstShotMissObserved = true;
                }
                if (frame == 5995)
                {
                    Require(level.GetCollisionBlock(30, 134).CollisionKind == RoomCollisionType.ShootableBlock,
                        "the first shot really leaves the door closed rather than only delaying its animation");
                }
                if (frame == 6106)
                {
                    Require(runtime.Projectiles.LastFrameResult.FiredSlot is not null &&
                        runtime.Projectiles.LastFiredProjectileSnapshot is { XPosition: 494, YPosition: 2182 },
                        "the wall-stopped shot matches the original CPU's shorter muzzle offset");
                    Require(runtime.Plms.ActiveCount == baselinePlms + 1,
                        "the wall-stopped shot allocates the cap actor");
                    stoppedShotAccepted = true;
                }
                if (frame == 6124)
                {
                    for (int row = 134; row < 138; row++)
                        Require(level.GetCollisionBlock(30, row).CollisionKind == RoomCollisionType.Air,
                            $"the opening actor clears cap row {row} at the native 18-tick endpoint");
                    capCleared = true;
                }
            });
        Require(firstShotMissObserved && stoppedShotAccepted && capCleared, "both actual shot outcomes were observed");
        Console.WriteLine("#1159: the running first-shot miss and later wall-stopped opening match the original cartridge CPU; gameplay is unchanged.");
        return exit;
    }

    private static void Require(bool condition, string property)
    {
        if (!condition) throw new InvalidDataException("#1159 confirmation failed: " + property);
    }
}

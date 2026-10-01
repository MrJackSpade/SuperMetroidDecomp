using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

/// <summary>Exact terrain and passage assertions for #1160's supplied Morph Ball recording.</summary>
internal static class ReportedMorphBlockConfirmation
{
    internal static int Run(string recording, string installedRoot, string trace)
    {
        int baselinePlms = -1;
        bool breakConfirmed = false;
        bool passageConfirmed = false;
        int exit = InstalledInputReplay.Run(recording, installedRoot, 8568, 9406, trace,
            (game, frame) =>
            {
                if (frame < 8568) return;
                var runtime = game.RuntimeForVerification ?? throw new InvalidDataException("Reported Morph Ball frame lacks runtime.");
                Require(runtime.ActiveRoom?.Pointer == 0x9e9f, "recorded sequence remains in the reported Morph Ball room");
                var level = runtime.LevelData ?? throw new InvalidDataException("Reported Morph Ball frame lacks terrain.");
                RoomCollisionBlock block = level.GetCollisionBlock(76, 44);
                if (frame == 8568)
                {
                    baselinePlms = runtime.Plms.ActiveCount;
                    Require(block.CollisionKind == RoomCollisionType.ShootableBlock && block.Behavior == 4,
                        "reported passage starts as the permanent 1x1 shot block");
                }
                if (frame == 8569)
                {
                    Require(runtime.Projectiles.LastFrameResult.FiredSlot is not null &&
                        runtime.Projectiles.LastFrameResult.CollisionStartedExplosion,
                        "recorded first shot hits the progression block");
                    Require(block.LevelWord == 0x0053, "first compiled break frame replaces solid collision");
                }
                if (frame == 8573) Require(block.LevelWord == 0x0054, "second break frame at four ticks");
                if (frame == 8577) Require(block.LevelWord == 0x0055, "third break frame at eight ticks");
                if (frame == 8582)
                {
                    Require(block.LevelWord == 0x00ff && runtime.Plms.ActiveCount == baselinePlms,
                        "break program permanently clears the block and deletes its actor");
                    breakConfirmed = true;
                }
                if (frame == 9406)
                {
                    Require(runtime.Samus is { } samus && samus.Pose == SamusPoseIds.MorphBallGroundRightPose &&
                        samus.XPosition == 1242 && samus.YPosition == 713,
                        "recorded Morph Ball movement passes fully through the cleared opening");
                    passageConfirmed = true;
                }
            });
        Require(breakConfirmed && passageConfirmed, "both break and passage checkpoints observed");
        Console.WriteLine("#1160: recorded shot clears the Morph Ball progression block and Samus rolls through; production fix is #1157's compiled operand handoff.");
        return exit;
    }

    private static void Require(bool condition, string property)
    {
        if (!condition) throw new InvalidDataException("#1160 confirmation failed: " + property);
    }
}

using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

/// <summary>Shared deterministic geometry and player seed for search and fixed-input replay.</summary>
internal static class BombTraversalFixture
{
    public static SuperMetroidRuntime Create(string rom, bool ceiling, bool left = false)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = FlatFloorMovementFixture.Create(bus, false, wideRunway: true);
        foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
        foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
        var level = runtime.LevelData!;
        for (int x = 0; x < level.WidthInBlocks; x++)
            level.SetForegroundEntry((ceiling ? 12 : 0) * level.WidthInBlocks + x, 0x8000);
        var samus = runtime.Samus!;
        samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
        samus.XPosition = 128; samus.YPosition = 249;
        samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
        samus.PoseId = left ? SamusPoseId.MorphBallGroundLeftPose : SamusPoseId.MorphBallGroundRightPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.SetAnimationFrameFromSpecialHandler(0, 1);
        samus.PoseHistory.PreviousPose = samus.Pose;
        samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(((byte)SamusMovementType.MorphBallGround << 8) | (left ? 4 : 8));
        samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
        runtime.Controller1.Latch(0);
        return runtime;
    }
}

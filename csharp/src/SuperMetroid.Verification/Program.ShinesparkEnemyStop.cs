using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyShinesparkEnemyStop()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var level = CreateRoom(16, 16, new ushort[256], new byte[256]);
        foreach (bool frozen in new[] { false, true })
        foreach (int gap in new[] { 4, 20 })
        {
            var samus = new SamusState { XPosition = 100, YPosition = 100, Health = 399 };
            samus.Shinespark.TryStoreFromSpeedBooster(SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter);
            samus.Shinespark.BeginWindup(samus);
            samus.Shinespark.BeginDirectionalLaunch(bus, samus, SamusPoseIds.ShinesparkVerticalLeftPose);
            samus.Kinematics.YSubposition = ushort.MaxValue;
            samus.Kinematics.InteractiveEnemies =
            [new SolidEnemyCollisionBody(Index: 0, XPosition: 100, YPosition: (ushort)(100 - 19 - 8 - gap),
                XRadius: 8, YRadius: 8, FreezeTimer: (ushort)(frozen ? 1 : 0),
                Properties: (ushort)(frozen ? EnemyProperties.None : EnemyProperties.SolidToSamus))];
            uint initialY = samus.Kinematics.YFixed;
            var result = samus.Shinespark.Step(bus, level, samus, 0);
            if (gap == 4)
            {
                AssertEqual(initialY, samus.Kinematics.YFixed, "spark enemy hit retains exact crash Y instead of advancing to boundary");
                AssertEqual(0, result.Vertical!.Value.AcceptedDisplacement, "spark solid-enemy stop accepts no vertical displacement");
                AssertTrue(result.Vertical.Value.EnemyCollision is { Collided: true }, "spark stop retains enemy collision evidence");
                AssertEqual(ShinesparkPhase.Crash, samus.Shinespark.Phase, "spark enemy hit starts crash");
            }
            else
            {
                AssertTrue(samus.Kinematics.YFixed < initialY, "clear spark enemy probe still moves upward");
                AssertEqual(ShinesparkPhase.Vertical, samus.Shinespark.Phase, "distant enemy does not force crash");
            }
        }
    }
}

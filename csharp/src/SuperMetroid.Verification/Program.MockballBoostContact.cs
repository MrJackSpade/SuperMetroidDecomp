using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyMockballBoostContact()
    {
        // The report concerns the retained boost after a mockball has landed. Exercise
        // the real moving-ball handler, then the enemy contact pass that consumes its
        // publication on the following frame, as the native alpha/beta ordering does.
        foreach (SamusPoseId pose in new[] { SamusPoseId.MorphBallMovingRightPose, SamusPoseId.SpringBallMovingRightPose })
        {
            var samus = new SamusState
            {
                Pose = pose, XPosition = 64, YPosition = 57,
                EquippedItems = (ushort)(SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.MorphBall),
            };
            var fixture = CreateEnemyDropFixture(samus, [1]);
            samus.RefreshCollisionRadii(fixture.Bus);
            ushort[] blocks = new ushort[32 * 8];
            for (int x = 0; x < 32; x++) blocks[4 * 32 + x] = 0x8000;
            var floor = CreateRoom(32, 8, blocks, new byte[blocks.Length]);
            var speed = samus.HorizontalSpeed;
            speed.HasRunningMomentum = true;
            speed.ExtraRunSpeed = 7;
            speed.SpeedBoostCounter = 0x0401;
            var enemy = fixture.System.Slots[0];
            enemy.EnemyDefinitionPointer = (EnemyDefinitionId)0x9000;
            enemy.Definition = default(RoomEnemyDefinition) with
            {
                Bank = 0xa3, TouchAiPointer = EnemyAiCodePointers.BankA0.NormalEnemyTouch,
                VulnerabilityPointer = EnemyVulnerabilityDefinitions.DefaultPointer,
            };
            enemy.XRadius = enemy.YRadius = 8;
            enemy.SpritemapPointer = 0x8000;
            for (int frame = 0; frame < 3; frame++)
            {
                speed.ContactDamageIndex = 0; // Runtime beta clears before movement.
                SamusMorphBallMovement.StepGrounded(fixture.Bus, floor, samus, (ushort)frame);
                enemy.XPosition = samus.XPosition;
                enemy.YPosition = samus.YPosition;
                enemy.Health = 1000;
                samus.InvincibilityTimer = 1; // Non-attacking control cannot hurt Samus.
                typeof(RoomEnemySystem).GetMethod("DetermineWhichEnemiesToProcess",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .Invoke(fixture.System, new object[] { (ushort)0, (ushort)0 });
                fixture.System.ResolveOrdinarySamusContact(samus, 0, floor);
                AssertEqual(500, enemy.Health, $"pose {(int)pose:X2} mockball frame {frame} inflicts native 500 boost damage");
                AssertEqual(1, speed.ContactDamageIndex, "moving ball republishes Speed Booster contact attack");
                AssertEqual(7, speed.ExtraRunSpeed, "moving ball retains boost momentum");
            }
            speed.SpeedBoostCounter = 0x0301;
            speed.ContactDamageIndex = 0;
            SamusMorphBallMovement.StepGrounded(fixture.Bus, floor, samus, 4);
            AssertEqual(0, speed.ContactDamageIndex, "sub-threshold boost does not grant a ball contact attack");
            speed.SpeedBoostCounter = 0x0401;
            speed.ContactDamageIndex = 0;
            samus.Pose = pose == SamusPoseId.MorphBallMovingRightPose
                ? SamusPoseId.MorphBallGroundRightPose : SamusPoseId.SpringBallGroundRightPose;
            speed.AccelerationMode = 0;
            SamusMorphBallMovement.StepGrounded(fixture.Bus, floor, samus, 5);
            AssertEqual(0, speed.ContactDamageIndex, "stationary ball does not run the moving-ball boost publication");
            AssertEqual(0, speed.SpeedBoostCounter, "stopping retains native boost cancellation");
        }
        Console.WriteLine("Mockball boost: moving Morph/Spring Ball republishes contact damage for consecutive frames, deals 500 damage, and respects boost threshold and stopping.");
    }
}

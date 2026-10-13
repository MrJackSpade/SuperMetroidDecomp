using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static EnemyDropsVerification;
using static VerificationHelpersVerification;

internal static class MockballBoostContactVerification
{
    internal static void VerifyMockballBoostContact()
    {
        // The report concerns the retained boost after a mockball has landed. Exercise
        // the real moving-ball handler, then the enemy contact pass that consumes its
        // publication on the following frame, as the native alpha/beta ordering does.
        foreach (SamusPoseId pose in new[] { SamusPoseId.MorphBallMovingRightPose, SamusPoseId.SpringBallMovingRightPose })
        {
            var samus = new SamusState
            {
                Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(64, 57)),
                EquippedItems = (ushort)(SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.MorphBall),
            };
            var fixture = CreateEnemyDropFixture(samus, [1]);
            samus.PoseMetadata.RefreshCollisionRadii(fixture.Bus);
            ushort[] blocks = new ushort[32 * 8];
            for (int x = 0; x < 32; x++) blocks[4 * 32 + x] = 0x8000;
            var floor = CreateRoom(new(32, 8), blocks, new byte[blocks.Length]);
            var speed = samus.HorizontalSpeed;
            speed.HasRunningMomentum = true;
            speed.ExtraRunSpeed = 7;
            speed.SpeedBoostCounter = 0x0401;
            var enemy = fixture.System.SlotTable.Slots[0];
            enemy.EnemyDefinitionPointer = (EnemyDefinitionId)0x9000;
            enemy.Definition = default(RoomEnemyDefinition) with
            {
                Bank = 0xa3, TouchAiPointer = EnemyAiCodePointers.BankA0.NormalEnemyTouch,
                VulnerabilityPointer = EnemyVulnerabilityDefinitions.DefaultPointer,
            };
            enemy.Radius = new(8, 8);
            enemy.SpritemapPointer = 0x8000;
            for (int frame = 0; frame < 3; frame++)
            {
                speed.ContactDamageIndex = SamusContactDamageIndex.None; // Runtime beta clears before movement.
                SamusMorphBallMovement.StepGrounded(fixture.Bus, floor, samus, (ushort)frame);
                enemy.Position = enemy.Position.WithWhole(samus.Position.Whole);
                enemy.Health = 1000;
                samus.InvincibilityTimer = NativeCountdown.Start(1); // Non-attacking control cannot hurt Samus.
                fixture.System.ProcessingWindow.DetermineWhichEnemiesToProcess(Position16<RoomPixels>.Zero);
                fixture.System.Collision.ResolveOrdinarySamusContact(samus, 0, floor);
                AssertEqual(500, enemy.Health, $"pose {(int)pose:X2} mockball frame {frame} inflicts native 500 boost damage");
                AssertEqual(SamusContactDamageIndex.SpeedBooster, speed.ContactDamageIndex, "moving ball republishes Speed Booster contact attack");
                AssertEqual(7, speed.ExtraRunSpeed, "moving ball retains boost momentum");
            }
            speed.SpeedBoostCounter = 0x0301;
            speed.ContactDamageIndex = SamusContactDamageIndex.None;
            SamusMorphBallMovement.StepGrounded(fixture.Bus, floor, samus, 4);
            AssertEqual(SamusContactDamageIndex.None, speed.ContactDamageIndex, "sub-threshold boost does not grant a ball contact attack");
            speed.SpeedBoostCounter = 0x0401;
            speed.ContactDamageIndex = SamusContactDamageIndex.None;
            samus.Pose = pose == SamusPoseId.MorphBallMovingRightPose
                ? SamusPoseId.MorphBallGroundRightPose : SamusPoseId.SpringBallGroundRightPose;
            speed.AccelerationMode = 0;
            SamusMorphBallMovement.StepGrounded(fixture.Bus, floor, samus, 5);
            AssertEqual(SamusContactDamageIndex.None, speed.ContactDamageIndex, "stationary ball does not run the moving-ball boost publication");
            AssertEqual(0, speed.SpeedBoostCounter, "stopping retains native boost cancellation");
        }
        Console.WriteLine("Mockball boost: moving Morph/Spring Ball republishes contact damage for consecutive frames, deals 500 damage, and respects boost threshold and stopping.");
    }

    [VerificationCommand("--mockball-boost-contact")]
    internal static int MockballBoostContactCommand()
    {
     VerifyMockballBoostContact(); return 0; 
    }
}

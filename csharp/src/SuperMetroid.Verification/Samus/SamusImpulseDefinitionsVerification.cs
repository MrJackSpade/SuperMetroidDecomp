using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Samus;

/// <summary>Checks Samus bomb-jump and hurt launch impulses against native medium-dependent values.</summary>
internal static class SamusImpulseDefinitionsVerification
{
    /// <summary>Compares bomb launches and knockback transitions across liquid edges, equipment, pose, and direction with physics reads blocked.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus impulse definitions")]
    internal static void VerifySamusImpulseDefinitions(SuperMetroidAddressSpace rom)
    {
        var noReads = NewSlopeHeightNoReadBus();
        // Bomb jumps start in morph ball. Since #1258 the medium test samples the current pose's
        // native collision radius, so the liquid threshold is that pose's bottom edge.
        var bomb = new SamusState { Pose = SamusPoseId.MorphBallGroundRightPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)) };
        int bombBottom = 128 + SamusPoseCollisionDefinitions.ReadVerticalRadius(bomb.Pose) - 1;
        int bombCases = 0;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        for (int liquid = 1; liquid <= 2; liquid++)
        for (int gravity = 0; gravity <= 1; gravity++)
        for (int direction = 1; direction <= 3; direction++)
        {
            bomb.EquippedItems = (ushort)(SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpeedBooster |
                (gravity == 1 ? SamusEquipmentFlags.GravitySuit : 0));
            bomb.HorizontalSpeed.ExtraRunSpeed = ushort.MaxValue;
            bomb.HorizontalSpeed.ExtraRunSubspeed = ushort.MaxValue;
            bomb.LiquidPhysics.FxYPosition = liquid == 1 ? (ushort)raw : ushort.MaxValue;
            bomb.LiquidPhysics.LavaAcidYPosition = liquid == 2 ? (ushort)raw : ushort.MaxValue;
            bomb.BombJumpDirection = (ushort)(0x0800 | direction);
            bomb.BombJumpStarting = true;
            bomb.BombJumpActive = false;
            bomb.Kinematics.YAcceleration = 7;
            bomb.Kinematics.YSubacceleration = 123;
            SamusBombJumpMovement.Start(noReads, bomb);
            int medium = gravity == 0 && raw < bombBottom ? liquid : 0;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x909ef5 + medium * 2)), bomb.Kinematics.YSpeed, "Bomb native whole launch");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x909efb + medium * 2)), bomb.Kinematics.YSubspeed, "Bomb native fractional launch");
            AssertEqual(SamusVerticalDirection.Up, bomb.Kinematics.YDirection, "Bomb upward launch");
            AssertEqual((ushort)7, bomb.Kinematics.YAcceleration, "Bomb launch retains live whole gravity");
            AssertEqual((ushort)123, bomb.Kinematics.YSubacceleration, "Bomb launch retains live fractional gravity");
            AssertEqual((ushort)128, bomb.Position.Y.Whole, "Bomb setup does not move Samus");
            AssertTrue(bomb.BombJumpActive && !bomb.BombJumpStarting, "Bomb mover handoff");
            bombCases++;
        }

        var poseOnly = NewImpulsePoseReadGuard(rom);
        SamusPoseId[] poses = [SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose,
            SamusPoseId.MorphBallGroundRightPose, SamusPoseId.MorphBallGroundLeftPose];
        int hurtCases = 0;
        foreach (SamusPoseId pose in poses)
        for (int environment = 0; environment < 4; environment++)
        for (int equipment = 0; equipment < 8; equipment++)
        for (ushort side = 0; side <= 1; side++)
        foreach (SnesButton input in new SnesButton[] { 0, SnesButton.Left, SnesButton.Right })
        {
            var hurt = new SamusState { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)) };
            hurt.PoseMetadata.RefreshCollisionRadii(poseOnly);
            hurt.EquippedItems = (ushort)(((equipment & 1) != 0 ? SamusEquipmentFlags.HiJumpBoots : 0) |
                ((equipment & 2) != 0 ? SamusEquipmentFlags.SpeedBooster : 0) |
                ((equipment & 4) != 0 ? SamusEquipmentFlags.GravitySuit : 0));
            hurt.LiquidPhysics.FxYPosition = environment is 1 or 3 ? (ushort)0 : ushort.MaxValue;
            hurt.LiquidPhysics.LavaAcidYPosition = environment == 2 ? (ushort)0 : ushort.MaxValue;
            hurt.LiquidPhysics.LiquidOptions = environment == 3 ? (ushort)4 : (ushort)0;
            hurt.BombJumpDirection = 0x0801;
            hurt.BombJumpStarting = true;
            hurt.BombJumpActive = true;
            hurt.HorizontalSpeed.ExtraRunSpeed = ushort.MaxValue;
            hurt.HorizontalSpeed.ExtraRunSubspeed = ushort.MaxValue;
            hurt.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.SpeedBooster;
            AssertTrue(SamusKnockbackMovement.Start(poseOnly, hurt, input, new KnockbackLaunch { KnockbackXDirection = side, KnockbackTimer = 11 }), "Native hurt branch admitted");
            int medium = (equipment & 4) != 0 || environment == 3 ? 0 : environment;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x909ee9 + medium * 2)), hurt.Kinematics.YSpeed, "Hurt native whole launch");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x909eef + medium * 2)), hurt.Kinematics.YSubspeed, "Hurt native fractional launch");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x909ea1 + medium * 2)), hurt.Kinematics.YSubacceleration, "Hurt refreshes native gravity");
            AssertEqual((ushort)11, hurt.KnockbackTimer.RawValue, "Hurt retains producer timer");
            AssertTrue(hurt.KnockbackActive && !hurt.BombJumpActive && !hurt.BombJumpStarting, "Hurt replaces bomb mover");
            AssertEqual((ushort)0, hurt.BombJumpDirection, "Hurt clears bomb direction");
            AssertEqual((ushort)1, hurt.HurtFlashCounter, "Hurt starts flash");
            AssertEqual(SamusContactDamageIndex.None, hurt.HorizontalSpeed.ContactDamageIndex, "Hurt cancels contact attack");
            hurtCases++;
        }
        Console.WriteLine($"Samus impulse definitions: {bombCases} surface/equipment/direction bomb launches and {hurtCases} hurt transitions pass; physics ROM reads forbidden.");
    }

    /// <summary>Allows remaining pose or animation reads in bank $91 while rejecting physics ROM reads and writes.</summary>
    internal static CartridgeReadGuard NewImpulsePoseReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => SnesAddressMath.BankNumber(address) != 0x91,
                address => $"Impulse test permits only remaining pose/animation reads, got ${address:X6}.")
            .RejectWrites(_ => "Unexpected impulse bus write.");
}

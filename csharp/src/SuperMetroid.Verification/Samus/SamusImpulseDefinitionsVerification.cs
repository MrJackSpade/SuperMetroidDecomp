using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Samus;

/// <summary>Checks Samus bomb-jump and hurt launch impulses against native medium-dependent values.</summary>
internal static class SamusImpulseDefinitionsVerification
{
    /// <summary>Compares bomb launches and knockback transitions across liquid edges, equipment, pose, and direction with physics reads blocked.</summary>
    internal static void VerifySamusImpulseDefinitions(CartridgeImportAddressSpace rom)
    {
        // Bomb jumps start in morph ball. Since #1258 the medium test samples the current pose's
        // native collision radius, so the liquid threshold is that pose's bottom edge.
        var bomb = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)) }.InPose(SamusPoseId.MorphBallGroundRightPose);
        int bombBottom = 128 + SamusPoseMetadata.ReadPoseYRadius(bomb.Pose) - 1;
        int bombCases = 0;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        for (int liquid = 1; liquid <= 2; liquid++)
        for (int gravity = 0; gravity <= 1; gravity++)
        foreach (SamusBombJumpDirection direction in new[] { SamusBombJumpDirection.Left, SamusBombJumpDirection.Straight, SamusBombJumpDirection.Right })
        {
            bomb.EquippedItems = (SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpeedBooster | (gravity == 1 ? SamusEquipmentFlags.GravitySuit : 0));
            bomb.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(ushort.MaxValue, ushort.MaxValue);
            // A surface word from 32768 up lies above the room, where the game's sign test finds no surface.
            int? surface = raw <= short.MaxValue ? raw : null;
            bomb.LiquidPhysics.Surface = (liquid, surface) switch
            {
                (1, int line) => new global::SuperMetroid.Core.Samus.Movement.SamusLiquidSurface.Water(line, IgnoredUntilEnabled: false),
                (2, int line) => new global::SuperMetroid.Core.Samus.Movement.SamusLiquidSurface.LavaOrAcid(line),
                _ => global::SuperMetroid.Core.Samus.Movement.SamusLiquidSurface.None,
            };
            bomb.BombJump = SamusBombJump.FromBlast(direction).Arm();
            bomb.BombJumpPhase = SuperMetroid.Core.Samus.Projectiles.SamusBombJumpPhase.Starting;
            bomb.Kinematics.YAcceleration = new UnsignedSixteenSixteen(7, 123);
            SamusBombJumpMovement.Start(bomb);
            int medium = gravity == 0 && raw < bombBottom ? liquid : 0;
            AssertEqual(rom.ReadCartridgeWord((0x909ef5 + medium * 2)), bomb.Kinematics.VerticalVelocity.Speed.Whole, "Bomb native whole launch");
            AssertEqual(rom.ReadCartridgeWord((0x909efb + medium * 2)), bomb.Kinematics.VerticalVelocity.Speed.Fraction, "Bomb native fractional launch");
            AssertEqual(SamusVerticalDirection.Up, bomb.Kinematics.VerticalVelocity.Direction, "Bomb upward launch");
            AssertEqual(7, bomb.Kinematics.YAcceleration.Whole, "Bomb launch retains live whole gravity");
            AssertEqual(123, bomb.Kinematics.YAcceleration.Fraction, "Bomb launch retains live fractional gravity");
            AssertEqual((ushort)128, bomb.Position.Y.Whole, "Bomb setup does not move Samus");
            AssertTrue(bomb.BombJumpPhase == SamusBombJumpPhase.Rising, "Bomb mover handoff");
            bombCases++;
        }

        SamusPoseId[] poses = [SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose,
            SamusPoseId.MorphBallGroundRightPose, SamusPoseId.MorphBallGroundLeftPose];
        int hurtCases = 0;
        foreach (SamusPoseId pose in poses)
        for (int environment = 0; environment < 4; environment++)
        for (int equipment = 0; equipment < 8; equipment++)
        for (ushort side = 0; side <= 1; side++)
        foreach (GameplayAction input in new GameplayAction[] { 0, GameplayAction.Left, GameplayAction.Right })
        {
            var hurt = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)) }.InPose(pose);
            SamusPoseMetadata.RefreshCollisionRadii(hurt);
            hurt.EquippedItems = (((equipment & 1) != 0 ? SamusEquipmentFlags.HiJumpBoots : 0) | ((equipment & 2) != 0 ? SamusEquipmentFlags.SpeedBooster : 0) | ((equipment & 4) != 0 ? SamusEquipmentFlags.GravitySuit : 0));
            hurt.LiquidPhysics.Surface = environment switch
            {
                1 or 3 => new global::SuperMetroid.Core.Samus.Movement.SamusLiquidSurface.Water(0, IgnoredUntilEnabled: environment == 3),
                2 => new global::SuperMetroid.Core.Samus.Movement.SamusLiquidSurface.LavaOrAcid(0),
                _ => global::SuperMetroid.Core.Samus.Movement.SamusLiquidSurface.None,
            };
            hurt.BombJump = SamusBombJump.FromBlast(SamusBombJumpDirection.Left).Arm();
            hurt.BombJumpPhase = SuperMetroid.Core.Samus.Projectiles.SamusBombJumpPhase.Starting;
            hurt.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(ushort.MaxValue, ushort.MaxValue);
            hurt.HorizontalSpeed.BodyAttack = SamusBodyAttack.SpeedBooster;
            AssertTrue(SamusKnockbackMovement.Start(hurt, input, new KnockbackLaunch { KnockbackXDirection = side == 0 ? KnockbackHorizontalDirection.Left : KnockbackHorizontalDirection.Right, KnockbackTimer = 11 }, poseCollision: null), "Native hurt branch admitted");
            int medium = (equipment & 4) != 0 || environment == 3 ? 0 : environment;
            AssertEqual(rom.ReadCartridgeWord((0x909ee9 + medium * 2)), hurt.Kinematics.VerticalVelocity.Speed.Whole, "Hurt native whole launch");
            AssertEqual(rom.ReadCartridgeWord((0x909eef + medium * 2)), hurt.Kinematics.VerticalVelocity.Speed.Fraction, "Hurt native fractional launch");
            AssertEqual(rom.ReadCartridgeWord((0x909ea1 + medium * 2)), hurt.Kinematics.YAcceleration.Fraction, "Hurt refreshes native gravity");
            AssertEqual(11, hurt.KnockbackHold.UpdatesLeft, "Hurt retains producer timer");
            AssertTrue(hurt.KnockbackActive && hurt.BombJumpPhase is null, "Hurt replaces bomb mover");
            AssertEqual(null, hurt.BombJump, "Hurt drops the bomb-jump request");
            AssertEqual(1, hurt.HurtFlash.Count, "Hurt starts flash");
            AssertEqual(SamusBodyAttack.None, hurt.HorizontalSpeed.BodyAttack, "Hurt cancels contact attack");
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

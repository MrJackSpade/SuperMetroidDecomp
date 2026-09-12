using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifySamusImpulseDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int a) => (ushort)(rom.ReadByte(a) | rom.ReadByte(a + 1) << 8);
        var noReads = new SlopeHeightNoReadBus();
        var bomb = new SamusState { XPosition = 128, YPosition = 128 };
        bomb.Kinematics.YRadius = 12;
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
            var result = SamusBombJumpMovement.Start(noReads, bomb);
            int medium = gravity == 0 && raw < 139 ? liquid : 0;
            AssertEqual(Word(0x909ef5 + medium * 2), bomb.Kinematics.YSpeed, "Bomb native whole launch");
            AssertEqual(Word(0x909efb + medium * 2), bomb.Kinematics.YSubspeed, "Bomb native fractional launch");
            AssertEqual((ushort)1, bomb.Kinematics.YDirection, "Bomb upward launch");
            AssertEqual((ushort)7, bomb.Kinematics.YAcceleration, "Bomb launch retains live whole gravity");
            AssertEqual((ushort)123, bomb.Kinematics.YSubacceleration, "Bomb launch retains live fractional gravity");
            AssertEqual((ushort)128, bomb.YPosition, "Bomb setup does not move Samus");
            AssertTrue(result.Started && bomb.BombJumpActive && !bomb.BombJumpStarting, "Bomb mover handoff");
            bombCases++;
        }

        var poseOnly = new ImpulsePoseReadGuard(rom);
        byte[] poses = [SamusPoseIds.FacingRightNormalPose, SamusPoseIds.FacingLeftNormalPose,
            SamusPoseIds.MorphBallGroundRightPose, SamusPoseIds.MorphBallGroundLeftPose];
        int hurtCases = 0;
        foreach (byte pose in poses)
        for (int environment = 0; environment < 4; environment++)
        for (int equipment = 0; equipment < 8; equipment++)
        for (ushort side = 0; side <= 1; side++)
        foreach (ushort input in new ushort[] { 0, (ushort)SnesButton.Left, (ushort)SnesButton.Right })
        {
            var hurt = new SamusState { Pose = pose, XPosition = 128, YPosition = 128 };
            hurt.RefreshCollisionRadii(poseOnly);
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
            hurt.HorizontalSpeed.ContactDamageIndex = 1;
            AssertTrue(SamusKnockbackMovement.Start(poseOnly, hurt, input, side, knockbackTimer: 11), "Native hurt branch admitted");
            int medium = (equipment & 4) != 0 || environment == 3 ? 0 : environment;
            AssertEqual(Word(0x909ee9 + medium * 2), hurt.Kinematics.YSpeed, "Hurt native whole launch");
            AssertEqual(Word(0x909eef + medium * 2), hurt.Kinematics.YSubspeed, "Hurt native fractional launch");
            AssertEqual(Word(0x909ea1 + medium * 2), hurt.Kinematics.YSubacceleration, "Hurt refreshes native gravity");
            AssertEqual((ushort)11, hurt.KnockbackTimer, "Hurt retains producer timer");
            AssertTrue(hurt.KnockbackActive && !hurt.BombJumpActive && !hurt.BombJumpStarting, "Hurt replaces bomb mover");
            AssertEqual((ushort)0, hurt.BombJumpDirection, "Hurt clears bomb direction");
            AssertEqual((ushort)1, hurt.HurtFlashCounter, "Hurt starts flash");
            AssertEqual((ushort)0, hurt.HorizontalSpeed.ContactDamageIndex, "Hurt cancels contact attack");
            hurtCases++;
        }
        Console.WriteLine($"Samus impulse definitions: {bombCases} surface/equipment/direction bomb launches and {hurtCases} hurt transitions pass; physics ROM reads forbidden.");
    }

    private sealed class ImpulsePoseReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address >> 16 == 0x91
            ? source.ReadByte(address)
            : throw new InvalidOperationException($"Impulse test permits only remaining pose/animation reads, got ${address:X6}.");
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected impulse bus write.");
    }
}

using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares compiled Kraid arm component hitbox selectors and enumeration order with native frame records, including component and pose bounds.</summary>
    /// <param name="rom">Retail address space containing Kraid's arm-frame component records.</param>
    private static void VerifyKraidArmComponentHitboxes(SuperMetroidAddressSpace rom)
    {
        ushort[] frames = NativeKraidArmPhysicalFrames(rom);
        int checkedComponents = 0;
        for (int ordinal = 0; ordinal < frames.Length; ordinal++)
        {
            ushort pointer = frames[ordinal];
            int pose = ordinal < 20 ? ordinal % 10 : ordinal - 10;
            int count = ReadKraidArmInstructionWord(rom, pointer);
            AssertTrue(KraidArmCollisionDefinitions.TryGetComponents(pointer, out var actual), "Native arm frame accepted");
            AssertEqual(count, actual.Length, "Native arm component count");
            int component = 0;
            foreach (var value in actual)
            {
                ushort expected = ReadKraidArmInstructionWord(rom, (ushort)(pointer + 8 + 8 * component));
                AssertEqual(expected, KraidArmCollisionDefinitions.ComponentHitbox(pose, component), "Native component selector");
                AssertEqual(expected, value.HitboxPointer, "Hitbox selector through production enumeration");
                AssertEqual(value, actual[component], "Component enumeration preserves order");
                component++;
                checkedComponents++;
            }
            AssertEqual(count, component, "Arm component enumeration ends");
            AssertThrows<IndexOutOfRangeException>(() => _ = actual[-1], "Negative arm component index");
            AssertThrows<IndexOutOfRangeException>(() => _ = actual[count], "Past arm component index");
            AssertThrows<IndexOutOfRangeException>(() => KraidArmCollisionDefinitions.ComponentHitbox(pose, -1), "Negative hitbox component");
            AssertThrows<IndexOutOfRangeException>(() => KraidArmCollisionDefinitions.ComponentHitbox(pose, count), "Past hitbox component");
        }
        AssertEqual(102, checkedComponents, "All native arm component selectors checked");
        AssertThrows<IndexOutOfRangeException>(() => KraidArmCollisionDefinitions.ComponentHitbox(-1, 0), "Negative arm pose");
        AssertThrows<IndexOutOfRangeException>(() => KraidArmCollisionDefinitions.ComponentHitbox(12, 0), "Past arm pose");
    }
}

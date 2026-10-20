using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Kraid.KraidArmCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidArmPhysicalFrameMappingsVerification;
using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks Kraid arm component hitbox selectors in all native physical frames.</summary>
internal static class KraidArmComponentHitboxesVerification
{
    /// <summary>Compares 102 selectors, native component counts, enumeration order, and pose/index bounds.</summary>
    internal static void VerifyKraidArmComponentHitboxes(SuperMetroidAddressSpace rom)
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
                AssertEqual(KraidArmCollisionDefinitions.HitboxesAt(expected), value.Hitboxes,
                    "Rectangles the native selector names, through production enumeration");
                AssertEqual(value, actual[component], "Component enumeration preserves order");
                component++;
                checkedComponents++;
            }
            AssertEqual(count, component, "Arm component enumeration ends");
            AssertThrows<IndexOutOfRangeException>(() => _ = actual[-1], "Negative arm component index");
            AssertThrows<IndexOutOfRangeException>(() => _ = actual[count], "Past arm component index");
            AssertThrows<ArgumentOutOfRangeException>(() => KraidArmCollisionDefinitions.ComponentHitbox(pose, -1), "Negative hitbox component");
            AssertThrows<ArgumentOutOfRangeException>(() => KraidArmCollisionDefinitions.ComponentHitbox(pose, count), "Past hitbox component");
        }
        AssertEqual(102, checkedComponents, "All native arm component selectors checked");
        AssertThrows<ArgumentOutOfRangeException>(() => KraidArmCollisionDefinitions.ComponentHitbox(-1, 0), "Negative arm pose");
        AssertThrows<ArgumentOutOfRangeException>(() => KraidArmCollisionDefinitions.ComponentHitbox(12, 0), "Past arm pose");
    }
}

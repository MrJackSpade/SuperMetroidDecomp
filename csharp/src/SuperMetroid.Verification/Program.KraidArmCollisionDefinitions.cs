using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidArmCollisionDefinitions(
        SuperMetroidAddressSpace rom)
    {
        VerifyKraidArmTouchCallbacks(rom);
        VerifyKraidArmShotCallbacks(rom);
        VerifyKraidArmPhysicalFramePointers(rom);
        HashSet<ushort> hitboxPointers = VerifyKraidArmPhysicalLayoutSelection(rom);

        AssertEqual(16, hitboxPointers.Count,
            "Kraid arm selected frames reference all 16 physical hitbox lists");
        int hitboxCount = 0;
        foreach (ushort pointer in hitboxPointers)
        {
            KraidArmHitboxSequence compiled =
                KraidArmCollisionDefinitions.HitboxesAt(pointer);
            AssertEqual(ReadKraidArmInstructionWord(rom, pointer), compiled.Length,
                $"Kraid arm hitbox count $A7:{pointer:X4}");
            for (int index = 0; index < compiled.Length; index++)
            {
                KraidArmCollisionHitbox hitbox = compiled[index];
                ushort record = unchecked((ushort)(pointer + 2 + index * 12));
                AssertEqual(unchecked((short)ReadKraidArmInstructionWord(rom, record)),
                    hitbox.Left, $"Kraid arm hitbox left $A7:{record:X4}");
                AssertEqual(unchecked((short)ReadKraidArmInstructionWord(rom,
                        unchecked((ushort)(record + 2)))),
                    hitbox.Top, $"Kraid arm hitbox top $A7:{record:X4}");
                AssertEqual(unchecked((short)ReadKraidArmInstructionWord(rom,
                        unchecked((ushort)(record + 4)))),
                    hitbox.Right, $"Kraid arm hitbox right $A7:{record:X4}");
                AssertEqual(unchecked((short)ReadKraidArmInstructionWord(rom,
                        unchecked((ushort)(record + 6)))),
                    hitbox.Bottom, $"Kraid arm hitbox bottom $A7:{record:X4}");
                hitboxCount++;
            }
        }
        AssertEqual(24, hitboxCount, "Kraid arm compiled physical rectangles");
        AssertThrows<InvalidDataException>(
            () => KraidArmCollisionDefinitions.HitboxesAt(0x8000).ToArray(),
            "uncatalogued Kraid arm hitbox pointer fails loudly");
        Console.WriteLine(
            "Kraid arm physical definitions: 22 frame maps, 16 hitbox lists, " +
            "and 24 rectangles/callback pairs match the retail ROM.");
    }
}

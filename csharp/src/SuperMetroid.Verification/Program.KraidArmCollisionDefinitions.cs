using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidArmCollisionDefinitions(
        SuperMetroidAddressSpace rom)
    {
        AssertEqual(22, KraidArmCollisionDefinitions.FrameCount,
            "Kraid arm physical frame count");
        HashSet<ushort> hitboxPointers = [];
        for (int frameIndex = 0; frameIndex < KraidArmCollisionDefinitions.FrameCount;
             frameIndex++)
        {
            ushort pointer = KraidArmCollisionDefinitions.FramePointer(frameIndex);
            AssertTrue(KraidArmCollisionDefinitions.TryGetComponents(
                    pointer, out var compiled),
                $"Kraid arm physical frame $A7:{pointer:X4} exists");
            AssertEqual(rom.ReadByte(0xa70000 | pointer), compiled.Length,
                $"Kraid arm physical component count $A7:{pointer:X4}");
            for (int componentIndex = 0; componentIndex < compiled.Length;
                 componentIndex++)
            {
                KraidArmCollisionComponent component = compiled.Span[componentIndex];
                ushort record = unchecked((ushort)(pointer + 2 + componentIndex * 8));
                AssertEqual(unchecked((short)ReadKraidArmInstructionWord(rom, record)),
                    component.X, $"Kraid arm component X $A7:{record:X4}");
                AssertEqual(unchecked((short)ReadKraidArmInstructionWord(rom,
                        unchecked((ushort)(record + 2)))),
                    component.Y, $"Kraid arm component Y $A7:{record:X4}");
                AssertEqual(ReadKraidArmInstructionWord(rom,
                        unchecked((ushort)(record + 6))),
                    component.HitboxPointer,
                    $"Kraid arm hitbox pointer $A7:{record:X4}");
                hitboxPointers.Add(component.HitboxPointer);
            }
        }

        AssertEqual(16, hitboxPointers.Count,
            "Kraid arm selected frames reference all 16 physical hitbox lists");
        int hitboxCount = 0;
        foreach (ushort pointer in hitboxPointers)
        {
            ReadOnlySpan<KraidArmCollisionHitbox> compiled =
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
                AssertEqual(ReadKraidArmInstructionWord(rom,
                        unchecked((ushort)(record + 8))),
                    hitbox.TouchAi, $"Kraid arm touch AI $A7:{record:X4}");
                AssertEqual(ReadKraidArmInstructionWord(rom,
                        unchecked((ushort)(record + 10))),
                    hitbox.ShotAi, $"Kraid arm shot AI $A7:{record:X4}");
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

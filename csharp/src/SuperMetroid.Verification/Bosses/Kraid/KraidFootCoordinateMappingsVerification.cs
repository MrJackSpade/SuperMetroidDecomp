using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks Kraid foot component coordinates and shared hitboxes against native frames.</summary>
internal static class KraidFootCoordinateMappingsVerification
{
    /// <summary>$A7:8F35, ExtendedSpritemap_KraidFoot_21, which no instruction list shows.</summary>
    private const ushort UnshownFrame = 0x8f35;

    /// <summary>
    /// Compares both components' offsets of the 35 native walk frames and the small-Kraid initial frame with each foot
    /// frame's authored components (#1867).
    /// </summary>
    internal static void VerifyKraidFootCoordinates(CartridgeImportAddressSpace rom)
    {
        for (int component = 0; component < 2; component++)
        {
            VerifyKraidFootCoordinate(rom, component, vertical: false);
            VerifyKraidFootCoordinate(rom, component, vertical: true);
        }
    }

    /// <summary>Compares one coordinate of one component across the native foot frames with production's.</summary>
    private static void VerifyKraidFootCoordinate(CartridgeImportAddressSpace rom, int component, bool vertical)
    {
        ushort Word(int pointer) => LittleEndianBytes.Word(KraidFootCoordinateRomByte(rom, 0xa70000 | pointer), KraidFootCoordinateRomByte(rom, 0xa70000 | (pointer + 1)));
        for (int ordinal = 0; ordinal <= 35; ordinal++)
        {
            ushort pointer = ordinal == 35 ? (ushort)0xa565 : (ushort)(0x8ce3 + 18 * ordinal);
            short expected = unchecked((short)Word(pointer + 2 + component * 8 + (vertical ? 2 : 0)));
            // No instruction list shows ExtendedSpritemap_KraidFoot_21, so it is no frame Kraid's foot can show.
            if (pointer == UnshownFrame)
            {
                AssertTrue(RoomEnemySlotFrameAccess.TryFrameAt(KraidFootCollisionDefinitions.Bank, pointer) is null, "Unshown foot frame has no identity");
                continue;
            }
            AssertTrue(KraidFootCollision.TryGetComponents(pointer, out var components), "Native foot components exist");
            AssertEqual(2, components.Length, "Foot has two components");
            var actual = components[component];
            AssertEqual(expected, vertical ? actual.Offset.Y : actual.Offset.X, "Foot coordinate through production view");
        }
    }

    /// <summary>Reads one native foot component byte solely as the verification oracle.</summary>
    private static byte KraidFootCoordinateRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks the shared foot rectangle and exhaustive native component-frame membership.</summary>
    internal static void VerifyKraidFootSharedHitbox(CartridgeImportAddressSpace rom)
    {
        ushort Word(int pointer) => LittleEndianBytes.Word(KraidFootSharedHitboxRomByte(rom, 0xa70000 | pointer), KraidFootSharedHitboxRomByte(rom, 0xa70000 | (pointer + 1)));
        var boxes = KraidFootCollisionDefinitionsConstants.HitboxesAt(0x9453);
        AssertEqual(Word(0x9453), boxes.Length, "Native shared rectangle count");
        EnemyCollisionHitbox shared = boxes[0];
        AssertEqual(unchecked((short)Word(0x9455)), shared.Left, "Original shared hitbox left");
        AssertEqual(unchecked((short)Word(0x9457)), shared.Top, "Original shared hitbox top");
        AssertEqual(unchecked((short)Word(0x9459)), shared.Right, "Original shared hitbox right");
        AssertEqual(unchecked((short)Word(0x945b)), shared.Bottom, "Original shared hitbox bottom");
        AssertEqual(EnemyReactionAccess.Of(0xa7, Word(0x945d), shot: false), (EnemyReaction?)shared.Touch, "Original shared hitbox touch callback");
        AssertEqual(EnemyReactionAccess.Of(0xa7, Word(0x945f), shot: true), (EnemyReaction?)shared.Shot, "Original shared hitbox shot callback");
        EnemyCollisionHitbox expected = shared;
        int count = 0;
        foreach (var box in boxes) { AssertEqual(expected, box, "Shared rectangle enumeration"); count++; }
        AssertEqual(1, count, "One enumerated rectangle");
        AssertEqual(expected, boxes.ToArray()[0], "Shared rectangle materialization");
        AssertThrows<IndexOutOfRangeException>(() => _ = boxes[-1], "Negative shared rectangle index");
        AssertThrows<IndexOutOfRangeException>(() => _ = boxes[1], "Past shared rectangle index");
        AssertThrows<InvalidDataException>(() => KraidFootCollisionDefinitionsConstants.HitboxesAt(0x9454), "Unaligned hitbox rejects");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            bool expectedMember = raw == 0xa565 || (raw >= 0x8ce3 && raw <= 0x8f47 && (raw - 0x8ce3) % 18 == 0 && raw != UnshownFrame);
            AssertEqual(expectedMember, KraidFootCollision.TryGetComponents((ushort)raw, out var components),
                "Complete native foot frame membership");
            if (!expectedMember) { AssertEqual(0, components.Length, "Rejected frame has no components"); continue; }
            AssertEqual(Word(raw), components.Length, "Native component count");
            count = 0;
            foreach (var component in components)
            {
                AssertEqual(components[count], component, "Calculated component enumeration");
                AssertEqual(KraidFootCollisionDefinitionsConstants.HitboxesAt(Word(raw + 8 + 8 * count)), component.Hitboxes,
                    "Rectangles the original component hitbox selector names");
                count++;
            }
            AssertEqual(2, count, "Two enumerated foot components");
            AssertThrows<IndexOutOfRangeException>(() => _ = components[-1], "Negative component index");
            AssertThrows<IndexOutOfRangeException>(() => _ = components[2], "Past component index");
        }
    }

    /// <summary>Reads one native shared-hitbox byte solely as the verification oracle.</summary>
    private static byte KraidFootSharedHitboxRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}

using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks Kraid mouth hitbox mappings and reads around the bank-$A7 low-half boundary.</summary>
internal static class KraidMouthMappingsVerification
{
    /// <summary>Compares all eight authored mouth shapes and rejects non-shape pointers between them.</summary>
    internal static void VerifyKraidMouthShapeCases(SuperMetroidAddressSpace rom)
    {
        short Word(int address) => unchecked((short)(LittleEndianBytes.Word(KraidMouthShapeCasesRomByte(rom, address), KraidMouthShapeCasesRomByte(rom, address + 1))));
        for (int entry = 0; entry < 8; entry++)
        {
            ushort pointer = (ushort)(0x9788 + 8 * entry);
            int address = 0xa70000 | pointer;
            var expected = new EnemyHitboxRectangle(Word(address), Word(address + 2), Word(address + 4), Word(address + 6));
            AssertTrue(KraidMouthHitboxDefinitionsConstants.IsDefined(pointer), "Native mouth shape is defined");
            AssertEqual(expected, KraidMouthHitboxDefinitionsConstants.Resolve(pointer), "Native mouth shape case");
            AssertEqual(new KraidMouthCollisionEdges(checked((short)expected.Left), checked((short)expected.Top), checked((short)expected.Bottom)),
                KraidMouthHitboxDefinitionsConstants.ResolveCollision(pointer), "Collision uses left/top/bottom only");
        }
        for (int raw = 0x9787; raw <= 0x97c8; raw++)
        {
            ushort pointer = (ushort)raw;
            bool expected = raw >= 0x9788 && raw <= 0x97c0 && (raw - 0x9788) % 8 == 0;
            AssertEqual(expected, KraidMouthHitboxDefinitionsConstants.IsDefined(pointer), "Native shape alignment");
            if (!expected)
                AssertThrows<InvalidDataException>(() => KraidMouthHitboxDefinitionsConstants.Resolve(pointer), "Non-shape address rejects");
        }
    }

    /// <summary>Reads an original shape-table byte for the authored-case comparison.</summary>
    private static byte KraidMouthShapeCasesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

}

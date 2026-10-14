using SuperMetroid.Tooling;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>
/// Checks the bank-$84 PLM list catalog for unique, readable native pointers and
/// optionally confirms the named entries against the private retail cartridge.
/// </summary>
internal static class RoomPlmInstructionListCatalogVerification
{
    /// <summary>
    /// Protects the semantic bank-$84 list catalog from accidental aliases and invalid
    /// LoROM offsets. When the private cartridge is available, every named list is also
    /// read through the production address space so a typo cannot survive as documentation.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for room PLM instruction list catalog")]
    internal static void VerifyRoomPlmInstructionListCatalog()
    {
        FieldInfo[] fields = CatalogFields.Of(typeof(RoomPlmInstructionLists), BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(ushort))
            .ToArray();
        ushort[] pointers = fields
            .Select(field => field.RequiredConstant<ushort>())
            .ToArray();

        AssertTrue(fields.Length > 0, "PLM instruction-list catalog is populated");
        AssertEqual(fields.Length, pointers.Distinct().Count(),
            "PLM instruction-list catalog has no duplicate native pointers");
        foreach ((FieldInfo field, ushort pointer) in fields.Zip(pointers))
        {
            AssertTrue(pointer >= 0x8000,
                $"PLM instruction list {field.Name} uses readable bank-$84 space");
        }

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  PLM instruction lists: shape passes; retail readability skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        foreach ((FieldInfo field, ushort pointer) in fields.Zip(pointers))
        {
            // Read a complete first word. We intentionally do not interpret it here: an
            // instruction-list entry can begin with either a positive timer or any native
            // opcode, including routines outside the currently translated execution slice.
            byte low = bus.ReadCartridgeByte((int)new SnesAddress(0x84, pointer));
            byte high = bus.ReadCartridgeByte((int)new SnesAddress(
                0x84,
                checked((ushort)(pointer + 1))));
            ushort firstWord = unchecked((ushort)(SnesLittleEndian.Word(low, high)));
            AssertTrue(firstWord is not (0x0000 or 0xffff),
                $"PLM instruction list {field.Name} begins with readable retail data");
        }

        Console.WriteLine(
            $"  PLM instruction lists: {fields.Length} unique named pointers read from retail bank $84.");
    }
}

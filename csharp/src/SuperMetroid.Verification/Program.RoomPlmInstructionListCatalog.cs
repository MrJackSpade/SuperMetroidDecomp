using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Protects the semantic bank-$84 list catalog from accidental aliases and invalid
    /// LoROM offsets. When the private cartridge is available, every named list is also
    /// read through the production address space so a typo cannot survive as documentation.
    /// </summary>
    static void VerifyRoomPlmInstructionListCatalog()
    {
        FieldInfo[] fields = typeof(RoomPlmInstructionLists)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(ushort))
            .ToArray();
        ushort[] pointers = fields
            .Select(field => (ushort)field.GetRawConstantValue()!)
            .ToArray();

        AssertTrue(fields.Length > 0, "PLM instruction-list catalog is populated");
        AssertEqual(fields.Length, pointers.Distinct().Count(),
            "PLM instruction-list catalog has no duplicate native pointers");
        foreach ((FieldInfo field, ushort pointer) in fields.Zip(pointers))
        {
            AssertTrue(pointer >= 0x8000,
                $"PLM instruction list {field.Name} uses readable bank-$84 space");
        }

        AssertEqual(8, RoomPlmInstructionLists.CollisionBombByReactionIndex.Length,
            "collision-bomb instruction table preserves all native reactions");
        AssertEqual(8, RoomPlmInstructionLists.ReactionBombByReactionIndex.Length,
            "reaction-bomb instruction table preserves all native reactions");
        AssertEqual(4, RoomPlmInstructionLists.RespawningShotBySize.Length,
            "respawning shot instruction table preserves all native sizes");
        AssertEqual(4, RoomPlmInstructionLists.PermanentShotBySize.Length,
            "permanent shot instruction table preserves all native sizes");
        AssertEqual(4, RoomPlmInstructionLists.CrumbleRevealBySize.Length,
            "crumble-reveal instruction table preserves all native sizes");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  PLM instruction lists: shape passes; retail readability skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        foreach ((FieldInfo field, ushort pointer) in fields.Zip(pointers))
        {
            // Read a complete first word. We intentionally do not interpret it here: an
            // instruction-list entry can begin with either a positive timer or any native
            // opcode, including routines outside the currently translated execution slice.
            byte low = bus.ReadByte((int)new SnesAddress(0x84, pointer));
            byte high = bus.ReadByte((int)new SnesAddress(
                0x84,
                checked((ushort)(pointer + 1))));
            ushort firstWord = unchecked((ushort)(low | (high << 8)));
            AssertTrue(firstWord is not (0x0000 or 0xffff),
                $"PLM instruction list {field.Name} begins with readable retail data");
        }

        Console.WriteLine(
            $"  PLM instruction lists: {fields.Length} unique named pointers read from retail bank $84.");
    }
}

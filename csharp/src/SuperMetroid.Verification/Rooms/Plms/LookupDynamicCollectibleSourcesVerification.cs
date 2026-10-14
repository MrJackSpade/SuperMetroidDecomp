using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Plms.CompiledDynamicCollectibleGraphicsVerification;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>Checks native graphics source pointers and kind identities for dynamically drawn collectibles.</summary>
internal static class LookupDynamicCollectibleSourcesVerification
{
    /// <summary>Confirms each of seventeen native PLM graphics programs selects the compiled source and that its 256 artwork bytes retain that source identity.</summary>
    internal static void VerifyDynamicCollectibleSourcePointers(SuperMetroidAddressSpace rom)
    {
        var exported = RoomPlmDynamicCollectibleGraphicsDefinitions.All.ToArray();
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            var kind = (InWorldCollectibleKind)raw;
            if (raw is < 4 or > 20)
            {
                AssertThrows<InvalidDataException>(() => RoomPlmDynamicCollectibleGraphicsDefinitions.GraphicsPointer(kind), "Dynamic source rejected byte kind");
                continue;
            }
            var graphic = RoomPlmDynamicCollectibleGraphicsDefinitions.Get(kind);
            foreach (int firstHeader in new[] {0xeed7,0xef2b,0xef7f})
            {
                ushort program = ReadCollectibleGraphicsWord(rom, firstHeader + raw * 4 + 2);
                AssertEqual((ushort)0x8764, ReadCollectibleGraphicsWord(rom,program), "Native source upload opcode");
                ushort expected = ReadCollectibleGraphicsWord(rom,program + 2);
                AssertEqual(expected, RoomPlmDynamicCollectibleGraphicsDefinitions.GraphicsPointer(kind), "Native calculated graphics source");
                AssertEqual(expected, graphic.GraphicsPointer, "Native graphics DTO source");
                AssertEqual(expected, exported[raw - 4].GraphicsPointer, "Native graphics enumerated source");
                for (int offset = 0; offset < 256; offset++)
                    AssertEqual(DynamicCollectibleSourcePointersRomByte(rom, 0x890000 | (expected + offset)), graphic.Tiles.Span[offset], "Reordered artwork remains associated with its original native source");
            }
        }
    }

    /// <summary>Reads an artwork byte from the retail source solely for the dynamic graphics oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for dynamic collectible source pointers")]
    private static byte DynamicCollectibleSourcePointersRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks ascending identities for kinds four through twenty and rejects all other byte values.</summary>
    internal static void VerifyDynamicCollectibleKindIdentity()
    {
        var exported = RoomPlmDynamicCollectibleGraphicsDefinitions.All.ToArray();
        AssertEqual(17, exported.Length, "Original dynamic kind count");
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            var kind = (InWorldCollectibleKind)raw;
            if (raw is < 4 or > 20)
                AssertThrows<InvalidDataException>(() => RoomPlmDynamicCollectibleGraphicsDefinitions.Get(kind), "Original graphics kind domain");
            else
            {
                AssertEqual(kind, RoomPlmDynamicCollectibleGraphicsDefinitions.Get(kind).Kind, "Original direct kind identity");
                AssertEqual(kind, exported[raw - 4].Kind, "Original ascending kind enumeration");
            }
        }
    }
}

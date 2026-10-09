using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks setup words for all 70 supported retail PLM headers against the native ROM and population table.</summary>
    /// <param name="rom">The SNES address space containing the retail room-PLM data.</param>
    private static void VerifyRetailPlmHeaderSetups(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyRetailPlmHeaderField), () => VerifyRetailPlmHeaderField(rom, false));

    /// <summary>Checks initial-instruction words for all 70 supported retail PLM headers against the native ROM and population table.</summary>
    /// <param name="rom">The SNES address space containing the retail room-PLM data.</param>
    private static void VerifyRetailPlmHeaderInstructions(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyRetailPlmHeaderField), () => VerifyRetailPlmHeaderField(rom, true));

    /// <summary>Verifies one field of every supported header, rejects unsupported selectors, and matches the catalog domain to native populations.</summary>
    /// <param name="rom">The SNES address space used to read native header words and population records.</param>
    /// <param name="instruction">Selects the initial-instruction word when true, or the setup word when false.</param>
    private static void VerifyRetailPlmHeaderField(SuperMetroidAddressSpace rom, bool instruction)
    {
        ushort[] headers = [0xb63b,0xb63f,0xb643,0xb647,0xb6d3,0xb6df,0xb6eb,0xb703,0xb70b,
            0xb76f,0xb8ac,0xbaf4,0xbb05,0xc82a,0xc836,0xc842,0xc848,0xc84e,0xc854,0xc85a,
            0xc860,0xc866,0xc86c,0xc872,0xc878,0xc87e,0xc884,0xc88a,0xc890,0xc8ca,
            0xd6de,0xd6ea,0xd70c,0xdb44,0xdb48,0xdb4c,0xdb52,0xdb56,0xdb5a,0xdb60,
            0xdf59,0xdf65,0xdf71,0xeed7,0xeedb,0xeedf,0xeee3,0xef23,0xef2f,0xef33,
            0xef37,0xef3b,0xef3f,0xef43,0xef47,0xef4b,0xef4f,0xef53,0xef57,0xef5b,
            0xef5f,0xef63,0xef67,0xef6b,0xef6f,0xef73,0xef7b,0xef7f,0xef83,0xef87];
        var exported = RoomPlmHeaderDefinitionsTooling.All.ToArray();
        AssertEqual(70, RoomPlmHeaderDefinitions.RetailHeaderCount, "Retail header contract count");
        AssertEqual(70, exported.Length, "Retail header enumeration count");
        for (int index = 0; index < headers.Length; index++)
        {
            AssertEqual(headers[index], exported[index].Header, "Retail header ascending enumeration");
            ushort expected = ReadSamusEaterPlmWord(rom, 0x840000 | (headers[index] + (instruction ? 2 : 0)));
            AssertEqual(expected, instruction ? exported[index].InitialInstruction : exported[index].Setup,
                "Retail header original enumerated field");
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort header = (ushort)raw;
            if (!headers.Contains(header))
                AssertThrows<InvalidDataException>(() => RoomPlmHeaderDefinitions.Get(header), "Retail header rejects all unsupported inputs");
            else
            {
                RoomPlmHeaderDefinition selected = RoomPlmHeaderDefinitions.Get(header);
                AssertEqual(header, selected.Header, "Retail header identity preserved");
                ushort expected = ReadSamusEaterPlmWord(rom, 0x840000 | (raw + (instruction ? 2 : 0)));
                AssertEqual(expected, instruction ? selected.InitialInstruction : selected.Setup, "Retail header original selected field");
            }
        }
        // Native records independently establish which header identities the retail
        // populations reference; expected pointer fields never come from the new cases.
        var referenced = new HashSet<ushort>();
        foreach (ushort pointer in RoomPlmPopulationDefinitions.Pointers)
        {
            int cursor = pointer;
            bool terminated = false;
            for (int record = 0; record <= 256; record++, cursor += 6)
            {
                ushort header = ReadSamusEaterPlmWord(rom, 0x8f0000 | cursor);
                if (header == 0) { terminated = true; break; }
                referenced.Add(header);
            }
            AssertTrue(terminated, "Retail original population bounded terminator");
        }
        AssertTrue(referenced.SetEquals(headers), "Retail header domain equals native population references");
    }
}

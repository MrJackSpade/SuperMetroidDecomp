using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>Checks the 70 retail PLM headers against native records and room-population references.</summary>
internal static class LookupRetailPlmHeadersVerification
{
    /// <summary>
    /// Checks the seventy retail headers across the full word domain and independently traverses native room
    /// populations. The compiled header record no longer carries the cartridge's setup and list words (#1414); this
    /// check now asserts that the compiled headers are exactly the words the retail populations place, translated
    /// through the native-word table, and that every other word is rejected.
    /// </summary>
    internal static void VerifyRetailPlmHeaders(SuperMetroidAddressSpace rom)
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
            AssertEqual(headers[index], PlmNativeWords.Word(exported[index].Header), "Retail header ascending enumeration");
        AssertThrows<InvalidOperationException>(() => RoomPlmHeaderDefinitions.Get((PlmHeaderId)(-1)), "Undefined header identities fail");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort word = (ushort)raw;
            if (!PlmNativeWords.Headers.TryDecode(word, out PlmHeaderId header))
                continue;
            if (!headers.Contains(word))
                AssertThrows<InvalidDataException>(() => RoomPlmHeaderDefinitions.Get(header), "Retail header rejects all unsupported headers");
            else
                AssertEqual(header, RoomPlmHeaderDefinitions.Get(header).Header, "Retail header identity preserved");
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

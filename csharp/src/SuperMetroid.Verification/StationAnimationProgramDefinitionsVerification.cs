using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction;
using static LookupStationLayoutsVerification;
using static LookupStationVisualIdsVerification;
using static RoomPlmPopulationVerification;
using static StationAccessPlmDefinitionsVerification;
using static StationVisualsVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rooms.Plms;

/// <summary>Checks station animation words, draw lists, access selections, and sparse-bus activation.</summary>
internal static class StationAnimationProgramDefinitionsVerification
{
    /// <summary>Runs native duration and draw comparisons plus station layout and population checks.</summary>
    internal static void VerifyStationAnimationProgramDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Station animation oracle revision");
        Suite(nameof(VerifyStationVisualIds), () => VerifyStationVisualIds());
        Suite(nameof(VerifyStationAnimationDurations), () => VerifyStationAnimationDurations(rom));
        Suite(nameof(VerifyStationAnimationDraws), () => VerifyStationAnimationDraws(rom));

        Suite(nameof(VerifyStationLayoutGeometry), () => VerifyStationLayoutGeometry(rom));
        Suite(nameof(VerifyStationLayoutCollision), () => VerifyStationLayoutCollision(rom));
        Suite(nameof(VerifyStationLayoutVisuals), () => VerifyStationLayoutVisuals(rom));

        Suite(nameof(VerifyStationAccessRetractedDraws), () => VerifyStationAccessRetractedDraws(rom));
        Suite(nameof(VerifyStationAccessExtendedDraws), () => VerifyStationAccessExtendedDraws(rom));

        // The population fixture carries no station animation, access selection,
        // or draw-list bytes.
        // Its real map, missile, and save paths can finish only with compiled data.
        Suite(nameof(VerifySequentialRoomPlmPopulationLoader), () => VerifySequentialRoomPlmPopulationLoader());
        Suite(nameof(VerifyStationVisuals), () => VerifyStationVisuals(rom));
        Console.WriteLine(
            "Station animations: all 15 frame records, 20 draw lists, and 12 access selections match ROM; sparse-bus station activation and save animation use compiled data.");
    }

    /// <summary>Compares station frame hold durations with the bank-$84 lists.</summary>
    private static void VerifyStationAnimationDurations(SuperMetroidAddressSpace rom) => VerifyStationAnimationField(rom, false);
    /// <summary>Compares station frame draw selectors with the bank-$84 lists.</summary>
    private static void VerifyStationAnimationDraws(SuperMetroidAddressSpace rom) => VerifyStationAnimationField(rom, true);

    /// <summary>Checks every native list word, frame bound, and enumeration address for the selected field.</summary>
    private static void VerifyStationAnimationField(SuperMetroidAddressSpace rom, bool draw)
    {
        ushort[] lists = [0xad66,0xad76,0xadc6,0xae50,0xafe8,0xaffa,0xaffe];
        var words = StationAnimationProgramDefinitions.NativeWords().ToArray();
        AssertEqual(30, words.Length, "Station original enumeration word count");
        AssertTrue(lists.SequenceEqual(Enum.GetValues<StationAnimationList>().Select(list => (ushort)list)),
            "Station list domain is exactly the seven native entries");
        foreach (ushort raw in new ushort[] { 0, 0xad62, 0xafe9, 0xffff })
            AssertThrows<InvalidOperationException>(
                () => StationAnimationProgramDefinitions.Resolve((StationAnimationList)raw, 0),
                "Undefined station list is rejected at the typed boundary");
        int ordinal = 0;
        for (int listIndex = 0; listIndex < lists.Length; listIndex++)
        {
            var list = (StationAnimationList)lists[listIndex];
            int count = listIndex < 4 ? 3 : 1;
            foreach (int bad in new[] { int.MinValue,-1,count,int.MaxValue })
                AssertThrows<InvalidDataException>(() => StationAnimationProgramDefinitions.Resolve(list, bad), "Station exact frame bounds");
            for (int frame = 0; frame < count; frame++, ordinal += 2)
            {
                ushort address = (ushort)(lists[listIndex] + frame * 4 + (draw ? 2 : 0));
                ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | address);
                var actual = StationAnimationProgramDefinitions.Resolve(list, frame);
                AssertEqual(expected, draw ? actual.DrawPointer : actual.Duration, "Station original animation field");
                var enumerated = words[ordinal + (draw ? 1 : 0)];
                AssertEqual(address, enumerated.Address, "Station original field enumeration address/order");
                AssertEqual(expected, enumerated.Value, "Station original enumerated field");
            }
        }
    }

    /// <summary>Runs the focused station animation definition check.</summary>
    [VerificationCommand("--station-animation-programs")]
    internal static int StationAnimationProgramsCommand()
    {
        Suite(nameof(VerifyStationAnimationProgramDefinitions), () => VerifyStationAnimationProgramDefinitions());
        return 0;
    }
}

using SuperMetroid.AssetExtraction.Rooms;
using static SuperMetroid.Verification.Rooms.RoomObjects.LookupStationLayoutsVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.LookupStationVisualIdsVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectPopulationVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.StationAccessObjectDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.StationVisualsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.Verification.Harness;

/// <summary>Checks station animation words, draw lists, access selections, and sparse-bus activation.</summary>
internal static class StationAnimationProgramDefinitionsVerification
{
    /// <summary>Runs native duration and draw comparisons plus station layout and population checks.</summary>
    internal static void VerifyStationAnimationProgramDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
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
        Suite(nameof(VerifySequentialRoomObjectPopulationLoader), () => VerifySequentialRoomObjectPopulationLoader());
        Suite(nameof(VerifyStationVisuals), () => VerifyStationVisuals(rom));
        Console.WriteLine(
            "Station animations: all 15 frame records, 20 draw lists, and 12 access selections match ROM; sparse-bus station activation and save animation use compiled data.");
    }

    /// <summary>Compares station frame hold durations with the bank-$84 lists.</summary>
    private static void VerifyStationAnimationDurations(CartridgeImportAddressSpace rom) => VerifyStationAnimationField(rom, false);
    /// <summary>Compares station frame draw selectors with the bank-$84 lists.</summary>
    private static void VerifyStationAnimationDraws(CartridgeImportAddressSpace rom) => VerifyStationAnimationField(rom, true);

    /// <summary>Checks every native list word, frame bound, and enumeration address for the selected field.</summary>
    private static void VerifyStationAnimationField(CartridgeImportAddressSpace rom, bool draw)
    {
        ushort[] lists = [0xad66,0xad76,0xadc6,0xae50,0xafe8,0xaffa,0xaffe];
        var words = StationAnimationDefinitions.NativeWords().ToArray();
        AssertEqual(30, words.Length, "Station original enumeration word count");
        // The identities' values are arbitrary (#1414): the domain check translates each through the native-word table.
        AssertTrue(lists.SequenceEqual(StationNativeWords.Lists), "Station lists are exactly the seven native entries");
        int[] imageCounts = [3, 3, 3, 3, 1, 2];
        AssertTrue(imageCounts.SequenceEqual(Enum.GetValues<StationAnimation>().Select(animation => StationAnimationDefinitions.Segments.Beats(animation).Count)),
            "Each station animation shows its native lists' images (#1994)");
        int ordinal = 0;
        foreach (StationAnimation animation in Enum.GetValues<StationAnimation>())
        {
            IReadOnlyList<AnimationBeat<StationDraw, NoAnimationCue>> beats = StationAnimationDefinitions.Segments.Beats(animation);
            for (int beat = 0; beat < beats.Count; beat++, ordinal += 2)
            {
                var image = AssertIs<AnimationBeat<StationDraw, NoAnimationCue>.Frame>(beats[beat], $"station {animation} image {beat}");
                ushort address = (ushort)(StationNativeWords.Record(animation, beat) + (draw ? 2 : 0));
                ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | address);
                AssertEqual(expected, draw ? RoomObjectDrawAddressCodec.Of(RoomObjectDraw.Of(image.Shown)) : (ushort)image.Duration,
                    "Station original animation field");
                var enumerated = words[ordinal + (draw ? 1 : 0)];
                AssertEqual(address, enumerated.Address, "Station original field enumeration address/order");
                AssertEqual(expected, enumerated.Value, "Station original enumerated field");
            }
        }
        AssertEqual(AnimationEnd.Stops, StationAnimationDefinitions.Animations[StationAnimation.SaveFlash].End,
            "the save's flash is over after its loops");
        AssertEqual(SaveStationAnimationDefinitions.SaveAnimationLoops, StationAnimationDefinitions.Animations[StationAnimation.SaveFlash].Phases[0].Plays,
            "the save's flash plays its authored loops");
    }

    /// <summary>Runs the focused station animation definition check.</summary>
    [VerificationCommand("--station-animation-programs")]
    internal static int StationAnimationProgramsCommand()
    {
        Suite(nameof(VerifyStationAnimationProgramDefinitions), () => VerifyStationAnimationProgramDefinitions());
        return 0;
    }
}

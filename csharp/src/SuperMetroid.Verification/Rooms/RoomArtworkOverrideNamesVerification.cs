using System.Text.Json;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms;

/// <summary>Checks that room artwork overrides saved under their old cartridge-address names move to their set names (#1363).</summary>
internal static class RoomArtworkOverrideNamesVerification
{
    /// <summary>
    /// An override under its old name moves to its set name with its bytes, a file the rename does not know stays, an
    /// override already under its new name is kept, and one under both names is refused. Block compositions are no
    /// override surface (#1431), so an old block-composition file is not renamed.
    /// </summary>
    internal static void VerifyRoomArtworkOverrideNames()
    {
        using var scratch = new TestScratchDirectory("room-artwork-override-names");
        var installation = new GameInstallation(scratch.Root);
        string blockCompositions = Directory.CreateDirectory(
            Path.Combine(scratch.Root, "overrides", GameInstallationLayout.RoomMetatileDirectoryName)).FullName;
        Directory.CreateDirectory(installation.RoomPaletteOverrideDirectory);
        string oldBlocks = Path.Combine(blockCompositions, "room-blocks-C1D8DC.json");
        string oldPalette = Path.Combine(installation.RoomPaletteOverrideDirectory, "room-palette-C2BC9C.json");
        string unknown = Path.Combine(installation.RoomPaletteOverrideDirectory, "notes.txt");
        string kept = Path.Combine(installation.RoomPaletteOverrideDirectory, RoomStaticPaletteFormat.FileName(RoomPaletteSet.Kraid));
        File.WriteAllBytes(oldBlocks, [4]);
        File.WriteAllBytes(oldPalette, [5]);
        File.WriteAllBytes(unknown, [6]);
        File.WriteAllBytes(kept, [7]);

        RoomArtworkOverrideNames.Migrate(installation);

        AssertTrue(File.Exists(oldBlocks) && !File.Exists(Path.Combine(blockCompositions, "room-blocks-red-brinstar-kraid-statues-hall.json")),
            "an old block-composition file is not renamed: blocks change only through their pictures (#1431)");
        AssertTrue(File.ReadAllBytes(Path.Combine(installation.RoomPaletteOverrideDirectory, "room-palette-map-statues.json"))
            .SequenceEqual(new byte[] { 5 }) && !File.Exists(oldPalette), "the old palette override moves to its set name with its bytes");
        AssertTrue(File.Exists(unknown) && File.ReadAllBytes(kept).SequenceEqual(new byte[] { 7 }),
            "files the rename does not know, and overrides already under their new names, stay as they are");

        RoomArtworkOverrideNames.Migrate(installation);
        AssertTrue(File.Exists(Path.Combine(installation.RoomPaletteOverrideDirectory, "room-palette-map-statues.json")),
            "a second rename leaves the moved overrides in place");

        File.WriteAllBytes(oldPalette, [9]);
        AssertThrows<IOException>(() => RoomArtworkOverrideNames.Migrate(installation),
            "an override under both its old and new names is refused");
        AssertTrue(File.Exists(oldPalette), "a refused rename leaves the old override where it was");
        Console.WriteLine("Room artwork overrides: old cartridge-address names move to their set names; conflicts are refused.");
    }

    /// <summary>
    /// A room-layout override under its old source name (#1414) moves to its level's name and its document names the
    /// level instead of the source, keeping its words; an old document of another source is refused, and an override
    /// under both names is refused.
    /// </summary>
    internal static void VerifyRoomLayoutOverrideNames()
    {
        using var scratch = new TestScratchDirectory("room-layout-override-names");
        var installation = new GameInstallation(scratch.Root);
        string directory = Directory.CreateDirectory(installation.RoomVisualLayoutOverrideDirectory).FullName;
        RoomLevelId level = RoomLevelDefinitions.LandingSite;
        int source = RoomLevelSourceDefinitions.Addresses[level];
        string legacy = Path.Combine(directory, $"level-{source:X6}.json");
        string legacyDocument = $$"""{"formatVersion":1,"sourceAddress":{{source}},"widthInBlocks":2,"heightInBlocks":1,"foregroundVisualWords":[5,1029],"backgroundVisualWords":[0,7]}""";
        File.WriteAllText(legacy, legacyDocument);

        RoomArtworkOverrideNames.Migrate(installation);

        string current = Path.Combine(directory, RoomVisualLayoutFiles.LevelFileName(level));
        AssertEqual("level-LandingSite.json", Path.GetFileName(current), "a layout's file name is its level's name");
        AssertTrue(File.Exists(current) && !File.Exists(legacy), "the old layout override moves to its level's name");
        using (JsonDocument moved = JsonDocument.Parse(File.ReadAllText(current)))
        {
            JsonElement root = moved.RootElement;
            AssertEqual(2, root.GetProperty("formatVersion").GetInt32(), "the moved layout is a version-two document");
            AssertEqual("LandingSite", root.GetProperty("level").GetString(), "the moved layout names its level");
            AssertTrue(!root.TryGetProperty("sourceAddress", out _), "the moved layout no longer names a cartridge source");
            AssertTrue(root.GetProperty("foregroundVisualWords").EnumerateArray().Select(word => word.GetInt32()).SequenceEqual(new[] { 5, 1029 }) &&
                root.GetProperty("backgroundVisualWords").EnumerateArray().Select(word => word.GetInt32()).SequenceEqual(new[] { 0, 7 }) &&
                root.GetProperty("widthInBlocks").GetInt32() == 2 && root.GetProperty("heightInBlocks").GetInt32() == 1,
                "the moved layout keeps its dimensions and words");
        }

        File.WriteAllText(legacy, legacyDocument);
        AssertThrows<IOException>(() => RoomArtworkOverrideNames.Migrate(installation),
            "a layout override under both its old and new names is refused");
        File.Delete(current);
        File.WriteAllText(legacy, legacyDocument.Replace($":{source},", ":0,", StringComparison.Ordinal));
        AssertThrows<InvalidDataException>(() => RoomArtworkOverrideNames.Migrate(installation),
            "an old layout override whose document names another source is refused");
        AssertTrue(File.Exists(legacy) && !File.Exists(current), "a refused layout rename leaves the old override where it was");
        Console.WriteLine("Room layout overrides: old source names move to their level names, rewritten to name the level.");
    }

    /// <summary>Runs the room artwork override rename checks.</summary>
    [VerificationCommand("--room-artwork-override-names")]
    internal static int RoomArtworkOverrideNamesCommand()
    {
        Suite(nameof(VerifyRoomArtworkOverrideNames), () => VerifyRoomArtworkOverrideNames());
        Suite(nameof(VerifyRoomLayoutOverrideNames), () => VerifyRoomLayoutOverrideNames());
        return 0;
    }
}

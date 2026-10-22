using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms;

/// <summary>Checks that room artwork overrides saved under their old cartridge-address names move to their set names (#1363).</summary>
internal static class RoomArtworkOverrideNamesVerification
{
    /// <summary>
    /// An override under its old name moves to its set name with its bytes, a file the rename does not know stays, an
    /// override already under its new name is kept, and one under both names is refused.
    /// </summary>
    internal static void VerifyRoomArtworkOverrideNames()
    {
        using var scratch = new TestScratchDirectory("room-artwork-override-names");
        var installation = new GameInstallation(scratch.Root);
        Directory.CreateDirectory(installation.RoomCharacterOverrideDirectory);
        Directory.CreateDirectory(installation.RoomMetatileOverrideDirectory);
        Directory.CreateDirectory(installation.RoomPaletteOverrideDirectory);
        string oldCharacters = Path.Combine(installation.RoomCharacterOverrideDirectory, "room-characters-BAC629.png");
        string oldBlocks = Path.Combine(installation.RoomMetatileOverrideDirectory, "room-blocks-C1D8DC.json");
        string oldPalette = Path.Combine(installation.RoomPaletteOverrideDirectory, "room-palette-C2BC9C.json");
        string unknown = Path.Combine(installation.RoomCharacterOverrideDirectory, "notes.txt");
        string kept = Path.Combine(installation.RoomPaletteOverrideDirectory, RoomStaticPaletteFormat.FileName(RoomPaletteSet.Kraid));
        File.WriteAllBytes(oldCharacters, [1, 2, 3]);
        File.WriteAllBytes(oldBlocks, [4]);
        File.WriteAllBytes(oldPalette, [5]);
        File.WriteAllBytes(unknown, [6]);
        File.WriteAllBytes(kept, [7]);

        RoomArtworkOverrideNames.Migrate(installation);

        AssertEqual("room-characters-upper-crateria.png", RoomCharacterAtlasFormat.FileName(RoomCharacterSet.UpperCrateria),
            "a character sheet's file name is its hyphenated set name");
        AssertTrue(File.ReadAllBytes(Path.Combine(installation.RoomCharacterOverrideDirectory, "room-characters-upper-crateria.png"))
            .SequenceEqual(new byte[] { 1, 2, 3 }) && !File.Exists(oldCharacters), "the old character override moves to its set name with its bytes");
        AssertTrue(File.Exists(Path.Combine(installation.RoomMetatileOverrideDirectory, "room-blocks-red-brinstar-kraid-statues-hall.json")) &&
            !File.Exists(oldBlocks), "the old block override moves to its set name");
        AssertTrue(File.Exists(Path.Combine(installation.RoomPaletteOverrideDirectory, "room-palette-map-statues.json")) &&
            !File.Exists(oldPalette), "the old palette override moves to its set name");
        AssertTrue(File.Exists(unknown) && File.ReadAllBytes(kept).SequenceEqual(new byte[] { 7 }),
            "files the rename does not know, and overrides already under their new names, stay as they are");

        RoomArtworkOverrideNames.Migrate(installation);
        AssertTrue(File.Exists(Path.Combine(installation.RoomCharacterOverrideDirectory, "room-characters-upper-crateria.png")),
            "a second rename leaves the moved overrides in place");

        File.WriteAllBytes(oldCharacters, [9]);
        AssertThrows<IOException>(() => RoomArtworkOverrideNames.Migrate(installation),
            "an override under both its old and new names is refused");
        AssertTrue(File.Exists(oldCharacters), "a refused rename leaves the old override where it was");
        Console.WriteLine("Room artwork overrides: old cartridge-address names move to their set names; conflicts are refused.");
    }

    /// <summary>Runs the room artwork override rename check.</summary>
    [VerificationCommand("--room-artwork-override-names")]
    internal static int RoomArtworkOverrideNamesCommand()
    {
        Suite(nameof(VerifyRoomArtworkOverrideNames), () => VerifyRoomArtworkOverrideNames());
        return 0;
    }
}

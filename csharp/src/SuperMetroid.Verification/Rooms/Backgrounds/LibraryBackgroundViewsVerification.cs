using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Backgrounds;

/// <summary>
/// Checks that a library background the player never sees as its own picture is not one (#1649): the import installs no
/// picture of it and a room loading it binds none, while a background seen as its own picture keeps both.
/// </summary>
internal static class LibraryBackgroundViewsVerification
{
    /// <summary>
    /// Each retail library background's view is the one its list leaves: a list copying the tilemap it staged to a
    /// background page leaves the background's own picture, one copying the enemies' staging the boss's body, one copying
    /// a sky page the sky's rows, and one clearing the layer nothing; no list leaves two.
    /// </summary>
    internal static void VerifyViewsFollowTheirLists()
    {
        var seen = new Dictionary<LibraryBackgroundView, int>();
        foreach (LibraryBackgroundProgram program in LibraryBackgroundProgramDefinitions.All)
        {
            LibraryBackgroundView[] left = [.. program.Instructions.Select(ViewLeftBy).OfType<LibraryBackgroundView>().Distinct()];
            AssertEqual(1, left.Length, $"library background {program.Name} leaves one view");
            AssertEqual(left[0], program.Background.View, $"library background {program.Name}'s view");
            seen[left[0]] = seen.GetValueOrDefault(left[0]) + 1;
        }
        foreach (LibraryBackgroundView view in Enum.GetValues<LibraryBackgroundView>())
            AssertTrue(seen.ContainsKey(view), $"some retail library background leaves {view}");
        Console.WriteLine($"  Library background views: {string.Join(", ", seen.Select(entry => $"{entry.Value} {entry.Key}"))}.");
    }

    /// <summary>
    /// A room state whose library background copies the boss's own layer installs no background picture and binds none, a
    /// sky room's installs only the sky's rows, and a cleared one installs nothing; a state otherwise like them that shows its
    /// background as its own picture installs and looks up its picture.
    /// </summary>
    internal static void VerifyUnseenBackgroundsAreNotPictures()
    {
        RoomStateSettings pictured = CrateriaRoomStateDefinitions.ParlorDefault;
        TilesetDefinition tileset = RoomTilesets.Get(pictured.GraphicsSet);
        LibraryBackground rocks = pictured.Background ?? throw new InvalidOperationException("The Parlor loads a library background.");
        AssertTrue(RoomBlockUploads.BackgroundNamesOf([pictured]).Contains(RoomBackgroundImage.NameOf(rocks, null, tileset)),
            "a background seen as its own picture is installed");
        AssertThrows<InvalidOperationException>(() => RoomLibraryBackgroundPicture.Of(rocks, _ => throw new InvalidOperationException("looked up")),
            "a room loading a background seen as its own picture looks up its installed picture");

        // The boss's body: the fixture is the Parlor's state loading Phantoon's background and his population.
        RoomStateSettings boss = pictured with
        {
            Background = LibraryBackgroundDefinitions.PhantoonDefault,
            EnemyPopulation = RoomEnemyPopulationDefinitions.PhantoonDefault,
        };
        AssertEqual(0, RoomBlockUploads.BackgroundNamesOf([boss]).Count, "a background showing the boss's body installs no picture");
        AssertBindsNone(boss.Background, "a room showing the boss's body on its background");
        AssertThrows<InvalidDataException>(() => RoomBlockUploads.BackgroundNamesOf([pictured with { Background = LibraryBackgroundDefinitions.PhantoonDefault }]),
            "a background showing a boss's body in a room with no boss is refused");

        // The sky's rows: the Landing Site installs its sky's rows and none of its library background's door pictures.
        RoomStateSettings sky = CrateriaRoomStateDefinitions.LandingSiteDefault;
        TilesetDefinition skyTileset = RoomTilesets.Get(sky.GraphicsSet);
        string[] skyNames = [.. RoomBlockUploads.BackgroundNamesOf([sky])];
        string[] skyRows = [RoomBackgroundImage.SkyNameOf(ScrollingSkyState.VariantOf(sky.MainBehavior), skyTileset)];
        AssertTrue(skyNames.SequenceEqual(skyRows, StringComparer.Ordinal),
            $"a sky room installs only its sky's rows, not [{string.Join(", ", skyNames)}]");
        AssertBindsNone(sky.Background, "a sky room");
        AssertThrows<InvalidDataException>(() => RoomBlockUploads.BackgroundNamesOf([sky with { SetupBehavior = RoomSetupBehavior.None }]),
            "a background showing the sky's rows in a room without the sky's bands is refused");

        // Nothing: the Parlor's state loading the Ceres elevator shaft's cleared background.
        RoomStateSettings cleared = pictured with { Background = LibraryBackgroundDefinitions.CeresElev };
        AssertEqual(0, RoomBlockUploads.BackgroundNamesOf([cleared]).Count, "a cleared background installs no picture");
        AssertBindsNone(cleared.Background, "a room whose background is cleared");

        AssertEqual(RoomLibraryBackgroundPicture.NoneLoaded, RoomLibraryBackgroundPicture.Of(null, NoLookup), "a room loading no library background binds none");
        Console.WriteLine("  Library background views: the boss's body, the sky's rows and a cleared background install and bind no picture.");
    }

    /// <summary>Asserts a room loading <paramref name="background"/>, seen as something else, binds no picture, looking none up.</summary>
    private static void AssertBindsNone(LibraryBackground? background, string where) =>
        AssertEqual(RoomLibraryBackgroundPicture.SeenOtherwise,
            RoomLibraryBackgroundPicture.Of(background, NoLookup), $"{where} binds no background picture");

    /// <summary>A picture lookup a background the player does not see as its own picture never makes.</summary>
    private static RoomBackgroundImageResolver NoLookup(LibraryBackground background) =>
        throw new InvalidOperationException($"{background} is not seen as its own picture, but its picture was looked up.");

    /// <summary>
    /// The view <paramref name="instruction"/> leaves the background showing, or null for one that writes no background
    /// page: a decompression, or a transfer of characters.
    /// </summary>
    private static LibraryBackgroundView? ViewLeftBy(LibraryBackgroundInstruction instruction) => instruction switch
    {
        { Command: LibraryBackgroundCommand.ClearBg2 or LibraryBackgroundCommand.ClearBg2ForKraid } => LibraryBackgroundView.Nothing,
        { Destination: LibraryBackgroundDestination.BackgroundFirstPage or LibraryBackgroundDestination.BackgroundSecondPage
            or LibraryBackgroundDestination.KraidBackgroundPages } page => page.Source switch
        {
            LibraryBackgroundSource.StagedTilemap => LibraryBackgroundView.Picture,
            LibraryBackgroundSource.StagedEnemyTilemap => LibraryBackgroundView.BossBody,
            LibraryBackgroundSource.SkyPage => LibraryBackgroundView.SkyRows,
            _ => throw new InvalidDataException($"A library background copies {page.Source} to a background page."),
        },
        _ => null,
    };

    /// <summary>Runs the library background view checks.</summary>
    [VerificationCommand("--library-background-views")]
    internal static int LibraryBackgroundViewsCommand()
    {
        Suite(nameof(VerifyViewsFollowTheirLists), VerifyViewsFollowTheirLists);
        Suite(nameof(VerifyUnseenBackgroundsAreNotPictures), VerifyUnseenBackgroundsAreNotPictures);
        return 0;
    }
}

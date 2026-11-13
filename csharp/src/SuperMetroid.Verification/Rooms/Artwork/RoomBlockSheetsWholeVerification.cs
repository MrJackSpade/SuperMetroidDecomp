using System.Collections.Immutable;
using System.Globalization;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Artwork;

/// <summary>
/// Checks every installed room block sheet holds each block once, whole, from its own artwork (#1513): a block has
/// further pictures only where the game shows that block over other artwork, an animated tile's frames, Kraid's backdrop
/// or an encounter's look. A collectible that shows its own picture is pictured as its kind alone, in each frame, by its
/// kind and frame and by no input (#1538), and only in a tileset whose rooms place it. The rule is read from the room
/// data (the collectibles each room state places) and from the installed sheets' records, not from the import's
/// enumeration of pictures.
/// </summary>
internal static class RoomBlockSheetsWholeVerification
{
    /// <summary>
    /// Every block of every installed room block sheet depends only on the animations, Kraid's backdrop and encounter looks
    /// that show over it, and each sheet pictures both frames of each collectible its tileset's rooms place and of no other.
    /// </summary>
    internal static void VerifyRoomBlockSheetsWhole()
    {
        // The installation's room artwork is the process's shared fixture, loaded once for every suite reading it (#1532).
        InstalledRoomArtwork artwork = InstalledFixtures.RoomAssets.BlockSheets;
        var placed = CollectiblesPlaced();
        var failures = new List<string>();
        int blocks = 0, collectiblePictures = 0;
        foreach (KeyValuePair<TilesetDefinition, RoomBlockSheet> entry in artwork.Sheets.OrderBy(entry => RoomBlockSheet.NameOf(entry.Key), StringComparer.Ordinal))
        {
            RoomBlockSheet sheet = entry.Value;
            string name = RoomBlockSheet.NameOf(entry.Key);
            HashSet<InWorldCollectibleKind> kinds = placed.TryGetValue(entry.Key, out HashSet<InWorldCollectibleKind>? known) ? known : [];
            for (int block = 0; block < sheet.Inputs.Count; block++)
            {
                // A block no room of the tileset shows has no picture to depend on anything (#1636).
                if (!sheet.Inputs.Shows(block))
                    continue;
                blocks++;
                // Each artwork the tileset's rooms show their blocks over has its own pictures of them (#1648).
                foreach (RoomBlockArtwork shownOver in Enum.GetValues<RoomBlockArtwork>().Where(sheet.Inputs.ShowsOver))
                {
                    ImmutableArray<AppearanceInput> inputs = sheet.Inputs.Of(block, shownOver);
                    foreach (AppearanceInput input in inputs.Where(input => input is not (AnimationAppearance or KraidBackdropAppearance or EncounterLookAppearance)))
                        failures.Add($"{name}: block {Hex(block)} depends on {input.Token} over {shownOver}, which shows no artwork over a room's blocks");
                }
            }
            foreach (CollectiblePicture collectible in Enum.GetValues<InWorldCollectibleKind>().Where(CollectibleObjects.ShowsOwnPicture)
                .SelectMany(kind => Enumerable.Range(0, CollectiblePicture.Frames).Select(frame => new CollectiblePicture(kind, frame))))
            {
                string picture = RoomBlockPictureNames.OfCollectible(collectible);
                bool pictured = sheet.Sheet.frames.ContainsKey(picture);
                if (kinds.Contains(collectible.Kind) && pictured)
                    collectiblePictures++;
                else if (kinds.Contains(collectible.Kind))
                    failures.Add($"{name}: no picture {picture} of the {collectible.Kind} its rooms place");
                else if (pictured)
                    failures.Add($"{name}: picture {picture} shows a {collectible.Kind}, which no room of the tileset places");
            }
        }
        AssertTrue(artwork.Sheets.Count > 0, "the installation holds room block sheets");
        AssertTrue(collectiblePictures > 0, "some room block sheet pictures the collectibles its rooms place");
        AssertTrue(failures.Count == 0,
            $"{failures.Count} installed room block sheet faults:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
        Console.WriteLine($"Room block sheets whole: {blocks} blocks of {artwork.Sheets.Count} sheets show their own artwork; "
            + $"{collectiblePictures} collectible pictures show the collectibles their rooms place.");
    }

    /// <summary>
    /// The collectibles that show their own picture each tileset's rooms place, by tileset: those every retail room state
    /// of the tileset's population sets up.
    /// </summary>
    private static Dictionary<TilesetDefinition, HashSet<InWorldCollectibleKind>> CollectiblesPlaced()
    {
        var placed = new Dictionary<TilesetDefinition, HashSet<InWorldCollectibleKind>>();
        foreach (RoomHeaderDefinition room in RetailRoomHeaders.All)
        {
            foreach (RoomStateSettings state in RoomStateLists.GetStates(room))
            {
                foreach (RoomObjectPlacement placement in state.ObjectPopulation.Placements.Span)
                {
                    if (!CollectibleObjects.TryIdentifyPermanentCollectible(placement.Kind, out InWorldCollectibleKind kind, out _)
                        || !CollectibleObjects.ShowsOwnPicture(kind))
                        continue;
                    TilesetDefinition tileset = RoomTilesets.Get(state.GraphicsSet);
                    if (!placed.TryGetValue(tileset, out HashSet<InWorldCollectibleKind>? kinds))
                        placed.Add(tileset, kinds = []);
                    kinds.Add(kind);
                }
            }
        }
        return placed;
    }

    /// <summary>A block's index as picture names spell it.</summary>
    private static string Hex(int block) => block.ToString("X3", CultureInfo.InvariantCulture);

    /// <summary>Runs the room block sheets' whole-block check.</summary>
    [VerificationCommand("--room-block-sheets-whole")]
    internal static int RoomBlockSheetsWholeCommand()
    {
        Suite(nameof(VerifyRoomBlockSheetsWhole), VerifyRoomBlockSheetsWhole);
        return 0;
    }
}

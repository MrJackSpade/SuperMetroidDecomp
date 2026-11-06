using System.Collections.Immutable;
using System.Globalization;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Artwork;

/// <summary>
/// Checks every installed room block sheet holds each block once, whole, from its own artwork (#1513): a block has
/// further pictures only where the game shows that block over other artwork, an animated tile's frames, Kraid's backdrop
/// or an encounter's look, and a collectible's picture is the collectible's own, drawn on the blocks the collectible
/// writes where it lies. No block, and no background cell, is pictured over a collectible's graphics, and a collectible is
/// pictured only in a tileset whose rooms place it. The rule is read from the room data (the collectibles each room state
/// places and the blocks the installed collectible visuals draw for each slot) and from the installed sheets' records,
/// not from the import's enumeration of pictures.
/// </summary>
internal static class RoomBlockSheetsWholeVerification
{
    /// <summary>The animation frames a collectible shows where it lies, each drawn as one block.</summary>
    private const int CollectibleFrames = 2;

    /// <summary>Blocks named in one sheet's failure before the rest are counted only.</summary>
    private const int NamedExamples = 8;

    /// <summary>
    /// Every block of every installed room block sheet depends only on the animations, Kraid's backdrop and encounter looks
    /// that show over it, except the blocks a collectible draws where it lies, which depend on their slot alone and are
    /// pictured for each collectible the tileset's rooms place and for none other; no other picture names a collectible.
    /// </summary>
    internal static void VerifyRoomBlockSheetsWhole()
    {
        // The installation's room artwork is the process's shared fixture, loaded once for every suite reading it (#1532).
        InstalledRoomArtwork artwork = InstalledFixtures.RoomAssets.BlockSheets;
        InstalledRoomPlmCollectibleVisuals visuals = RepositoryInstallation.Installation.LoadRoomPlmCollectibleVisuals();
        var placed = CollectiblesPlaced();
        var collectibleBlocks = CollectibleBlocks(visuals);
        var failures = new List<string>();
        int blocks = 0, collectiblePictures = 0;
        foreach (KeyValuePair<TilesetDefinition, RoomBlockSheet> entry in artwork.Sheets.OrderBy(entry => RoomBlockSheet.NameOf(entry.Key), StringComparer.Ordinal))
        {
            RoomBlockSheet sheet = entry.Value;
            string name = RoomBlockSheet.NameOf(entry.Key);
            HashSet<InWorldCollectibleKind> kinds = placed.TryGetValue(entry.Key, out HashSet<InWorldCollectibleKind>? known) ? known : [];
            var composedOverCollectibles = new List<int>();
            for (int block = 0; block < sheet.Inputs.Count; block++)
            {
                blocks++;
                ImmutableArray<AppearanceInput> inputs = sheet.Inputs.Of(block, RoomBlockArtwork.Tileset);
                foreach (AppearanceInput input in inputs.Where(input => input is not (AnimationAppearance or KraidBackdropAppearance
                    or EncounterLookAppearance or CollectibleSlotAppearance)))
                    failures.Add($"{name}: block {Hex(block)} depends on {input.Token}, which shows no artwork over a room's blocks");
                if (kinds.Count > 0 && collectibleBlocks.TryGetValue(block, out int slot))
                {
                    // A collectible's block shows the collectible in its slot, or the room's load while the slot holds none.
                    var own = new CollectibleSlotAppearance(slot);
                    if (inputs.Length != 1 || !inputs[0].Equals(own))
                    {
                        failures.Add($"{name}: collectible slot {slot.ToString(CultureInfo.InvariantCulture)}'s block {Hex(block)} depends on "
                            + $"[{string.Join(", ", inputs.Select(input => input.Token))}], not its slot alone");
                        continue;
                    }
                    foreach (InWorldCollectibleKind kind in kinds)
                    {
                        string picture = RoomBlockPictureNames.Of(block, RoomBlockArtwork.Tileset, inputs, [CollectibleSlotAppearance.ValueOf(kind)]);
                        if (sheet.Sheet.frames.ContainsKey(picture))
                            collectiblePictures++;
                        else
                            failures.Add($"{name}: no picture {picture} of the {kind} its rooms place");
                    }
                }
                else if (inputs.Any(input => input is CollectibleSlotAppearance))
                    composedOverCollectibles.Add(block);
            }
            if (composedOverCollectibles.Count > 0)
            {
                failures.Add($"{name}: {composedOverCollectibles.Count.ToString(CultureInfo.InvariantCulture)} blocks are pictured over a collectible's graphics "
                    + $"({string.Join(", ", composedOverCollectibles.Take(NamedExamples).Select(Hex))}{(composedOverCollectibles.Count > NamedExamples ? ", ..." : "")})");
            }
            if (sheet.Inputs.sprites is { } overSprites && overSprites.Any(inputs => inputs.Any(input => input is CollectibleSlotAppearance)))
                failures.Add($"{name}: a block over the sprites' artwork depends on a collectible slot");
            CheckPictureNames(name, sheet, kinds, collectibleBlocks, failures);
        }
        foreach (RoomBackgroundImage background in artwork.Backgrounds.Values.OrderBy(background => background.Name, StringComparer.Ordinal))
        {
            int cells = background.Cells.Count(cell => cell.Inputs.Any(input => input is CollectibleSlotAppearance));
            if (cells > 0)
                failures.Add($"{background.Name}: {cells.ToString(CultureInfo.InvariantCulture)} background cells are pictured over a collectible's graphics");
        }
        AssertTrue(artwork.Sheets.Count > 0, "the installation holds room block sheets");
        AssertTrue(collectiblePictures > 0, "some room block sheet pictures the collectibles its rooms place");
        AssertTrue(failures.Count == 0,
            $"{failures.Count} installed room block sheet faults:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
        Console.WriteLine($"Room block sheets whole: {blocks} blocks of {artwork.Sheets.Count} sheets show their own artwork; "
            + $"{collectiblePictures} collectible pictures lie on the collectibles' own blocks.");
    }

    /// <summary>
    /// Records in <paramref name="failures"/> the pictures of <paramref name="sheet"/> named for a collectible that are not
    /// a collectible's own block showing one the tileset's rooms place, <paramref name="kinds"/>: a block or background
    /// cell pictured over a collectible's graphics, or a collectible no room of the tileset places.
    /// </summary>
    private static void CheckPictureNames(string name, RoomBlockSheet sheet, HashSet<InWorldCollectibleKind> kinds,
        Dictionary<int, int> collectibleBlocks, List<string> failures)
    {
        string[] shown = [.. kinds.Select(CollectibleSlotAppearance.ValueOf)];
        int foreign = 0, unplaced = 0;
        foreach (string picture in sheet.Sheet.frames.Keys)
        {
            int separator = picture.IndexOf('|', StringComparison.Ordinal);
            if (separator < 0 || !picture.Contains($"|{AppearanceInputTokens.CollectibleSlotPrefix}", StringComparison.Ordinal))
                continue;
            string subject = picture[..separator];
            if (kinds.Count == 0 || subject.Length != 3
                || !int.TryParse(subject, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int block)
                || !collectibleBlocks.ContainsKey(block))
            {
                foreign++;
                continue;
            }
            string value = picture[(picture.IndexOf('=', separator) + 1)..];
            if (value != RoomAppearanceValues.Loaded && !shown.Contains(value, StringComparer.Ordinal))
                unplaced++;
        }
        if (foreign > 0)
            failures.Add($"{name}: {foreign.ToString(CultureInfo.InvariantCulture)} pictures show a block or background cell over a collectible's graphics");
        if (unplaced > 0)
            failures.Add($"{name}: {unplaced.ToString(CultureInfo.InvariantCulture)} pictures show a collectible no room of the tileset places");
    }

    /// <summary>
    /// The collectibles that load their own graphics each tileset's rooms place, by tileset: those every retail room state
    /// of the tileset's population sets up.
    /// </summary>
    private static Dictionary<TilesetDefinition, HashSet<InWorldCollectibleKind>> CollectiblesPlaced()
    {
        var placed = new Dictionary<TilesetDefinition, HashSet<InWorldCollectibleKind>>();
        foreach (RoomHeaderDefinition room in RetailRoomHeaders.All)
        {
            foreach (RoomStateSettings state in RoomStateLists.GetStates(room))
            {
                foreach (RoomPlmPlacement placement in state.PlmPopulation.Placements.Span)
                {
                    if (!CollectiblePlms.TryIdentifyPermanentCollectible(placement.Header.Header, out InWorldCollectibleKind kind, out _)
                        || !CollectiblePlms.UsesDynamicGraphics(kind))
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

    /// <summary>
    /// The blocks a collectible that loads its own graphics draws where it lies, each with its graphics slot, as the
    /// installed collectible visuals draw each slot's two frames.
    /// </summary>
    private static Dictionary<int, int> CollectibleBlocks(InstalledRoomPlmCollectibleVisuals visuals)
    {
        InWorldCollectibleKind any = Enum.GetValues<InWorldCollectibleKind>().First(CollectiblePlms.UsesDynamicGraphics);
        var blocks = new Dictionary<int, int>();
        for (int slot = 0; slot < CollectiblePlms.GraphicsSlots; slot++)
        {
            for (int frame = 0; frame < CollectibleFrames; frame++)
                blocks.Add(visuals.GetAppearance(CollectiblePlmDraws.VisibleFrame(any, frame, slot)).Block, slot);
        }
        return blocks;
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

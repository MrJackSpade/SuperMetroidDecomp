using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// A graphics set's OBJ characters as its room load leaves them, and as each look of the encounters its members play leaves
/// them (#1392, #1538). A frame gets a finished picture of each look it is drawn in
/// (<see cref="EncounterArtworkDrawingDefinitions"/>), composed by copying every look up to the one shown, in order, over
/// the loaded characters; looks that leave the frame alike give it one picture, and a frame the looks of an encounter
/// gameplay shows change has a picture of each, named by the looks. Gameplay knows nothing of where the copies land. A
/// rotting corpse's frame is pictured once per look, with where the corpse lies in it, and the game draws the whole picture
/// through the corpse's own rows as its rot leaves them (#1539).
/// </summary>
internal sealed class SetCharacters
{
    /// <summary>The OBJ character table as the room load leaves it.</summary>
    private readonly byte[] table;

    /// <summary>The encounters the set's members play, and what each of their looks copies.</summary>
    internal IReadOnlyList<EncounterLookSteps> Encounters { get; }

    /// <summary>Each encounter's characters as each of its looks leaves them, from the loaded table.</summary>
    private readonly Dictionary<ComposedEncounter, Dictionary<ComposedLook, byte[]>> afterLook = [];

    /// <summary>The corpses that rot in the set's rooms, whose frames are drawn through their own rows.</summary>
    private readonly IReadOnlyList<RottingCorpseCells> corpses;

    /// <summary>Each combination of looks' characters, by the looks it shows.</summary>
    private readonly Dictionary<EncounterLookCombination, SpriteArtwork> pages = [];

    /// <summary>Each combination of looks' character table, by the looks it shows.</summary>
    private readonly Dictionary<EncounterLookCombination, byte[]> lookTables = [];

    /// <summary>
    /// The table as the room load leaves it, the encounters the set's members play, and the corpses that rot in the set's
    /// rooms.
    /// </summary>
    internal SetCharacters(byte[] table, IReadOnlyList<EncounterLookSteps> Encounters, IReadOnlyList<RottingCorpseCells> corpses)
    {
        this.table = Ensure.NotNull(table);
        this.Encounters = Ensure.NotNull(Encounters);
        this.corpses = Ensure.NotNull(corpses);
        foreach (EncounterLookSteps encounter in Encounters)
        {
            byte[] shown = [.. table];
            var shownAfter = new Dictionary<ComposedLook, byte[]>();
            foreach (EncounterLookStep step in encounter.InOrder)
            {
                foreach (EncounterArtworkUpload upload in step.Copies)
                    upload.ApplyTo(shown);
                shownAfter.Add(step.Look, [.. shown]);
            }
            afterLook.Add(encounter.Encounter, shownAfter);
        }
        Loaded = ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), table);
    }

    /// <summary>The characters as the room load leaves them.</summary>
    internal SpriteArtwork Loaded { get; }

    /// <summary>
    /// The finished pictures of <paramref name="frame"/>, named for <paramref name="name"/> (#1538): for each encounter
    /// <paramref name="drawings"/> lists the frame for, the frame is composed over the look it is drawn in, or over each
    /// look it is drawn in where those looks leave it otherwise, which only an encounter gameplay shows the looks of may
    /// do; an encounter that lists it for none leaves it as loaded. A frame whose artwork is its one look's own reads only
    /// another family's artwork where that look's copies never write, so it shows nothing. A picture in which every character
    /// the frame reads is blank shows nothing: it is named among the frame's blank pictures instead of composed. A frame that
    /// shows a rotting corpse names where the corpse lies in its pictures (#1539).
    /// </summary>
    /// <param name="name">The frame's name, which names its pictures.</param>
    /// <param name="frame">The frame.</param>
    /// <param name="drawings">Which looks of each encounter the frame is drawn in.</param>
    /// <param name="showsCorpse">Whether the frame is one the game shows a rotting corpse in, over the corpse's rows it reads.</param>
    /// <exception cref="InvalidDataException">
    /// A corpse's frame reads the rows of two corpses, or does not say where its corpse lies in it; a frame whose artwork is its
    /// look's own is drawn in more than one look; or the looks of an encounter gameplay never shows the looks of leave the
    /// frame otherwise.
    /// </exception>
    internal FramePictures Pictures(string name, SpriteFrame frame, FrameDrawings drawings, bool showsCorpse = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Ensure.NotNull(frame);
        Ensure.NotNull(drawings);
        RottingCorpseCells? corpse = showsCorpse ? CorpseRowPictures.CorpseOf(name, frame, corpses) : null;
        int[] read = [.. frame.Parts.SelectMany(Read).Distinct().Order()];
        var parts = new List<FrameLookPart>();
        foreach (EncounterLookSteps encounter in Encounters)
        {
            if (drawings.For(encounter.Encounter) is not FrameDrawnLooks drawn)
                continue;
            if (drawn.ArtworkOfLook)
            {
                if (drawn.Looks is not [ComposedLook own])
                    throw new InvalidDataException($"Frame {name}'s artwork is its {encounter.Encounter} look's own, yet it is drawn in {drawn.Looks.Count} looks.");
                // A frame its own look's copies never write reads only what the room's load or an earlier look leaves there,
                // another family's artwork: it has none of its own to show, so it shows nothing (#1513), as Mother Brain's
                // initial body frame, drawn only while her body is hidden, reads the head artwork her room loads where her
                // legs never land.
                if (!own.IsLoaded && !Writes(encounter.CopiesOf(own), read))
                    return new FramePictures(name, [], [], [name]);
            }
            ComposedLook[] distinct = [.. drawn.Looks.DistinctBy(look => Shown(afterLook[encounter.Encounter][look], read))];
            if (distinct.Length == 1)
            {
                parts.Add(new FrameLookPart(encounter, [drawn.Looks[0]], null));
                continue;
            }
            EncounterArtwork shownAs = EncounterLookUploads.ShownAs(encounter.Encounter) ?? throw new InvalidDataException(
                $"Frame {name} shows {distinct.Length} pictures in the looks of {encounter.Encounter}, whose looks gameplay never shows.");
            parts.Add(new FrameLookPart(encounter, drawn.Looks, shownAs));
        }
        FrameLookPart[] changing = [.. parts.Where(part => part.ShownAs is not null)];
        FrameLookChange[] changes = [.. changing.Select(part => new FrameLookChange(
            part.ShownAs ?? throw new InvalidOperationException($"Frame {name}'s changing part names no shown encounter."),
            [.. part.Looks.Select(look => look.Name)]))
            .OrderBy(change => Installation.EnemySheetFiles.EncounterOrder.PlaceOf(change.Encounter))];
        var sources = new List<SheetFrameSource>();
        var blank = new List<string>();
        var placements = new Dictionary<string, CorpsePicturePlacement>(StringComparer.Ordinal);
        foreach (PartLook[] combination in Combinations(parts))
        {
            string picture = EncounterLook.PictureName(name, combination.Select(choice => choice.Part.ShownAs is EncounterArtwork shown
                ? new EncounterLook(shown, choice.Look.Name) : (EncounterLook?)null)
                .OfType<EncounterLook>().OrderBy(look => Installation.EnemySheetFiles.EncounterOrder.PlaceOf(look.Encounter)));
            byte[] characters = Characters(combination);
            if (ShowsNothing(characters, read))
            {
                blank.Add(picture);
                continue;
            }
            sources.Add(new SheetFrameSource(picture, frame, Page(combination)));
            if (corpse is not null)
                placements.Add(picture, CorpseRowPictures.PlacementOf(picture, frame, corpse));
        }
        return new FramePictures(name, sources, changes, blank) { Corpses = placements };
    }

    /// <summary>
    /// Every combination of the looks <paramref name="parts"/> picture the frame in, one look of each part, by part: a
    /// part that leaves the frame alike in all its looks pictures it in one.
    /// </summary>
    private static IEnumerable<PartLook[]> Combinations(IReadOnlyList<FrameLookPart> parts)
    {
        IEnumerable<PartLook[]> combinations = [[]];
        foreach (FrameLookPart part in parts)
            combinations = [.. combinations.SelectMany(earlier => part.Looks.Select(look => (PartLook[])[.. earlier, new PartLook(part, look)]))];
        return combinations;
    }

    /// <summary>Whether <paramref name="copies"/> copy over any character whose first table byte <paramref name="read"/> names.</summary>
    private static bool Writes(IReadOnlyList<EncounterArtworkUpload> copies, int[] read)
    {
        foreach (EncounterArtworkUpload upload in copies)
        {
            int end = upload.FirstByte + upload.Characters.Length;
            if (read.Any(character => character < end && character + SpriteArtwork.CharacterByteCount > upload.FirstByte))
                return true;
        }
        return false;
    }

    /// <summary>Whether every character <paramref name="read"/> names holds only blank bytes in <paramref name="characters"/>.</summary>
    private static bool ShowsNothing(byte[] characters, int[] read)
    {
        foreach (int first in read)
        {
            if (characters.AsSpan(first, SpriteArtwork.CharacterByteCount).ContainsAnyExcept((byte)0))
                return false;
        }
        return true;
    }

    /// <summary>The characters with every look of each part up to the one <paramref name="combination"/> names copied in order.</summary>
    private SpriteArtwork Page(PartLook[] combination)
    {
        var key = new EncounterLookCombination(combination.Select(choice => choice.Look));
        if (pages.TryGetValue(key, out SpriteArtwork? page))
            return page;
        byte[] characters = Characters(combination);
        page = ReferenceEquals(characters, table) ? Loaded : ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), characters);
        pages.Add(key, page);
        return page;
    }

    /// <summary>
    /// The character table <see cref="Page"/> shows for <paramref name="combination"/>, built once: the loaded table where
    /// every part shows its room's load.
    /// </summary>
    private byte[] Characters(PartLook[] combination)
    {
        if (combination.All(choice => choice.Look.IsLoaded))
            return table;
        var key = new EncounterLookCombination(combination.Select(choice => choice.Look));
        if (lookTables.TryGetValue(key, out byte[]? known))
            return known;
        byte[] shown = [.. table];
        foreach (PartLook choice in combination)
        {
            foreach (EncounterArtworkUpload upload in choice.Part.Encounter.CopiesThrough(choice.Look))
                upload.ApplyTo(shown);
        }
        lookTables.Add(key, shown);
        return shown;
    }

    /// <summary>The bytes of <paramref name="characters"/>'s characters <paramref name="read"/> names, spelled for comparing.</summary>
    private static string Shown(byte[] characters, int[] read) =>
        Convert.ToBase64String(read.SelectMany(first => characters.AsSpan(first, SpriteArtwork.CharacterByteCount).ToArray()).ToArray());

    /// <summary>The first table byte of each character <paramref name="part"/> reads that lies in the table; one past it reads the page's blank overhang.</summary>
    private static IEnumerable<int> Read(SpriteFramePart part)
    {
        for (int row = 0; row < part.Size.Y / SpriteArtwork.CharacterPixels; row++)
        {
            for (int column = 0; column < part.Size.X / SpriteArtwork.CharacterPixels; column++)
            {
                int firstByte = (part.Character + row * SpriteArtwork.CharactersPerRow + column) * SpriteArtwork.CharacterByteCount;
                if (firstByte < SpriteCharacterTableLayout.TableByteCount)
                    yield return firstByte;
            }
        }
    }

    /// <summary>The looks of one encounter a frame is pictured in.</summary>
    /// <param name="Encounter">The encounter and what its looks copy.</param>
    /// <param name="Looks">The looks the frame is pictured in.</param>
    /// <param name="ShownAs">The encounter gameplay shows, whose looks name the frame's pictures; none for a part that pictures the frame in one look.</param>
    internal sealed record FrameLookPart(EncounterLookSteps Encounter, IReadOnlyList<ComposedLook> Looks, EncounterArtwork? ShownAs);

    /// <summary>One part of a frame with the look a combination shows it in.</summary>
    /// <param name="Part">The part.</param>
    /// <param name="Look">The look of the part's encounter shown.</param>
    private readonly record struct PartLook(FrameLookPart Part, ComposedLook Look);
}

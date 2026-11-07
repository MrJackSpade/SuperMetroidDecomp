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
/// rotting corpse's frame is pictured once per look, with where the corpse lies in it, and the game draws it through the
/// corpse's rows as its rot leaves them (#1531).
/// </summary>
internal sealed class SetCharacters
{
    /// <summary>The OBJ character table as the room load leaves it.</summary>
    private readonly byte[] table;

    /// <summary>The encounters the set's members play, and what each of their looks copies.</summary>
    private readonly IReadOnlyList<EncounterLookSteps> encounters;

    /// <summary>Each encounter's characters as each of its looks leaves them, from the loaded table.</summary>
    private readonly Dictionary<ComposedEncounter, byte[][]> afterLook = [];

    /// <summary>The corpses that rot in the set's rooms, whose frames are drawn through their rows.</summary>
    private readonly IReadOnlyList<RottingCorpseCells> corpses;

    /// <summary>Each combination of looks' characters, by the looks it shows.</summary>
    private readonly Dictionary<string, SpriteArtwork> pages = new(StringComparer.Ordinal);

    /// <summary>Each combination of looks' character table, by the looks it shows.</summary>
    private readonly Dictionary<string, byte[]> lookTables = new(StringComparer.Ordinal);

    /// <summary>
    /// The table as the room load leaves it, the encounters the set's members play, and the corpses that rot in the set's
    /// rooms.
    /// </summary>
    /// <exception cref="InvalidDataException">An encounter's look changes a corpse's rows after the corpse's own characters load.</exception>
    internal SetCharacters(byte[] table, IReadOnlyList<EncounterLookSteps> encounters, IReadOnlyList<RottingCorpseCells> corpses)
    {
        this.table = Ensure.NotNull(table);
        this.encounters = Ensure.NotNull(encounters);
        this.corpses = Ensure.NotNull(corpses);
        foreach (RottingCorpseCells corpse in corpses)
            corpse.RequireUnchangedBy(encounters);
        foreach (EncounterLookSteps encounter in encounters)
        {
            byte[] shown = [.. table];
            afterLook.Add(encounter.Encounter, [.. encounter.Looks.Select(look =>
            {
                foreach (EncounterArtworkUpload upload in look)
                    upload.ApplyTo(shown);
                return (byte[])[.. shown];
            })]);
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
    /// shows a rotting corpse names where the corpse lies in its pictures (#1531).
    /// </summary>
    /// <param name="name">The frame's name, which names its pictures.</param>
    /// <param name="frame">The frame.</param>
    /// <param name="drawings">Which looks of each encounter the frame is drawn in.</param>
    /// <param name="showsCorpse">Whether the frame is one the game shows a rotting corpse in, over the corpse's rows it reads.</param>
    /// <exception cref="InvalidDataException">
    /// A corpse's frame reads the rows of two corpses, or is not drawn as its corpse's rows rot; a frame whose artwork is its
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
        foreach (EncounterLookSteps encounter in encounters)
        {
            if (drawings.For(encounter.Encounter) is not FrameDrawnLooks drawn)
                continue;
            if (drawn.ArtworkOfLook)
            {
                if (drawn.Looks is not [int own])
                    throw new InvalidDataException($"Frame {name}'s artwork is its {encounter.Encounter} look's own, yet it is drawn in {drawn.Looks.Count} looks.");
                // A frame its own look's copies never write reads only what the room's load or an earlier look leaves there,
                // another family's artwork: it has none of its own to show, so it shows nothing (#1513), as Mother Brain's
                // initial body frame, drawn only while her body is hidden, reads the head artwork her room loads where her
                // legs never land.
                if (own > 0 && !Writes(encounter.Looks[own], read))
                    return new FramePictures(name, [], [], [name]);
            }
            int[] distinct = [.. drawn.Looks.DistinctBy(look => Shown(afterLook[encounter.Encounter][look], read))];
            if (distinct.Length == 1)
            {
                parts.Add(new FrameLookPart(encounter, [drawn.Looks[0]], null));
                if (corpse is not null)
                    RequireOneCover(name, corpse, encounter, drawn.Looks);
                continue;
            }
            EncounterArtwork shownAs = EncounterLookUploads.ShownAs(encounter.Encounter) ?? throw new InvalidDataException(
                $"Frame {name} shows {distinct.Length} pictures in the looks of {encounter.Encounter}, whose looks gameplay never shows.");
            parts.Add(new FrameLookPart(encounter, drawn.Looks, shownAs));
        }
        FrameLookPart[] changing = [.. parts.Where(part => part.ShownAs is not null)];
        FrameLookChange[] changes = [.. changing.Select(part => new FrameLookChange(
            part.ShownAs ?? throw new InvalidOperationException($"Frame {name}'s changing part names no shown encounter."),
            [.. part.Looks.Select(look => EncounterLookUploads.Looks(part.Encounter.Encounter)[look])])).OrderBy(change => change.Encounter)];
        var sources = new List<SheetFrameSource>();
        var blank = new List<string>();
        var layouts = new Dictionary<string, CorpsePictureLayout>(StringComparer.Ordinal);
        foreach (int[] combination in Combinations(parts))
        {
            string picture = EncounterLook.PictureName(name, parts.Select((part, index) => part.ShownAs is EncounterArtwork shown
                ? new EncounterLook(shown, EncounterLookUploads.Looks(part.Encounter.Encounter)[combination[index]]) : (EncounterLook?)null)
                .OfType<EncounterLook>().OrderBy(look => look.Encounter));
            byte[] characters = Characters(parts, combination);
            if (ShowsNothing(characters, read))
            {
                blank.Add(picture);
                continue;
            }
            sources.Add(new SheetFrameSource(picture, frame, Page(parts, combination)));
            if (corpse is not null)
                layouts.Add(picture, CorpseRowPictures.LayoutOf(picture, frame, corpse, Covered(corpse, parts, combination)));
        }
        return new FramePictures(name, sources, changes, blank) { Corpses = layouts };
    }

    /// <summary>
    /// Every combination of the looks <paramref name="parts"/> picture the frame in, one look of each part, by part: a
    /// part that leaves the frame alike in all its looks pictures it in one.
    /// </summary>
    private static IEnumerable<int[]> Combinations(IReadOnlyList<FrameLookPart> parts)
    {
        IEnumerable<int[]> combinations = [[]];
        foreach (FrameLookPart part in parts)
            combinations = [.. combinations.SelectMany(earlier => part.Looks.Select(look => (int[])[.. earlier, look]))];
        return combinations;
    }

    /// <summary>
    /// The characters of <paramref name="corpse"/>'s rows the looks of each part, up to the one <paramref name="combination"/>
    /// names, have copied other characters over since the corpse's own last loaded (#1531).
    /// </summary>
    private static HashSet<int> Covered(RottingCorpseCells corpse, IReadOnlyList<FrameLookPart> parts, int[] combination)
    {
        var covered = new HashSet<int>();
        for (int index = 0; index < parts.Count; index++)
            covered.UnionWith(corpse.CoveredBy(parts[index].Encounter, combination[index]));
        return covered;
    }

    /// <summary>
    /// Requires each of <paramref name="looks"/> a frame showing <paramref name="corpse"/> is pictured once for to cover the
    /// corpse's characters alike once the corpse's own characters have loaded (#1531): the picture names where the corpse
    /// still lies in it, so every look that shows it must leave the corpse alike.
    /// </summary>
    /// <exception cref="InvalidDataException">Two looks that show one picture leave the corpse's characters covered otherwise.</exception>
    private static void RequireOneCover(string name, RottingCorpseCells corpse, EncounterLookSteps encounter, IReadOnlyList<int> looks)
    {
        IReadOnlySet<int> pictured = corpse.CoveredBy(encounter, looks[0]);
        foreach (int look in looks.Where(look => corpse.LoadedBy(encounter, look)))
        {
            if (!corpse.CoveredBy(encounter, look).SetEquals(pictured))
                throw new InvalidDataException(
                    $"Frame {name} shows one picture in every look of {encounter.Encounter} it is drawn in, yet look {look} leaves {corpse.Name}'s characters covered otherwise.");
        }
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

    /// <summary>
    /// The characters as the set's encounters leave them once each shows its look of <paramref name="shown"/> (#1538): every
    /// look of each such encounter up to the one shown copied in order over the room's load, which a frame's picture of
    /// those looks paints like.
    /// </summary>
    /// <exception cref="InvalidDataException">A look is of an encounter the set's members do not play, or the encounter has no such look.</exception>
    internal SpriteArtwork Page(IReadOnlyList<EncounterLook> shown)
    {
        Ensure.NotNull(shown);
        FrameLookPart[] parts = [.. shown.Select(look => encounters.FirstOrDefault(encounter => EncounterLookUploads.ShownAs(encounter.Encounter) == look.Encounter)
            is { } played ? new FrameLookPart(played, [EncounterLookUploads.IndexOf(played.Encounter, look.Name)], look.Encounter)
            : throw new InvalidDataException($"The set's members play no encounter that shows {look}."))];
        return Page(parts, [.. parts.Select(part => part.Looks[0])]);
    }

    /// <summary>
    /// The characters as the set's members leave them once <paramref name="encounter"/> shows its look at
    /// <paramref name="look"/> (#1538): every look of it up to that one copied in order over the room's load, which the
    /// finished pictures of a frame drawn in that look paint like, whether or not gameplay shows the encounter's looks.
    /// </summary>
    /// <exception cref="InvalidDataException">The set's members do not play the encounter.</exception>
    internal SpriteArtwork Page(ComposedEncounter encounter, int look)
    {
        EncounterLookSteps played = encounters.FirstOrDefault(candidate => candidate.Encounter == encounter)
            ?? throw new InvalidDataException($"The set's members do not play {encounter}.");
        return Page([new FrameLookPart(played, [look], EncounterLookUploads.ShownAs(encounter))], [look]);
    }

    /// <summary>The characters with every look of each part up to the one <paramref name="combination"/> names copied in order.</summary>
    private SpriteArtwork Page(IReadOnlyList<FrameLookPart> parts, int[] combination)
    {
        string key = Key(parts, combination);
        if (pages.TryGetValue(key, out SpriteArtwork? page))
            return page;
        byte[] characters = Characters(parts, combination);
        page = ReferenceEquals(characters, table) ? Loaded : ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), characters);
        pages.Add(key, page);
        return page;
    }

    /// <summary>
    /// The character table <see cref="Page"/> shows for <paramref name="combination"/>, built once: the loaded table where
    /// every part shows its room's load.
    /// </summary>
    private byte[] Characters(IReadOnlyList<FrameLookPart> parts, int[] combination)
    {
        if (combination.All(look => look == 0))
            return table;
        string key = Key(parts, combination);
        if (lookTables.TryGetValue(key, out byte[]? known))
            return known;
        byte[] shown = [.. table];
        for (int index = 0; index < parts.Count; index++)
        {
            foreach (EncounterArtworkUpload upload in parts[index].Encounter.Looks.Take(combination[index] + 1).SelectMany(copies => copies))
                upload.ApplyTo(shown);
        }
        lookTables.Add(key, shown);
        return shown;
    }

    /// <summary>The key of <paramref name="combination"/>'s characters: each part's encounter and the look it shows.</summary>
    private static string Key(IReadOnlyList<FrameLookPart> parts, int[] combination) =>
        string.Concat(parts.Select((part, index) => $"|{part.Encounter.Encounter}={combination[index]}"));

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
    /// <param name="Looks">The places of the looks the frame is pictured in.</param>
    /// <param name="ShownAs">The encounter gameplay shows, whose looks name the frame's pictures; none for a part that pictures the frame in one look.</param>
    private sealed record FrameLookPart(EncounterLookSteps Encounter, IReadOnlyList<int> Looks, EncounterArtwork? ShownAs);
}

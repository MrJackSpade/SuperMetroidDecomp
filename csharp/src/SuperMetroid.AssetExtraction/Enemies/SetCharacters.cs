using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// A graphics set's OBJ characters as its room load leaves them, and as each step of the artwork sequences its encounters
/// play leaves them (#1392, #1513). A frame the sequences change gets a pre-assembled picture of each distinct look they
/// give it at the steps it is drawn at (<see cref="EncounterArtworkDrawingDefinitions"/>), composed by copying every step up to the one shown, in order, over the loaded characters; the game names the
/// picture by the steps reached and knows nothing of where the copies land. A rotting corpse's frame is pictured once, with
/// where the corpse lies in it, and the game draws it through the corpse's rows as its rot leaves them (#1531).
/// </summary>
internal sealed class SetCharacters
{
    /// <summary>The OBJ character table as the room load leaves it.</summary>
    private readonly byte[] table;

    /// <summary>The artwork sequences the set's encounters play, and what each step copies.</summary>
    private readonly IReadOnlyList<EncounterArtworkSteps> sequences;

    /// <summary>Each sequence's characters after each of its steps, from the loaded table.</summary>
    private readonly Dictionary<EncounterArtworkSequence, byte[][]> afterStep = [];

    /// <summary>The corpses that rot in the set's rooms, whose frames are drawn through their rows.</summary>
    private readonly IReadOnlyList<RottingCorpseCells> corpses;

    /// <summary>Each picture's characters, by the steps it shows.</summary>
    private readonly Dictionary<string, SpriteArtwork> pages = new(StringComparer.Ordinal);

    /// <summary>Each combination of sequence steps' character table, by the steps it shows.</summary>
    private readonly Dictionary<string, byte[]> stepTables = new(StringComparer.Ordinal);

    /// <summary>
    /// The table as the room load leaves it, the artwork sequences the set's encounters play, and the corpses that rot in
    /// the set's rooms.
    /// </summary>
    /// <exception cref="InvalidDataException">An encounter's sequence changes a corpse's rows after the corpse's own characters load.</exception>
    internal SetCharacters(byte[] table, IReadOnlyList<EncounterArtworkSteps> sequences, IReadOnlyList<RottingCorpseCells> corpses)
    {
        this.table = Ensure.NotNull(table);
        this.sequences = Ensure.NotNull(sequences);
        this.corpses = Ensure.NotNull(corpses);
        foreach (RottingCorpseCells corpse in corpses)
            corpse.RequireUnchangedBy(sequences);
        foreach (EncounterArtworkSteps sequence in sequences)
        {
            byte[] shown = [.. table];
            afterStep.Add(sequence.Sequence, [.. sequence.Steps.Select(step =>
            {
                foreach (EncounterArtworkUpload upload in step)
                    upload.ApplyTo(shown);
                return (byte[])[.. shown];
            })]);
        }
        Loaded = ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), table);
    }

    /// <summary>The characters as the room load leaves them.</summary>
    internal SpriteArtwork Loaded { get; }

    /// <summary>
    /// The pictures of <paramref name="frame"/>, named for <paramref name="name"/>: the frame as the room load leaves it when
    /// no sequence changes the characters it reads at a step it is drawn at, else its picture of each combination of the
    /// distinct looks the sequences give it at the steps <paramref name="drawings"/> says it is drawn at (#1513), each step
    /// of a sequence that leaves the frame as an earlier step did sharing that step's picture, and its loaded picture only
    /// when it is drawn before a sequence's first step or a step it is drawn at leaves it as loaded. A sequence no entry
    /// lists the frame for leaves it as loaded. A picture in which every character the frame reads is blank shows nothing:
    /// it is named among the frame's blank pictures instead of composed. A frame that shows a rotting corpse names where the
    /// corpse lies in its pictures (#1531).
    /// </summary>
    /// <param name="name">The frame's name, which names its pictures.</param>
    /// <param name="frame">The frame.</param>
    /// <param name="drawings">When the frame is drawn under each sequence.</param>
    /// <param name="showsCorpse">Whether the frame is one the game shows a rotting corpse in, over the corpse's rows it reads.</param>
    /// <exception cref="InvalidDataException">A corpse's frame reads the rows of two corpses, or is not drawn as its corpse's rows rot.</exception>
    internal FramePictures Pictures(string name, SpriteFrame frame, FrameDrawings drawings, bool showsCorpse = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Ensure.NotNull(frame);
        Ensure.NotNull(drawings);
        RottingCorpseCells? corpse = showsCorpse ? CorpseRowPictures.CorpseOf(name, frame, corpses) : null;
        int[] read = [.. frame.Parts.SelectMany(Read).Distinct().Order()];
        string loaded = Shown(table, read);
        var variations = new List<FrameArtworkVariation>();
        foreach (EncounterArtworkSteps sequence in sequences)
        {
            if (drawings.For(sequence.Sequence) is not FrameDrawnSteps drawn)
                continue;
            bool[] drawnAt = [.. Enumerable.Range(0, sequence.Steps.Count).Select(drawn.DrawnAt)];
            var first = new Dictionary<string, int>(StringComparer.Ordinal);
            int?[] pictures = [.. afterStep[sequence.Sequence].Select((shown, step) =>
            {
                // A step the frame is never drawn at has no picture of it.
                if (!drawnAt[step])
                    return null;
                string look = Shown(shown, read);
                if (look == loaded)
                    return (int?)null;
                if (!first.TryGetValue(look, out int earlier))
                    first.Add(look, earlier = step);
                return earlier;
            })];
            if (pictures.Any(step => step is not null))
                variations.Add(new FrameArtworkVariation(sequence.Sequence, [.. pictures], [.. drawnAt], drawn.DrawnBeforeFirstStep));
        }
        // A picture in which every character the frame reads is blank shows nothing: it is on no sheet, and the frame draws
        // nothing while it shows it (#1513).
        if (variations.Count == 0)
        {
            if (ShowsNothing(table, read))
                return new FramePictures(name, [], [], [name]);
            return new FramePictures(name, [new SheetFrameSource(name, frame, Loaded)], [], [])
            {
                Corpses = corpse is null ? new Dictionary<string, CorpsePictureLayout>(StringComparer.Ordinal)
                    : new Dictionary<string, CorpsePictureLayout>(StringComparer.Ordinal) { [name] = CorpseRowPictures.LayoutOf(name, frame, corpse, new HashSet<int>()) },
            };
        }
        var sources = new List<SheetFrameSource>();
        var blank = new List<string>();
        var layouts = new Dictionary<string, CorpsePictureLayout>(StringComparer.Ordinal);
        if (corpse is not null)
            RequireOneCoverPerPicture(name, corpse, variations);
        foreach (IReadOnlyList<EncounterArtworkStep> combination in FrameVariationPictures.Combinations(variations))
        {
            string picture = FrameArtworkVariation.PictureName(name, combination);
            if (ShowsNothing(Characters(combination), read))
                blank.Add(picture);
            else
            {
                sources.Add(new SheetFrameSource(picture, frame, Page(combination)));
                if (corpse is not null)
                    layouts.Add(picture, CorpseRowPictures.LayoutOf(picture, frame, corpse, Covered(corpse, combination)));
            }
        }
        return new FramePictures(name, sources, [.. variations], blank) { Corpses = layouts };
    }

    /// <summary>
    /// The characters of <paramref name="corpse"/>'s rows the steps of each sequence <paramref name="combination"/> names,
    /// up to the one it names, have copied other characters over since the corpse's own last loaded (#1531).
    /// </summary>
    private HashSet<int> Covered(RottingCorpseCells corpse, IReadOnlyList<EncounterArtworkStep> combination)
    {
        var covered = new HashSet<int>();
        foreach ((EncounterArtworkSequence sequence, int step) in combination)
            covered.UnionWith(corpse.CoveredBy(sequences.Single(played => played.Sequence == sequence), step));
        return covered;
    }

    /// <summary>
    /// Requires each step a frame showing <paramref name="corpse"/> is drawn at once the corpse's own characters have
    /// loaded to cover the corpse's characters as the step whose picture it shows does (#1531): the picture names where
    /// the corpse still lies in it, so every step that shows it must leave the corpse alike.
    /// </summary>
    /// <exception cref="InvalidDataException">Two steps that show one picture leave the corpse's characters covered differently.</exception>
    private void RequireOneCoverPerPicture(string name, RottingCorpseCells corpse, IReadOnlyList<FrameArtworkVariation> variations)
    {
        foreach (FrameArtworkVariation variation in variations)
        {
            EncounterArtworkSteps sequence = sequences.Single(played => played.Sequence == variation.Sequence);
            for (int step = 0; step < variation.Pictures.Length; step++)
            {
                if (!variation.DrawnAt(step) || !corpse.LoadedBy(sequence, step) || variation.Pictures[step] == step)
                    continue;
                // A step showing the loaded picture shows the corpse uncovered.
                bool alike = variation.Pictures[step] is int shown
                    ? corpse.LoadedBy(sequence, shown) && corpse.CoveredBy(sequence, step).SetEquals(corpse.CoveredBy(sequence, shown))
                    : corpse.CoveredBy(sequence, step).Count == 0;
                if (!alike)
                    throw new InvalidDataException(
                        $"Frame {name} shows another step's picture of {variation.Sequence} at step {step}, which leaves {corpse.Name}'s characters covered otherwise.");
            }
        }
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

    /// <summary>The characters with every step of each sequence <paramref name="combination"/> names, up to the one it names, copied in order.</summary>
    internal SpriteArtwork Page(IReadOnlyList<EncounterArtworkStep> combination)
    {
        string key = FrameArtworkVariation.PictureName("page", combination);
        if (pages.TryGetValue(key, out SpriteArtwork? page))
            return page;
        page = ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), Characters(combination));
        pages.Add(key, page);
        return page;
    }

    /// <summary>The character table <see cref="Page"/> shows for <paramref name="combination"/>, built once.</summary>
    private byte[] Characters(IReadOnlyList<EncounterArtworkStep> combination)
    {
        string key = FrameArtworkVariation.PictureName("page", combination);
        if (stepTables.TryGetValue(key, out byte[]? known))
            return known;
        byte[] shown = [.. table];
        foreach ((EncounterArtworkSequence sequence, int step) in combination)
        {
            foreach (EncounterArtworkUpload upload in sequences.Single(played => played.Sequence == sequence).Steps.Take(step + 1).SelectMany(copies => copies))
                upload.ApplyTo(shown);
        }
        stepTables.Add(key, shown);
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
}

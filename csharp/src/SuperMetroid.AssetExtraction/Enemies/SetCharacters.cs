using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// A graphics set's OBJ characters as its room load leaves them, and as each step of the artwork sequences its encounters
/// play leaves them (#1392, #1513). A frame the sequences change gets a pre-assembled picture of each distinct look they
/// give it, composed by copying every step up to the one shown, in order, over the loaded characters; the game names the
/// picture by the steps reached and knows nothing of where the copies land. A corpse's frame drawn over the rows its kind
/// uploads gets a picture of each rot the kind can show (#1406).
/// </summary>
internal sealed class SetCharacters
{
    /// <summary>The OBJ character table as the room load leaves it.</summary>
    private readonly byte[] table;

    /// <summary>The artwork sequences the set's encounters play, and what each step copies.</summary>
    private readonly IReadOnlyList<EncounterArtworkSteps> sequences;

    /// <summary>Each sequence's characters after each of its steps, from the loaded table.</summary>
    private readonly Dictionary<EncounterArtworkSequence, byte[][]> afterStep = [];

    /// <summary>The corpses of a kind that rot in the set's rooms, whose frames select the rot they show.</summary>
    private readonly IReadOnlyList<CorpseRotSimulation> corpses;

    /// <summary>Each picture's characters, by the steps or rot it shows.</summary>
    private readonly Dictionary<string, SpriteArtwork> pages = new(StringComparer.Ordinal);

    /// <summary>
    /// The table as the room load leaves it, the artwork sequences the set's encounters play, and the corpses of a kind that
    /// rot in the set's rooms.
    /// </summary>
    /// <exception cref="InvalidDataException">A corpse is not of a kind, or uploads a row over characters an encounter's sequence copies.</exception>
    internal SetCharacters(byte[] table, IReadOnlyList<EncounterArtworkSteps> sequences, IReadOnlyList<CorpseRotSimulation> corpses)
    {
        this.table = Ensure.NotNull(table);
        this.sequences = Ensure.NotNull(sequences);
        this.corpses = Ensure.NotNull(corpses);
        EncounterArtworkUpload[] uploads = [.. sequences.SelectMany(sequence => sequence.Steps).SelectMany(step => step)];
        foreach (CorpseRotSimulation corpse in corpses)
        {
            if (corpse.Kind is null)
                throw new InvalidDataException($"{corpse.Flesh} is pictured by its encounter's artwork sequence, not by a kind's rot.");
            if (corpse.Rows.Any(row => uploads.Any(upload =>
                    row.FirstByte < upload.FirstByte + upload.Characters.Length && upload.FirstByte < row.FirstByte + row.ByteCount)))
                throw new InvalidDataException($"{corpse.Flesh} uploads a row over characters an encounter's artwork sequence copies.");
        }
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
    /// no sequence changes the characters it reads, else its picture of each combination of the distinct looks the
    /// sequences give it, each step of a sequence that leaves the frame as an earlier step did sharing that step's picture.
    /// Only a corpse's own frames show its rot (#1406).
    /// </summary>
    /// <param name="name">The frame's name, which names its pictures.</param>
    /// <param name="frame">The frame.</param>
    /// <param name="showsRot">Whether the frame is a corpse's own, which shows the rot of the corpses' rows it reads.</param>
    /// <exception cref="InvalidDataException">A corpse's frame reads the rows of two kinds of corpse.</exception>
    internal FramePictures Pictures(string name, SpriteFrame frame, bool showsRot = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Ensure.NotNull(frame);
        if (showsRot)
        {
            CorpseRotSimulation[] rotting = [.. corpses.Where(corpse => frame.Parts.Any(part =>
                corpse.Rows.Any(row => Read(part).Any(first => first < row.FirstByte + row.ByteCount && row.FirstByte < first + SpriteArtwork.CharacterByteCount))))];
            if (rotting.Length > 1)
                throw new InvalidDataException($"Frame {name} reads the rows of {string.Join(" and ", rotting.Select(corpse => corpse.Flesh))}.");
            if (rotting.Length == 1)
                return RotPictures(name, frame, rotting[0]);
        }
        int[] read = [.. frame.Parts.SelectMany(Read).Distinct().Order()];
        string loaded = Shown(table, read);
        var variations = new List<FrameArtworkVariation>();
        foreach (EncounterArtworkSteps sequence in sequences)
        {
            var first = new Dictionary<string, int>(StringComparer.Ordinal);
            int?[] pictures = [.. afterStep[sequence.Sequence].Select((shown, step) =>
            {
                string look = Shown(shown, read);
                if (look == loaded)
                    return (int?)null;
                if (!first.TryGetValue(look, out int earlier))
                    first.Add(look, earlier = step);
                return earlier;
            })];
            if (pictures.Any(step => step is not null))
                variations.Add(new FrameArtworkVariation(sequence.Sequence, [.. pictures]));
        }
        if (variations.Count == 0)
            return new FramePictures(name, [new SheetFrameSource(name, frame, Loaded)], [], null);
        return new FramePictures(name, [.. FrameVariationPictures.Combinations(variations).Select(combination =>
            new SheetFrameSource(FrameArtworkVariation.PictureName(name, combination), frame, Page(combination)))], [.. variations], null);
    }

    /// <summary>
    /// The pictures of a frame drawn over the rows of <paramref name="corpse"/> (#1406): the frame whole over the set's
    /// characters, which it shows before the corpse's kind first rots, then over each rot the kind can show.
    /// </summary>
    private FramePictures RotPictures(string name, SpriteFrame frame, CorpseRotSimulation corpse) =>
        new(name, [new SheetFrameSource(name, frame, Loaded), .. corpse.States.Select(state =>
            new SheetFrameSource(state.View.PictureName(name), frame, RotPage(corpse, state)))], [], corpse.Flesh);

    /// <summary>The characters with each of <paramref name="corpse"/>'s rows showing the step <paramref name="state"/> gives it.</summary>
    private SpriteArtwork RotPage(CorpseRotSimulation corpse, CorpseRotState state)
    {
        string key = state.View.PictureName(corpse.Flesh);
        if (pages.TryGetValue(key, out SpriteArtwork? page))
            return page;
        byte[] shown = [.. table];
        foreach ((CorpseRotRow row, int? step) in state.Steps)
        {
            if (step is int taken)
                corpse.RowBytes(row, taken).CopyTo(shown.AsSpan(row.FirstByte));
        }
        page = ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), shown);
        pages.Add(key, page);
        return page;
    }

    /// <summary>The characters with every step of each sequence <paramref name="combination"/> names, up to the one it names, copied in order.</summary>
    internal SpriteArtwork Page(IReadOnlyList<(EncounterArtworkSequence Sequence, int Step)> combination)
    {
        string key = FrameArtworkVariation.PictureName("page", combination);
        if (pages.TryGetValue(key, out SpriteArtwork? page))
            return page;
        byte[] shown = [.. table];
        foreach ((EncounterArtworkSequence sequence, int step) in combination)
        {
            foreach (EncounterArtworkUpload upload in sequences.Single(played => played.Sequence == sequence).Steps.Take(step + 1).SelectMany(copies => copies))
                upload.ApplyTo(shown);
        }
        page = ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), shown);
        pages.Add(key, page);
        return page;
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

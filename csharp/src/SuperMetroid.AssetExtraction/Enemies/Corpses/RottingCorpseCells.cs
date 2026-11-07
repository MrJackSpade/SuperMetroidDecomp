using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// The sprite characters a rotting corpse's rows show in (#1531): the characters the game uploads the corpse's rows into
/// after its rot calls. The game uploads each of the corpse's rows of characters into one row of the sprite character
/// table, the corpse's first row topmost, and each of its columns into one column of the table in every row, so a
/// character's column of the corpse is its column of the table and the band of the corpse's rows it holds is its row of the
/// table less the corpse's first. A column of the corpse begins rotting at the first band the corpse uploads it in: the
/// game's movers skip every row of a column above its flesh, and the game uploads a column only from its flesh down.
/// </summary>
internal sealed class RottingCorpseCells
{
    /// <summary>No encounter's look loads the corpse's own characters; declared first, as the corpses below are built from it.</summary>
    private static readonly Func<ComposedEncounter, int, bool> LoadedByNoLook = static (_, _) => false;

    /// <summary>The band of the corpse's rows each of its characters holds, by character.</summary>
    private readonly Dictionary<int, int> bandOf;

    /// <summary>The encounter and its looks that load the corpse's own characters before it rots; none for a corpse no look loads.</summary>
    private readonly Func<ComposedEncounter, int, bool> loadsFlesh;

    /// <summary>A corpse's characters, from runs of the table it uploads its rows into, each run within one row of the table.</summary>
    /// <exception cref="InvalidDataException">A run is empty or crosses a row of the table.</exception>
    private RottingCorpseCells(string name, IEnumerable<CorpseCharacterRun> runs, Func<ComposedEncounter, int, bool> loadsFlesh)
    {
        Name = name;
        this.loadsFlesh = loadsFlesh;
        CorpseCharacterRun[] uploaded = [.. runs];
        if (uploaded.Length == 0)
            throw new InvalidDataException($"{name} uploads no rows.");
        int firstRow = uploaded.Min(run => run.FirstCharacter / SpriteArtwork.CharactersPerRow);
        bandOf = [];
        foreach (CorpseCharacterRun run in uploaded)
        {
            int row = run.FirstCharacter / SpriteArtwork.CharactersPerRow;
            if (run.Count <= 0 || (run.FirstCharacter + run.Count - 1) / SpriteArtwork.CharactersPerRow != row)
                throw new InvalidDataException($"{name} uploads a run of {run.Count} characters from {run.FirstCharacter} that is not within one row of the table.");
            for (int character = run.FirstCharacter; character < run.FirstCharacter + run.Count; character++)
                bandOf[character] = row - firstRow;
        }
    }

    /// <summary>The corpse, named for messages.</summary>
    internal string Name { get; }

    /// <summary>Whether <paramref name="character"/> is one the corpse's rows show in.</summary>
    internal bool Holds(int character) => bandOf.ContainsKey(character);

    /// <summary>The band of the corpse's rows <paramref name="character"/> holds.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The corpse's rows do not show in the character.</exception>
    internal int Band(int character) => bandOf.TryGetValue(character, out int band) ? band
        : throw new ArgumentOutOfRangeException(nameof(character), character, $"{Name}'s rows do not show in character {character}.");

    /// <summary>The corpse's column <paramref name="character"/> shows: its column of the table.</summary>
    internal static int Column(int character) => character % SpriteArtwork.CharactersPerRow;

    /// <summary>The band column <paramref name="column"/> of the corpse begins rotting at: the first it uploads the column in.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The corpse uploads no character of the column.</exception>
    internal int FirstBand(int column)
    {
        int[] bands = [.. bandOf.Where(entry => Column(entry.Key) == column).Select(entry => entry.Value)];
        return bands.Length > 0 ? bands.Min()
            : throw new ArgumentOutOfRangeException(nameof(column), column, $"{Name} uploads no character of column {column}.");
    }

    /// <summary>
    /// Requires every look of <paramref name="encounters"/> that copies over the characters the corpse's rows show in to
    /// belong to an encounter whose looks load the corpse's own characters: the game draws the corpse's frames through its
    /// rows over those characters, and only such an encounter's pictures say where its looks have covered them.
    /// </summary>
    /// <exception cref="InvalidDataException">An encounter whose looks never load the corpse's own characters copies over them.</exception>
    internal void RequireUnchangedBy(IReadOnlyList<EncounterLookSteps> encounters)
    {
        foreach (EncounterLookSteps encounter in Ensure.NotNull(encounters))
        {
            if (Enumerable.Range(0, encounter.Looks.Count).Any(look => loadsFlesh(encounter.Encounter, look)))
                continue;
            for (int look = 0; look < encounter.Looks.Count; look++)
            {
                if (Written(encounter.Looks[look]).Where(Holds).ToArray() is [int character, ..])
                    throw new InvalidDataException(
                        $"Look {look} of {encounter.Encounter} copies over character {character} of {Name}'s rows, which the game draws the corpse's frames through.");
            }
        }
    }

    /// <summary>
    /// Whether some look of <paramref name="encounter"/> up to <paramref name="look"/> has loaded the corpse's own
    /// characters, so that the corpse can rot once the encounter shows it.
    /// </summary>
    internal bool LoadedBy(EncounterLookSteps encounter, int look) =>
        Enumerable.Range(0, Ensure.NotNull(encounter).Looks.Count).Take(look + 1).Any(earlier => loadsFlesh(encounter.Encounter, earlier));

    /// <summary>
    /// The characters the corpse's rows show in that the looks of <paramref name="encounter"/> up to <paramref name="look"/>
    /// have copied other characters over since the corpse's own last loaded (#1531): a picture of that look shows those
    /// characters as the later look left them, not rotting.
    /// </summary>
    internal IReadOnlySet<int> CoveredBy(EncounterLookSteps encounter, int look)
    {
        Ensure.NotNull(encounter);
        var covered = new HashSet<int>();
        for (int earlier = 0; earlier <= look && earlier < encounter.Looks.Count; earlier++)
        {
            IEnumerable<int> written = Written(encounter.Looks[earlier]).Where(Holds);
            if (loadsFlesh(encounter.Encounter, earlier))
                covered.ExceptWith(written);
            else
                covered.UnionWith(written);
        }
        return covered;
    }

    /// <summary>Every character the copies of one look land over.</summary>
    private static IEnumerable<int> Written(IReadOnlyList<EncounterArtworkUpload> look) =>
        look.SelectMany(upload => Enumerable.Range(upload.FirstCharacter, upload.Characters.Length / SpriteArtwork.CharacterByteCount));
    /// <summary>
    /// Whether frame <paramref name="frame"/>, drawn by <paramref name="enemy"/> or its encounter, is one the game shows a
    /// rotting corpse in: any frame of a dead monster's or the dead Torizo's, and Mother Brain's dead brain.
    /// </summary>
    internal static bool Shows(EnemyDefinitionId enemy, EnemyFrameId frame) =>
        enemy is EnemyDefinitionId.CorpseSidehopper or EnemyDefinitionId.CorpseTorizo || DeadTourianBehavior.IsDeadTourianCorpseDefinition(enemy)
        || enemy == EnemyDefinitionId.MotherBrainHead && EncounterArtworkDrawingDefinitions.MotherBrainCorpseFrames.Contains(frame);

    /// <summary>Whether encounter frame <paramref name="frame"/> is one the game shows a rotting corpse in: the dead Torizo's hook.</summary>
    internal static bool ShowsInEncounter(EnemyFrameId frame) =>
        EncounterFrameDefinitions.ByEnemy.Any(encounter => encounter.Value.Contains(frame) && Shows(encounter.Key, frame));

    /// <summary>The rotting corpses among <paramref name="drawn"/>: each dead monster's variants, the dead Torizo's body and sand, and Mother Brain's corpse.</summary>
    internal static IReadOnlyList<RottingCorpseCells> Of(IEnumerable<EnemyDefinitionId> drawn)
    {
        var corpses = new List<RottingCorpseCells>();
        foreach (EnemyDefinitionId enemy in Ensure.NotNull(drawn).Distinct())
        {
            if (enemy == EnemyDefinitionId.MotherBrainHead)
                corpses.Add(MotherBrain);
            else if (enemy == EnemyDefinitionId.CorpseTorizo)
                corpses.AddRange([TorizoBody, TorizoSand]);
            else if (enemy == EnemyDefinitionId.CorpseSidehopper)
                corpses.AddRange([DeadMonster(DeadSidehopperCorpseDefinitions.InitiallyAlive.Kind), DeadMonster(DeadSidehopperCorpseDefinitions.InitiallyDead.Kind)]);
            else if (DeadTourianBehavior.IsDeadTourianCorpseDefinition(enemy))
            {
                DeadTourianBehavior.DeadTourianCorpseProfile profile = DeadTourianBehavior.ProfileForDeadTourianCorpse(enemy);
                for (int variant = 0; variant < DeadTourianCorpses.VariantCount(profile.Species); variant++)
                    corpses.Add(DeadMonster(DeadTourianCorpses.For(profile.Species, variant).Kind));
            }
        }
        return corpses;
    }

    /// <summary>A dead monster of <paramref name="kind"/>, which no encounter's look loads.</summary>
    private static RottingCorpseCells DeadMonster(RottingCorpseKind kind) =>
        new(kind.Name, DeadMonsterRowUploadDefinitions.ForKind(kind).Select(Run), LoadedByNoLook);

    /// <summary>The dead Torizo's body: the body rows of both its halves.</summary>
    private static RottingCorpseCells TorizoBody { get; } = new("Torizo",
        [.. Halves.SelectMany(half => half.Take(DeadTorizoRowUploadDefinitions.BodyRows)).Select(Run)], LoadedByNoLook);

    /// <summary>The dead Torizo's sand heap: the strip of each half, the odd half's on top.</summary>
    private static RottingCorpseCells TorizoSand { get; } = new("TorizoSand",
        [.. Halves.Select(half => half[DeadTorizoRowUploadDefinitions.BodyRows]).Select(Run)], LoadedByNoLook);

    /// <summary>The dead Torizo's two halves of uploads, even then odd.</summary>
    private static IEnumerable<DeadTorizoRowUploadDefinitions.PhaseRows> Halves =>
        new[] { false, true }.Select(DeadTorizoRowUploadDefinitions.ForHalf);

    /// <summary>Mother Brain's corpse, whose own characters her corpse's look loads.</summary>
    private static RottingCorpseCells MotherBrain { get; } = new("MotherBrain",
        [.. MotherBrainCorpseSheet.RotRows.Select(row => new CorpseCharacterRun(MotherBrainCorpseSheet.RotRowFirstCharacter(row), row.ColumnCount))],
        static (encounter, look) => encounter == ComposedEncounter.MotherBrain
            && EncounterLookUploads.Looks(encounter)[look] == EncounterLookUploads.MotherBrainCorpseLook);


    /// <summary>The characters <paramref name="upload"/> uploads a row into.</summary>
    private static CorpseCharacterRun Run(CorpseRowUpload upload) =>
        new(upload.FirstTableByte / SpriteArtwork.CharacterByteCount, upload.SizeInBytes / SpriteArtwork.CharacterByteCount);

    /// <summary>A run of characters in one row of the sprite character table.</summary>
    /// <param name="FirstCharacter">The first.</param>
    /// <param name="Count">How many.</param>
    private readonly record struct CorpseCharacterRun(int FirstCharacter, int Count);
}

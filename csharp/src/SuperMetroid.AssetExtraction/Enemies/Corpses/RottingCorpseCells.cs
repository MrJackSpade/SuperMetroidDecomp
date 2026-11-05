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
    /// <summary>No encounter's sequence loads the corpse's own characters; declared first, as the corpses below are built from it.</summary>
    private static readonly Func<EncounterArtworkSequence, int, bool> LoadedByNoSequence = static (_, _) => false;

    /// <summary>The band of the corpse's rows each of its characters holds, by character.</summary>
    private readonly Dictionary<int, int> bandOf;

    /// <summary>The sequence and its steps that load the corpse's own characters before it rots; none for a corpse no sequence loads.</summary>
    private readonly Func<EncounterArtworkSequence, int, bool> loadsFlesh;

    /// <summary>A corpse's characters, from runs of the table it uploads its rows into, each run within one row of the table.</summary>
    /// <exception cref="InvalidDataException">A run is empty or crosses a row of the table.</exception>
    private RottingCorpseCells(string name, IEnumerable<CorpseCharacterRun> runs, Func<EncounterArtworkSequence, int, bool> loadsFlesh)
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
    /// Requires every step of <paramref name="sequences"/> that copies over the characters the corpse's rows show in to
    /// belong to a sequence that loads the corpse's own characters: the game draws the corpse's frames through its rows
    /// over those characters, and only such a sequence's pictures say where its steps have covered them.
    /// </summary>
    /// <exception cref="InvalidDataException">A sequence that never loads the corpse's own characters copies over them.</exception>
    internal void RequireUnchangedBy(IReadOnlyList<EncounterArtworkSteps> sequences)
    {
        foreach (EncounterArtworkSteps sequence in Ensure.NotNull(sequences))
        {
            if (Enumerable.Range(0, sequence.Steps.Count).Any(step => loadsFlesh(sequence.Sequence, step)))
                continue;
            for (int step = 0; step < sequence.Steps.Count; step++)
            {
                if (Written(sequence.Steps[step]).Where(Holds).ToArray() is [int character, ..])
                    throw new InvalidDataException(
                        $"Step {step} of {sequence.Sequence} copies over character {character} of {Name}'s rows, which the game draws the corpse's frames through.");
            }
        }
    }

    /// <summary>
    /// Whether some step of <paramref name="sequence"/> up to <paramref name="step"/> has loaded the corpse's own
    /// characters, so that the corpse can rot once the sequence has reached it.
    /// </summary>
    internal bool LoadedBy(EncounterArtworkSteps sequence, int step) =>
        Enumerable.Range(0, Ensure.NotNull(sequence).Steps.Count).Take(step + 1).Any(earlier => loadsFlesh(sequence.Sequence, earlier));

    /// <summary>
    /// The characters the corpse's rows show in that the steps of <paramref name="sequence"/> up to <paramref name="step"/>
    /// have copied other characters over since the corpse's own last loaded (#1531): a picture of that step shows those
    /// characters as the later step left them, not rotting.
    /// </summary>
    internal IReadOnlySet<int> CoveredBy(EncounterArtworkSteps sequence, int step)
    {
        Ensure.NotNull(sequence);
        var covered = new HashSet<int>();
        for (int earlier = 0; earlier <= step && earlier < sequence.Steps.Count; earlier++)
        {
            IEnumerable<int> written = Written(sequence.Steps[earlier]).Where(Holds);
            if (loadsFlesh(sequence.Sequence, earlier))
                covered.ExceptWith(written);
            else
                covered.UnionWith(written);
        }
        return covered;
    }

    /// <summary>Every character the uploads of one step copy over.</summary>
    private static IEnumerable<int> Written(IReadOnlyList<EncounterArtworkUpload> step) =>
        step.SelectMany(upload => Enumerable.Range(upload.FirstCharacter, upload.Characters.Length / SpriteArtwork.CharacterByteCount));
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
                for (int variant = 0; variant < profile.VariantCount; variant++)
                    corpses.Add(DeadMonster(DeadTourianCorpses.For(profile.Species, variant).Kind));
            }
        }
        return corpses;
    }

    /// <summary>A dead monster of <paramref name="kind"/>, which no encounter's sequence loads.</summary>
    private static RottingCorpseCells DeadMonster(RottingCorpseKind kind) =>
        new(kind.Name, DeadMonsterRowUploadDefinitions.ForKind(kind).Select(Run), LoadedByNoSequence);

    /// <summary>The dead Torizo's body: the body rows of both its halves.</summary>
    private static RottingCorpseCells TorizoBody { get; } = new("Torizo",
        [.. Halves.SelectMany(half => half.Take(DeadTorizoRowUploadDefinitions.BodyRows)).Select(Run)], LoadedByNoSequence);

    /// <summary>The dead Torizo's sand heap: the strip of each half, the odd half's on top.</summary>
    private static RottingCorpseCells TorizoSand { get; } = new("TorizoSand",
        [.. Halves.Select(half => half[DeadTorizoRowUploadDefinitions.BodyRows]).Select(Run)], LoadedByNoSequence);

    /// <summary>The dead Torizo's two halves of uploads, even then odd.</summary>
    private static IEnumerable<DeadTorizoRowUploadDefinitions.PhaseRows> Halves =>
        new[] { false, true }.Select(DeadTorizoRowUploadDefinitions.ForHalf);

    /// <summary>Mother Brain's corpse, whose own characters her corpse's pages load.</summary>
    private static RottingCorpseCells MotherBrain { get; } = new("MotherBrain",
        [.. MotherBrainCorpseSheet.RotRows.Select(row => new CorpseCharacterRun(MotherBrainCorpseSheet.RotRowFirstCharacter(row), row.ColumnCount))],
        static (sequence, step) => sequence == EncounterArtworkSequence.MotherBrainPages
            && MotherBrainSpritePageDefinitions.Sequence[step].Sheet == MotherBrainSpritePageSheet.Corpse);


    /// <summary>The characters <paramref name="upload"/> uploads a row into.</summary>
    private static CorpseCharacterRun Run(CorpseRowUpload upload) =>
        new(upload.FirstTableByte / SpriteArtwork.CharacterByteCount, upload.SizeInBytes / SpriteArtwork.CharacterByteCount);

    /// <summary>A run of characters in one row of the sprite character table.</summary>
    /// <param name="FirstCharacter">The first.</param>
    /// <param name="Count">How many.</param>
    private readonly record struct CorpseCharacterRun(int FirstCharacter, int Count);
}

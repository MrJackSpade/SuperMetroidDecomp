using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// The sprite characters a rotting corpse's rows show in (#1531, #1539): the characters the game uploads the corpse's rows
/// into after its rot calls. The game uploads each of the corpse's rows of characters into one row of the sprite character
/// table, the corpse's first row topmost, so the band of eight of the corpse's rows a character holds is its row of the
/// table less the corpse's first. The extraction reads them only to find where the corpse lies in each of its pictures.
/// </summary>
internal sealed class RottingCorpseCells
{
    /// <summary>The band of the corpse's rows each of its characters holds, by character.</summary>
    private readonly Dictionary<int, int> bandOf;

    /// <summary>A corpse's characters, from runs of the table it uploads its rows into, each run within one row of the table.</summary>
    /// <exception cref="InvalidDataException">A run is empty or crosses a row of the table.</exception>
    private RottingCorpseCells(string name, IEnumerable<CorpseCharacterRun> runs)
    {
        Name = name;
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
            {
                corpses.AddRange([DeadMonster(new CorpseRowUploadList(RottingCorpseSpecies.Sidehopper, 0)),
                    DeadMonster(new CorpseRowUploadList(RottingCorpseSpecies.Sidehopper, 1))]);
            }
            else if (DeadTourianBehavior.IsDeadTourianCorpseDefinition(enemy))
            {
                DeadTourianBehavior.DeadTourianCorpseProfile profile = DeadTourianBehavior.ProfileForDeadTourianCorpse(enemy);
                for (int variant = 0; variant < DeadTourianCorpses.VariantCount(profile.Species); variant++)
                    corpses.Add(DeadMonster(DeadTourianCorpses.RowUploadList(profile.Species, variant)));
            }
        }
        return corpses;
    }

    /// <summary>The dead monster whose rows <paramref name="list"/> uploads.</summary>
    private static RottingCorpseCells DeadMonster(CorpseRowUploadList list) =>
        new(list.Name, DeadMonsterRowUploadDefinitions.ForList(list).Select(Run));

    /// <summary>The dead Torizo's body: the body rows of both its upload lists.</summary>
    private static RottingCorpseCells TorizoBody { get; } = new("Torizo",
        [.. Halves.SelectMany(half => half.Take(DeadTorizoRowUploadDefinitions.BodyRows)).Select(Run)]);

    /// <summary>The dead Torizo's sand heap: the strip of each upload list, the odd list's on top.</summary>
    private static RottingCorpseCells TorizoSand { get; } = new("TorizoSand",
        [.. Halves.Select(half => half[DeadTorizoRowUploadDefinitions.BodyRows]).Select(Run)]);

    /// <summary>The dead Torizo's two upload lists, even then odd.</summary>
    private static IEnumerable<DeadTorizoRowUploadDefinitions.PhaseRows> Halves =>
        new[] { false, true }.Select(DeadTorizoRowUploadDefinitions.ForHalf);

    /// <summary>Mother Brain's corpse.</summary>
    private static RottingCorpseCells MotherBrain { get; } = new("MotherBrain",
        [.. MotherBrainCorpseSheet.RotRows.Select(row => new CorpseCharacterRun(MotherBrainCorpseSheet.RotRowFirstCharacter(row), row.ColumnCount))]);

    /// <summary>The characters <paramref name="upload"/> uploads a row into.</summary>
    private static CorpseCharacterRun Run(CorpseRowUpload upload) =>
        new(upload.FirstTableByte / SpriteArtwork.CharacterByteCount, upload.SizeInBytes / SpriteArtwork.CharacterByteCount);

    /// <summary>A run of characters in one row of the sprite character table.</summary>
    /// <param name="FirstCharacter">The first.</param>
    /// <param name="Count">How many.</param>
    private readonly record struct CorpseCharacterRun(int FirstCharacter, int Count);
}

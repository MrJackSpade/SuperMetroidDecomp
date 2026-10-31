using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// A rotting corpse's staged characters, rotted as the cartridge rots them (#1392): the bytes of each row it uploads after
/// each rot step, from which its sheet pictures every rot its frames can show, and the rots its rows can show together. A
/// dead monster or the dead Torizo selects the rot its frames show by its kind (#1406); Mother Brain's corpse is pictured
/// for the step each of its rows was uploaded at (<see cref="CorpseRotSteps"/>). Nothing rots pixels at run time.
/// </summary>
internal sealed class CorpseRotSimulation
{
    /// <summary>Every row's bytes after each step, by row then step; a row the corpse never uploads at a step holds none.</summary>
    private readonly Dictionary<CorpseRotRow, byte[]?[]> rowBytes;

    /// <summary>Each state the corpse's uploads can leave its rows in: the step each row shows, or none for the set's characters.</summary>
    private readonly IReadOnlyList<IReadOnlyDictionary<CorpseRotRow, int?>> reachable;

    /// <summary>A corpse's simulated rot: its rows, each row's bytes after each step, and the states its uploads reach.</summary>
    private CorpseRotSimulation(string flesh, RottingCorpseKind? kind, IReadOnlyList<CorpseRotRow> rows,
        Dictionary<CorpseRotRow, byte[]?[]> rowBytes, IReadOnlyList<IReadOnlyDictionary<CorpseRotRow, int?>> reachable,
        IReadOnlyList<CorpseRotState> states)
    {
        Flesh = flesh;
        Kind = kind;
        Rows = rows;
        this.rowBytes = rowBytes;
        this.reachable = reachable;
        States = states;
    }

    /// <summary>The corpse whose characters rot, named for messages, checks and the manifest.</summary>
    internal string Flesh { get; }

    /// <summary>The kind of corpse, whose frames select the rot they show; none for Mother Brain's corpse.</summary>
    internal RottingCorpseKind? Kind { get; }

    /// <summary>The rows of characters the corpse uploads.</summary>
    internal IReadOnlyList<CorpseRotRow> Rows { get; }

    /// <summary>
    /// Each rot the frames of a corpse of a kind can show other than the graphics set's characters, once each, with the step
    /// each row shows in it; none for Mother Brain's corpse.
    /// </summary>
    internal IReadOnlyList<CorpseRotState> States { get; }

    /// <summary>The names of the pictures of sheet frame <paramref name="frame"/> drawn over a kind's rows: the frame itself, then each rot.</summary>
    internal IEnumerable<string> PictureNames(string frame) => States.Select(state => state.View.PictureName(frame)).Prepend(frame);

    /// <summary>The bytes <paramref name="row"/> uploads after rot step <paramref name="step"/>.</summary>
    /// <exception cref="InvalidDataException">The corpse never uploads the row after that step.</exception>
    internal ReadOnlySpan<byte> RowBytes(CorpseRotRow row, int step)
    {
        byte[]?[] steps = rowBytes.TryGetValue(Ensure.NotNull(row), out byte[]?[]? found) ? found
            : throw new InvalidDataException($"{Flesh} uploads no row at table byte {row.FirstByte}.");
        return (uint)step < (uint)steps.Length && steps[step] is { } bytes ? bytes
            : throw new InvalidDataException($"{Flesh} never uploads its row at table byte {row.FirstByte} after rot step {step}.");
    }

    /// <summary>
    /// What <paramref name="rows"/> can show together: for each state the corpse's uploads can leave them in, the step each
    /// shows, or none while it shows the graphics set's characters; each distinct state once.
    /// </summary>
    internal IEnumerable<int?[]> Shown(IReadOnlyList<CorpseRotRow> rows)
    {
        Ensure.NotNull(rows);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (IReadOnlyDictionary<CorpseRotRow, int?> state in reachable)
        {
            int?[] shown = [.. rows.Select(row => state[row])];
            if (seen.Add(string.Join(',', shown.Select(step => step?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "set"))))
                yield return shown;
        }
    }

    /// <summary>
    /// The step each of <paramref name="rows"/> shows once the corpse has finished rotting, when no further rot step uploads
    /// over them, so an encounter's later upload can replace any of them (#1435).
    /// </summary>
    internal int?[] Finished(IReadOnlyList<CorpseRotRow> rows) => [.. Ensure.NotNull(rows).Select(row => reachable[^1][row])];

    /// <summary>Whether <paramref name="enemy"/>'s frames show a corpse whose characters rot: a corpse's, or Mother Brain's.</summary>
    internal static bool Rots(EnemyDefinitionId enemy) => ShowsSelectedRot(enemy) || ShowsUploadedRot(enemy);

    /// <summary>Whether <paramref name="enemy"/>'s frames show the rot a corpse of a kind selects: a dead monster's or the dead Torizo's.</summary>
    internal static bool ShowsSelectedRot(EnemyDefinitionId enemy) =>
        enemy is EnemyDefinitionId.CorpseTorizo or EnemyDefinitionId.CorpseSidehopper || DeadTourianBehavior.IsDeadTourianCorpseDefinition(enemy);

    /// <summary>Whether <paramref name="enemy"/>'s frames show Mother Brain's corpse as its uploaded rows leave it.</summary>
    internal static bool ShowsUploadedRot(EnemyDefinitionId enemy) =>
        enemy is EnemyDefinitionId.MotherBrainHead or EnemyDefinitionId.MotherBrainBody or EnemyDefinitionId.MotherBrainTubes;

    /// <summary>
    /// The rotting corpses among <paramref name="drawn"/>, each rotted from installed <paramref name="enemies"/> or
    /// <paramref name="bosses"/> art.
    /// </summary>
    internal static IReadOnlyList<CorpseRotSimulation> Of(EnemyTileSourceArtwork enemies, BossArtworkCatalog bosses, IEnumerable<EnemyDefinitionId> drawn)
    {
        Ensure.NotNull(enemies);
        Ensure.NotNull(bosses);
        var corpses = new List<CorpseRotSimulation>();
        foreach (EnemyDefinitionId enemy in Ensure.NotNull(drawn).Distinct())
        {
            if (enemy == EnemyDefinitionId.MotherBrainHead)
                corpses.Add(MotherBrain(bosses));
            else if (enemy == EnemyDefinitionId.CorpseTorizo)
                corpses.Add(Torizo(enemies));
            else if (enemy == EnemyDefinitionId.CorpseSidehopper)
            {
                corpses.Add(Sidehopper(enemies, 0, DeadSidehopperCorpseDefinitions.InitiallyAlive));
                corpses.Add(Sidehopper(enemies, 2, DeadSidehopperCorpseDefinitions.InitiallyDead));
            }
            else if (DeadTourianBehavior.IsDeadTourianCorpseDefinition(enemy))
            {
                DeadTourianBehavior.DeadTourianCorpseProfile profile = DeadTourianBehavior.ProfileForDeadTourianCorpse(enemy);
                for (int variant = 0; variant < profile.VariantCount; variant++)
                    corpses.Add(Tourian(enemies, profile.Species, variant));
            }
        }
        return corpses;
    }

    /// <summary>The Sidehopper corpse of graphics variant <paramref name="graphicsVariant"/> (population parameter 0 or 2).</summary>
    private static CorpseRotSimulation Sidehopper(EnemyTileSourceArtwork enemies, int graphicsVariant, DeadSidehopperCorpseDefinition definition)
    {
        CorpseStaging staged = CorpseStaging.Sidehopper(
            Atlas(enemies, CorpseSheetDefinitions.DeadMonsterSheet, CorpseSheetDefinitions.DeadMonsterSheetByteCount), graphicsVariant);
        return Batch(definition.Kind, staged, CorpseRowGeometry.Sidehopper(graphicsVariant), definition.RowCount);
    }

    /// <summary>Variant <paramref name="variant"/> of a dead Zoomer, Ripper or Skree.</summary>
    private static CorpseRotSimulation Tourian(EnemyTileSourceArtwork enemies, DeadTourianCorpseSpecies species, int variant)
    {
        DeadTourianCorpseVariant layout = DeadTourianCorpseStagingDefinitions.Variants(species)[variant];
        DeadTourianCorpseDefinition definition = DeadTourianCorpses.For(species, variant);
        CorpseStaging staged = CorpseStaging.Tourian(
            Atlas(enemies, CorpseSheetDefinitions.DeadMonsterSheet, CorpseSheetDefinitions.DeadMonsterSheetByteCount), layout);
        return Batch(definition.Kind, staged, CorpseRowGeometry.Tourian(species, layout), definition.RowCount);
    }

    /// <summary>
    /// A corpse of <paramref name="kind"/>, <paramref name="rowCount"/> pixel rows tall, that uploads all its rows after each
    /// rot step it takes, and none before the first.
    /// </summary>
    private static CorpseRotSimulation Batch(RottingCorpseKind kind, CorpseStaging staged, CorpseRowGeometry geometry, int rowCount) =>
        Batch(kind.Name, kind, [.. DeadMonsterRowUploadDefinitions.ForKind(kind).Select((upload, index) => Row(index, upload))],
            new CorpseRotSchedule(rowCount), new StagedCorpse(staged, geometry), uploadsLastStep: true);

    /// <summary>
    /// Mother Brain's corpse, which uploads its six rows after each rot step but the last, whose finished table returns
    /// before its uploads are queued ($A9:B1DD).
    /// </summary>
    private static CorpseRotSimulation MotherBrain(BossArtworkCatalog bosses)
    {
        RoomCharacterAtlas atlas = bosses.MotherBrainCorpse
            ?? throw new InvalidDataException($"Installed boss art holds no Mother Brain corpse ({MotherBrainCorpseArtworkDefinitions.FileName}).");
        if (atlas.Characters.Length != MotherBrainCorpseArtworkDefinitions.ByteCount)
            throw new InvalidDataException($"Installed {MotherBrainCorpseArtworkDefinitions.FileName} holds {atlas.Characters.Length} bytes, not {MotherBrainCorpseArtworkDefinitions.ByteCount}.");
        return Batch("MotherBrain", null, [.. MotherBrainCorpseArtworkDefinitions.RotRows.Select(row =>
                new CorpseRotRow(row.Row, EnemySourceDefinitions.CorpseRottingHeap + row.RottingOffset, row.ByteCount,
                    row.FirstCharacter * SuperMetroid.Core.Assets.SpriteArtwork.CharacterByteCount))],
            new CorpseRotSchedule(MotherBrainCorpseArtworkDefinitions.PixelRowCount),
            new StagedCorpse(CorpseStaging.MotherBrain(atlas.Characters.Span), CorpseRowGeometry.MotherBrain), uploadsLastStep: false);
    }

    /// <summary>
    /// A corpse that uploads all its <paramref name="rows"/> after each rot step it takes (the last one too unless
    /// <paramref name="uploadsLastStep"/> is false), and none before the first.
    /// </summary>
    private static CorpseRotSimulation Batch(string flesh, RottingCorpseKind? kind, CorpseRotRow[] rows, CorpseRotSchedule schedule,
        StagedCorpse corpse, bool uploadsLastStep)
    {
        CorpseStaging staged = corpse.Copy;
        CorpseRowGeometry geometry = corpse.Geometry;
        var snapshots = new List<byte[]?[]> { new byte[]?[rows.Length] };
        bool rotting = true;
        while (rotting)
        {
            rotting = schedule.Step((y, operation) => staged.MoveRow(geometry, y, operation), _ => { });
            snapshots.Add(rotting || uploadsLastStep ? [.. rows.Select(staged.Upload)] : new byte[]?[rows.Length]);
        }
        int steps = uploadsLastStep ? snapshots.Count - 1 : snapshots.Count - 2;
        var reachable = new List<IReadOnlyDictionary<CorpseRotRow, int?>> { rows.ToDictionary(row => row, _ => (int?)null) };
        var states = new List<CorpseRotState>();
        for (int step = 1; step <= steps; step++)
        {
            int shown = step;
            IReadOnlyDictionary<CorpseRotRow, int?> state = rows.ToDictionary(row => row, _ => (int?)shown);
            reachable.Add(state);
            if (kind is RottingCorpseKind selected)
                states.Add(new CorpseRotState(new CorpseRotView(selected, [shown]), state));
        }
        return new CorpseRotSimulation(flesh, kind, rows, ByRow(rows, snapshots), reachable, states);
    }

    /// <summary>
    /// The dead Torizo, which uploads half its rows each frame from its first, alternating halves, while its sand falls into
    /// two strips every fifteenth frame of its rot.
    /// </summary>
    private static CorpseRotSimulation Torizo(EnemyTileSourceArtwork enemies)
    {
        ReadOnlySpan<byte> atlas = Atlas(enemies, CorpseSheetDefinitions.DeadTorizoSheet, CorpseSheetDefinitions.DeadTorizoSheetByteCount);
        CorpseStaging staged = CorpseStaging.Torizo(atlas);
        CorpseRowGeometry geometry = CorpseRowGeometry.Torizo;
        var halves = new Dictionary<bool, CorpseRotRow[]>();
        foreach (bool odd in new[] { false, true })
        {
            DeadTorizoRowUploadDefinitions.PhaseRows uploads = DeadTorizoRowUploadDefinitions.ForHalf(odd);
            halves[odd] = [.. uploads.Select((upload, index) => Row(TorizoRowIndex(odd, index, uploads.Count), upload))];
        }
        CorpseRotRow[] rows = [.. halves[false], .. halves[true]];
        var schedule = new CorpseRotSchedule(DeadTorizoCorpseDefinitions.RowCount);
        var sand = new DeadTorizoSandCadence();
        List<byte[]?[]> snapshots = [[.. rows.Select(staged.Upload)]];
        bool rotting = true;
        while (rotting)
        {
            if (sand.Advance() is int line)
                staged.CopySandLine(atlas, line);
            rotting = schedule.Step((y, operation) => staged.MoveRow(geometry, y, operation), _ => { });
            snapshots.Add([.. rows.Select(staged.Upload)]);
        }
        List<IReadOnlyDictionary<CorpseRotRow, int?>> reachable = TorizoReachable(halves, snapshots.Count - 1, out List<CorpseRotState> states);
        return new CorpseRotSimulation(DeadTorizoCorpseDefinitions.Kind.Name, DeadTorizoCorpseDefinitions.Kind, rows, ByRow(rows, snapshots),
            reachable, states);
    }

    /// <summary>
    /// The states the dead Torizo's halves can show: the odd half uploads first, one half each frame its AI runs, after rot
    /// step 0 until its rot starts, which a touch or a power bomb can do on any frame, then after one more rot step each frame
    /// until the last. Each also gives <paramref name="states"/> the rot it selects, once each, every one but the set's.
    /// </summary>
    private static List<IReadOnlyDictionary<CorpseRotRow, int?>> TorizoReachable(Dictionary<bool, CorpseRotRow[]> halves, int lastStep,
        out List<CorpseRotState> states)
    {
        var reachable = new List<IReadOnlyDictionary<CorpseRotRow, int?>>();
        states = [];
        var named = new HashSet<string>(StringComparer.Ordinal);
        var seen = new HashSet<TorizoUploadState>();
        var pending = new Queue<TorizoUploadState>();
        pending.Enqueue(new TorizoUploadState(null, null, NextOdd: true, Step: 0));
        while (pending.TryDequeue(out TorizoUploadState? state))
        {
            if (!seen.Add(state))
                continue;
            IReadOnlyDictionary<CorpseRotRow, int?> shown = halves[false].Select(row => new KeyValuePair<CorpseRotRow, int?>(row, state.Even))
                .Concat(halves[true].Select(row => new KeyValuePair<CorpseRotRow, int?>(row, state.Odd)))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            reachable.Add(shown);
            var view = new CorpseRotView(DeadTorizoCorpseDefinitions.Kind, [state.Even, state.Odd]);
            if ((state.Even is not null || state.Odd is not null) && named.Add(view.PictureName("frame")))
                states.Add(new CorpseRotState(view, shown));
            if (state.Step == 0)
                pending.Enqueue(state.Upload(0));
            if (state.Step < lastStep)
                pending.Enqueue(state.Upload(state.Step + 1));
            if (state.Step == lastStep)
                pending.Enqueue(state.Upload(lastStep));
        }
        return reachable;
    }

    /// <summary>Which of the dead Torizo's rows each step shows, and which half uploads next.</summary>
    private sealed record TorizoUploadState(int? Even, int? Odd, bool NextOdd, int Step)
    {
        /// <summary>The state after a frame that uploads the next half after rot step <paramref name="step"/>.</summary>
        internal TorizoUploadState Upload(int step) => NextOdd
            ? this with { Odd = step, NextOdd = false, Step = step }
            : this with { Even = step, NextOdd = true, Step = step };
    }

    /// <summary>
    /// The row a Dead Torizo upload lands: a half's body rows, after the even half's six, then its sand strip, the odd
    /// half's after the even's.
    /// </summary>
    private static int TorizoRowIndex(bool odd, int upload, int uploads) =>
        upload < uploads - 1 ? upload + (odd ? uploads - 1 : 0) : 2 * (uploads - 1) + (odd ? 1 : 0);

    /// <summary>The installed tiles a corpse copies its staged characters from.</summary>
    private static ReadOnlySpan<byte> Atlas(EnemyTileSourceArtwork enemies, EnemyDefinitionId sheet, int byteCount)
    {
        ReadOnlyMemory<byte> tiles = enemies.TilesOf(sheet);
        return tiles.Length == byteCount ? tiles.Span
            : throw new InvalidDataException($"Installed {sheet} tiles are {tiles.Length} bytes, not the corpse sheet's {byteCount}.");
    }

    /// <summary>The row <paramref name="upload"/> uploads, as the <paramref name="index"/>th of its corpse's rows.</summary>
    private static CorpseRotRow Row(int index, CorpseRowUpload upload) =>
        new(index, upload.SourceAddress, upload.SizeInBytes, upload.FirstTableByte);

    /// <summary>The snapshots, by row.</summary>
    private static Dictionary<CorpseRotRow, byte[]?[]> ByRow(CorpseRotRow[] rows, List<byte[]?[]> snapshots) =>
        rows.Select((row, index) => new KeyValuePair<CorpseRotRow, byte[]?[]>(row, [.. snapshots.Select(snapshot => snapshot[index])]))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

    /// <summary>The staged copy a corpse rots and how its rows move within it.</summary>
    /// <param name="Copy">The staged copy.</param>
    /// <param name="Geometry">How its rows move.</param>
    private sealed record StagedCorpse(CorpseStaging Copy, CorpseRowGeometry Geometry);
}

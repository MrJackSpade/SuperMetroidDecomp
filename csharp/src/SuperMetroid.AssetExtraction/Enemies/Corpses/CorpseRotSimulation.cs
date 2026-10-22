using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// A rotting corpse's staged characters, rotted as the cartridge rots them (#1392): the bytes of each row it uploads after
/// each rot step, from which its sheet pictures every step its frames can show, and the steps its rows can show together.
/// The room keeps only the step each row showed last (<see cref="CorpseRotUploads"/>), so nothing rots pixels at run time.
/// </summary>
internal sealed class CorpseRotSimulation
{
    /// <summary>Every row's bytes after each step, by row then step; a row the corpse never uploads at a step holds none.</summary>
    private readonly Dictionary<CorpseRotRow, byte[]?[]> rowBytes;

    private readonly IReadOnlyList<IReadOnlyDictionary<CorpseRotRow, int?>> reachable;

    private CorpseRotSimulation(string flesh, IReadOnlyList<CorpseRotRow> rows, Dictionary<CorpseRotRow, byte[]?[]> rowBytes,
        IReadOnlyList<IReadOnlyDictionary<CorpseRotRow, int?>> reachable)
    {
        Flesh = flesh;
        Rows = rows;
        this.rowBytes = rowBytes;
        this.reachable = reachable;
    }

    /// <summary>The corpse whose characters rot, named for messages and checks.</summary>
    internal string Flesh { get; }

    /// <summary>The rows of characters the corpse uploads.</summary>
    internal IReadOnlyList<CorpseRotRow> Rows { get; }

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

    /// <summary>Whether <paramref name="enemy"/>'s frames show a corpse whose characters rot: a corpse's, or Mother Brain's.</summary>
    internal static bool Rots(EnemyDefinitionId enemy) =>
        enemy is EnemyDefinitionId.CorpseTorizo or EnemyDefinitionId.CorpseSidehopper or EnemyDefinitionId.MotherBrainHead
            or EnemyDefinitionId.MotherBrainBody or EnemyDefinitionId.MotherBrainTubes
        || DeadTourianBehavior.IsDeadTourianCorpseDefinition(enemy);

    /// <summary>
    /// The rotting corpses among <paramref name="drawn"/>, each rotted from installed <paramref name="enemies"/> or
    /// <paramref name="bosses"/> art.
    /// </summary>
    internal static IReadOnlyList<CorpseRotSimulation> Of(EnemyTileArtworkCatalog enemies, BossArtworkCatalog bosses, IEnumerable<EnemyDefinitionId> drawn)
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
                for (int variant = 0; variant < profile.Variants.Length; variant++)
                    corpses.Add(Tourian(enemies, enemy, profile, variant));
            }
        }
        return corpses;
    }

    /// <summary>The Sidehopper corpse of graphics variant <paramref name="graphicsVariant"/> (population parameter 0 or 2).</summary>
    private static CorpseRotSimulation Sidehopper(EnemyTileArtworkCatalog enemies, ushort graphicsVariant, DeadSidehopperCorpseDefinition definition)
    {
        CorpseStaging staged = CorpseStaging.Sidehopper(
            Atlas(enemies, DeadTourianCorpseArtworkDefinitions.SourceAddress, DeadTourianCorpseArtworkDefinitions.ByteCount), graphicsVariant);
        CorpseRowGeometry geometry = CorpseRowGeometry.Sidehopper(graphicsVariant, definition.WrapOffset);
        int populated = Populated(EnemyDefinitionId.CorpseSidehopper, graphicsVariant);
        return Batch(definition.VramTransfers, staged, geometry, definition.RotTableOffset, definition.EntryCount, populated);
    }

    /// <summary>Variant <paramref name="variant"/> of a dead Zoomer, Ripper or Skree.</summary>
    private static CorpseRotSimulation Tourian(EnemyTileArtworkCatalog enemies, EnemyDefinitionId enemy,
        DeadTourianBehavior.DeadTourianCorpseProfile profile, int variant)
    {
        DeadTourianBehavior.DeadTourianCorpseVariant layout = profile.Variants[variant];
        DeadTourianCorpseDefinition definition = DeadTourianCorpseDefinitions.For(profile.Species, variant);
        CorpseStaging staged = CorpseStaging.Tourian(
            Atlas(enemies, DeadTourianCorpseArtworkDefinitions.SourceAddress, DeadTourianCorpseArtworkDefinitions.ByteCount), layout);
        CorpseRowGeometry geometry = CorpseRowGeometry.Tourian(profile, layout, definition.WrapOffset);
        int populated = Populated(enemy, checked((ushort)(variant * 2)));
        return Batch(definition.VramTransfers, staged, geometry, definition.RotTableOffset, definition.EntryCount, populated);
    }

    /// <summary>
    /// A corpse that uploads all its rows after each rot step it takes, and none before the first: each population record
    /// of it rots the one staged copy they share, so once the first to rot has finished, each further one steps it once more.
    /// </summary>
    private static CorpseRotSimulation Batch(CorpseTransferTable flesh, CorpseStaging staged, CorpseRowGeometry geometry,
        int rotTableOffset, int entryCount, int populated) =>
        Batch(flesh.ToString(), [.. DeadMonsterRottingDefinitions.ForTransferTable(flesh).Select((transfer, index) =>
                Row(index, transfer.SourceAddress, transfer.SizeInBytes, transfer.Destination))],
            new EnemyStagingSurfaces().RotTable(rotTableOffset, entryCount), staged, geometry, populated, uploadsLastStep: true);

    /// <summary>
    /// Mother Brain's corpse, which uploads its six rows after each rot step but the last, whose finished table returns
    /// before its uploads are queued ($A9:B1DD).
    /// </summary>
    private static CorpseRotSimulation MotherBrain(BossArtworkCatalog bosses)
    {
        RoomCharacterAtlas atlas = bosses.MotherBrainCorpse
            ?? throw new InvalidDataException($"Installed boss art holds no Mother Brain corpse ({MotherBrainCorpseArtworkDefinitions.FileName}).");
        if (atlas.Transfer.Length != MotherBrainCorpseArtworkDefinitions.ByteCount)
            throw new InvalidDataException($"Installed {MotherBrainCorpseArtworkDefinitions.FileName} holds {atlas.Transfer.Length} bytes, not {MotherBrainCorpseArtworkDefinitions.ByteCount}.");
        return Batch("MotherBrain", [.. MotherBrainCorpseArtworkDefinitions.RotTransfers.Select((transfer, index) =>
                Row(index, checked((int)transfer.SourceAddress), transfer.Size, VramDmaDestination.FromNative(checked((ushort)transfer.VramDestination))))],
            new EnemyStagingSurfaces().MotherBrainRotTable, CorpseStaging.MotherBrain(atlas.Transfer.Span), CorpseRowGeometry.MotherBrain,
            populated: 1, uploadsLastStep: false);
    }

    /// <summary>
    /// A corpse that uploads all its <paramref name="rows"/> after each rot step it takes (the last one too unless
    /// <paramref name="uploadsLastStep"/> is false), and none before the first.
    /// </summary>
    private static CorpseRotSimulation Batch(string flesh, CorpseRotRow[] rows, CorpseRotTable table, CorpseStaging staged, CorpseRowGeometry geometry,
        int populated, bool uploadsLastStep)
    {
        CorpseRottingTableProcessor.Initialize(table);
        int yLimit = table.EntryCount - 1;
        var snapshots = new List<byte[]?[]> { new byte[]?[rows.Length] };
        bool rotting = true;
        int further = 0;
        while (rotting || further < populated - 1)
        {
            if (!rotting)
                further++;
            bool stillRotting = CorpseRottingTableProcessor.Step(table, yLimit, yLimit - 1,
                (y, operation) => staged.MoveRow(geometry, y, operation), _ => { });
            rotting &= stillRotting;
            snapshots.Add(stillRotting || uploadsLastStep ? [.. rows.Select(staged.Upload)] : new byte[]?[rows.Length]);
        }
        int steps = uploadsLastStep ? snapshots.Count - 1 : snapshots.Count - 2;
        var reachable = new List<IReadOnlyDictionary<CorpseRotRow, int?>> { rows.ToDictionary(row => row, _ => (int?)null) };
        for (int step = 1; step <= steps; step++)
        {
            int shown = step;
            reachable.Add(rows.ToDictionary(row => row, _ => (int?)shown));
        }
        return new CorpseRotSimulation(flesh, rows, ByRow(rows, snapshots), reachable);
    }

    /// <summary>
    /// The dead Torizo, which uploads half its rows each frame from its first, alternating halves, while its sand falls into
    /// two strips every fifteenth frame of its rot.
    /// </summary>
    private static CorpseRotSimulation Torizo(EnemyTileArtworkCatalog enemies)
    {
        ReadOnlySpan<byte> atlas = Atlas(enemies, DeadTorizoArtworkDefinitions.SourceAddress, DeadTorizoArtworkDefinitions.ByteCount);
        CorpseStaging staged = CorpseStaging.Torizo(atlas);
        CorpseRowGeometry geometry = CorpseRowGeometry.Torizo;
        var halves = new Dictionary<bool, CorpseRotRow[]>();
        foreach (bool odd in new[] { false, true })
        {
            DeadTorizoVramTransferDefinitions.PhaseRows transfers = DeadTorizoVramTransferDefinitions.ForHalf(odd);
            halves[odd] = [.. transfers.Select((transfer, index) => Row(TorizoRowIndex(odd, index, transfers.Count), transfer.SourceAddress,
                transfer.SizeInBytes, transfer.Destination))];
        }
        CorpseRotRow[] rows = [.. halves[false], .. halves[true]];
        DeadTorizoCorpseDefinition definition = DeadTorizoCorpseDefinitions.Corpse;
        CorpseRotTable table = new EnemyStagingSurfaces().RotTable(definition.RotTableOffset, definition.EntryCount);
        CorpseRottingTableProcessor.Initialize(table);
        int yLimit = definition.EntryCount - 1;
        var sand = new DeadTorizoSandCadence();
        List<byte[]?[]> snapshots = [[.. rows.Select(staged.Upload)]];
        bool rotting = true;
        while (rotting)
        {
            if (sand.Advance() is int line)
                staged.CopySandLine(atlas, line);
            rotting = CorpseRottingTableProcessor.Step(table, yLimit, yLimit - 1,
                (y, operation) => staged.MoveRow(geometry, y, operation), _ => { });
            snapshots.Add([.. rows.Select(staged.Upload)]);
        }
        return new CorpseRotSimulation(new CorpseTransferTable(CorpseRotationTable.Torizo, 0).ToString(), rows, ByRow(rows, snapshots),
            TorizoReachable(halves, snapshots.Count - 1));
    }

    /// <summary>
    /// The states the dead Torizo's halves can show: the odd half uploads first, one half each frame its AI runs, after rot
    /// step 0 until its rot starts, which a touch or a power bomb can do on any frame, then after one more rot step each frame
    /// until the last.
    /// </summary>
    private static List<IReadOnlyDictionary<CorpseRotRow, int?>> TorizoReachable(Dictionary<bool, CorpseRotRow[]> halves, int lastStep)
    {
        var states = new List<IReadOnlyDictionary<CorpseRotRow, int?>>();
        var seen = new HashSet<TorizoUploadState>();
        var pending = new Queue<TorizoUploadState>();
        pending.Enqueue(new TorizoUploadState(null, null, NextOdd: true, Step: 0));
        while (pending.TryDequeue(out TorizoUploadState? state))
        {
            if (!seen.Add(state))
                continue;
            states.Add(halves[false].Select(row => new KeyValuePair<CorpseRotRow, int?>(row, state.Even))
                .Concat(halves[true].Select(row => new KeyValuePair<CorpseRotRow, int?>(row, state.Odd)))
                .ToDictionary(pair => pair.Key, pair => pair.Value));
            if (state.Step == 0)
                pending.Enqueue(state.Upload(0));
            if (state.Step < lastStep)
                pending.Enqueue(state.Upload(state.Step + 1));
            if (state.Step == lastStep)
                pending.Enqueue(state.Upload(lastStep));
        }
        return states;
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
    /// The row a Dead Torizo transfer uploads: a half's body rows, after the even half's six, then its sand strip, the odd
    /// half's after the even's.
    /// </summary>
    private static int TorizoRowIndex(bool odd, int transfer, int transfers) =>
        transfer < transfers - 1 ? transfer + (odd ? transfers - 1 : 0) : 2 * (transfers - 1) + (odd ? 1 : 0);

    /// <summary>How many population records of <paramref name="enemy"/> with first parameter <paramref name="parameter"/> any room state holds at most.</summary>
    private static int Populated(EnemyDefinitionId enemy, ushort parameter) =>
        Math.Max(1, RoomStateDefinitions.All.Max(state => state.EnemyPopulation.Records.ToArray()
            .Count(record => record.DefinitionId == enemy && record.Parameter1 == parameter)));

    /// <summary>The installed tiles a corpse copies its staged characters from.</summary>
    private static ReadOnlySpan<byte> Atlas(EnemyTileArtworkCatalog enemies, int sourceAddress, int byteCount) =>
        enemies.TryResolve(sourceAddress, byteCount, out ReadOnlyMemory<byte> tiles) ? tiles.Span
            : throw new InvalidDataException($"Installed enemy tiles hold no corpse art at ${sourceAddress:X6}+{byteCount}.");

    /// <summary>A row a transfer uploads.</summary>
    private static CorpseRotRow Row(int index, int sourceAddress, int byteCount, VramDmaDestination destination) =>
        new(index, sourceAddress, byteCount, ObjCharacterUploads.TableByte(destination));

    /// <summary>The snapshots, by row.</summary>
    private static Dictionary<CorpseRotRow, byte[]?[]> ByRow(CorpseRotRow[] rows, List<byte[]?[]> snapshots) =>
        rows.Select((row, index) => new KeyValuePair<CorpseRotRow, byte[]?[]>(row, [.. snapshots.Select(snapshot => snapshot[index])]))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
}

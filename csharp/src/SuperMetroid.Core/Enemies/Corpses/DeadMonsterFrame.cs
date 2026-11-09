using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// State shared by the dead-monster family (dead sidehoppers and Tourian corpses): the rows each kind of corpse shows as
/// its corpses' rot calls have left them (#1531), which the kind's frames are drawn through and which every corpse of the
/// kind rots together, and the dust sound the family requests.
/// </summary>
[RoomLifetime]
internal sealed class DeadMonsterFrame
{
    /// <summary>Receives the family's end-of-update dust sound.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Builds the family's shared state on the room's enemy audio queue.</summary>
    internal DeadMonsterFrame(EnemyAudioQueue audio) => _audio = audio;

    /// <summary>The rows each kind of corpse shows as its corpses' rot calls have left them (#1531).</summary>
    private readonly Dictionary<RottingCorpseKind, RottingCorpseRows> rottingRows = [];

    /// <summary>
    /// Starts the rows of <paramref name="kind"/>, <paramref name="rowCount"/> pixel rows tall, over, as a corpse of that
    /// kind copies its characters in afresh; every corpse of the kind rots the one copy.
    /// </summary>
    internal void BeginRows(RottingCorpseKind kind, int rowCount) => rottingRows[kind] = new RottingCorpseRows(rowCount);

    /// <summary>Moves pixel row <paramref name="row"/> of <paramref name="kind"/>'s rows as one of its corpses' rot calls does.</summary>
    /// <exception cref="InvalidOperationException">No corpse of the kind has copied its characters in.</exception>
    internal void MoveRow(RottingCorpseKind kind, int row, CorpseRowOperation operation) => RowsOf(kind).MoveRow(row, operation);

    /// <summary>
    /// The rows every corpse of <paramref name="kind"/> shows now, which its frames are drawn through: the game uploads the
    /// kind's rows after each rot call, the last too.
    /// </summary>
    /// <exception cref="InvalidOperationException">No corpse of the kind has copied its characters in.</exception>
    internal CorpseRotRows ShownRows(RottingCorpseKind kind) => RowsOf(kind).Shown;

    /// <summary>The rows of <paramref name="kind"/>, which a corpse of the kind has begun.</summary>
    /// <exception cref="InvalidOperationException">No corpse of the kind has copied its characters in.</exception>
    private RottingCorpseRows RowsOf(RottingCorpseKind kind) => rottingRows.TryGetValue(kind, out RottingCorpseRows? rows) ? rows
        : throw new InvalidOperationException($"{kind.Name}'s rows are shown before a corpse copies its characters in.");

    /// <summary>
    /// Spawns the dust cloud a finished corpse row raises, spread by <paramref name="random"/> across the corpse, and
    /// queues the dust sound on every eighth enemy frame.
    /// </summary>
    internal void SpawnCorpseRowDust(RoomEnemySlot slot, int random, EnemyProjectileEngine projectiles, int randomEnemyCounter)
    {
        projectiles.SpawnRoomGraphicsDustExplosion(
            new(CoordinateAxis.Wrap(slot.Position.X.Whole + CorpseDustSpread(random) - 14), CoordinateAxis.Wrap(slot.Position.Y.Whole + 16)),
            MiscDustAnimation.CorpseDustCloud);
        if ((randomEnemyCounter & 7) == 0)
            _audio.RequestSoundAtUpdateEnd(EnemySoundSource.DeadMonsters, EnemySound.LiquidDamage, priority: SoundRequestPriority.Low);
    }

    /// <summary>How far right of its leftmost place <paramref name="random"/> spreads a corpse row's dust cloud.</summary>
    private static int CorpseDustSpread(int random) =>
        (CorpseDustDefinitions.SpreadFineSteps.Of(random) * CorpseDustDefinitions.SpreadFineStep) +
        (CorpseDustDefinitions.SpreadCoarseSteps.Of(random) * CorpseDustDefinitions.SpreadCoarseStep);
}

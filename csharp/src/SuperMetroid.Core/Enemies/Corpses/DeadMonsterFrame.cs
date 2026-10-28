using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// State shared by the dead-monster family (dead sidehoppers and Tourian corpses): the rot steps each kind of corpse has
/// taken, which corpses of one kind take together, the rot each kind's frames show, and the dust sound the family
/// publishes each frame.
/// </summary>
[RoomLifetime]
internal sealed class DeadMonsterFrame
{
    /// <summary>The rot steps each kind of corpse's staged characters have taken since a corpse of that kind copied them in.</summary>
    private readonly Dictionary<RottingCorpseKind, int> rotSteps = [];

    /// <summary>The rot step each kind of corpse's frames show, from the first step it takes.</summary>
    private readonly Dictionary<RottingCorpseKind, int> shownSteps = [];

    /// <summary>The dust sound requested by a completed sidehopper or corpse row this frame, or null.</summary>
    public SoundEffectId? LastSoundEffect { get; internal set; }

    /// <summary>
    /// Starts the rot of <paramref name="kind"/>'s staged characters over, as a corpse of that kind copies them in afresh;
    /// every corpse of the kind rots the one copy.
    /// </summary>
    internal void BeginRot(RottingCorpseKind kind) => rotSteps[kind] = 0;

    /// <summary>
    /// Counts one rot step of <paramref name="kind"/>'s staged characters and selects it as the rot every corpse of the
    /// kind shows (#1406). The cartridge uploads the kind's rows after each step it takes, the last too, so its frames
    /// show the step from this update on.
    /// </summary>
    /// <exception cref="InvalidOperationException">No corpse of the kind has copied its characters in.</exception>
    internal void ShowRotStep(RottingCorpseKind kind)
    {
        if (!rotSteps.TryGetValue(kind, out int taken))
            throw new InvalidOperationException($"{kind.Name} rots before a corpse copies its characters in.");
        int step = taken + 1;
        rotSteps[kind] = step;
        shownSteps[kind] = step;
    }

    /// <summary>The rot every corpse of <paramref name="kind"/> shows: the step last selected, or the set's characters before the first.</summary>
    internal CorpseRotView Shown(RottingCorpseKind kind) =>
        new(kind, [shownSteps.TryGetValue(kind, out int step) ? step : null]);

    /// <summary>
    /// Spawns the dust cloud a finished corpse row raises, spread by <paramref name="random"/> across the corpse, and
    /// queues the dust sound on every eighth enemy frame.
    /// </summary>
    internal void SpawnCorpseRowDust(RoomEnemySlot slot, int random, EnemyProjectileEngine projectiles, int randomEnemyCounter)
    {
        projectiles.SpawnRoomGraphicsDustExplosion(
            new(unchecked((ushort)(slot.Position.X.Whole + CorpseDustSpread(random) - 14)), unchecked((ushort)(slot.Position.Y.Whole + 16))),
            MiscDustAnimation.CorpseDustCloud);
        if ((randomEnemyCounter & 7) == 0)
            LastSoundEffect = Library2Sound.LiquidDamage;
    }

    /// <summary>How far right of its leftmost place <paramref name="random"/> spreads a corpse row's dust cloud.</summary>
    private static int CorpseDustSpread(int random) =>
        (CorpseDustDefinitions.SpreadFineSteps.Of(random) * CorpseDustDefinitions.SpreadFineStep) +
        (CorpseDustDefinitions.SpreadCoarseSteps.Of(random) * CorpseDustDefinitions.SpreadCoarseStep);

    /// <summary>Drops this frame's sound request.</summary>
    internal void Clear() => LastSoundEffect = null;

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastSoundEffect is SoundEffectId sound)
            audio.QueueEnemySound(sound, maximumQueued: SoundQueueLimit.Max3);
    }
}

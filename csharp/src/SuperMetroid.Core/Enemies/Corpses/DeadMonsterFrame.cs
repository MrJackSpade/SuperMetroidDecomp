using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// State shared by the dead-monster family (dead sidehoppers and Tourian corpses): the rot steps each corpse's staged
/// characters have taken, which corpses of one kind take together, and the dust sound the family publishes each frame.
/// </summary>
[RoomLifetime]
internal sealed class DeadMonsterFrame
{
    /// <summary>Records the rows the corpses upload.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>The rot steps each kind of corpse's staged characters have taken since a corpse of that kind copied them in.</summary>
    private readonly Dictionary<CorpseTransferTable, int> rotSteps = [];

    /// <summary>Connects the dead monsters' rot uploads to the room's presentation.</summary>
    internal DeadMonsterFrame(EnemyPresentation presentation)
    {
        _presentation = presentation;
    }

    /// <summary>The dust sound requested by a completed sidehopper or corpse row this frame, or null.</summary>
    public SoundEffectId? LastSoundEffect { get; internal set; }

    /// <summary>
    /// Starts the rot of <paramref name="flesh"/>'s staged characters over, as a corpse of that kind copies them in afresh;
    /// every corpse of the kind rots the one copy.
    /// </summary>
    internal void BeginRot(CorpseTransferTable flesh) => rotSteps[flesh] = 0;

    /// <summary>
    /// Counts one rot step of <paramref name="flesh"/>'s staged characters and uploads its rows (#1392): the corpse's sheet
    /// pictures each step, so the room records only the step its rows show.
    /// </summary>
    /// <exception cref="InvalidOperationException">No corpse of the kind has copied its characters in.</exception>
    internal void ShowRotStep(CorpseTransferTable flesh)
    {
        if (!rotSteps.TryGetValue(flesh, out int taken))
            throw new InvalidOperationException($"{flesh} rots before a corpse copies its characters in.");
        int step = taken + 1;
        rotSteps[flesh] = step;
        foreach (DeadMonsterVramTransferDefinition transfer in DeadMonsterRottingDefinitions.ForTransferTable(flesh))
            _presentation.ShowRot(transfer.Destination, transfer.SizeInBytes, step);
    }

    /// <summary>
    /// Spawns the dust cloud a finished corpse row raises, spread by <paramref name="random"/> across the corpse, and
    /// queues the dust sound on every eighth enemy frame.
    /// </summary>
    internal void SpawnCorpseRowDust(RoomEnemySlot slot, int random, EnemyProjectileEngine projectiles, int randomEnemyCounter)
    {
        projectiles.SpawnRoomGraphicsDustExplosion(
            new(unchecked((ushort)(slot.Position.X.Whole + (random & CorpseDustDefinitions.SpreadBits) - 14)), unchecked((ushort)(slot.Position.Y.Whole + 16))),
            MiscDustAnimation.CorpseDustCloud);
        if ((randomEnemyCounter & 7) == 0)
            LastSoundEffect = SoundEffectLibrary2Sounds.DeadMonsterDust;
    }

    /// <summary>Drops this frame's sound request.</summary>
    internal void Clear() => LastSoundEffect = null;

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastSoundEffect is SoundEffectId sound)
            audio.QueueEnemySound(sound, maximumQueued: 3);
    }
}

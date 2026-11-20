namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// How a special Mother Brain Rinka shares the room's spawn points (#2256): the point it appears and respawns at, and the
/// point it holds against the others.
/// </summary>
public sealed class RinkaSharedSpawn
{
    /// <summary>Starts sharing at <paramref name="start"/>, holding no point yet.</summary>
    internal RinkaSharedSpawn(RinkaSpawnPoint start) => SpawnPoint = start;

    /// <summary>
    /// The spawn point the Rinka appears and respawns at: the one its spawn settings start it at, then whichever it last
    /// reserved, kept while every point is held.
    /// </summary>
    public RinkaSpawnPoint SpawnPoint { get; internal set; }

    /// <summary>The spawn point the Rinka holds, which no other Rinka can take until it is released; null when it holds none.</summary>
    public RinkaSpawnPoint? HeldSpawnPoint { get; internal set; }
}

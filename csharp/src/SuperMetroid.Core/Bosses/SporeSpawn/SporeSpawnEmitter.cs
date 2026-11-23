namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// One of the ceiling spore emitters in Spore Spawn's room (#2334): where it hangs and the way the spores it releases
/// first drift. A spore drifting right follows its drift path as authored; one drifting left follows it mirrored.
/// </summary>
/// <param name="Place">Where the emitter hangs, in room pixels.</param>
/// <param name="Drift">The way the emitter's spores first drift.</param>
internal readonly partial record struct SporeSpawnEmitter(Position16<RoomPixels> Place, HorizontalDirection Drift);

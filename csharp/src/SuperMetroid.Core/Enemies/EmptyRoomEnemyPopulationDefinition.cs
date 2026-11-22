namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A room state's enemy population that places no enemies. It has no death quota: loading it leaves the enemy count at
/// zero and keeps the quota of the last room that placed enemies, since the room load stops at the list's terminator
/// before anything after it is read (#2584).
/// </summary>
/// <param name="name">The list's name.</param>
public sealed partial class EmptyRoomEnemyPopulationDefinition([InstalledKey] string name) : RoomEnemyPopulationDefinition(name, []);

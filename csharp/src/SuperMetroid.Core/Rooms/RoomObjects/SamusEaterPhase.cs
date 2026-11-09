namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Where a Brinstar plant is in its course (<see cref="SamusEaterObject"/>).</summary>
internal enum SamusEaterPhase
{
    /// <summary>Spawned: its first update bites, starting the hold and the first chew.</summary>
    Biting,
    /// <summary>Chewing the held Samus.</summary>
    Chewing,
    /// <summary>Samus released: the closed mouth shows.</summary>
    Released,
    /// <summary>The idle plant shows for its last update.</summary>
    Resting,
}

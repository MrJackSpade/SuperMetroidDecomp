namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Where Maridia's glass tube is in its course (<see cref="NoobTubeObject"/>).</summary>
internal enum NoobTubePhase
{
    /// <summary>Spawned: its first update finds the tube already broken or shows it intact.</summary>
    Starting,
    /// <summary>Intact, waiting for a Power Bomb.</summary>
    WaitingForPowerBomb,
    /// <summary>A Power Bomb struck it: waiting for the player's input, which breaks it.</summary>
    WaitingForInput,
    /// <summary>The player's input woke it: the update locks Samus, cracks the glass and shows the first cracking image.</summary>
    Breaking,
    /// <summary>Cracking, image by image.</summary>
    Cracking,
    /// <summary>Shattered and open; the water returns once this image's time is up.</summary>
    Shattered,
}

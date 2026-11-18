namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One run of Maridia's glass tube's images (<see cref="NoobTubeAnimations"/>, #2480).</summary>
internal enum NoobTubeSegment
{
    /// <summary>The intact tube, shown for one update before it rests waiting for a Power Bomb.</summary>
    Intact,

    /// <summary>The tube cracking, image by image, before it shatters.</summary>
    Cracking,

    /// <summary>The shattered, open tube, before the room's water returns.</summary>
    Shattered,
}

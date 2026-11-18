namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One run of a Brinstar plant's images (<see cref="SamusEaterAnimations"/>, #2480).</summary>
internal enum SamusEaterSegment
{
    /// <summary>One chew of the held Samus, its images in order, some biting first.</summary>
    Chew,

    /// <summary>The plant's fangs out as it releases Samus.</summary>
    Released,

    /// <summary>The idle plant for its last update.</summary>
    Idle,
}

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>What Kraid's room backdrop shows (#1512).</summary>
internal enum KraidBackdropShows
{
    /// <summary>The room's own backdrop colours, before Kraid grows.</summary>
    RoomColours,

    /// <summary>Fading from the room's own backdrop colours into Kraid's, as he grows.</summary>
    FadingInFromRoomColours,

    /// <summary>Kraid's backdrop colours: after he grows, and in his defeated room.</summary>
    KraidColours,

    /// <summary>Fading from Kraid's backdrop colours to black, as he dies.</summary>
    FadingOut,

    /// <summary>Black, while his body sinks.</summary>
    Black,

    /// <summary>Fading from black into Kraid's backdrop colours, after his body has sunk.</summary>
    FadingInFromBlack,
}

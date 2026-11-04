namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The colours Kraid's room backdrop settles on between its fades (#1512).</summary>
internal enum KraidBackdropSettledColours
{
    /// <summary>The room's own backdrop colours, before Kraid grows.</summary>
    RoomColours,

    /// <summary>Kraid's backdrop colours: after his growth fade, after the fade back once his body has sunk, and in his defeated room.</summary>
    KraidColours,

    /// <summary>Black, after his death fade.</summary>
    Black,
}

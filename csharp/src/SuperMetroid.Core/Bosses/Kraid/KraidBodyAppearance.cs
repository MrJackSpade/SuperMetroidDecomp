namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>How a half of Kraid's body shows on his body layer (#1352, #1466).</summary>
internal enum KraidBodyAppearance
{
    /// <summary>Nothing: the half shows his blank, before he rises or once his death has cleared it.</summary>
    Hidden,

    /// <summary>His body behind the room's sprites, as he rises and fights small.</summary>
    BehindSprites,

    /// <summary>His body in front of the room's sprites, once he has grown.</summary>
    InFrontOfSprites,
}

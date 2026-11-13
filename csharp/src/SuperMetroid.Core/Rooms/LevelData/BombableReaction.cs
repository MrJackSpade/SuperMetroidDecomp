namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What a bombable block or bombable air does with its behaviour (#1722): the bomb block a bomb, a Power Bomb or Samus's
/// breaking contact sets off. Its values are arbitrary; a behaviour written for another collision kind is read as one of
/// these through <see cref="RoomBlockBehavior"/>.
/// </summary>
public enum BombableReaction
{
    /// <summary>One bomb block that grows back.</summary>
    RespawningBreak1x1,

    /// <summary>Two bomb blocks side by side that grow back.</summary>
    RespawningBreak2x1,

    /// <summary>Two stacked bomb blocks that grow back.</summary>
    RespawningBreak1x2,

    /// <summary>A two-by-two square of bomb blocks that grows back.</summary>
    RespawningBreak2x2,

    /// <summary>One bomb block that breaks for good.</summary>
    PermanentBreak1x1,

    /// <summary>Two bomb blocks side by side that break for good.</summary>
    PermanentBreak2x1,

    /// <summary>Two stacked bomb blocks that break for good.</summary>
    PermanentBreak1x2,

    /// <summary>A two-by-two square of bomb blocks that breaks for good.</summary>
    PermanentBreak2x2,

    /// <summary>The empty reaction past the breaks: a bomb sets off a one-update object; breaking contact rejects it.</summary>
    Nothing,

    /// <summary>A reaction past the end of the bomb table: a bomb or breaking contact rejects it.</summary>
    PastTable,

    /// <summary>One of the reactions the room's area gives the block: bombs and contact leave it alone and it stays solid.</summary>
    AreaIgnored,
}

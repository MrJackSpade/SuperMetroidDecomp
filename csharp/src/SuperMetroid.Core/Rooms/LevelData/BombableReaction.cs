namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What a bombable block or bombable air does with its behaviour (#1722): the bomb block a bomb, a Power Bomb or Samus's
/// breaking contact sets off. Its values are arbitrary; a behaviour written for another collision kind is read as one of
/// these through <see cref="RoomBlockBehavior"/>, or as none when the bombable kinds have no reaction for it.
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

    /// <summary>
    /// A reaction that breaks nothing: a bomb sets off a room object that ends on its first update, and Samus's breaking
    /// contact sets off nothing.
    /// </summary>
    Nothing,

    /// <summary>Bombs and Samus's breaking contact leave the block alone and set nothing off; a bombable block stays solid.</summary>
    Inert,
}

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// Where a boss body frame is drawn among the room's layers (#1634): with the room's background behind the sprites, or with
/// its background in front of them. A frame is drawn whole at its one depth.
/// </summary>
public enum BossBodyDepth
{
    /// <summary>Behind the sprites and the foreground, as Draygon's body and Kraid's body as he rises are drawn.</summary>
    BehindSprites,

    /// <summary>In front of the sprites drawn among the room's tiles, as Phantoon's body and Kraid's grown body are drawn.</summary>
    InFrontOfSprites,
}

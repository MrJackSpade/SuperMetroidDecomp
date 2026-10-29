namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>A boss whose body is drawn on its room's body layer from its installed body sheet (#1466).</summary>
public enum BossBodyOwner
{
    /// <summary>Kraid: his upper and lower body, each behind or in front of the room's sprites, and his head's mouth stages.</summary>
    Kraid,

    /// <summary>Phantoon: his body, eye, eyeball, tentacle and mouth frames.</summary>
    Phantoon,

    /// <summary>Draygon: his body frames.</summary>
    Draygon,

    /// <summary>Crocomire: his body frames and his two melt images.</summary>
    Crocomire,

    /// <summary>Mother Brain: her body poses from her second phase.</summary>
    MotherBrain,
}

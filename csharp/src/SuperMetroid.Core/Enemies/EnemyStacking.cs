namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Where an enemy's picture stands among the room's actor pictures where they overlap (#2618): in front of Samus and her
/// shots or behind them, and in a fixed order among the other enemies. The game draws its enemies in this order, so a Ceres
/// door's frame covers Samus as she walks through it and the Baby covers her while it drains her.
/// <see cref="EnemyStackingOrder"/> lists the standings front to back.
/// </summary>
public enum EnemyStacking
{
    /// <summary>In front of every other actor: the Chozo statues.</summary>
    Frontmost,

    /// <summary>In front of Samus, behind the frontmost enemies.</summary>
    InFrontOfSamus,

    /// <summary>Behind Samus, in front of the enemies that stand behind her: Evir's arms and Shaktool's arms.</summary>
    JustBehindSamus,

    /// <summary>Behind Samus, where most enemies stand.</summary>
    BehindSamus,

    /// <summary>Behind Samus and most enemies: a Tourian statue, and a Skultera swimming right.</summary>
    FarBehindSamus,

    /// <summary>Behind every other actor: Botwoon's head inside the wall.</summary>
    Rearmost,
}

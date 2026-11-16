namespace SuperMetroid.Core.Enemies;

/// <summary>What common normal-shot damage did to the enemy a shot reached (#2255).</summary>
internal enum NormalShotHit
{
    /// <summary>The enemy's vulnerability froze it outright, without damage.</summary>
    Froze,

    /// <summary>The shot's damage was zero: the shot shows the dud flash and is spent, and the enemy is untouched.</summary>
    Dud,

    /// <summary>The shot hurt an enemy that was frozen, which makes no cry and shows no hit burst.</summary>
    HurtFrozen,

    /// <summary>The shot hurt an enemy that was not frozen.</summary>
    HurtUnfrozen,
}

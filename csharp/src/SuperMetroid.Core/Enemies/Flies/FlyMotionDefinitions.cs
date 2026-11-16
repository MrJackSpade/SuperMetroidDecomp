namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>How a fly circles, dives at Samus and retreats (#2149).</summary>
internal static class FlyMotionDefinitions
{
    /// <summary>How close Samus comes horizontally, in pixels and exclusive, before a circling fly dives at her.</summary>
    internal const int AttackHorizontalRange = 112;

    /// <summary>How far a circling fly turns each update: sixteen 256ths of a turn.</summary>
    internal const int CircleTurnSteps = 16;

    /// <summary>How long a fly that has retreated circles before it can dive again, in updates.</summary>
    internal const int AttackCooldown = 24;

    /// <summary>The fly's diving speed across, in whole pixels an update along its bearing's sine.</summary>
    internal const int DiveAcrossSpeed = 2;

    /// <summary>The fly's diving speed down, in whole pixels an update along its bearing's sine a quarter turn behind.</summary>
    internal const int DiveDownSpeed = 4;
}

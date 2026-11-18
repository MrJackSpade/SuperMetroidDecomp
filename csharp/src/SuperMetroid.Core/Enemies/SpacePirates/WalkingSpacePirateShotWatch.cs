namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>Whether a walking space pirate looks out for Samus's shots, as its placement says (#2453). Its values are arbitrary.</summary>
internal enum WalkingSpacePirateShotWatch
{
    /// <summary>The pirate pays Samus's shots no heed.</summary>
    IgnoresShots,

    /// <summary>The pirate looks for Samus's shots after its normal AI and flinches from one close enough.</summary>
    FlinchesFromShots,
}

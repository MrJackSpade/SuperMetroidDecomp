namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>How far Crocomire's death's colours have reached his sprites (#2560), in the order his death shows them.</summary>
internal enum CrocomireDeathColours
{
    /// <summary>None: his sprites draw in their own colours.</summary>
    None,
    /// <summary>The wall spikes' colours, which the wall's rumble shows.</summary>
    WallSpikes,
    /// <summary>The wall spikes' and the skeleton's arm's, which the skeleton's reveal adds.</summary>
    WallSpikesAndSkeletonArm,
}

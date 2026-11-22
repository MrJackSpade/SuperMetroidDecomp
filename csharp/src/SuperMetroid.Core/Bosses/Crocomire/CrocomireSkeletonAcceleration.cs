namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>How a stage of his skeleton's flight gathers sideways speed (#2182).</summary>
/// <param name="PerUpdate">The sideways speed each update adds.</param>
/// <param name="MaximumWholePixels">The whole pixels an update the speed never passes.</param>
internal sealed partial record CrocomireSkeletonAcceleration(SignedSixteenSixteen PerUpdate, int MaximumWholePixels);

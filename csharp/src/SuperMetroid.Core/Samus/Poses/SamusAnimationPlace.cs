namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// A named frame of one animation (#2048), as a path that sets Samus's animation from outside it, or compares her animation
/// with it, names the frame: the place's name, the animation the frame belongs to and the frame. Set on a Samus animating by
/// that animation it is the frame itself; on one animating by another it lands on the frame the import decoded for the
/// place in that animation (<see cref="SamusAnimationDelays.Land"/>).
/// </summary>
/// <param name="Name">The place's name, which the installation keys its landings by.</param>
/// <param name="Animation">The animation the frame belongs to.</param>
/// <param name="Frame">The frame.</param>
internal readonly partial record struct SamusAnimationPlace(SamusAnimationPlaceName Name, SamusPoseAnimation Animation, SamusAnimationFrame Frame);

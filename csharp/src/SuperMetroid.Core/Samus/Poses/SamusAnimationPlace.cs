namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// A named frame of one animation (#2048), as a path that sets Samus's animation from outside it names the frame it sets:
/// the animation the frame belongs to and the frame. Set on a Samus animating by that animation it is the frame itself;
/// set on one animating by another it lands where the game's layout of the animations puts it
/// (<see cref="SamusAnimationDelays.Land"/>).
/// </summary>
/// <param name="Animation">The animation the frame belongs to.</param>
/// <param name="Frame">The frame.</param>
internal readonly record struct SamusAnimationPlace(SamusPoseAnimation Animation, SamusAnimationFrame Frame);

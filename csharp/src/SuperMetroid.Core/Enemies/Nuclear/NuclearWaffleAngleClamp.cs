namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>A Nuclear Waffle angle after clamping, and whether the clamp finished the sweep.</summary>
/// <param name="Angle">The angle.</param>
/// <param name="Finished">The finished.</param>
[IndependentFlag("whether the clamp reached the end of the sweep, a fact of the angle and the sweep bounds")]
internal readonly record struct NuclearWaffleAngleClamp(int Angle, bool Finished);

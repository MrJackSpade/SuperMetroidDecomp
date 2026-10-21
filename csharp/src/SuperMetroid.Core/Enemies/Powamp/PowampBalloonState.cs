namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>Whether a Powamp balloon is sinking, and how many poses it shows.</summary>
/// <param name="Sinking">The sinking.</param>
/// <param name="ShownPoses">The shown poses.</param>
[IndependentFlag("whether the balloon program is a sinking one, a fact of the running animation program")]
internal readonly record struct PowampBalloonState(bool Sinking, int ShownPoses);

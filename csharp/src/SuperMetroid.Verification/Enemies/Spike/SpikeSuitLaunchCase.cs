using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Enemies.Spike;

/// <summary>A spike suit launch frame and the shinespark state it should leave.</summary>
/// <param name="LaunchFrame">The frame the launch is pressed on.</param>
/// <param name="Phase">The expected shinespark phase.</param>
/// <param name="Pose">The expected pose.</param>
/// <param name="Palette">The expected special palette.</param>
internal readonly record struct SpikeSuitLaunchCase(int LaunchFrame, ShinesparkPhase Phase, SamusPoseId Pose, SamusSpecialPaletteType Palette);

namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>How a Fireflea flies (#1472, #1519).</summary>
/// <param name="Path">Whether it bobs vertically or circles.</param>
/// <param name="SpeedSign">Whether it starts at its speed or at the speed's negation.</param>
/// <param name="InitialAngle">Its first angle on its circle.</param>
/// <param name="Speed">
/// Its speed: in pixels per update while bobbing, in 256ths of a turn per update, with a 16-bit fraction, while circling.
/// </param>
/// <param name="Radius">The radius of its circle, or how far it bobs to either side of where it starts, in pixels.</param>
internal sealed record FirefleaSpawnSettings(FirefleaPath Path, EnemySpeedSign SpeedSign, ByteAngle InitialAngle, SignedSixteenSixteen Speed, int Radius) : EnemySpawnSettings;

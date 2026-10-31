using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Enemies.Fireflea;

/// <summary>The y and expected speed sign of one verify fireflea vertical extents wrap case.</summary>
/// <param name="Y">The y.</param>
/// <param name="ExpectedSpeedSign">Which of the row's speeds the Fireflea moves at afterwards.</param>
internal readonly record struct VerifyFirefleaVerticalExtentsWrapCase(ushort Y, EnemySpeedSign ExpectedSpeedSign);

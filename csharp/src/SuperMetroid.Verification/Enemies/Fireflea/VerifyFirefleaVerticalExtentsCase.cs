namespace SuperMetroid.Verification.Enemies.Fireflea;

/// <summary>One Fireflea vertical extents case: where the Fireflea stands, and the way it moves afterwards.</summary>
/// <param name="Y">The row it stands at before its update.</param>
/// <param name="ExpectedDirection">The way it moves after the update.</param>
internal readonly record struct VerifyFirefleaVerticalExtentsCase(ushort Y, VerticalDirection ExpectedDirection);

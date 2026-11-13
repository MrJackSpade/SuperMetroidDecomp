namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>Whether the chain runs reversed, and the head orientation the centre should then choose.</summary>
/// <param name="Reversed">Whether the chain runs reversed.</param>
/// <param name="ExpectedOrientation">The direction the centre's head should aim.</param>
internal readonly record struct VerifyShaktoolOwnedStateCase(bool Reversed, CompassDirection ExpectedOrientation);

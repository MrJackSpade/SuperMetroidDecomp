namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>The reversed and expected bucket of one verify shaktool owned state case.</summary>
/// <param name="Reversed">The reversed.</param>
/// <param name="ExpectedBucket">The expected bucket.</param>
internal readonly record struct VerifyShaktoolOwnedStateCase(bool Reversed, int ExpectedBucket);

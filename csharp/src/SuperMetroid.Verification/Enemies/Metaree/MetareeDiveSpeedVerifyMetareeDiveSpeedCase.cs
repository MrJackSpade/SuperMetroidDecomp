using SuperMetroid.Core.Enemies.Metaree;

namespace SuperMetroid.Verification.Enemies.Metaree;

/// <summary>The yvelocity and function of one metaree dive speed verify metaree dive speed case.</summary>
/// <param name="YVelocity">The yvelocity.</param>
/// <param name="Function">The function.</param>
internal readonly record struct MetareeDiveSpeedVerifyMetareeDiveSpeedCase(int YVelocity, MetareeEnemyFunction? Function);

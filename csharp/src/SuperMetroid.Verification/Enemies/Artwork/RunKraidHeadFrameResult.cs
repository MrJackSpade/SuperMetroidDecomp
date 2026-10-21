using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>The state and shown of one run kraid head frame result.</summary>
/// <param name="State">The state.</param>
/// <param name="Shown">The shown.</param>
internal readonly record struct RunKraidHeadFrameResult(KraidEnemyState State, ushort[] Shown);

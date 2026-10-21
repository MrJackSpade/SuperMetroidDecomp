using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>The state and tilemap of one read imported kraid head frame result.</summary>
/// <param name="State">The state.</param>
/// <param name="Tilemap">The tilemap.</param>
internal readonly record struct ReadImportedKraidHeadFrameResult(KraidEnemyState State, ushort[] Tilemap);

using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>A fixture's boss frames and the body sheets composed from them (#1466).</summary>
/// <param name="Frames">The boss artwork whose frames the sheets were composed from.</param>
/// <param name="Sheets">The body sheets composed from <paramref name="Frames"/>.</param>
internal sealed record BossBodyArtworkFixture(BossSourceArtwork Frames, BossBodySheets Sheets);

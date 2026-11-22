using SuperMetroid.Core.Frontend.GameOver;

namespace SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;

/// <summary>A Baby palette phase and its authored color-array identity (#2563).</summary>
/// <param name="Phase">The palette phase.</param>
/// <param name="Name">The identity the game-over document keys its colours by.</param>
internal sealed partial record GameOverBabyPaletteName(GameOverBabyColouring Phase, string Name);

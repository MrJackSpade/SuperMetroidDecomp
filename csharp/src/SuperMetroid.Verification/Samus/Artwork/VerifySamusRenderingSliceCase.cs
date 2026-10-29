using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Verification.Samus.Artwork;

/// <summary>The draw, pose-half frame and instance of one verify samus rendering slice case.</summary>
/// <param name="Draw">The draw.</param>
/// <param name="Frame">The pose half and animation frame the draw shows.</param>
/// <param name="Instance">The instance.</param>
internal readonly record struct VerifySamusRenderingSliceCase(SpriteDraw Draw, SamusBodyHalfFrame Frame, int Instance);

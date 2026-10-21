using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Samus.Artwork;

/// <summary>The draw, index and instance of one verify samus rendering slice case.</summary>
/// <param name="Draw">The draw.</param>
/// <param name="Index">The index.</param>
/// <param name="Instance">The instance.</param>
internal readonly record struct VerifySamusRenderingSliceCase(SpriteDraw Draw, int Index, int Instance);

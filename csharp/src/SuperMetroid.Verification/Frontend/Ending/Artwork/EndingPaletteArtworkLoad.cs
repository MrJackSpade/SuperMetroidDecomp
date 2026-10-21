using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Verification.Frontend.Ending.Artwork;

/// <summary>The role, load and long of one ending palette artwork load.</summary>
/// <param name="Role">The role.</param>
/// <param name="Load">The load.</param>
/// <param name="Long">The long.</param>
internal readonly record struct EndingPaletteArtworkLoad(EndingPaletteId Role, int Load, bool Long);

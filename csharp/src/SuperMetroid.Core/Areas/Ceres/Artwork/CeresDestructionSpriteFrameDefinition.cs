namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>One stable destruction/reveal composition identity with its original native OAM geometry; stock part count is descriptive and does not restrict independently edited compositions.</summary>
/// <param name="Name">Stable JSON key for a backdrop or numbered blast frame.</param>
/// <param name="Pointer">Bank-relative $8C spritemap pointer selected by the scene's compiled sprite instruction lists.</param>
/// <param name="StockPartCount">Number of ordered five-byte OAM parts in the original record, excluding its two-byte count header.</param>
/// <param name="Backdrop">The backdrop this frame draws, or <see langword="null"/> for a blast frame.</param>
public readonly record struct CeresDestructionSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount, CeresDestructionBackdrop? Backdrop = null);

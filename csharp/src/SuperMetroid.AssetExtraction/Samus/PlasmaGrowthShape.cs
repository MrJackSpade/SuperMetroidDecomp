
namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>A plasma growth stage and whether it travels diagonally.</summary>
/// <param name="Stage">The stage.</param>
/// <param name="Diagonal">The diagonal.</param>
internal readonly record struct PlasmaGrowthShape(PlasmaGrowthStage Stage, bool Diagonal);

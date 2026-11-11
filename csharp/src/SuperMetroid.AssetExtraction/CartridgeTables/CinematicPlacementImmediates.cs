namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where in bank $8B a cinematic actor's initializer holds its start's horizontal and vertical immediates, when the two do
/// not lie one store apart: the stationary vortex's parameter-zero branch loads its own column before the vortex's shared
/// row (#1615).
/// </summary>
/// <param name="Id">The actor's asset id.</param>
/// <param name="FirstOperand">The bank-$8B address of its horizontal immediate.</param>
/// <param name="SecondOperand">The bank-$8B address of its vertical immediate.</param>
internal readonly record struct CinematicPlacementImmediates(string Id, int FirstOperand, int SecondOperand);

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>How many updates each pose of a Dachora's stride shows (#2477, #2565), by the pose it times.</summary>
/// <param name="FarLegBack">The far leg back.</param>
/// <param name="FarLegPassing">The far leg passing.</param>
/// <param name="FarLegReaching">The far leg reaching.</param>
/// <param name="NearLegBack">The near leg back.</param>
/// <param name="NearLegPassing">The near leg passing.</param>
/// <param name="NearLegReaching">The near leg reaching.</param>
internal sealed partial record DachoraStridePace(int FarLegBack, int FarLegPassing, int FarLegReaching, int NearLegBack, int NearLegPassing, int NearLegReaching);

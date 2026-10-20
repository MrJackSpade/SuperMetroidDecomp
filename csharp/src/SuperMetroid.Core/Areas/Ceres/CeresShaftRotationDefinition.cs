namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One record of the Ceres elevator shaft's rotation table: how long it lasts and the matrix's sine and cosine.</summary>
/// <param name="Timer">How many frames the record lasts.</param>
/// <param name="Sine">The rotation matrix's sine word.</param>
/// <param name="Cosine">The rotation matrix's cosine word.</param>
public readonly record struct CeresShaftRotationDefinition(ushort Timer, ushort Sine, ushort Cosine);

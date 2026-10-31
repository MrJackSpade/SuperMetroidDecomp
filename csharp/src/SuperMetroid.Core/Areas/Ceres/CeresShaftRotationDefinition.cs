namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One step of the Ceres elevator shaft's rotation: how long it lasts and the matrix's sine and cosine.</summary>
/// <param name="Timer">How many frames the step lasts.</param>
/// <param name="Sine">The rotation matrix's signed 8.8 sine.</param>
/// <param name="Cosine">The rotation matrix's signed 8.8 cosine.</param>
public readonly record struct CeresShaftRotationDefinition(int Timer, int Sine, int Cosine);

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One step of the Ceres elevator shaft's rotation: how long it lasts and how far the shaft is tilted.</summary>
/// <param name="Timer">How many frames the step lasts.</param>
/// <param name="Tilt">How far the shaft's floor is turned clockwise on the screen, in radians.</param>
public readonly record struct CeresShaftRotationDefinition(int Timer, double Tilt);

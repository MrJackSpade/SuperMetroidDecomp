namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One tilt of the Ceres elevator shaft's rotation: how long the shaft holds it and how far it is tilted.</summary>
/// <param name="HoldFrames">The updates the shaft holds the tilt, counting the update that turns it there.</param>
/// <param name="Tilt">How far the shaft's floor is turned clockwise on the screen, in radians.</param>
public readonly record struct CeresShaftRotationDefinition(int HoldFrames, double Tilt);

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The angles of a shinespark's two departure trails.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
public readonly record struct ShinesparkDepartureAngles(RotationAngle First, RotationAngle Second);

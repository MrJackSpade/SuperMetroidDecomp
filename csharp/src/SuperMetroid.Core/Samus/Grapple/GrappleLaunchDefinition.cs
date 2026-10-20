namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>A grapple beam's launch: its extension velocity and angle.</summary>
/// <param name="Velocity">The extension velocity.</param>
/// <param name="Angle">The launch angle.</param>
internal readonly record struct GrappleLaunchDefinition(EightEightVector<RoomPixels> Velocity, SnesAngle Angle);

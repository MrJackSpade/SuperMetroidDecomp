namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Physical bullet launch for one turret direction.</summary>
/// <param name="BulletOffset">The bullet's spawn offset from the turret, in room pixels.</param>
/// <param name="BulletVelocity">The bullet's signed 8.8 launch velocity.</param>
internal readonly record struct MotherBrainTurretDirectionDefinition(
    Offset16<RoomPixels> BulletOffset,
    EightEightVector<RoomPixels> BulletVelocity);

using SuperMetroid.Core.Enemies.WorkRobots;

namespace SuperMetroid.Verification.Enemies.WorkRobots;

/// <summary>The definition and xvelocity of one verify work robot laser behavior case.</summary>
/// <param name="Definition">The definition.</param>
/// <param name="XVelocity">The xvelocity.</param>
internal readonly record struct VerifyWorkRobotLaserBehaviorCase(WorkRobotLaser Definition, short XVelocity);

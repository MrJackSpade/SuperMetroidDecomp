namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One physical bomb's fuse and initial velocity; X retains the native direction bit rather than signed-short interpretation.</summary>
internal readonly record struct BombSpreadLaunchDefinition(
    ushort FuseTimer, ushort XVelocity, SnesUnsignedSixteenSixteen YSpeed);

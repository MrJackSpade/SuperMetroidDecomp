namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Kraid's rock projectile programs, which the Kago bug's initial pose shares.</summary>
public enum KraidRockSegment
{
    /// <summary>One still pose, shared by the spat, ceiling and left rising rocks and the Kago bug.</summary>
    SharedPose,
    /// <summary>The right rising rock's still pose.</summary>
    RisingRightPose,
    /// <summary>The spat rock's five-frame burst after a Samus shot.</summary>
    SpitRockShot,
}

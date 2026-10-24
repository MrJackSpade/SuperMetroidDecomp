namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The tags a pose transition leaves on the shot direction it hands to the next HUD producer.</summary>
public static class SamusShotDirectionHandoff
{
    /// <summary>$91:F8F3-$F903: tag on the prior Moonwalk muzzle direction, consumed by turning HUD dispatch.</summary>
    public const ushort MoonwalkTag = 1 << 8;
    /// <summary>$91:F5E3: normal-jump initializer tags the new pose's shot direction for the next HUD producer.</summary>
    public const ushort NormalJumpTag = 1 << 15;
}

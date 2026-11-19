namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The installed key of each <see cref="SamusAnimationSegmentName"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class SamusAnimationSegmentNameKeys
{
    /// <summary>Each SamusAnimationSegmentName member's installed key.</summary>
    internal static InstalledKeys<SamusAnimationSegmentName> Installed { get; } = new(new Dictionary<SamusAnimationSegmentName, string>
    {
        [SamusAnimationSegmentName.Main] = "Main",
        [SamusAnimationSegmentName.Run] = "Run",
        [SamusAnimationSegmentName.SlowStride] = "SlowStride",
        [SamusAnimationSegmentName.StrideHold] = "StrideHold",
        [SamusAnimationSegmentName.RecoilHold] = "RecoilHold",
        [SamusAnimationSegmentName.RollHold] = "RollHold",
        [SamusAnimationSegmentName.Collapse] = "Collapse",
        [SamusAnimationSegmentName.Fall] = "Fall",
        [SamusAnimationSegmentName.Held] = "Held",
        [SamusAnimationSegmentName.StandUp] = "StandUp",
        [SamusAnimationSegmentName.FailedStand] = "FailedStand",
        [SamusAnimationSegmentName.FailedStandEnd] = "FailedStandEnd",
        [SamusAnimationSegmentName.Frozen] = "Frozen",
        [SamusAnimationSegmentName.Breathe] = "Breathe",
        [SamusAnimationSegmentName.LowEnergyBreathe] = "LowEnergyBreathe",
        [SamusAnimationSegmentName.FallStart] = "FallStart",
        [SamusAnimationSegmentName.FastFall] = "FastFall",
        [SamusAnimationSegmentName.Aim] = "Aim",
        [SamusAnimationSegmentName.AimHeld] = "AimHeld",
        [SamusAnimationSegmentName.Knockback] = "Knockback",
        [SamusAnimationSegmentName.Roll] = "Roll",
        [SamusAnimationSegmentName.Spin] = "Spin",
        [SamusAnimationSegmentName.WallJumpReady] = "WallJumpReady",
        [SamusAnimationSegmentName.Launch] = "Launch",
        [SamusAnimationSegmentName.OrdinarySpin] = "OrdinarySpin",
        [SamusAnimationSegmentName.SpaceJumpSpin] = "SpaceJumpSpin",
        [SamusAnimationSegmentName.ScrewAttackSpin] = "ScrewAttackSpin",
        [SamusAnimationSegmentName.Rise] = "Rise",
        [SamusAnimationSegmentName.Glow] = "Glow",
        [SamusAnimationSegmentName.Finish] = "Finish",
        [SamusAnimationSegmentName.Still] = "Still",
        [SamusAnimationSegmentName.Appearance] = "Appearance",
    });
}

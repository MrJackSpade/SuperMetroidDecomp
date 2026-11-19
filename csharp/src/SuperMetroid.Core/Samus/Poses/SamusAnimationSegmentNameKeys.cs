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
        [SamusAnimationSegmentName.Cycle] = "Cycle",
        [SamusAnimationSegmentName.Run] = "Run",
        [SamusAnimationSegmentName.SlowStride] = "SlowStride",
        [SamusAnimationSegmentName.StrideHold] = "StrideHold",
        [SamusAnimationSegmentName.RecoilHold] = "RecoilHold",
        [SamusAnimationSegmentName.RollHold] = "RollHold",
        [SamusAnimationSegmentName.Collapse] = "Collapse",
        [SamusAnimationSegmentName.Fall] = "Fall",
        [SamusAnimationSegmentName.Landing] = "Landing",
        [SamusAnimationSegmentName.Held] = "Held",
        [SamusAnimationSegmentName.HeldReturn] = "HeldReturn",
        [SamusAnimationSegmentName.StandUp] = "StandUp",
        [SamusAnimationSegmentName.FailedStand] = "FailedStand",
        [SamusAnimationSegmentName.FailedStandEnd] = "FailedStandEnd",
        [SamusAnimationSegmentName.Frozen] = "Frozen",
        [SamusAnimationSegmentName.Breathe] = "Breathe",
        [SamusAnimationSegmentName.LowEnergyBreathe] = "LowEnergyBreathe",
        [SamusAnimationSegmentName.FallStart] = "FallStart",
        [SamusAnimationSegmentName.SlowFall] = "SlowFall",
        [SamusAnimationSegmentName.FastFall] = "FastFall",
        [SamusAnimationSegmentName.FastFallHeld] = "FastFallHeld",
        [SamusAnimationSegmentName.Aim] = "Aim",
        [SamusAnimationSegmentName.AimHeld] = "AimHeld",
        [SamusAnimationSegmentName.Knockback] = "Knockback",
        [SamusAnimationSegmentName.Recoil] = "Recoil",
        [SamusAnimationSegmentName.Roll] = "Roll",
        [SamusAnimationSegmentName.SpinStart] = "SpinStart",
        [SamusAnimationSegmentName.Spin] = "Spin",
        [SamusAnimationSegmentName.WallJumpReady] = "WallJumpReady",
        [SamusAnimationSegmentName.Launch] = "Launch",
        [SamusAnimationSegmentName.OrdinarySpin] = "OrdinarySpin",
        [SamusAnimationSegmentName.SpaceJumpSpin] = "SpaceJumpSpin",
        [SamusAnimationSegmentName.ScrewAttackSpin] = "ScrewAttackSpin",
        [SamusAnimationSegmentName.Rise] = "Rise",
        [SamusAnimationSegmentName.Rising] = "Rising",
        [SamusAnimationSegmentName.Glow] = "Glow",
        [SamusAnimationSegmentName.Finish] = "Finish",
        [SamusAnimationSegmentName.Still] = "Still",
        [SamusAnimationSegmentName.Appearance] = "Appearance",
        [SamusAnimationSegmentName.AppearanceHeld] = "AppearanceHeld",
    });
}

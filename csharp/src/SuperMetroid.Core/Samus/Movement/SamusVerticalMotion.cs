using static SuperMetroid.Core.Samus.Movement.SamusVerticalMotionDefinitions;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Samus's launch speeds and gravity in each medium.</summary>
internal static class SamusVerticalMotion
{
    /// <summary>The bomb-jump launch speed in <paramref name="medium"/>.</summary>
    internal static UnsignedSixteenSixteen BombJump(SamusLiquidMedium medium) => SplitSpeed(InMedium(BombJumpSpeeds, medium));

    /// <summary>The knockback launch speed in <paramref name="medium"/>.</summary>
    internal static UnsignedSixteenSixteen Knockback(SamusLiquidMedium medium) => SplitSpeed(InMedium(KnockbackSpeeds, medium));

    /// <summary>The launch speed by medium, jump equipment and launch kind.</summary>
    internal static UnsignedSixteenSixteen Launch(SamusLiquidMedium medium, bool highJump, SamusLaunchKind kind)
    {
        IReadOnlyDictionary<SamusLaunchSpeedKey, int> speeds = highJump ? HighJumpLaunchSpeeds : UnassistedLaunchSpeeds;
        var key = new SamusLaunchSpeedKey(medium, kind);
        return speeds.ContainsKey(key) ? SplitSpeed(speeds[key])
            : throw new InvalidOperationException($"Undefined {nameof(SamusLiquidMedium)} {medium} or {nameof(SamusLaunchKind)} {kind}.");
    }

    /// <summary>The gravity in <paramref name="medium"/>.</summary>
    internal static UnsignedSixteenSixteen Gravity(SamusLiquidMedium medium) => new(0, InMedium(GravityFractions, medium));

    /// <summary>The value <paramref name="table"/> lists for <paramref name="medium"/>; the cartridge defines three media.</summary>
    private static int InMedium(IReadOnlyDictionary<SamusLiquidMedium, int> table, SamusLiquidMedium medium) =>
        table.ContainsKey(medium) ? table[medium] : throw new InvalidOperationException($"Undefined {nameof(SamusLiquidMedium)} {medium}.");

    /// <summary>Splits a native 8.8 launch magnitude into the separate whole and 16.16 fractional words Samus stores.</summary>
    private static UnsignedSixteenSixteen SplitSpeed(int speed) =>
        new(EightEight.WholeOf(speed), SixteenSixteen.FractionOf(SixteenSixteen.FromEightEight(speed)));
}

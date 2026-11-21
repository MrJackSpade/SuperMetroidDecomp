using static SuperMetroid.Core.Samus.Movement.SamusVerticalMotionDefinitions;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Samus's launch speeds and gravity in each medium.</summary>
internal static class SamusVerticalMotion
{
    /// <summary>The bomb-jump launch speed in <paramref name="medium"/>.</summary>
    internal static UnsignedSixteenSixteen BombJump(SamusLiquidMedium medium) => InMedium(BombJumpSpeeds, medium);

    /// <summary>The knockback launch speed in <paramref name="medium"/>.</summary>
    internal static UnsignedSixteenSixteen Knockback(SamusLiquidMedium medium) => InMedium(KnockbackSpeeds, medium);

    /// <summary>The launch speed by medium, jump equipment and launch kind.</summary>
    internal static UnsignedSixteenSixteen Launch(SamusLiquidMedium medium, bool highJump, SamusLaunchKind kind)
    {
        IdentityTable<SamusLiquidMedium, IdentityTable<SamusLaunchKind, UnsignedSixteenSixteen>> speeds = highJump ? HighJumpLaunchSpeeds : UnassistedLaunchSpeeds;
        return speeds[medium][kind];
    }

    /// <summary>The gravity in <paramref name="medium"/>.</summary>
    internal static UnsignedSixteenSixteen Gravity(SamusLiquidMedium medium) => InMedium(Gravities, medium);

    /// <summary>The value <paramref name="table"/>, which names every medium, lists for <paramref name="medium"/>.</summary>
    private static UnsignedSixteenSixteen InMedium(IdentityTable<SamusLiquidMedium, UnsignedSixteenSixteen> table, SamusLiquidMedium medium) =>
        table[medium];
}

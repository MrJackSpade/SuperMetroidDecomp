using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// The presentation boundary of Ridley's breakup fragments (#641): the frame each
/// <see cref="RidleyBreakupFrame"/> shows.
/// </summary>
internal static class RidleyBreakupFrameDefinitions
{

    /// <summary>The installed frame each <see cref="RidleyBreakupFrame"/> shows.</summary>
    internal static EnemyFrameTable<RidleyBreakupFrame> ShownFrames { get; } = new()
    {
        { RidleyBreakupFrame.ClawFacingLeft, EnemyFrameId.RidleyBreakupClawLeft },
        { RidleyBreakupFrame.ClawFacingRight, EnemyFrameId.RidleyBreakupClawRight },
        { RidleyBreakupFrame.LegsFacingLeft, EnemyFrameId.RidleyBreakupLegsLeft },
        { RidleyBreakupFrame.LegsFacingRight, EnemyFrameId.RidleyBreakupLegsRight },
        { RidleyBreakupFrame.OpenHeadAndNeckFacingLeft, EnemyFrameId.RidleyBreakupTorsoLeft },
        { RidleyBreakupFrame.OpenHeadAndNeckFacingRight, EnemyFrameId.RidleyBreakupTorsoRight },
        { RidleyBreakupFrame.TailOuterLink, EnemyFrameId.RidleyTailOuterLink },
        { RidleyBreakupFrame.TailMiddleLink, EnemyFrameId.RidleyTailMiddleLink },
        { RidleyBreakupFrame.TailBaseLink, EnemyFrameId.RidleyTailBaseLink },
        { RidleyBreakupFrame.TailTipPointingDown, EnemyFrameId.RidleyTailTipPointingToward180 },
        { RidleyBreakupFrame.TailTipPointingDownDownLeft, EnemyFrameId.RidleyTailTipPointingToward157 },
        { RidleyBreakupFrame.TailTipPointingUpRight, EnemyFrameId.RidleyTailTipPointingToward315 },
        { RidleyBreakupFrame.TailTipPointingRightUpRight, EnemyFrameId.RidleyTailTipPointingToward292 },
        { RidleyBreakupFrame.TailTipPointingRight, EnemyFrameId.RidleyTailTipPointingToward270 },
        { RidleyBreakupFrame.TailTipPointingRightDownRight, EnemyFrameId.RidleyTailTipPointingToward247 },
        { RidleyBreakupFrame.TailTipPointingDownRight, EnemyFrameId.RidleyTailTipPointingToward225 },
        { RidleyBreakupFrame.TailTipPointingDownDownRight, EnemyFrameId.RidleyTailTipPointingToward202 },
        { RidleyBreakupFrame.TailTipPointingDownLeft, EnemyFrameId.RidleyTailTipPointingToward135 },
        { RidleyBreakupFrame.TailTipPointingLeftDownLeft, EnemyFrameId.RidleyTailTipPointingToward112 },
        { RidleyBreakupFrame.TailTipPointingLeft, EnemyFrameId.RidleyTailTipPointingToward90 },
        { RidleyBreakupFrame.TailTipPointingLeftUpLeft, EnemyFrameId.RidleyTailTipPointingToward67 },
        { RidleyBreakupFrame.TailTipPointingUpLeft, EnemyFrameId.RidleyTailTipPointingToward45 },
        { RidleyBreakupFrame.TailTipPointingUpUpLeft, EnemyFrameId.RidleyTailTipPointingToward22 },
        { RidleyBreakupFrame.TailTipPointingUp, EnemyFrameId.RidleyTailTipPointingToward0 },
        { RidleyBreakupFrame.TailTipPointingUpUpRight, EnemyFrameId.RidleyTailTipPointingToward337 },
        { RidleyBreakupFrame.TorsoFacingLeft, EnemyFrameId.RidleyBreakupHeadLeft },
        { RidleyBreakupFrame.TorsoFacingRight, EnemyFrameId.RidleyBreakupHeadRight },
        { RidleyBreakupFrame.WingsFacingLeft, EnemyFrameId.RidleyWingFacingLeftRaised },
        { RidleyBreakupFrame.WingsFacingRight, EnemyFrameId.RidleyWingFacingRightRaised },
    };
}

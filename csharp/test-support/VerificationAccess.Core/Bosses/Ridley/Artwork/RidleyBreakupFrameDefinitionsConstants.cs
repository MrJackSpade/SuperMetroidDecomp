using SuperMetroid.Core.Enemies.Artwork;
using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>The cartridge spritemap each of Ridley's breakup pictures is stored at, which only verification reads.</summary>
internal static class RidleyBreakupFrameDefinitionsConstants
{
    /// <summary>The spritemap <paramref name="frame"/>, one of Ridley's breakup pictures, shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is no breakup picture.</exception>
    internal static ushort Spritemap(EnemyFrameId frame) => Spritemaps.TryGetValue(frame, out ushort spritemap)
        ? spritemap
        : throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Ridley breakup frame.");

    /// <summary>Each breakup picture by the spritemap it shows.</summary>
    private static readonly Dictionary<EnemyFrameId, ushort> Spritemaps = new()
    {
        [F.RidleyTailBaseLink] = 0xdc90,
        [F.RidleyTailMiddleLink] = 0xdc97,
        [F.RidleyTailOuterLink] = 0xdc9e,
        [F.RidleyWingFacingLeftRaised] = 0xdd4a,
        [F.RidleyWingFacingRightRaised] = 0xdde2,
        [F.RidleyBreakupLegsLeft] = 0xed29,
        [F.RidleyBreakupLegsRight] = 0xef25,
        [F.RidleyBreakupOpenHeadAndNeckLeft] = 0xecdc,
        [F.RidleyBreakupOpenHeadAndNeckRight] = 0xeed8,
        [F.RidleyBreakupTorsoLeft] = 0xed95,
        [F.RidleyBreakupTorsoRight] = 0xef91,
        [F.RidleyBreakupClawLeft] = 0xed8e,
        [F.RidleyBreakupClawRight] = 0xef8a,
        [F.RidleyTailTipPointingToward180] = 0xdd2e,
        [F.RidleyTailTipPointingToward157] = 0xdd27,
        [F.RidleyTailTipPointingToward135] = 0xdd20,
        [F.RidleyTailTipPointingToward112] = 0xdd19,
        [F.RidleyTailTipPointingToward90] = 0xdd12,
        [F.RidleyTailTipPointingToward67] = 0xdd0b,
        [F.RidleyTailTipPointingToward45] = 0xdd04,
        [F.RidleyTailTipPointingToward22] = 0xdcfd,
        [F.RidleyTailTipPointingToward0] = 0xdcf6,
        [F.RidleyTailTipPointingToward337] = 0xdcef,
        [F.RidleyTailTipPointingToward315] = 0xdce8,
        [F.RidleyTailTipPointingToward292] = 0xdce1,
        [F.RidleyTailTipPointingToward270] = 0xdcda,
        [F.RidleyTailTipPointingToward247] = 0xdd43,
        [F.RidleyTailTipPointingToward225] = 0xdd3c,
        [F.RidleyTailTipPointingToward202] = 0xdd35,
    };
}

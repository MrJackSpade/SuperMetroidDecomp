using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics shared by the three Samus-loading palette programs.</summary>
/// <remarks>
/// Issue #855 / #625: the Power, Varia and Gravity definitions
/// share one layout.
/// Each runs four groups, repeated 36, 3, 3 and 2 times, of two records of sixteen
/// colours held for three frames each. One final record, held for one frame, follows,
/// and the program ends. Thus
/// <c>(36 + 3 + 3 + 2)*2*3 + 1 = 265</c> frames elapse. The 432 colours
/// remain live presentation data supplied by the presentation compiler;
/// this catalog owns only placement, timing, replay, and deletion.
/// </remarks>
public static class SamusLoadingSuitPaletteFxColorDefinitions
{

    /// <summary>Each record writes sixteen live colours.</summary>
    public const int ColorsPerFrame = 16;
    /// <summary>The Power-suit definition's program.</summary>
    private static readonly SamusLoadingSuitPaletteFxColorDefinition Power =
        new()
        {
            Owner = SamusLoadingSuitPaletteFxProgramOwner.PowerSuit,
            Object = PaletteFxObject.SamusLoadingPowerSuit,
            ColorFrames = PaletteFxColorFrames.Of(PaletteFxSegment.SamusLoadingPowerSuit1, ColorsPerFrame),
        };
    /// <summary>The Varia-suit definition's program.</summary>
    private static readonly SamusLoadingSuitPaletteFxColorDefinition Varia =
        new()
        {
            Owner = SamusLoadingSuitPaletteFxProgramOwner.VariaSuit,
            Object = PaletteFxObject.SamusLoadingVariaSuit,
            ColorFrames = PaletteFxColorFrames.Of(PaletteFxSegment.SamusLoadingVairaSuit1, ColorsPerFrame),
        };
    /// <summary>The Gravity-suit definition's program.</summary>
    private static readonly SamusLoadingSuitPaletteFxColorDefinition Gravity =
        new()
        {
            Owner = SamusLoadingSuitPaletteFxProgramOwner.GravitySuit,
            Object = PaletteFxObject.SamusLoadingGravitySuit,
            ColorFrames = PaletteFxColorFrames.Of(PaletteFxSegment.SamusLoadingGravitySuit1, ColorsPerFrame),
        };

    /// <summary>The power, Varia, and gravity programs in definition order.</summary>
    /// <remarks>Named program identities replace the former indexed descriptor array.
    /// The view preserves original order/count/index rejection; runtime mechanics
    /// dispatches directly to the same three named owners without enumeration.
    /// Header words and all program ownership are independently checked for #1165.</remarks>
    public static IReadOnlyList<SamusLoadingSuitPaletteFxColorDefinition> All { get; } = [Power, Varia, Gravity];

}

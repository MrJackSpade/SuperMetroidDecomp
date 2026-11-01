namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics shared by the three Samus-loading palette programs.</summary>
/// <remarks>
/// Issue #855 / #625: the Power, Varia and Gravity definitions
/// share one layout.
/// Each starts with <c>SetColorIndex(384)</c>, then
/// four <c>SetTimer(byte)</c> groups, whose
/// timers <c>36,3,3,2</c> replay each
/// group's pair of <c>3, colors[16], Wait</c> records via
/// <c>DecrementTimerAndGoto(group start)</c>. One final
/// <c>1, colors[16], Wait</c> record follows, and
/// <c>Delete</c> ends the program. Thus
/// <c>(36 + 3 + 3 + 2)*2*3 + 1 = 265</c> frames elapse. All 99
/// control words and 12 timer bytes match the original programs. The 432 BGR555 words
/// remain live presentation data supplied by the presentation compiler;
/// this catalog owns only placement, timing, replay, and deletion.
/// </remarks>
public static class SamusLoadingSuitPaletteFxColorDefinitions
{

    /// <summary>Each record writes sixteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 16;
    /// <summary>The Power-suit definition's program.</summary>
    private static readonly SamusLoadingSuitPaletteFxColorDefinition Power =
        new()
        {
            Owner = SamusLoadingSuitPaletteFxProgramOwner.PowerSuit,
            Object = PaletteFxObject.SamusLoadingPowerSuit,
            ColorFrames = PaletteFxColorFrames.Of(PaletteFxProgram.SamusLoadingPowerSuit1, ColorsPerFrame),
        };
    /// <summary>The Varia-suit definition's program.</summary>
    private static readonly SamusLoadingSuitPaletteFxColorDefinition Varia =
        new()
        {
            Owner = SamusLoadingSuitPaletteFxProgramOwner.VariaSuit,
            Object = PaletteFxObject.SamusLoadingVariaSuit,
            ColorFrames = PaletteFxColorFrames.Of(PaletteFxProgram.SamusLoadingVairaSuit1, ColorsPerFrame),
        };
    /// <summary>The Gravity-suit definition's program.</summary>
    private static readonly SamusLoadingSuitPaletteFxColorDefinition Gravity =
        new()
        {
            Owner = SamusLoadingSuitPaletteFxProgramOwner.GravitySuit,
            Object = PaletteFxObject.SamusLoadingGravitySuit,
            ColorFrames = PaletteFxColorFrames.Of(PaletteFxProgram.SamusLoadingGravitySuit1, ColorsPerFrame),
        };

    /// <summary>The power, Varia, and gravity programs in definition order.</summary>
    /// <remarks>Named program identities replace the former indexed descriptor array.
    /// The view preserves original order/count/index rejection; runtime mechanics
    /// dispatches directly to the same three named owners without enumeration.
    /// Header words and all program ownership are independently checked for #1165.</remarks>
    public static IReadOnlyList<SamusLoadingSuitPaletteFxColorDefinition> All { get; } = [Power, Varia, Gravity];

}

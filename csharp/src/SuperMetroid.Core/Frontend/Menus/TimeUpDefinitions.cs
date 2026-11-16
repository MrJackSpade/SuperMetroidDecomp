using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>How an expired escape countdown whites out the screen.</summary>
internal static class TimeUpDefinitions
{
    /// <summary>The colour every screen colour fades towards.</summary>
    public static RgbColor WhiteColor => RgbColor.White;

    /// <summary>The steps the screen takes to fade to white.</summary>
    public const int WhiteOutDenominator = 8;
}

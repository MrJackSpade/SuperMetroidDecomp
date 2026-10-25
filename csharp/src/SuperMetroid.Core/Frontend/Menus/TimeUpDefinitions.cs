namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>How an expired escape countdown whites out the screen.</summary>
internal static class TimeUpDefinitions
{
    /// <summary>The colour every palette colour fades towards.</summary>
    public static Bgr555 WhiteColor => Bgr555.White;

    /// <summary>The denominator of the gradual colour change towards white.</summary>
    public const int WhiteOutDenominator = 8;
}

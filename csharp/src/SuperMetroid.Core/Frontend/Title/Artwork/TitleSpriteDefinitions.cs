namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>The complete set of title compositions the installed title artwork must provide.</summary>
public static class TitleSpriteDefinitions
{
    /// <summary>Every <see cref="TitleSprite"/>, each exactly once: the four cards' reveals, the logo and the copyright.</summary>
    /// <summary>The title sheet's frame for <paramref name="sprite"/>.</summary>
    public static string FrameName(TitleSprite sprite) => $"Title.{sprite}";

    /// <summary>Every title composition.</summary>
    public static readonly IReadOnlyList<TitleSprite> All =
    [
        TitleSprite.YearReveal1, TitleSprite.YearReveal2, TitleSprite.YearReveal3, TitleSprite.YearReveal4,
        TitleSprite.NintendoReveal1, TitleSprite.NintendoReveal2, TitleSprite.NintendoReveal3, TitleSprite.NintendoReveal4,
        TitleSprite.NintendoReveal5, TitleSprite.NintendoReveal6, TitleSprite.NintendoReveal7, TitleSprite.NintendoReveal8,
        TitleSprite.PresentsReveal1, TitleSprite.PresentsReveal2, TitleSprite.PresentsReveal3, TitleSprite.PresentsReveal4,
        TitleSprite.PresentsReveal5, TitleSprite.PresentsReveal6, TitleSprite.PresentsReveal7, TitleSprite.PresentsReveal8,
        TitleSprite.MetroidThreeReveal1, TitleSprite.MetroidThreeReveal2, TitleSprite.MetroidThreeReveal3,
        TitleSprite.MetroidThreeReveal4, TitleSprite.MetroidThreeReveal5, TitleSprite.MetroidThreeReveal6,
        TitleSprite.MetroidThreeReveal7, TitleSprite.MetroidThreeReveal8, TitleSprite.MetroidThreeReveal9,
        TitleSprite.SuperMetroidLogo, TitleSprite.NintendoCopyright,
    ];
}

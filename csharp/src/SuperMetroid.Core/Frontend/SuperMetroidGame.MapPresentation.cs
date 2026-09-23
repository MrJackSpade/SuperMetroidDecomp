using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    // Host-owned immutable content is rebound after graph restoration, not embedded
    // in debugger states where it would hide newer user overrides on disk.
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;

    public string? MapPresentationIdentity => mapPresentation?.ContentIdentity;

    /// <summary>Attaches the host's current catalog after construction or debugger-state load.</summary>
    public void BindMapPresentation(AreaMapPresentationCatalog? catalog)
    {
        mapPresentation = catalog;
        if (runtime is not null) runtime.MapPresentation = catalog;
        ceresDestruction?.BindFixedColors(catalog?.PowerBombFixedColors);
        pauseMenu?.BindMapPresentation(catalog);
        fileSelectMap?.BindMapPresentation(catalog);
        gameOver?.BindMapPresentation(catalog);
        options?.BindMapPresentation(catalog);
        fileSelect?.BindMapPresentation(catalog);
        title?.BindTitleGradient(catalog?.TitleGradient);
        title?.BindTitlePalette(catalog?.TitlePalette);
        if (intro is not null) intro.NarrationPresentation = catalog?.IntroNarration;
        intro?.BindIntroFont(catalog?.IntroFont);
        endingCredits?.BindEndingText(catalog?.EndingText);
        endingCredits?.BindEndingFont(catalog?.EndingFont);
        endingCredits?.BindStaffCredits(catalog?.StaffCredits);
    }
}

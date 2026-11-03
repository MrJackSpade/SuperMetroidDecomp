namespace SuperMetroid.Core.Frontend;

/// <summary>
/// What a group of a frontend screen's colours is for (#1464): the colours a screen's effects animate and its actors draw
/// in, named by their meaning on that screen. Effects and draws address a screen's colours only through these keys;
/// <see cref="ScreenColourGroups"/> alone knows where the screen holds each group.
/// </summary>
public enum ScreenColourGroup
{
    /// <summary>The colours of Samus's suit on a screen that shows her, which palette effects and her hurt flash write.</summary>
    SamusSuit,

    /// <summary>The colours of Samus's beams on a screen that shows her.</summary>
    SamusBeams,

    /// <summary>The map's highlight colours, which the map highlight cycle animates on the pause and file-select maps.</summary>
    MapHighlight,

    /// <summary>The reserve-tank arrow's darker bevel colour, which its pulse animates.</summary>
    PauseReserveArrowShade,

    /// <summary>The reserve-tank arrow's lighter bevel colour, which its pulse animates.</summary>
    PauseReserveArrowLight,

    /// <summary>The file-copy arrows' visible colours, which rotate while a copy destination is chosen.</summary>
    FileCopyArrows,

    /// <summary>The colours of the baby Metroid's tube on the title, which the tube light dims and brightens.</summary>
    TitleTubeLight,

    /// <summary>The colours of the title's flickering displays.</summary>
    TitleDisplays,

    /// <summary>The title's warm paint whose colour the displays show when dim.</summary>
    TitleDimWarmPaint,

    /// <summary>The title's green paint whose colour the displays show when dim.</summary>
    TitleDimGreenPaint,

    /// <summary>The game-over baby Metroid's colours, which its glow animates.</summary>
    GameOverBaby,

    /// <summary>The ending clear-time digits' and colon's colours.</summary>
    EndingClearTimeDigits,

    /// <summary>The ending clear-time label's colours.</summary>
    EndingTimeLabel,

    /// <summary>The ending's "operation completed" text colours.</summary>
    EndingText,

    /// <summary>The ending clouds' colours.</summary>
    EndingClouds,

    /// <summary>The colours of the exploding planet's lava and silhouette, which the explosion crossfade fades in.</summary>
    EndingExplosionLava,

    /// <summary>The exploding planet's colours, shared by its glow and stars, which the explosion crossfade fades in.</summary>
    EndingExplodingPlanet,

    /// <summary>The exploding planet's glow colour, darkened as the explosion's planet fade begins.</summary>
    EndingExplodingPlanetGlow,

    /// <summary>The explosion afterglow's colours.</summary>
    EndingExplosionAfterglow,

    /// <summary>The explosion scene's background colours, which the explosion crossfade fades out.</summary>
    EndingExplosionBackground,

    /// <summary>The background colours the explosion's final white flash turns white.</summary>
    EndingFlashBackground,

    /// <summary>The transparent background colour the flash turns white and the flyaway clears.</summary>
    EndingFlashBackgroundTransparent,

    /// <summary>The transparent sprite colour the flash turns white and the flyaway clears.</summary>
    EndingFlashSpriteTransparent,

    /// <summary>The final gunship's colours, loaded as it leaves the planet.</summary>
    EndingFinalGunship,

    /// <summary>The colours of the animals' escape ship that leaves the planet when they were saved.</summary>
    EndingAnimalEscape,

    /// <summary>The ending logo's background colours, which the logo crossfade brightens.</summary>
    EndingLogoBackground,

    /// <summary>The ending logo's sprite colours, which the logo crossfade darkens.</summary>
    EndingLogoSprites,

    /// <summary>
    /// The background colours the post-credits scenes load from their own colours: all but the first few, which keep the
    /// credits' colours.
    /// </summary>
    EndingPostCreditsRestoredBackground,

    /// <summary>The sprite colours the post-credits scenes load from their own colours: every one.</summary>
    EndingPostCreditsRestoredSprites,

    /// <summary>The post-credits waiting backdrop's colours.</summary>
    EndingWaitingBackdrop,

    /// <summary>The colours of suitless Samus in the post-credits reward.</summary>
    EndingSuitlessSamus,

    /// <summary>The colours of suited Samus in the post-credits reward.</summary>
    EndingSuitedSamus,

    /// <summary>The colours of Samus as she falls back down in the suitless post-credits jump.</summary>
    EndingFallingSamus,

    /// <summary>The colours of Samus as she shoots at the end of the post-credits reward, which fade out after the shot.</summary>
    EndingShootingSamus,

    /// <summary>The post-credits shot graphic's colours, which fade out as it turns.</summary>
    EndingShot,

    /// <summary>The colours of Ceres station's background-drawn navigation lights, which their blink animates.</summary>
    CeresNavigationLights,

    /// <summary>The background lights of the opening's old Mother Brain fight, which their cycle animates.</summary>
    IntroOldMotherBrainLights,

    /// <summary>The foreground colours of the wide part of Zebes's explosion in the ending.</summary>
    EndingWideExplosionForeground,

    /// <summary>The background colours of the wide part of Zebes's explosion in the ending.</summary>
    EndingWideExplosionBackground,

    /// <summary>The colours the finale of Zebes's explosion in the ending flashes.</summary>
    EndingExplosionFinale,

    /// <summary>The colours of space, which the ending whites out as Zebes explodes.</summary>
    EndingExplosionSpace,

    /// <summary>The colours of the exploding planet's lava drawn on the ending's background.</summary>
    EndingExplodingZebesLava,

    /// <summary>The colours of the exploding planet's crust, which the ending fades out.</summary>
    EndingExplodingZebesCrust,

    /// <summary>The colours of the exploding planet's grey clouds, which the ending fades out.</summary>
    EndingExplodingZebesGreyClouds,

    /// <summary>The colours of the gunship as it emerges from Zebes's explosion in the ending.</summary>
    EndingGunshipEmerging,

    /// <summary>The colours the unused cinematic fade program fades.</summary>
    UnusedCinematicFade,

    /// <summary>The title logo's glowing colours, which a palette effect animates.</summary>
    TitleLogoGlow,

    /// <summary>The Nintendo boot logo's fading colours, which a palette effect animates.</summary>
    NintendoBootLogo,

    /// <summary>The Planet Zebes caption's fading colours, which a palette effect animates.</summary>
    PlanetZebesText,

    /// <summary>The ending gunship's glowing colour, which a palette effect animates.</summary>
    EndingGunshipGlow,

    /// <summary>The zoomed-out exploding Zebes' colours, which a palette effect animates.</summary>
    EndingZoomedOutExplodingZebes,

    /// <summary>The post-credits Super Metroid icon's colours, which a palette effect animates.</summary>
    EndingPostCreditsIcon,
}

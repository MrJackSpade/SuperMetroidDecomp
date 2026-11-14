namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The ending's background pictures and turning floors (#1544), each of which shows only the colour effects authored for
/// it, as <see cref="EndingColourOwnerDefinitions"/> lists them.
/// </summary>
public enum EndingScenery
{
    /// <summary>The side-cloud escape's turning floor: the clouds over the exploding planet's lava and crust.</summary>
    SideCloudEscapeFloor,

    /// <summary>The top-and-bottom-cloud escape's turning floor: the grey clouds.</summary>
    TopAndBottomCloudEscapeFloor,

    /// <summary>The turning floor the explosion's crossfade and hold show the planet on.</summary>
    PlanetExplosionFloor,

    /// <summary>The turning floor of the gunship flying away from the explosion.</summary>
    FlyawayFloor,

    /// <summary>The post-credits shot turning into the camera, drawn in the shot's colours.</summary>
    PostCreditsShotFloor,

    /// <summary>The explosion's burst and finale pictures.</summary>
    ExplosionPictures,

    /// <summary>The staff credits' rows.</summary>
    CreditsRows,

    /// <summary>The post-credits waiting backdrop.</summary>
    WaitingBackdrop,

    /// <summary>The wavy Samus glowing over the waiting backdrop.</summary>
    WavySamus,

    /// <summary>The post-credits text pages: the result, copyright and final pages and their typed glyphs.</summary>
    PostCreditsText,

    /// <summary>The logo's background picture, which its crossfade brings in.</summary>
    LogoPicture,
}

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Which of the ending's sprite layers a sprite shows on (#1514).</summary>
internal enum EndingSpriteLayer
{
    /// <summary>The starfields and the shooting stars, behind the planet, the gunship and the backdrops they shine through.</summary>
    Stars,

    /// <summary>
    /// Every other sprite: the clouds, the exploding planet and its debris, the completion text, the reward and the
    /// logo, in front of the scene's pictures.
    /// </summary>
    Actors,
}

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>One drawing step of an illustrated intro background page, applied in page order.</summary>
internal abstract record IntroBackgroundStroke
{
    /// <summary>Draws this step onto <paramref name="canvas"/>, over anything earlier steps drew there.</summary>
    internal abstract void Draw(IntroBackgroundCanvas canvas);
}

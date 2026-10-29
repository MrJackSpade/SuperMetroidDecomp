using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frontend screen's own colours by owner (#1358, #1404, #1464): its background sets and its sprite sets, each set the
/// colours that one colour group of the screen's pre-coloured images shows. Effects write an owner's colours, and the
/// screen publishes both owners' sets with its frame.
/// </summary>
public sealed class ScreenPalette
{
    /// <summary>The background sets, which the screen's background images show.</summary>
    public ScreenColourSets BackgroundSets { get; } = new();

    /// <summary>The sprite sets, which the screen's actors draw in.</summary>
    public ScreenColourSets SpriteSets { get; } = new();

    /// <summary>The screen's colours as a frame shows them now.</summary>
    public BackgroundColours Shown() => new(ShownColourSets.Of(BackgroundSets), ShownColourSets.Of(SpriteSets));

    /// <summary>A copy of both owners' colours as they are now, to fade from or restore.</summary>
    public ColourImage Capture() => new(BackgroundSets.All, SpriteSets.All);

    /// <summary>The colour behind every layer, where nothing is drawn: the first background set's first colour.</summary>
    public RgbColor Backdrop
    {
        get => BackgroundSets.Set(0)[0];
        set => BackgroundSets.Set(0).Write(0, value);
    }
}

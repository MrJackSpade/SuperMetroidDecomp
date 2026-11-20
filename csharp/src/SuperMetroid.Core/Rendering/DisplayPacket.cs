using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>
/// One displayed frame, closed and immutable: its scene (a layered scene, a turning-floor scene or a solid colour, #2505),
/// shown at the scene's brightness. It holds neither game objects nor the display's resources.
/// </summary>
public sealed class DisplayPacket
{
    /// <summary>The number of the game update this frame shows, counted as the game counts its updates.</summary>
    public int FrameNumber { get; }

    /// <summary>What the frame shows: a layered scene, a turning floor with its sprites, or one colour.</summary>
    public DisplayScene Scene { get; }

    /// <summary>The frame's visible size (<see cref="Spatial.ScreenGeometry.ScreenSize"/>), the HUD included where there is one, before the host scales it to the window.</summary>
    public Extent16<ScreenPixels> Size { get; } = ScreenGeometry.ScreenSize;

    /// <summary>
    /// The frame's one brightness, from black (zero) to <see cref="DisplayBrightness.Full"/>, applied once after the
    /// scene is composed: its scene's.
    /// </summary>
    public DisplayBrightness Brightness => Scene.Brightness;

    /// <summary>Creates a frame showing <paramref name="scene"/> at the scene's brightness.</summary>
    /// <param name="frameNumber">The number of the game update this frame shows.</param>
    /// <param name="scene">The complete scene, including its brightness.</param>
    /// <exception cref="ArgumentOutOfRangeException">The frame number is negative.</exception>
    public DisplayPacket(int frameNumber, DisplayScene scene)
    {
        FrameNumber = Ensure.AtLeastZero(frameNumber);
        Scene = Ensure.NotNull(scene);
    }

    /// <summary>Creates a frame filled with one colour, shown at <paramref name="brightness"/>.</summary>
    /// <param name="frameNumber">The number of the game update this frame shows.</param>
    /// <param name="solidColor">The colour filling the whole screen before the brightness applies.</param>
    /// <param name="brightness">The frame's brightness.</param>
    /// <exception cref="ArgumentOutOfRangeException">The frame number is negative.</exception>
    private DisplayPacket(int frameNumber, RgbaPixel solidColor, DisplayBrightness brightness)
        : this(frameNumber, new SolidColourScene(solidColor, brightness))
    {
    }

    /// <summary>A frame filled with <paramref name="solidColor"/>, shown undimmed.</summary>
    /// <param name="frameNumber">The number of the game update this frame shows.</param>
    /// <param name="solidColor">The colour filling the whole screen.</param>
    /// <exception cref="ArgumentOutOfRangeException">The frame number is negative.</exception>
    public DisplayPacket(int frameNumber, RgbaPixel solidColor)
        : this(frameNumber, solidColor, DisplayBrightness.Full)
    {
    }
}

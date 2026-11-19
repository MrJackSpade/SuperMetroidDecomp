namespace SuperMetroid.Core.Rendering;

/// <summary>
/// What one displayed frame shows (#2505): a layered scene (<see cref="LayeredScene"/>), a turning floor with the sprites
/// in front of it (<see cref="FloorScene"/>), or one colour filling the screen (<see cref="SolidColourScene"/>), each at
/// the brightness it shows at. These are the only kinds of scene; a backend draws each through <see cref="DrawWith"/>.
/// </summary>
public abstract class DisplayScene
{
    /// <summary>Keeps the scene's brightness; only the three kinds of scene derive from this.</summary>
    /// <param name="brightness">The frame's brightness, from black (zero) to <see cref="DisplayBrightness.Full"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The brightness is below black or above full.</exception>
    private protected DisplayScene(int brightness) => Brightness = DisplayBrightness.Require(brightness);

    /// <summary>
    /// The frame's one brightness, from black (zero) to <see cref="DisplayBrightness.Full"/>, applied once after the scene
    /// is composed.
    /// </summary>
    public int Brightness { get; }

    /// <summary>Draws the scene with <paramref name="drawer"/>'s method for its kind.</summary>
    /// <typeparam name="TDrawer">The drawer's type.</typeparam>
    /// <param name="drawer">What draws each kind of scene.</param>
    public abstract void DrawWith<TDrawer>(TDrawer drawer) where TDrawer : IDisplaySceneDrawer;

    /// <summary>The same scene shown at <paramref name="brightness"/>: this scene where that is its own brightness.</summary>
    /// <param name="brightness">The brightness, from black (zero) to <see cref="DisplayBrightness.Full"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The brightness is below black or above full.</exception>
    internal abstract DisplayScene AtBrightness(int brightness);
}

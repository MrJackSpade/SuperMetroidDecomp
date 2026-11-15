using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// The blast that destroys Ceres station (#1981): the Power Bomb's blast, burning through the same stages, radii and glow,
/// centred on the station as the screen shows it. Its centre is a screen point, never a room position: the cinematic has
/// no room, and the blast's oval of added colour covers the whole screen from its top line.
/// </summary>
internal sealed class CeresStationBlast
{
    /// <summary>
    /// The Power Bomb blast's stages, radii and glow the station blast burns through. Its own centre is never read: the
    /// station blast places the oval it shows at <see cref="centre"/>.
    /// </summary>
    private readonly SamusPowerBombExplosionState stages = new();

    /// <summary>Where on the screen the blast is centred.</summary>
    private Offset16<ScreenPixels> centre;

    /// <summary>The installed glow colours the blast shows: installed content, bound again when a game is restored.</summary>
    internal InstalledPowerBombGlowColors? PresentationColors
    {
        get => stages.PresentationColors;
        set => stages.PresentationColors = value;
    }

    /// <summary>Starts the blast centred at <paramref name="screenCentre"/>.</summary>
    internal void Spawn(Offset16<ScreenPixels> screenCentre)
    {
        centre = screenCentre;
        stages.Spawn(Position16<RoomPixels>.Zero);
    }

    /// <summary>Runs one update of the blast's stages; a blast that is not running stays still.</summary>
    internal void StepFrame() => _ = stages.StepFrame();

    /// <summary>The blast's oval of added colour as a screen layer from the screen's top line, or null while it is not running.</summary>
    internal ColourAddWindowLayer? Capture()
    {
        if (!stages.IsActive)
            return null;
        PowerBombBlastOval oval = stages.ShownOval;
        ScreenWindow window = oval.Radii.Y == 0 ? ScreenWindow.Closed
            : ScreenWindow.Of(WindowOval.Ellipse(centre, new Extent16<ScreenPixels>(oval.Radii.X, oval.Radii.Y)), firstLine: 0);
        return new(window, oval.Colour);
    }
}

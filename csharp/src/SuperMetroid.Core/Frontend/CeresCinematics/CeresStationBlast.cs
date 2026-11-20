using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// The blast that destroys Ceres station (#1981, #2000): a Power Bomb blast's timeline, burning through the same stages,
/// radii and glow, centred on the station as the screen shows it. Its centre is a screen point, never a room position:
/// the cinematic has no room, and the blast's oval of added colour covers the whole screen from its top line.
/// </summary>
internal sealed class CeresStationBlast
{
    /// <summary>The blast's stages, radii and glow: the cinematic's own timeline, shared with no Power Bomb of Samus's.</summary>
    private readonly PowerBombBlastTimeline timeline = new();

    /// <summary>Where on the screen the blast is centred.</summary>
    private Offset16<ScreenPixels> centre;

    /// <summary>Starts the blast centred at <paramref name="screenCentre"/>.</summary>
    internal void Spawn(Offset16<ScreenPixels> screenCentre)
    {
        centre = screenCentre;
        timeline.Start();
    }

    /// <summary>
    /// Runs one update of the blast's stages, glowing in <paramref name="glowColors"/>; a blast that is not running stays
    /// still.
    /// </summary>
    internal void StepFrame(InstalledPowerBombGlowColors glowColors) => timeline.StepFrame(glowColors);

    /// <summary>The blast's oval of added colour as a screen layer from the screen's top line, or null while it is not running.</summary>
    internal ColourAddWindowLayer? Capture()
    {
        if (!timeline.IsRunning)
            return null;
        int horizontalRadius = timeline.ShownRadius;
        int verticalRadius = PowerBombBlastOval.VerticalRadius(horizontalRadius);
        ScreenWindow window = verticalRadius == 0 ? ScreenWindow.Closed
            : ScreenWindow.Of(WindowOval.Ellipse(centre, new Extent16<ScreenPixels>(horizontalRadius, verticalRadius)), firstLine: 0);
        return new(window, timeline.GlowColor);
    }
}

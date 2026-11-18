using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's rainbow beam as a screen effect: whether it shows, the colour it adds and the cone it adds that colour
/// across, from her mouth between its two edge angles. The display draws the cone; the colour advances only with the
/// simulation, never when a frame is repainted.
/// </summary>
public sealed class MotherBrainRainbowBeamEffect
{
    /// <summary>Whether the last simulation update showed the beam; a hidden beam shows nothing.</summary>
    public bool Active => colours is not null;
    /// <summary>The colour the beam adds inside its cone; set when the beam appears and advanced only by later updates that show it.</summary>
    public RgbColor Color { get; private set; }
    /// <summary>
    /// The beam's colour sequence playing (#2404), a colour each update the beam shows: its first colour, then its hue
    /// cycle, which starts over at its first colour past its last, so no colour repeats (#2251); null while the beam is
    /// hidden.
    /// </summary>
    private AnimationPlayer<MotherBrainRainbowBeamSegment, RgbColor, NoAnimationCue>? colours;
    /// <summary>The installed colours <see cref="colours"/> plays.</summary>
    private MotherBrainRainbowBeamColours? played;
    /// <summary>The cone the last update that showed the beam aimed; null until the beam first shows.</summary>
    public MotherBrainRainbowBeamGeometry? Geometry { get; private set; }
    /// <summary>What the beam shows now, or null while it is hidden; taking it never advances the beam.</summary>
    public MotherBrainRainbowBeamRenderSnapshot? RenderSnapshot => Active && Geometry is { } cone ? new(Color, cone) : null;

    /// <summary>One update that hides the beam, without advancing its colour; the next update that shows it restarts the colour sequence.</summary>
    public void Hide() => colours = null;

    /// <summary>One update that shows the beam, after the head's position and beam aim have been resolved.</summary>
    /// <param name="head">Head enemy's whole-pixel room position. The mouth lies fourteen pixels right of it and five below.</param>
    /// <param name="angle">Beam center aim; only its whole angle units are used when computing the two wrapping edge angles.</param>
    /// <param name="angularWidth">Angular width; its whole steps are halved with integer truncation to obtain each edge's displacement in steps.</param>
    /// <param name="installed">The installed colours the beam plays, which the host binds (#2261).</param>
    public void Show(Position16<RoomPixels> head, RotationAngle angle, RotationAngle angularWidth, MotherBrainRainbowBeamColours installed)
    {
        Ensure.NotNull(installed);
        if (colours is null)
        {
            // A beam that appears starts its colour sequence over.
            colours = AnimationPlayer<MotherBrainRainbowBeamSegment, RgbColor, NoAnimationCue>.Starting(installed.BeamSegments, installed.BeamAnimation);
        }
        else if (!ReferenceEquals(played, installed))
        {
            // Colours the host binds anew play on from where the sequence stands.
            colours.Rebind(installed.BeamSegments, installed.BeamAnimation);
        }
        played = installed;
        colours.Step();
        Color = colours.Showing;
        // The cone's edges lie half its whole-step width either side of its direction.
        TrigonometryAngle halfWidth = angularWidth.WholeStep.DividedBy(2).ToTrigonometryAngle();
        Geometry = new(new(head.X + MotherBrainBeamDefinitions.MouthXOffset, head.Y + MotherBrainBeamDefinitions.MouthYOffset),
            angle.ToTrigonometryAngle() - halfWidth,
            angle.ToTrigonometryAngle() + halfWidth);
    }
}

using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The colours of Mother Brain's rainbow beam backdrop, which the game reads while the beam fires: its first colour and
/// every colour of its hue cycle, as installed (#1557). Her rainbow, drain, revival and restoration colours are the
/// import's, which pre-colours her pictures in them (#1512, #1527, #1616).
/// </summary>
public sealed class MotherBrainRainbowBeamColours
{
    /// <summary>Every colour of the backdrop's hue cycle, in the order the beam shows them, owned by this presentation.</summary>
    private readonly RgbColor[] beamCycle;

    /// <summary>Keeps the beam backdrop's colours.</summary>
    /// <param name="beamInitial">The beam backdrop's first colour.</param>
    /// <param name="beamCycle">Every colour of the beam backdrop's hue cycle, in order; copied.</param>
    /// <exception cref="ArgumentNullException"><paramref name="beamCycle"/> is null.</exception>
    /// <exception cref="InvalidDataException">The cycle holds no colour.</exception>
    internal MotherBrainRainbowBeamColours(RgbColor beamInitial, RgbColor[] beamCycle)
    {
        BeamInitialColor = beamInitial;
        this.beamCycle = [.. Ensure.NotNull(beamCycle)];
        if (this.beamCycle.Length == 0)
            throw new InvalidDataException("Mother Brain's installed beam colour cycle holds no colour.");
        BeamSegments = new(new Dictionary<MotherBrainRainbowBeamSegment, AnimationBeat<RgbColor, NoAnimationCue>[]>
        {
            [MotherBrainRainbowBeamSegment.FirstColour] = [new AnimationBeat<RgbColor, NoAnimationCue>.Frame(beamInitial, 1)],
            [MotherBrainRainbowBeamSegment.HueCycle] = [.. this.beamCycle.Select(colour => new AnimationBeat<RgbColor, NoAnimationCue>.Frame(colour, 1))],
        });
        BeamAnimation = Animation<MotherBrainRainbowBeamSegment>.LeadingInto(
            [BeamSegments.Whole(MotherBrainRainbowBeamSegment.FirstColour)], [BeamSegments.Whole(MotherBrainRainbowBeamSegment.HueCycle)]);
    }

    /// <summary>The backdrop's colours as the beam shows them (#2404), each for one update.</summary>
    internal AnimationSegments<MotherBrainRainbowBeamSegment, RgbColor, NoAnimationCue> BeamSegments { get; }

    /// <summary>
    /// The backdrop's colour sequence while the beam fires (#2404): its first colour, then its hue cycle, looping without
    /// showing any colour twice running.
    /// </summary>
    internal Animation<MotherBrainRainbowBeamSegment> BeamAnimation { get; }

    /// <summary>The backdrop colour the beam shows on its first active frame.</summary>
    public RgbColor BeamInitialColor { get; }

    /// <summary>The colours of the beam backdrop's hue cycle; after its last, the beam starts the cycle again.</summary>
    public int BeamCycleLength => beamCycle.Length;

    /// <summary>Beam cycle colour <paramref name="index"/>, zero through one before <see cref="BeamCycleLength"/>.</summary>
    /// <exception cref="InvalidDataException">The index is outside the cycle.</exception>
    public RgbColor BeamColor(int index) =>
        index >= 0 && index < beamCycle.Length
            ? beamCycle[index]
            : throw new InvalidDataException($"Mother Brain beam color {index} is outside its authored cycle.");
}

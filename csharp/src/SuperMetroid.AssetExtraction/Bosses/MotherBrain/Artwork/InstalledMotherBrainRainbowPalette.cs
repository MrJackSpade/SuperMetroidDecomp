using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>
/// Mother Brain's installed rainbow, drain, revival, fake-death and normal-restoration colors, every phase kept as
/// installed. Only the import reads them, to pre-colour her pictures (#1512); the game reads the beam backdrop colours of
/// the base type (#1527).
/// </summary>
public sealed class InstalledMotherBrainRainbowPalette : MotherBrainRainbowPalettePresentation
{
    /// <summary>Ten color phases read by the rainbow list.</summary>
    internal readonly PaletteFrame[] rainbow;
    /// <summary>The drain to grey.</summary>
    internal readonly PaletteFade toGrey;
    /// <summary>The revival from grey.</summary>
    internal readonly PaletteFade fromGrey;
    /// <summary>Brain-only descent used during the staged fake death.</summary>
    internal readonly PaletteFade fakeDeathToGrey;
    /// <summary>Body and rear-leg colors restored after the beam.</summary>
    internal readonly PaletteFrame normal;

    /// <summary>Keeps the installed rainbow, drain, revival, fake-death, restoration and beam-backdrop colours.</summary>
    /// <param name="rainbow">The ten rainbow phases.</param>
    /// <param name="toGrey">The drain fade.</param>
    /// <param name="fromGrey">The revival fade.</param>
    /// <param name="fakeDeathToGrey">The fake-death brain fade.</param>
    /// <param name="normal">The restored normal phase.</param>
    /// <param name="beamInitial">The beam backdrop's first colour.</param>
    /// <param name="beamCycle">Every colour of the beam backdrop's hue cycle, as installed.</param>
    internal InstalledMotherBrainRainbowPalette(PaletteFrame[] rainbow, PaletteFade toGrey,
        PaletteFade fromGrey, PaletteFade fakeDeathToGrey, PaletteFrame normal,
        RgbColor beamInitial, RgbColor[] beamCycle) : base(beamInitial, beamCycle)
    {
        this.rainbow = rainbow;
        this.toGrey = toGrey;
        this.fromGrey = fromGrey;
        this.fakeDeathToGrey = fakeDeathToGrey;
        this.normal = normal;
    }

    /// <summary>The installed rainbow colours <paramref name="colours"/> were read as.</summary>
    /// <exception cref="InvalidOperationException">The colours were not read from an installation.</exception>
    public static InstalledMotherBrainRainbowPalette Of(MotherBrainRainbowPalettePresentation colours) =>
        Ensure.NotNull(colours) as InstalledMotherBrainRainbowPalette
            ?? throw new InvalidOperationException("Mother Brain's rainbow colours were not read from an installation, so they have no rainbow, drain or revival phases.");

    /// <summary>One installed body and rear-leg phase.</summary>
    internal sealed class PaletteFrame
    {
        /// <summary>The installed rear-leg inks.</summary>
        private readonly RgbColor[] legs;

        /// <summary>Keeps copies of a phase's installed body and rear-leg inks.</summary>
        /// <param name="body">The body inks.</param>
        /// <param name="legs">The rear-leg inks.</param>
        internal PaletteFrame(RgbColor[] body, RgbColor[] legs)
        {
            Body = (RgbColor[])body.Clone();
            this.legs = (RgbColor[])legs.Clone();
        }

        /// <summary>The installed body inks.</summary>
        public RgbColor[] Body { get; }
        /// <summary>Number of rear-leg colors this phase publishes.</summary>
        public int LegCount => legs.Length;
        /// <summary>The installed rear-leg ink <paramref name="color"/>.</summary>
        public RgbColor Leg(int color) => legs[color];
    }

    /// <summary>An installed fade: every phase of it, kept as installed.</summary>
    internal sealed class PaletteFade
    {
        /// <summary>The installed phases, first to last.</summary>
        private readonly PaletteFrame[] frames;

        /// <summary>Keeps the installed phases.</summary>
        /// <param name="frames">Every phase, first to last.</param>
        internal PaletteFade(PaletteFrame[] frames) => this.frames = frames;

        /// <summary>Number of phases between the two endpoints, inclusive.</summary>
        public int Length => frames.Length;
        /// <summary>Body/brain ink count of each phase.</summary>
        public int BodyCount => frames[0].Body.Length;
        /// <summary>Rear-leg ink count of each phase.</summary>
        public int LegCount => frames[0].LegCount;
        /// <summary>The installed body colour <paramref name="color"/> of phase <paramref name="frame"/>.</summary>
        public RgbColor Body(int frame, int color) => frames[frame].Body[color];
        /// <summary>The installed rear-leg colour <paramref name="color"/> of phase <paramref name="frame"/>.</summary>
        public RgbColor Leg(int frame, int color) => frames[frame].Leg(color);
    }
}

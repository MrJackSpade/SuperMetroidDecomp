using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Where a door's fade is (#2553): no fade, or one of its running stages, each carrying everything it shows: the fading
/// stages the steps of their fade, the dark stage nothing, and the fade in whether the door has opened. The running stages
/// are a group of their own, so only a running stage shades the display, the faded pictures and Samus.
/// </summary>
internal abstract partial record DoorFadeStage
{
    /// <summary>Only the stages below exist.</summary>
    private DoorFadeStage()
    {
    }

    /// <summary>No fade runs: the room shows its own colours.</summary>
    internal static DoorFadeStage None { get; } = new NoFade();

    /// <summary>No fade runs.</summary>
    internal sealed partial record NoFade : DoorFadeStage;

    /// <summary>A running stage of the fade.</summary>
    internal abstract partial record Running : DoorFadeStage
    {
        /// <summary>Only the running stages below exist.</summary>
        private protected Running()
        {
        }

        /// <summary>The effect the stage shows over the whole display.</summary>
        internal abstract ColourEffect DisplayEffect { get; }

        /// <summary>Shades <paramref name="pictures"/> as far toward or from <paramref name="dark"/> as the stage has come.</summary>
        internal abstract void Shade(GameplayShadingBuilder frame, GameplayPictures pictures, PictureShade dark);

        /// <summary>The shades of <paramref name="samus"/>'s colours the stage keeps lit over its fade.</summary>
        internal abstract ImmutableArray<PictureShade> LitOnSamus(SamusColours samus);
    }

    /// <summary>The room being left fades out to black: pictures from their own colours toward their dark shades.</summary>
    internal sealed partial record FadingOut : Running
    {
        /// <summary>Fades the room out over <paramref name="steps"/>.</summary>
        internal FadingOut(GradualFadeSteps steps) => Steps = Ensure.NotNull(steps);

        /// <summary>The steps of the fade out, which the door takes one per update.</summary>
        internal GradualFadeSteps Steps { get; }

        /// <inheritdoc/>
        internal override ColourEffect DisplayEffect => Steps.Toward(RgbColor.Black);

        /// <inheritdoc/>
        internal override void Shade(GameplayShadingBuilder frame, GameplayPictures pictures, PictureShade dark) =>
            frame.Shade(pictures, Steps.Between(PictureShading.Standard, PictureShading.Shaded(dark)));

        /// <summary>Nothing of Samus's stays lit while the room being left fades out.</summary>
        internal override ImmutableArray<PictureShade> LitOnSamus(SamusColours samus) => [];
    }

    /// <summary>The faded-out room stays dark while the door loads and scrolls: pictures at their dark shades.</summary>
    internal sealed partial record Dark : Running
    {
        /// <inheritdoc/>
        internal override ColourEffect DisplayEffect => new ColourFlash(RgbColor.Black);

        /// <inheritdoc/>
        internal override void Shade(GameplayShadingBuilder frame, GameplayPictures pictures, PictureShade dark) =>
            frame.Shade(pictures, dark);

        /// <summary>Nothing of Samus's stays lit while the display is dark.</summary>
        internal override ImmutableArray<PictureShade> LitOnSamus(SamusColours samus) => [];
    }

    /// <summary>
    /// The room being entered fades in from black: pictures from their dark shades to their own colours, Samus's beams lit
    /// from the room's load, which the fade in starts with.
    /// </summary>
    internal abstract partial record Entering : Running
    {
        /// <summary>Fades the room in over <paramref name="steps"/>.</summary>
        private protected Entering(GradualFadeSteps steps) => Steps = Ensure.NotNull(steps);

        /// <summary>The steps of the fade in, which the door takes one per update.</summary>
        internal GradualFadeSteps Steps { get; }

        /// <inheritdoc/>
        internal override ColourEffect DisplayEffect => Steps.From(RgbColor.Black);

        /// <inheritdoc/>
        internal override void Shade(GameplayShadingBuilder frame, GameplayPictures pictures, PictureShade dark) =>
            frame.Shade(pictures, Steps.Between(PictureShading.Shaded(dark), PictureShading.Standard));
    }

    /// <summary>The room being entered fades in before its door has opened: Samus's beams stay lit.</summary>
    internal sealed partial record FadingIn : Entering
    {
        /// <summary>Fades the room in over <paramref name="steps"/>.</summary>
        internal FadingIn(GradualFadeSteps steps)
            : base(steps)
        {
        }

        /// <inheritdoc/>
        internal override ImmutableArray<PictureShade> LitOnSamus(SamusColours samus) => [Ensure.NotNull(samus).Beams];
    }

    /// <summary>
    /// The room being entered fades in once its door has opened: Samus's visor shows the door's completion green, which
    /// stays lit after her beams.
    /// </summary>
    internal sealed partial record FadingInDoorOpen : Entering
    {
        /// <summary>Goes on fading the room in over <paramref name="steps"/>, the fade in's own.</summary>
        internal FadingInDoorOpen(GradualFadeSteps steps)
            : base(steps)
        {
        }

        /// <inheritdoc/>
        internal override ImmutableArray<PictureShade> LitOnSamus(SamusColours samus) =>
            [Ensure.NotNull(samus).Beams, SamusShades.DoorCompletionVisor];
    }
}

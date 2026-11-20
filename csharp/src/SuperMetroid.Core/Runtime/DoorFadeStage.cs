using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Where a door's fade is (#2553): no fade, or one of its running stages, each over the steps of the fade it runs. The
/// running stages are a group of their own, so only a running stage shades the display and the faded pictures.
/// </summary>
internal abstract record DoorFadeStage
{
    /// <summary>Only the stages below exist.</summary>
    private DoorFadeStage()
    {
    }

    /// <summary>No fade runs: the room shows its own colours.</summary>
    internal static DoorFadeStage None { get; } = new NoFade();

    /// <summary>No fade runs.</summary>
    internal sealed record NoFade : DoorFadeStage;

    /// <summary>A running stage of the fade, over the steps of the fade it runs.</summary>
    internal abstract record Running : DoorFadeStage
    {
        /// <summary>Runs the stage over <paramref name="steps"/>.</summary>
        private protected Running(GradualFadeSteps steps) => Steps = Ensure.NotNull(steps);

        /// <summary>The steps of the fade running, which the door takes one per update; the dark stage keeps the fade out's.</summary>
        internal GradualFadeSteps Steps { get; }

        /// <summary>The effect the stage shows over the whole display.</summary>
        internal abstract ColourEffect DisplayEffect { get; }

        /// <summary>Shades <paramref name="pictures"/> as far toward or from <paramref name="dark"/> as the stage has come.</summary>
        internal abstract void Shade(GameplayShadingBuilder frame, GameplayPictures pictures, PictureShade dark);
    }

    /// <summary>The room being left fades out to black: pictures from their own colours toward their dark shades.</summary>
    internal sealed record FadingOut : Running
    {
        /// <summary>Runs the stage over <paramref name="steps"/>.</summary>
        internal FadingOut(GradualFadeSteps steps)
            : base(steps)
        {
        }

        /// <inheritdoc/>
        internal override ColourEffect DisplayEffect => Steps.Toward(RgbColor.Black);

        /// <inheritdoc/>
        internal override void Shade(GameplayShadingBuilder frame, GameplayPictures pictures, PictureShade dark) =>
            frame.Shade(pictures, Steps.Between(PictureShading.Standard, PictureShading.Shaded(dark)));
    }

    /// <summary>The faded-out room stays dark while the door loads and scrolls: pictures at their dark shades.</summary>
    internal sealed record Dark : Running
    {
        /// <summary>Runs the stage over <paramref name="steps"/>.</summary>
        internal Dark(GradualFadeSteps steps)
            : base(steps)
        {
        }

        /// <inheritdoc/>
        internal override ColourEffect DisplayEffect => new ColourFlash(RgbColor.Black);

        /// <inheritdoc/>
        internal override void Shade(GameplayShadingBuilder frame, GameplayPictures pictures, PictureShade dark) =>
            frame.Shade(pictures, dark);
    }

    /// <summary>The room being entered fades in from black: pictures from their dark shades to their own colours.</summary>
    internal sealed record FadingIn : Running
    {
        /// <summary>Runs the stage over <paramref name="steps"/>.</summary>
        internal FadingIn(GradualFadeSteps steps)
            : base(steps)
        {
        }

        /// <inheritdoc/>
        internal override ColourEffect DisplayEffect => Steps.From(RgbColor.Black);

        /// <inheritdoc/>
        internal override void Shade(GameplayShadingBuilder frame, GameplayPictures pictures, PictureShade dark) =>
            frame.Shade(pictures, Steps.Between(PictureShading.Shaded(dark), PictureShading.Standard));
    }
}

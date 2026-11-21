using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// What the opening cinematic shows (#2554): checks drive the intro as the game runs it and read the scene it composes, the
/// drawn Samus, Mother Brain's look and the actors' sprites, never the flashback's private actors.
/// </summary>
internal static class IntroSceneObservations
{
    /// <summary>The updates a drive to a scene may run before the intro is taken to have missed it.</summary>
    private const int UpdateLimit = 20_000;

    /// <summary>The update gap between the presses a drive gives, each a new press of A that turns a page whose text has typed.</summary>
    private const int PressEvery = 47;

    /// <summary>What a composed intro scene draws.</summary>
    extension(LayeredScene scene)
    {
        /// <summary>The scene's sprite draws, in the order they are drawn: the actors between the room and the narration.</summary>
        internal ImmutableArray<ImageSpriteDraw> SpriteDraws => scene.LayersOf<SpritesLayer>().Single().Draws;

        /// <summary>The body of the Samus the scene draws, or null when it draws none.</summary>
        internal ImageSpriteDraw? DrawnSamusBody => scene.SpriteDraws.SingleOrDefault(draw => draw.Source.Kind == SpriteSourceKind.Samus);

        /// <summary>Where the scene draws Samus's body, or null when it draws none.</summary>
        internal Offset16<ScreenPixels>? DrawnSamusOrigin => scene.DrawnSamusBody?.Origin;

        /// <summary>
        /// The shading the scene draws Mother Brain in: her colouring, whitened while a hit's flash shows; null when the scene
        /// draws no Mother Brain.
        /// </summary>
        internal PictureShading? DrawnMotherBrainShading => scene.SpriteDraws
            .FirstOrDefault(draw => draw.Shading.Shades.Contains(IntroMotherBrainSpriteState.Colouring))?.Shading;

        /// <summary>Whether the scene draws Mother Brain flashing white after a hit.</summary>
        internal bool DrawsMotherBrainFlashing => scene.DrawnMotherBrainShading is { } shading &&
            shading.Effects.Contains(new ColourFlash(RgbColor.White));
    }

    /// <summary>Drives an intro as the game runs it.</summary>
    extension(IntroCinematicState intro)
    {
        /// <summary>
        /// Runs <paramref name="intro"/> from where it stands, a new press of A every <see cref="PressEvery"/> updates from its
        /// first, its music queue advancing after each, until <paramref name="reached"/> holds after an update.
        /// </summary>
        /// <param name="audio">The intro's music queue.</param>
        /// <param name="reached">Whether the intro stands where the drive stops.</param>
        /// <returns>The updates the drive ran.</returns>
        /// <exception cref="InvalidOperationException">The intro does not get there within <see cref="UpdateLimit"/> updates.</exception>
        internal int StepUntil(AudioQueues audio, Func<IntroCinematicState, bool> reached)
        {
            int update = 0;
            for (; !reached(intro); update++)
            {
                if (update >= UpdateLimit)
                    throw new InvalidOperationException($"The intro did not reach the scene within {UpdateLimit} updates; it stands in {intro.Phase}.");
                intro.Step(update % PressEvery == 0 ? ControllerButton.A : ControllerButton.None);
                audio.AdvanceFrame(default);
            }
            return update;
        }

        /// <summary>
        /// Runs <paramref name="intro"/> to the end of the update the Mother Brain flashback's setup runs on: page one's input
        /// wait takes a press, and the crossfade into the fight starts.
        /// </summary>
        /// <param name="audio">The intro's music queue.</param>
        /// <returns>The updates the drive ran.</returns>
        internal int StepToMotherBrainFlashback(AudioQueues audio) =>
            intro.StepUntil(audio, scene => scene.Phase == IntroCinematicPhase.MotherBrainCrossfade);
    }
}

using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// The grapple's flare (#2396, #2554): the charge flare's main flare, which the grapple restarts at the main flare's loop
/// when fired, plays on from there and clears at teardown. It plays only forward, so it always shows a picture.
/// </summary>
internal sealed class SamusGrappleFlare
{
    /// <summary>The main flare's animation, which the grapple's flare plays.</summary>
    private static readonly Animation<ChargeFlareSegment> MainFlare = ChargeFlareAnimationDefinitions.Animations[ChargeFlareComponent.MainFlare];

    /// <summary>The play of the main flare's animation, on its first picture with its wait spent until fired.</summary>
    private ShowingAnimationPlayer<ChargeFlareSegment, ChargeFlarePicture, NoAnimationCue> Player { get; } =
        ShowingAnimationPlayer<ChargeFlareSegment, ChargeFlarePicture, NoAnimationCue>.StartShowing(
            ChargeFlareAnimationDefinitions.Segments, MainFlare, ChargeFlareAnimationDefinitions.ClearedPictureUpdates);

    /// <summary>The picture the flare shows.</summary>
    internal ChargeFlarePicture Picture => Player.Showing;

    /// <summary>
    /// The flare when fired: it restarts at the main flare's loop, showing the loop's first picture for that picture's own
    /// updates, and plays on through the loop.
    /// </summary>
    internal void RestartAtLoop() => Player.ShowFrom(MainFlare, AnimationPlace.Before(MainFlare.CyclePhase));

    /// <summary>Plays the flare one update on: once the picture's updates run out, the next picture shows.</summary>
    internal void PlayForward() => Player.Step();

    /// <summary>Returns the flare to its first picture, its wait spent.</summary>
    internal void Clear() => Player.Show(MainFlare, ChargeFlareAnimationDefinitions.ClearedPictureUpdates);
}

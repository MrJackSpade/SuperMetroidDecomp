using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The Fireflea room effect's state: where its flash stands (<see cref="FirefleaFlashAnimations"/>), which its per-frame
/// behaviour plays on, the darkness the dying Firefleas bring, and the shade it publishes (<see cref="Tint"/>). A Fireflea
/// room's effect creates it as the room loads, showing its flash's first look in an undarkened room, and it lives as long
/// as that room (#2053, #2367).
/// </summary>
[RoomLifetime]
public sealed class FirefleaFxState
{
    /// <summary>Where the flash plays on from while the darkness holds it: its brightest look.</summary>
    private static readonly AnimationPlace HeldFlash = FirefleaFlashAnimations.Segments.PlaceOfFrame(
        FirefleaFlashAnimations.Flashing, FirefleaFlashSegment.Flashing, FirefleaFlashShade.Brightest);

    /// <summary>
    /// The flash's animation (#2488), played by the shared player: each look shows for its updates, which frozen time holds.
    /// </summary>
    private readonly ShowingAnimationPlayer<FirefleaFlashSegment, FirefleaFlashShade, NoAnimationCue> flash =
        ShowingAnimationPlayer<FirefleaFlashSegment, FirefleaFlashShade, NoAnimationCue>.StartShowing(FirefleaFlashAnimations.Segments, FirefleaFlashAnimations.Flashing);

    /// <summary>Starts the flash on its first look, held for its updates, in a room no Fireflea death has darkened.</summary>
    public FirefleaFxState() => Tint = Shade(Flash, Darkness);

    /// <summary>The flash's look the room shows: it brightens over its first looks and dims over its last.</summary>
    private FirefleaFlashShade Flash => flash.Showing;

    /// <summary>The darkness published from the enemies' count of Firefleas killed.</summary>
    private FirefleaDarkness Darkness { get; set; }

    /// <summary>
    /// The shade Fireflea darkness subtracts from the room, grey on the game's fade scale: the flash's look plus the
    /// darkness's level, as the last unfrozen step chose it. A frozen step keeps it.
    /// </summary>
    public RgbColor Tint { get; private set; }

    /// <summary>
    /// Samples the enemies' darkness, and unless frozen plays the flash on and chooses the room's <see cref="Tint"/> from the
    /// flash's look and the darkness's level. When the flash's next look is due once the darkness holds the flash, the room
    /// shows its brightest look, from which a flash that resumes plays on.
    /// </summary>
    internal void Step(bool frozen, FirefleaDarkness darkness)
    {
        Darkness = darkness;
        if (frozen) return;
        if (darkness.HoldsFlash)
            flash.StepPlayingOnFrom(HeldFlash);
        else
            flash.Step();

        Tint = Shade(Flash, darkness);
    }

    /// <summary>
    /// The grey of <paramref name="flash"/> deepened by <paramref name="darkness"/>: the two fade levels add, and their sum
    /// never passes the scale's full level.
    /// </summary>
    private static RgbColor Shade(FirefleaFlashShade flash, FirefleaDarkness darkness)
    {
        return FadeLevelColour.Grey(flash.Level + darkness.Level).ToColour();
    }
}

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
    /// <summary>Where the flash's animation resumes: past the look it shows.</summary>
    private AnimationPlace place = AnimationPlace.StartOfAnimation;

    /// <summary>Starts the flash on its first look, held for its updates, in a room no Fireflea death has darkened.</summary>
    public FirefleaFxState()
    {
        ShowNextFlash();
        Tint = Shade(Flash, Darkness);
    }

    /// <summary>
    /// Updates until the flash shows its next look, held while time is frozen: a step that reaches zero shows the next look
    /// for its updates.
    /// </summary>
    public int UpdatesUntilNextFlash { get; private set; }

    /// <summary>The flash's look the room shows: it brightens over its first six looks and dims over the last six.</summary>
    public FirefleaFlashShade Flash { get; private set; }

    /// <summary>The darkness published from the enemies' count of Firefleas killed.</summary>
    public FirefleaDarkness Darkness { get; private set; }

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
        if (--UpdatesUntilNextFlash == 0)
        {
            if (darkness.HoldsFlash)
            {
                place = FirefleaFlashAnimations.Segments.PlaceOfFrame(FirefleaFlashAnimations.Flashing, FirefleaFlashSegment.Flashing,
                    FirefleaFlashShade.Brightest);
            }
            ShowNextFlash();
        }

        Tint = Shade(Flash, darkness);
    }

    /// <summary>Plays the flash on to its next look, which it shows for that look's updates.</summary>
    /// <exception cref="InvalidOperationException">The flash reached something other than a look, which its loop never does.</exception>
    private void ShowNextFlash()
    {
        AnimationShown<FirefleaFlashSegment, FirefleaFlashShade> shown =
            FirefleaFlashAnimations.Segments.PlayToNextFrame(FirefleaFlashAnimations.Flashing, ref place, static _ => { });
        if (shown is not AnimationShown<FirefleaFlashSegment, FirefleaFlashShade>.Frame look)
            throw new InvalidOperationException($"The Fireflea flash reached {shown}; it only shows looks.");
        Flash = look.Shown;
        UpdatesUntilNextFlash = look.Duration;
    }

    /// <summary>
    /// The grey of <paramref name="flash"/> deepened by <paramref name="darkness"/>: the two fade levels add, and their sum
    /// never passes the scale's full level.
    /// </summary>
    private static RgbColor Shade(FirefleaFlashShade flash, FirefleaDarkness darkness)
    {
        int level = flash.Level + darkness.Level;
        return new FadeLevelColour(level, level, level).ToColour();
    }
}

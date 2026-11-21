using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// What both Ridleys' bodies do to Samus and how they flash when hurt (#2229): his hand-authored extended body meets her
/// after the ordinary touch pass, and his body, tail and wings show the hurt flash.
/// </summary>
[RoomLifetime]
internal sealed partial class RidleyBodyContact
{
    /// <summary>Performs Samus's touch response against Ridley's extended body.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Builds the contact from the collaborators it uses.</summary>
    internal RidleyBodyContact(EnemyCollision collision)
    {
        _collision = collision;
    }

    /// <summary>
    /// Sets whether Ridley's body and his tail and wings show his hurt flash: his body on its phase of
    /// <paramref name="bodyUpdateCount"/>, his tail and wings on the phase one update ahead.
    /// </summary>
    internal static void UpdateHurtFlash(RoomEnemyPlace place, RidleyEncounterState ridley, int bodyUpdateCount) =>
        ridley.HurtFlashShows = RidleyHurtFlashShowing.Of(place.HurtFlash, bodyUpdateCount);

    /// <summary>Tests Samus against Ridley's extended body and applies contact response after invincibility gating.</summary>
    /// <param name="ridley">Ridley's body.</param>
    /// <param name="samus">Samus.</param>
    /// <param name="pseudoScrew">Samus's held charge, which her Pseudo Screw contact spends.</param>
    internal void ResolveSamusContact(RoomEnemyOccupant ridley, SamusState samus, PseudoScrewCharge pseudoScrew)
    {
        RoomEnemyPlace place = ridley.Place;
        Ensure.NotNull(samus);
        // Samus contact gates ordinary contact only on the contact-damage mode and Samus's invincibility timer, never on
        // whether her knockback is still running.
        if (!samus.InvincibilityTimer.IsZero)
            return;

        if (place.Frame is null ||
            place.Presence.HasAny(EnemyPresence.Hidden | EnemyPresence.Deleted) || place.Contact.HasAny(EnemyContact.IgnoresSamus))
        {
            return;
        }

        if (!RidleyInteractions.BodyOverlaps(place, EnemyHitboxProbe.Around(samus.Position.Whole, samus.Kinematics.Radius, HitboxContactKind.Touch)))
            return;

        // Ridley's touch enters the common touch response that does not check for death. That distinction matters for
        // Screw Attack: a lethal body touch leaves Lower Norfair Ridley alive long enough
        // to select the forced zero-health lunge and grab Samus for the authored death.
        _collision.ResolveOrdinaryTouch(
            ridley,
            samus,
            pseudoScrew,
            deathAnimation: TouchDeathAnimation.Skip);
    }
}

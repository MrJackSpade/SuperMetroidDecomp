using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// What both Ridleys' bodies do to Samus and how they flash when hurt (#2229): his hand-authored extended body meets her
/// after the ordinary touch pass, and his body, tail and wings show the hurt flash.
/// </summary>
[RoomLifetime]
internal sealed class RidleyBodyContact
{
    /// <summary>Performs Samus's touch response against Ridley's extended body.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Builds the contact from the collaborators it uses.</summary>
    internal RidleyBodyContact(EnemyCollision collision)
    {
        _collision = collision;
    }

    /// <summary>
    /// Runs Ridley's hurt flash and the common extended-enemy colour override. Ridley's composite is split between those two
    /// writers: the ordinary body uses the current execution counter, while the manual tail/wings deliberately test the
    /// counter after an increment.
    /// </summary>
    internal static void UpdateHurtFlash(RoomEnemyPlace place, RidleyEncounterState ridley, int enemyMainExecutionCounter)
    {
        ridley.BodyFlashing = place.FlashTimer != 0 && EnemyDrawPhaseDefinitions.HurtFlashShown.IsSetIn(enemyMainExecutionCounter);
        // His drawn parts take the phase one pass ahead of his body's colours.
        ridley.TailAndWingsFlashing = place.FlashTimer > 1 && EnemyDrawPhaseDefinitions.HurtFlashShown.IsSetIn(enemyMainExecutionCounter + 1);
    }

    /// <summary>Tests Samus against Ridley's extended body and applies contact response after invincibility gating.</summary>
    /// <param name="place">Ridley's body.</param>
    /// <param name="samus">Samus.</param>
    /// <param name="pseudoScrew">Samus's held charge, which her Pseudo Screw contact spends.</param>
    internal bool ResolveSamusContact(RoomEnemyPlace place, SamusState samus, PseudoScrewCharge pseudoScrew)
    {
        Ensure.NotNull(samus);
        // Samus contact gates ordinary contact only on the contact-damage mode and Samus's invincibility timer, never on
        // whether her knockback is still running.
        if (!samus.InvincibilityTimer.IsZero)
            return false;

        if (place.Frame is null ||
            place.Presence.HasAny(EnemyPresence.Hidden | EnemyPresence.Deleted) || place.Contact.HasAny(EnemyContact.IgnoresSamus))
        {
            return false;
        }

        if (!EnemyCollision.TryFindFrameHitboxReaction(
                place,
                samus.Position.Whole,
                samus.Kinematics.Radius,
                contactKind: HitboxContactKind.Touch,
                out EnemyReaction? bodyTouch))
        {
            return false;
        }

        if (bodyTouch != RidleyReactions.FrameHitboxTouch)
        {
            throw new InvalidDataException(
                $"Ridley extended body selected touch reaction {bodyTouch?.ToString() ?? "(unnamed)"}, expected " +
                $"{RidleyReactions.FrameHitboxTouch} from map {place.Frame}.");
        }

        // Ridley's touch enters the common touch response that does not check for death. That distinction matters for
        // Screw Attack: a lethal body touch leaves Lower Norfair Ridley alive long enough
        // to select the forced zero-health lunge and grab Samus for the authored death.
        _collision.ResolveNormalEnemyTouch(
            place,
            samus,
            pseudoScrew,
            deathAnimation: TouchDeathAnimation.Skip);
        return true;
    }
}

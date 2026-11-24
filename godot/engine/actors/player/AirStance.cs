using Engine.Collision;
using Engine.Input;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// What the states of the air share, the engine's and a game's own: the stance the player holds there, and the fall
/// that ends in a landing.
/// </summary>
public static class AirStance
{
    /// <summary>
    /// The stance the input asks for in the air, as a number that differs between stances: where the weapon points,
    /// and, aiming level, whether the player steers the way they face (where that makes a stance of its own) or holds
    /// the attack.
    /// </summary>
    public static int Of(Player owner, bool forwardCounts)
    {
        int level = 0;
        if (owner.Aim == Aim.Forward)
        {
            bool forward = owner.Intent != 0 && (owner.Intent > 0) == (owner.Facing == Facing.Right);
            level = forwardCounts && forward ? 1 : owner.Input.IsHeld(GameAction.Attack) ? 2 : 0;
        }

        return (int)owner.Aim * 3 + level;
    }

    /// <summary>
    /// The form of a player in the air: the form for aiming straight down while they do, where the game gives one,
    /// and otherwise the form named <paramref name="airForm"/>, or the upright form where the game gives none of that
    /// name.
    /// </summary>
    public static string FormFor(Player owner, string airForm) =>
        owner.Aim == Aim.Down && owner.Forms.ContainsKey(Player.AimingDown) ? Player.AimingDown : owner.FormOr(airForm);

    /// <summary>
    /// Whether the input asks nothing of a player in the air: no direction, no aim, no attack, and, where it counts,
    /// no jump.
    /// </summary>
    public static bool AsksNothing(Player owner, bool jumpCounts)
    {
        GameInput input = owner.Input;
        bool direction = input.IsHeld(GameAction.Left) || input.IsHeld(GameAction.Right);
        return !direction && !owner.AimIsHeld && !input.IsHeld(GameAction.Attack) && !(jumpCounts && input.IsHeld(GameAction.Jump));
    }

    /// <summary>
    /// Moves the player down under gravity and lands them when a fall is stopped by ground: their speed across comes
    /// to rest, they are steered, a held jump may count as pressed afresh, and they stand, settling for as long as the
    /// player's landings take unless <paramref name="settlingUpdates"/> says how long.
    /// </summary>
    public static PlayerState? FallAndLand(Player owner, int? settlingUpdates = null)
    {
        bool falling = !owner.Velocity.Y.IsNegative;
        MoveResult down = owner.MoveDownUnderGravity(keepsSpeedWhenStopped: true);
        if (falling && (down.Blocked || owner.IsOnGround))
        {
            owner.Land(owner.Velocity.Y);
            return new StandingState(settlingUpdates ?? owner.InMedium("landing_updates", owner.LandingUpdates));
        }

        // A ceiling ends a rise.
        if (down.Blocked)
            owner.FallFromRest();
        return null;
    }
}

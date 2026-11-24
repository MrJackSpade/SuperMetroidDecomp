using Engine.Shared;

namespace Engine.Actors;

/// <summary>Knocked back by a hit: thrown up and away for a moment, out of the player's control.</summary>
public sealed class KnockedBackState : PlayerState
{
    /// <summary>The way the player is thrown.</summary>
    private readonly Facing _toward;

    /// <summary>The updates left before the player has control again.</summary>
    private Countdown _left;

    /// <summary>A knockback throwing the player <paramref name="toward"/>.</summary>
    public KnockedBackState(Facing toward) => _toward = toward;

    /// <summary>A knockback throwing the player <paramref name="toward"/> for <paramref name="updates"/> updates, whatever knockbacks usually last.</summary>
    public KnockedBackState(Facing toward, int updates)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(updates);
        (_toward, _updates) = (toward, updates);
    }

    /// <summary>How long this knockback lasts, where that is not as long as the player's knockbacks usually do.</summary>
    private readonly int? _updates;

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.LoseMomentum();
        owner.Velocity = new PixelVelocity(Pixels.Zero, -Pixels.FromNumber(owner.Medium.KnockbackUpSpeed));
        _left = Countdown.Of(_updates ?? owner.KnockbackUpdates);
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        // The throw gains speed away from the hit, up to its top speed, whatever the player holds.
        MoveValues moves = owner.Moves(KnockedBackMoves);
        Pixels faster = owner.Velocity.X.Abs() + moves.Gain;
        owner.Velocity = owner.Velocity.WithX(_toward.Toward(faster > moves.Top ? moves.Top : faster));
        owner.MoveAcross();
        owner.MoveDown();
        owner.Fall();
        _left = _left.Stepped();
        if (_left.IsRunning)
            return null;
        owner.Velocity = owner.Velocity.WithX(Pixels.Zero);
        return owner.StateAtRest();
    }
}

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// What Ridley's tail is doing, as his fight steers it (#2553): each steered action (<see cref="RidleySteeredTailAction"/>)
/// steers the tail its own way each update, so the pogo's steps are reached only from a pogo action, and nothing steers a
/// resting tail.
/// </summary>
public abstract class RidleyTailAction
{
    /// <summary>The action's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>An action named <paramref name="name"/>.</summary>
    private protected RidleyTailAction(string name) => _name = name;

    /// <summary>Nothing steers the tail: it keeps its segments where they are.</summary>
    public static RidleyTailAction None { get; } = new RidleyRestingTailAction();

    /// <summary>The tail curls and whips freely behind Ridley in flight.</summary>
    public static RidleyTailAction Neutral { get; } = new RidleyNeutralTailAction();

    /// <summary>The tail bounces Ridley off the floor and whips at Samus.</summary>
    public static RidleyTailAction Pogo => RidleyPogoTailAction.Bouncing;

    /// <inheritdoc/>
    public override string ToString() => _name;
}

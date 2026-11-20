using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// What Ridley's tail is doing, as his fight steers it (#2553): each action steers the tail its own way each update, so the
/// pogo's steps are reached only from a pogo action.
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

    /// <summary>The tail swings down for the pogo, then points down.</summary>
    public static RidleyTailAction PogoSetup => RidleyPogoTailAction.Setup;

    /// <summary>The tail points straight down until its segments settle, then starts the pogo.</summary>
    public static RidleyTailAction PointDown => RidleyPogoTailAction.PointingDown;

    /// <summary>The tail bounces Ridley off the floor and whips at Samus.</summary>
    public static RidleyTailAction Pogo => RidleyPogoTailAction.Bouncing;

    /// <summary>The tail readies a downward stab at Samus as Ridley descends.</summary>
    public static RidleyTailAction StabSetup => RidleyPogoTailAction.ReadyingStab;

    /// <summary>The tail stabs straight down, slower than the setup's swing.</summary>
    public static RidleyTailAction Stab => RidleyPogoTailAction.Stabbing;

    /// <summary>Steers <paramref name="tail"/> for one update of Ridley's encounter.</summary>
    /// <param name="tail">Ridley's tail.</param>
    /// <param name="place">Ridley's body.</param>
    /// <param name="state">Ridley's state.</param>
    /// <param name="samus">Samus, whom the tail follows and whips at.</param>
    /// <param name="samusProjectiles">Samus's beams and missiles, a missile among which the Ceres whip may aim at instead.</param>
    internal abstract void Steer(RidleyTail tail, RoomEnemyPlace place, RidleyEncounterState state, SamusState samus, SamusProjectileSystem samusProjectiles);

    /// <inheritdoc/>
    public override string ToString() => _name;
}

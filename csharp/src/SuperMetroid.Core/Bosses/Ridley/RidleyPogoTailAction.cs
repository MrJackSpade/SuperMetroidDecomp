using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One step of Ridley's pogo tail (#2553): its setup's swing down, pointing down, the bouncing pogo, the readied stab and
/// the stab. Each step says whether it points the tail and how fast, whether it readies a stab, and the step that follows.
/// </summary>
public sealed partial class RidleyPogoTailAction : RidleySteeredTailAction
{
    /// <summary>The step after this one, from this step and how the tail has settled.</summary>
    private readonly Func<RidleyPogoTailAction, RidleyPogoTailSettling, RidleyPogoTailAction> _after;

    /// <summary>A pogo step.</summary>
    /// <param name="name">The step's name, for diagnostics.</param>
    /// <param name="pointedSwing">How fast the step points the tail down, or null for a step that does not.</param>
    /// <param name="after">The step after this one, from this step and how the tail has settled.</param>
    private RidleyPogoTailAction(string name, RidleyTailTurn? pointedSwing, Func<RidleyPogoTailAction, RidleyPogoTailSettling, RidleyPogoTailAction> after)
        : base(name)
    {
        PointedSwing = pointedSwing;
        _after = after;
    }

    /// <summary>Builds the steps, each after the steps it goes on to.</summary>
    static RidleyPogoTailAction()
    {
        Stabbing = new(nameof(Stab), RidleyTailDefinitions.StabSwing, static (self, _) => self);
        RidleyPogoTailAction stabbing = Stabbing;
        ReadyingStab = new(nameof(StabSetup), pointedSwing: null,
            (self, settling) => settling.RestsWhileFalling ? stabbing : self);
        RidleyPogoTailAction readyingStab = ReadyingStab;
        Bouncing = new(nameof(Pogo), pointedSwing: null,
            (self, settling) => settling.RestsWhileFalling ? readyingStab : self);
        RidleyPogoTailAction bouncing = Bouncing;
        PointingDown = new(nameof(PointDown), RidleyTailDefinitions.WhipSwing,
            (self, settling) => settling.LinkMoves ? self : bouncing);
        RidleyPogoTailAction pointingDown = PointingDown;
        Setup = new(nameof(PogoSetup), RidleyTailDefinitions.WhipSwing, (_, _) => pointingDown);
    }

    /// <summary>The tail swings down for the pogo, then points down.</summary>
    internal static RidleyPogoTailAction Setup { get; }

    /// <summary>The tail points straight down until its segments settle, then starts the pogo.</summary>
    internal static RidleyPogoTailAction PointingDown { get; }

    /// <summary>The tail bounces Ridley off the floor and whips at Samus, readying its stab once its links rest while he falls.</summary>
    internal static RidleyPogoTailAction Bouncing { get; }

    /// <summary>The tail readies a downward stab at Samus as Ridley descends, stabbing once its links rest while he falls.</summary>
    internal static RidleyPogoTailAction ReadyingStab { get; }

    /// <summary>The tail stabs straight down, slower than the setup's swing, and keeps stabbing.</summary>
    internal static RidleyPogoTailAction Stabbing { get; }

    /// <summary>How fast the step points the tail down, or null for a step that does not point it.</summary>
    internal RidleyTailTurn? PointedSwing { get; }

    /// <summary>Whether the step readies the stab, extending the tail once its links rest while Ridley falls.</summary>
    internal bool ReadiesStab => this == ReadyingStab;

    /// <summary>The step after this one, once the tail has settled as <paramref name="settling"/> says.</summary>
    internal RidleyPogoTailAction After(RidleyPogoTailSettling settling) => _after(this, settling);

    /// <inheritdoc/>
    internal override void Steer(RidleyTail tail, RoomEnemyPlace place, RidleyEncounterState state, SamusState samus, SamusProjectileSystem samusProjectiles) =>
        Ensure.NotNull(tail).TickRidleyPogoTail(place, state, samus, this);
}

using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One step of Ridley's pogo tail (#2553): its setup's swing down, pointing down, the bouncing pogo, the readied stab and
/// the stab. Each step says whether it points the tail and how fast, whether it readies a stab, and the step that follows.
/// </summary>
public sealed class RidleyPogoTailAction : RidleyTailAction
{
    /// <summary>
    /// The step after this one, from this step, whether a link still moves and whether the links rest while Ridley falls.
    /// </summary>
    private readonly Func<RidleyPogoTailAction, bool, bool, RidleyPogoTailAction> _after;

    /// <summary>A pogo step.</summary>
    /// <param name="name">The step's name, for diagnostics.</param>
    /// <param name="pointedSwing">How fast the step points the tail down, or null for a step that does not.</param>
    /// <param name="readiesStab">Whether the step readies the stab, extending the tail once its links rest while Ridley falls.</param>
    /// <param name="after">
    /// The step after this one, from this step, whether a link still moves and whether the links rest while Ridley falls.
    /// </param>
    private RidleyPogoTailAction(string name, RidleyTailTurn? pointedSwing, bool readiesStab, Func<RidleyPogoTailAction, bool, bool, RidleyPogoTailAction> after)
        : base(name)
    {
        PointedSwing = pointedSwing;
        ReadiesStab = readiesStab;
        _after = after;
    }

    /// <summary>Builds the steps, each after the steps it goes on to.</summary>
    static RidleyPogoTailAction()
    {
        Stabbing = new(nameof(Stab), RidleyTailDefinitions.StabSwing, readiesStab: false, static (self, _, _) => self);
        RidleyPogoTailAction stabbing = Stabbing;
        ReadyingStab = new(nameof(StabSetup), pointedSwing: null, readiesStab: true,
            (self, _, restsWhileFalling) => restsWhileFalling ? stabbing : self);
        RidleyPogoTailAction readyingStab = ReadyingStab;
        Bouncing = new(nameof(Pogo), pointedSwing: null, readiesStab: false,
            (self, _, restsWhileFalling) => restsWhileFalling ? readyingStab : self);
        RidleyPogoTailAction bouncing = Bouncing;
        PointingDown = new(nameof(PointDown), RidleyTailDefinitions.WhipSwing, readiesStab: false,
            (self, linkMoves, _) => linkMoves ? self : bouncing);
        RidleyPogoTailAction pointingDown = PointingDown;
        Setup = new(nameof(PogoSetup), RidleyTailDefinitions.WhipSwing, readiesStab: false, (_, _, _) => pointingDown);
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
    internal bool ReadiesStab { get; }

    /// <summary>The step after this one.</summary>
    /// <param name="linkMoves">Whether a link of the tail still moves after this update.</param>
    /// <param name="restsWhileFalling">Whether every link rests while Ridley falls or hangs still.</param>
    internal RidleyPogoTailAction After(bool linkMoves, bool restsWhileFalling) => _after(this, linkMoves, restsWhileFalling);

    /// <inheritdoc/>
    internal override void Steer(RidleyTail tail, RoomEnemyPlace place, RidleyEncounterState state, SamusState samus, SamusProjectileSystem samusProjectiles) =>
        Ensure.NotNull(tail).TickRidleyPogoTail(place, state, samus, this);
}

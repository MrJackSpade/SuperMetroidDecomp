using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>The Fireflea species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class FirefleaBehavior : IEnemySpecies, IEnemySpawnFactory<FirefleaSpawnSettings>
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Fireflea];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, FirefleaSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Running(new FirefleaEntity(this, occupant, place, common, settings));

    /// <summary>Runs the common touch, after which the Fireflea always dies.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Starts explosion and drop processing, including the second-death path.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Holds the shared darkness level that advances when Firefleas die.</summary>
    private readonly EnemyRoomStatus _roomStatus;

    /// <summary>Connects Fireflea motion and collision behaviours to room death and darkness state.</summary>
    internal FirefleaBehavior(EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops, EnemyRoomStatus roomStatus)
    {
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
        _roomStatus = roomStatus;
    }

    /// <summary>Initializes the Fireflea from its flight and radius.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static FirefleaEnemyState InitializeFireflea(FirefleaSpawnSettings settings, RoomEnemyPlace place)
    {
        FirefleaFlightState flight = settings.Flight.Match<FirefleaFlightState>(
            bobbing => StartBobbing(place, bobbing, settings.Radius),
            circling => StartCircling(place, circling, settings.Radius));
        var state = new FirefleaEnemyState(flight)
        {
            Playback = new(FirefleaAnimations.StartingWith(FirefleaSegment.BrightestWingbeat), AnimationPlace.StartOfAnimation),
        };
        return state;
    }

    /// <summary>Starts a circling Fireflea on its circle about where it spawned, at the circle's zero angle.</summary>
    private static CirclingFirefleaState StartCircling(RoomEnemyPlace place, FirefleaFlight.Circling circling, int radius)
    {
        var flight = new CirclingFirefleaState(circling)
        {
            Center = place.Position.Whole,
            Angle = RotationAngle.Zero,
        };
        place.Position = place.Position.WithWhole(flight.Center + EnemyMath.CircleOffset(flight.Angle.ToTrigonometryAngle(), radius));
        return flight;
    }

    /// <summary>Starts a bobbing Fireflea between the rows <paramref name="radius"/> above and below where it spawned.</summary>
    private static BobbingFirefleaState StartBobbing(RoomEnemyPlace place, FirefleaFlight.Bobbing bobbing, int radius)
    {
        int spawnY = place.Position.Y.Whole;
        return new BobbingFirefleaState(bobbing)
        {
            Direction = bobbing.StartDirection,
            MinimumY = spawnY - radius,
            MaximumY = spawnY + radius,
        };
    }

    /// <summary>Runs the Fireflea's main AI, advancing circular angle or vertical travel.</summary>
    private static void RunFirefleaMain(FirefleaSpawnSettings firefleaSpawnSettings, RoomEnemyPlace place, FirefleaEnemyState state)
    {
        state.Flight.React(RunFirefleaMainReactions, new FirefleaMainContext(firefleaSpawnSettings, place));
    }

    /// <summary>What <see cref="RunFirefleaMain"/> does on each <see cref="FirefleaFlightState"/>, built once (#2553).</summary>
    private static FirefleaFlightStateReactions<FirefleaMainContext> RunFirefleaMainReactions { get; } = new(
        BobbingFirefleaState: static (bobbing, context) =>
        {
            RoomEnemyPlace place = context.Place;
            place.Position = place.Position.AdvanceY(bobbing.Velocity);

            // The Fireflea overshoots a bound before its way reverses.
            bool crossedMinimum = place.Position.Y.Whole < bobbing.MinimumY;
            bool reachedMaximum = place.Position.Y.Whole >= bobbing.MaximumY;
            if (crossedMinimum || reachedMaximum)
                bobbing.Direction = bobbing.Direction.Reversed();
        },
        CirclingFirefleaState: static (circling, context) =>
        {
            RoomEnemyPlace place = context.Place;
            place.Position = place.Position.WithWhole(circling.Center
                + EnemyMath.CircleOffset(circling.Angle.ToTrigonometryAngle(), context.Settings.Radius));
            circling.Angle = circling.Angle.TurnedBy(circling.Definition.TurnPerUpdate);
        });

    /// <summary>What the flight reactions read besides the flight: the Fireflea's settings and its place.</summary>
    /// <param name="Settings">The settings its placement gives it.</param>
    /// <param name="Place">The place it moves.</param>
    private readonly record struct FirefleaMainContext(FirefleaSpawnSettings Settings, RoomEnemyPlace Place);

    /// <summary>
    /// Runs the Fireflea's reaction to Samus's touch, including its double death. The
    /// own touch reaction runs the common touch reaction and then kills the Fireflea unconditionally:
    /// normal contact therefore kills an otherwise healthy Fireflea, while Screw Attack and
    /// similar contact attacks can increment the room kill count twice.
    /// </summary>
    internal void ResolveFirefleaTouch(
        RoomEnemyOccupant fireflea,
        SamusState samus,
        PseudoScrewCharge pseudoScrew)
    {
        RoomEnemyPlace place = fireflea.Place;
        _collision.ResolveOrdinaryTouch(fireflea, samus, pseudoScrew);

        // The touch kills the Fireflea whatever it left, in the explosion the health the touch left names
        // (LeftoverNumberDeathExplosion). Queue that death animation before clearing the actor so its artwork and sound
        // can run. When common attacking contact already ran the death, the Fireflea's touch reaction runs it again on
        // the emptied place, whose cleared health names the small explosion: the second explosion appears at the room's
        // top-left corner for whatever the place now holds (nothing, or the respawn placeholder), clears that placeholder,
        // and counts a second kill.
        _deathsAndDrops.StartGenericEnemyDeath(fireflea,
            EnemyDeathExplosionDefinitions.LeftoverNumbered.Named(place.Health), samus);
        AdvanceFirefleaDarkness();
    }

    /// <summary>
    /// Deepens the room's Fireflea darkness after one Fireflea death (<see cref="EnemyRoomStatus.RecordFirefleaDeath"/>).
    /// Rooms contain at most five, so the darkness usually stops at its fifth step; a sixth step is nevertheless permitted,
    /// and the darkness stops there.
    /// </summary>
    private void AdvanceFirefleaDarkness() => _roomStatus.RecordFirefleaDeath();

    /// <summary>Advances the room's darkness after a bomb's common damage kills a Fireflea.</summary>
    /// <param name="c">The bomb's hit.</param>
    private void AdvanceFirefleaDarknessOnLethalBomb(EnemyBombHitContext c)
    {
        if (c.Enemy.Health == 0)
            AdvanceFirefleaDarkness();
    }

    /// <summary>Advances the room's darkness before a lethal Power Bomb's common death.</summary>
    private void AdvanceFirefleaDarkness(EnemyPowerBombContext context) => _roomStatus.RecordFirefleaDeath();

    /// <summary>Advances the room's darkness before a lethal shot's common death.</summary>
    private void AdvanceFirefleaDarkness(EnemyShotHitContext context) => _roomStatus.RecordFirefleaDeath();

    /// <summary>Advances darkness after common bomb damage kills a Fireflea.</summary>
    internal EnemyBombReaction FirefleaBombReaction => field ??= new()
    {
        AfterCommon = AdvanceFirefleaDarknessOnLethalBomb,
    };

    /// <summary>Advances Fireflea's typed animation.</summary>
    internal static void AdvanceFirefleaTypedAnimation(RoomEnemyPlace place, FirefleaEnemyState firefleaEnemyState) =>
        EnemyAnimationPlayback.AdvanceSilently(place, firefleaEnemyState, FirefleaAnimations.Segments,
            static (FirefleaEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback);

    /// <summary>Runs Fireflea's main AI.</summary>
    internal static void RunFirefleaMainAi(FirefleaSpawnSettings firefleaSpawnSettings, RoomEnemyPlace place, FirefleaEnemyState firefleaEnemyState)
    {
        RunFirefleaMain(firefleaSpawnSettings, place, firefleaEnemyState);
    }

    /// <summary>Advances darkness before a lethal common power-bomb death is finalized.</summary>
    internal EnemyPowerBombReaction FirefleaPowerBombReaction => field ??= new()
    {
        // Reactions around common power-bomb damage.
        Lethal = new EnemyPowerBombLethal.AfterStage(AdvanceFirefleaDarkness),
    };

    /// <summary>Advances darkness before a lethal common projectile death is finalized.</summary>
    internal EnemyShotReaction FirefleaShotReaction => field ??= new()
    {
        Lethal = new EnemyShotLethal.AfterStage(AdvanceFirefleaDarkness),
    };

    /// <summary>Runs Fireflea's touch reaction, the unconditional-death sequence.</summary>
    internal void ReactToFirefleaTouch(EnemyTouchContext context) =>
        ResolveFirefleaTouch(context.Occupant, context.Samus, context.PseudoScrew);
}

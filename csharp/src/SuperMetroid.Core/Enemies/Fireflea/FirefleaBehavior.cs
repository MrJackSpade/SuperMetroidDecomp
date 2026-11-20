using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>The Fireflea species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class FirefleaBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Fireflea];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new FirefleaEntity(this, place, common));

    /// <summary>Runs common touch handling before Fireflea's unconditional death call.</summary>
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
    internal static void InitializeFireflea(RoomEnemyPlace place)
    {
        FirefleaSpawnSettings settings = place.SpawnSettings<FirefleaSpawnSettings>();
        FirefleaFlightState flight = settings.Flight.Match<FirefleaFlightState>(
            bobbing => StartBobbing(place, bobbing, settings.Radius),
            circling => StartCircling(place, circling, settings.Radius));
        place.SetSpeciesState(new FirefleaEnemyState(flight)
        {
            Playback = new(FirefleaAnimations.StartingWith(FirefleaSegment.BrightestWingbeat), AnimationPlace.StartOfAnimation),
        });
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
    private static void RunFirefleaMain(RoomEnemyPlace place, FirefleaEnemyState state)
    {
        switch (state.Flight)
        {
            case CirclingFirefleaState circling:
                place.Position = place.Position.WithWhole(circling.Center
                    + EnemyMath.CircleOffset(circling.Angle.ToTrigonometryAngle(), place.SpawnSettings<FirefleaSpawnSettings>().Radius));
                circling.Angle = circling.Angle.TurnedBy(circling.Definition.TurnPerUpdate);
                return;

            case BobbingFirefleaState bobbing:
                place.Position = place.Position.AdvanceY(bobbing.Velocity);

                // The Fireflea overshoots a bound before its way reverses.
                bool crossedMinimum = place.Position.Y.Whole < bobbing.MinimumY;
                bool reachedMaximum = place.Position.Y.Whole >= bobbing.MaximumY;
                if (crossedMinimum || reachedMaximum)
                    bobbing.Direction = bobbing.Direction.Reversed();
                return;
        }
    }

    /// <summary>
    /// Runs the Fireflea's reaction to Samus's touch, including its double death. The
    /// own touch reaction runs the common touch reaction and then kills the Fireflea unconditionally:
    /// normal contact therefore kills an otherwise healthy Fireflea, while Screw Attack and
    /// similar contact attacks can increment the room kill count twice.
    /// </summary>
    private void ResolveFirefleaTouch(
        RoomEnemyPlace place,
        SamusState samus,
        PseudoScrewCharge pseudoScrew)
    {
        _collision.ResolveOrdinaryTouch(place, samus, pseudoScrew);

        // The touch kills the Fireflea whatever it left, in the explosion the health the touch left names
        // (LeftoverNumberDeathExplosion). Queue that death animation before clearing the actor so its artwork and sound
        // can run. When common attacking contact already ran the death, the Fireflea's touch reaction runs it again on
        // the emptied place, whose cleared health names the small explosion: the second explosion appears at the room's
        // top-left corner for whatever the place now holds (nothing, or the respawn placeholder), clears that placeholder,
        // and counts a second kill.
        _deathsAndDrops.StartGenericEnemyDeath(place,
            EnemyDeathExplosionDefinitions.LeftoverNumbered.Named(place.Health), samus);
        AdvanceFirefleaDarkness();
    }

    /// <summary>
    /// Deepens the room's Fireflea darkness after one Fireflea death (<see cref="EnemyRoomStatus.RecordFirefleaDeath"/>).
    /// Rooms contain at most five, so the darkness usually stops at its fifth step; a sixth step is nevertheless permitted,
    /// and the darkness stops there.
    /// </summary>
    private void AdvanceFirefleaDarkness() => _roomStatus.RecordFirefleaDeath();

    /// <summary>Advances darkness after common bomb damage kills a Fireflea.</summary>
    internal EnemyBombReaction FirefleaBombReaction => field ??= new()
    {
        AfterCommon = c =>
        {
            if (c.Enemy.Health == 0)
                AdvanceFirefleaDarkness();
        },
    };

    /// <summary>Advances Fireflea's typed animation.</summary>
    internal static void AdvanceFirefleaTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.AdvanceSilently(place, FirefleaAnimations.Segments,
            static (FirefleaEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback);

    /// <summary>Runs Fireflea's main AI.</summary>
    internal static void RunFirefleaMainAi(RoomEnemyPlace place)
    {
        RunFirefleaMain(place, place.RequireSpeciesState<FirefleaEnemyState>());
    }

    /// <summary>Advances darkness before a lethal common power-bomb death is finalized.</summary>
    internal EnemyPowerBombReaction FirefleaPowerBombReaction => field ??= new()
    {
        // Reactions around common power-bomb damage.
        BeforeGenericDeath = _ => AdvanceFirefleaDarkness(),
    };

    /// <summary>Advances darkness before a lethal common projectile death is finalized.</summary>
    internal EnemyShotReaction FirefleaShotReaction => field ??= new()
    {
        BeforeGenericDeath = _ => AdvanceFirefleaDarkness(),
    };

    /// <summary>Runs Fireflea's touch reaction, the unconditional-death sequence.</summary>
    internal void ReactToFirefleaTouch(EnemyTouchContext context) =>
        ResolveFirefleaTouch(context.Place, context.Samus, context.PseudoScrew);
}

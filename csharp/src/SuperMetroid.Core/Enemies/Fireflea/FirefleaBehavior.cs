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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new FirefleaEntity(this, slot, common);

    /// <summary>Runs common touch handling before Fireflea's unconditional death call.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Starts explosion and drop processing, including the retail second-death path.</summary>
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
    internal static void InitializeFireflea(RoomEnemySlot slot)
    {
        var state = new FirefleaEnemyState(slot);
        slot.SetSpeciesState(state);
        state.Playback = new(FirefleaAnimations.StartingWith(FirefleaSegment.Loop), AnimationPlace.StartOfAnimation);

        FirefleaSpawnSettings settings = slot.SpawnSettings<FirefleaSpawnSettings>();
        state.Radius = settings.Radius;

        switch (settings.Flight)
        {
            case FirefleaFlight.Circling circling:
                state.AngularSpeed = circling.TurnPerUpdate;
                state.Center = slot.Position.Whole;
                state.Angle = RotationAngle.Zero;
                slot.Position = slot.Position.WithWhole(state.Center + EnemyMath.CircleOffset(state.Angle.ToTrigonometryAngle(), state.Radius));
                return;
            case FirefleaFlight.Bobbing bobbing:
                state.Speed = bobbing.Speed;
                state.SpeedSign = bobbing.StartSign;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(FirefleaFlight)} {settings.Flight}.");
        }

        int spawnY = slot.Position.Y.Whole;
        state.MinimumY = CoordinateAxis.Wrap(spawnY - state.Radius);
        state.MaximumY = CoordinateAxis.Wrap(spawnY + state.Radius);
    }

    /// <summary>Runs the Fireflea's main AI, advancing circular angle or wrapped vertical travel.</summary>
    private static void RunFirefleaMain(RoomEnemySlot slot, FirefleaEnemyState state)
    {
        if (state.UsesCircularMovement)
        {
            slot.Position = slot.Position.WithWhole(state.Center + EnemyMath.CircleOffset(state.Angle.ToTrigonometryAngle(), state.Radius));

            state.Angle += state.AngularSpeed;
            return;
        }

        slot.Position = slot.Position.AdvanceY(state.Velocity);

        // The bounds compare by whether the wrapped subtraction is negative, not by ordinary
        // bounds comparisons. The actor is intentionally allowed to overshoot before its
        // speed reverses.
        bool crossedMinimum = CoordinateAxis.Precedes(slot.Position.Y.Whole, state.MinimumY);
        bool reachedMaximum = !CoordinateAxis.Precedes(slot.Position.Y.Whole, state.MaximumY);
        if (crossedMinimum || reachedMaximum)
            state.SpeedSign = Reversed(state.SpeedSign);
    }

    /// <summary>The sign opposite <paramref name="sign"/>.</summary>
    private static EnemySpeedSign Reversed(EnemySpeedSign sign) => sign switch
    {
        EnemySpeedSign.Positive => EnemySpeedSign.Negative,
        EnemySpeedSign.Negative => EnemySpeedSign.Positive,
        _ => throw new ArgumentOutOfRangeException(nameof(sign), sign, "Undefined enemy speed sign."),
    };

    /// <summary>
    /// Runs Fireflea touch AI, including its retail double-death bug. The
    /// own touch reaction runs the common touch reaction and then kills the Fireflea unconditionally:
    /// normal contact therefore kills an otherwise healthy Fireflea, while Screw Attack and
    /// similar contact attacks can increment the room kill count twice.
    /// </summary>
    internal void ResolveFirefleaTouch(
        RoomEnemySlot slot,
        SamusState samus)
    {
        _collision.ResolveNormalEnemyTouch(slot, samus);

        // The touch kills the Fireflea whatever it left, in the explosion FirefleaTouchDeathExplosion names. Queue that
        // death animation before clearing the actor so its artwork and sound can run.
        if (!slot.IsFree)
            _deathsAndDrops.StartGenericEnemyDeath(slot, deathAnimation:
                FirefleaTouchDeathExplosion.AfterSurvivedTouch(slot.Health, EnemyDeathExplosionDefinitions.AuthoredOrder));
        else
        {
            // Common attacking contact already ran the death; the Fireflea's touch reaction runs it again on the emptied
            // place. The second explosion appears at the room's top-left corner for whatever the place now holds (nothing,
            // or the respawn placeholder), clears that placeholder, and counts a second kill.
            _deathsAndDrops.StartGenericEnemyDeath(slot, deathAnimation:
                FirefleaTouchDeathExplosion.AfterKillingTouch(EnemyDeathExplosionDefinitions.AuthoredOrder));
        }
        AdvanceFirefleaDarkness();
    }

    /// <summary>
    /// Advances the room's Fireflea darkness by one step after one Fireflea death. Retail rooms contain at most
    /// five, so the darkness usually stops at its fifth step; a sixth step is nevertheless permitted, and the darkness
    /// stops there.
    /// </summary>
    private void AdvanceFirefleaDarkness()
    {
        if (_roomStatus.FirefleaDarknessStep < EnemyRoomStatus.MaximumFirefleaDarknessStep)
            _roomStatus.FirefleaDarknessStep++;
    }

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
    internal static void AdvanceFirefleaTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, FirefleaAnimations.Segments,
            static (FirefleaEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Fireflea animation raised {cue}."));

    /// <summary>Runs Fireflea's main AI.</summary>
    internal static void RunFirefleaMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunFirefleaMain(slot, slot.RequireSpeciesState<FirefleaEnemyState>());
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

    /// <summary>Runs Fireflea's touch reaction, the retail unconditional-death sequence.</summary>
    internal void ReactToFirefleaTouch(EnemyTouchContext context) =>
        ResolveFirefleaTouch(context.Slot, context.Samus);
}

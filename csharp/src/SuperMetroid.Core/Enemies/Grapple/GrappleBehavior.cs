using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Enemies.Grapple;

/// <summary>The Grapple species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class GrappleBehavior : IGrappleTargets
{
    /// <summary>Provides the interactive enemies in order and runs their main AI under the grapple's frame.</summary>
    private readonly IEnemyLifecycle _lifecycle;
    /// <summary>Guards endpoint checks until a room is loaded.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Starts the full generic death and drop sequence for grapple-killed enemies.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Binds enemy ordering, room state and death handling.</summary>
    internal GrappleBehavior(IEnemyLifecycle lifecycle, EnemyRoomBinding room, EnemyDeathsAndDrops deathsAndDrops)
    {
        _lifecycle = lifecycle;
        _room = room;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>
    /// Resolves the grapple beam's endpoint against enemies. Extended hitboxes are
    /// intentionally ignored: the beam compares the endpoint with each ordinary radius
    /// plus an eight-pixel beam-point radius and selects the first interactive slot.
    /// </summary>
    public GrappleEnemyCollision ResolveGrappleEndpoint(Position16<RoomPixels> endpoint)
    {
        _room.EnsureLoaded();
        foreach (RoomEnemySlot enemy in _lifecycle.InteractiveEnemies)
        {
            if (enemy.InvincibilityTimer != 0)
                continue;

            int xDistance = CoordinateAxis.Distance(enemy.Position.X.Whole, endpoint.X);
            int yDistance = CoordinateAxis.Distance(enemy.Position.Y.Whole, endpoint.Y);
            if (xDistance >= enemy.Radius.X + 8 || yDistance >= enemy.Radius.Y + 8)
                continue;

            // The collision stores exactly one, rather than combining it with an old
            // hurt/frozen handler. The next enemy pass therefore dispatches grapple AI.
            enemy.AiStates = EnemyAiStates.Grappled;
            GrappleEnemyReaction reaction = enemy.Definition.GrappleResponse switch
            {
                EnemyGrappleResponse.NoInteraction => GrappleEnemyReaction.None,
                EnemyGrappleResponse.Attach => GrappleEnemyReaction.Attach,
                EnemyGrappleResponse.Kill => GrappleEnemyReaction.Kill,
                EnemyGrappleResponse.Cancel => GrappleEnemyReaction.Cancel,
                EnemyGrappleResponse.AttachWithoutInvincibility =>
                    GrappleEnemyReaction.AttachWithoutInvincibility,
                EnemyGrappleResponse.AttachAndParalyze => GrappleEnemyReaction.AttachAndParalyze,
                EnemyGrappleResponse.HurtSamus => GrappleEnemyReaction.HurtSamus,
                EnemyGrappleResponse.None or EnemyGrappleResponse.SpeciesOwn or _ => throw new InvalidDataException(
                    $"Enemy {enemy.DefinitionId} grapple response {enemy.Definition.GrappleResponse} " +
                    "is not one of the translated common reactions."),
            };
            return new GrappleEnemyCollision(
                reaction,
                enemy.Position.Whole,
                enemy.Definition.Damage);
        }

        return default;
    }

    /// <summary>
    /// Executes the common grapple AI the grappled state selects. Returning true tells the
    /// ordinary scheduler that this call replaced main AI for the current enemy frame.
    /// </summary>
    internal bool RunCommonGrappleAi(RoomEnemySlot enemy, IEnemyAiFrame frame)
    {
        if ((enemy.AiStates & EnemyAiStates.Grappled) == EnemyAiStates.None)
            return false;

        switch (enemy.Definition.GrappleResponse)
        {
            case EnemyGrappleResponse.NoInteraction:
                enemy.AiStates = EnemyAiStates.None;
                enemy.InvincibilityTimer = 0;
                enemy.FrozenTimer = 0;
                enemy.ShakeTimer = 0;
                return true;

            case EnemyGrappleResponse.Attach:
                if (enemy.FrozenTimer != 0)
                {
                    enemy.AiStates = EnemyAiStates.Frozen;
                }
                else
                {
                    enemy.FlashTimer = enemy.HurtFlashFrames;
                    enemy.AiStates = EnemyAiStates.None;
                }
                return true;

            case EnemyGrappleResponse.Kill:
                // A grapple kill always ends in the small explosion. The shared death keeps the enemy's position, its drop
                // and any respawn marker; deleting the enemy outright would skip the explosion.
                _deathsAndDrops.StartGenericEnemyDeath(enemy, deathAnimation: EnemyDeathExplosion.Small);
                enemy.AiStates = EnemyAiStates.None;
                return true;

            case EnemyGrappleResponse.Cancel:
            case EnemyGrappleResponse.HurtSamus:
                enemy.AiStates = EnemyAiStates.Frozen;
                return true;

            case EnemyGrappleResponse.AttachWithoutInvincibility:
                if (enemy.FrozenTimer != 0)
                {
                    enemy.AiStates = EnemyAiStates.Frozen;
                    return true;
                }

                // This common wrapper is Powamp's crucial distinction: it invokes the
                // enemy's normal main behaviour while the grappled state remains observable, then
                // clears that state after the actor has moved its grapple anchor.
                _lifecycle.RunMainAi(enemy, new GrappleWrappedAiFrame(frame));
                enemy.AiStates = EnemyAiStates.None;
                return true;

            case EnemyGrappleResponse.AttachAndParalyze:
                enemy.FlashTimer = enemy.HurtFlashFrames;
                enemy.AiStates = EnemyAiStates.None;
                return true;

            case EnemyGrappleResponse.None:
            case EnemyGrappleResponse.SpeciesOwn:
            default:
                throw new InvalidDataException(
                    $"Enemy {enemy.DefinitionId} has no response to grapple response {enemy.Definition.GrappleResponse}.");
        }
    }

    /// <summary>
    /// The frame the common grapple wrapper hands to main AI: the enemy's own camera, Samus,
    /// input and room, but no Samus projectiles, turning-floor transform, shared bombs or artwork changes,
    /// exactly as the wrapper's main-AI call has always received them (#633).
    /// </summary>
    internal sealed class GrappleWrappedAiFrame(IEnemyAiFrame frame) : IEnemyAiFrame
    {
        /// <summary>Camera inherited from the current enemy update.</summary>
        public Position16<RoomPixels> Camera => frame.Camera;
        /// <summary>Samus state visible to the wrapped main AI.</summary>
        public SamusState? Samus => frame.Samus;
        /// <summary>New input edges inherited from the current update.</summary>
        public ControllerButton NewlyPressedControllerInput => frame.NewlyPressedControllerInput;
        /// <summary>Held buttons inherited from the current update.</summary>
        public ControllerButton ControllerInput => frame.ControllerInput;
        /// <summary>Current room collision data visible to wrapped main AI.</summary>
        public RoomLevelData Level => frame.Level;
        /// <summary>Unavailable projectile service in the common grapple wrapper.</summary>
        public SamusProjectileSystem? SamusProjectiles => null;
        /// <summary>Unavailable turning-floor transform in the common grapple wrapper.</summary>
        public TurningFloorTransform? TurningFloor => null;
        /// <summary>Unavailable shared bomb service in the common grapple wrapper.</summary>
        public SamusBombProjectileSystem? SharedProjectiles => null;

        /// <inheritdoc/>
        public FrameCounters Clock => frame.Clock;
    }
}

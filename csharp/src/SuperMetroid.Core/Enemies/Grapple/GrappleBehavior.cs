using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Enemies.Grapple;

/// <summary>The Grapple species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class GrappleBehavior : IGrappleTargets
{
    /// <summary>Provides the interactive enemies in order and runs their main AI under the grapple's frame.</summary>
    private readonly IEnemyLifecycle _lifecycle;
    /// <summary>Starts the full generic death and drop sequence for grapple-killed enemies.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Binds enemy ordering and death handling.</summary>
    internal GrappleBehavior(IEnemyLifecycle lifecycle, EnemyDeathsAndDrops deathsAndDrops)
    {
        _lifecycle = lifecycle;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>
    /// Resolves the grapple beam's endpoint against enemies. Extended hitboxes are
    /// intentionally ignored: the beam compares the endpoint with each ordinary radius
    /// plus the beam point's radius and selects the first interactive place.
    /// </summary>
    public GrappleEnemyCollision ResolveGrappleEndpoint(Position16<RoomPixels> endpoint)
    {
        foreach (RoomEnemyPlace enemy in _lifecycle.InteractiveEnemies)
        {
            if (enemy.InvincibilityTimer != 0)
                continue;

            int xDistance = CoordinateAxis.Distance(enemy.Position.X.Whole, endpoint.X);
            int yDistance = CoordinateAxis.Distance(enemy.Position.Y.Whole, endpoint.Y);
            if (xDistance >= enemy.Radius.X + GrappleTargetDefinitions.BeamPointRadius || yDistance >= enemy.Radius.Y + GrappleTargetDefinitions.BeamPointRadius)
                continue;

            // The collision stores exactly one, rather than combining it with an old
            // hurt or frozen state. The next enemy pass therefore runs grapple AI.
            enemy.AiStates = EnemyAiStates.Grappled;
            GrappleEnemyReaction reaction = enemy.Definition.GrappleResponse ?? throw new InvalidDataException(
                $"Enemy {enemy.DefinitionId} names no reaction to the grapple beam.");
            return new GrappleEnemyCollision(
                reaction,
                enemy.Position.Whole,
                enemy.Definition.Damage);
        }

        return default;
    }

    /// <summary>
    /// Executes the common grapple AI the grappled state selects. Returning true tells the
    /// ordinary scheduler that this update replaced main AI for the current enemy frame.
    /// </summary>
    internal bool RunCommonGrappleAi(RoomEnemyPlace enemy, IEnemyAiFrame frame)
    {
        if ((enemy.AiStates & EnemyAiStates.Grappled) == EnemyAiStates.None)
            return false;

        switch (enemy.Definition.GrappleResponse)
        {
            case GrappleEnemyReaction.None:
                enemy.AiStates = EnemyAiStates.None;
                enemy.InvincibilityTimer = 0;
                enemy.FrozenTimer = 0;
                enemy.ShakeTimer = 0;
                return true;

            case GrappleEnemyReaction.Attach:
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

            case GrappleEnemyReaction.Kill:
                // A grapple kill always ends in the small explosion. The shared death keeps the enemy's position, its drop
                // and any respawn marker; deleting the enemy outright would skip the explosion.
                _deathsAndDrops.StartGenericEnemyDeath(enemy, EnemyDeathExplosion.Small, frame.Samus);
                enemy.AiStates = EnemyAiStates.None;
                return true;

            case GrappleEnemyReaction.Cancel:
            case GrappleEnemyReaction.HurtSamus:
                enemy.AiStates = EnemyAiStates.Frozen;
                return true;

            case GrappleEnemyReaction.AttachWithoutInvincibility:
                if (enemy.FrozenTimer != 0)
                {
                    enemy.AiStates = EnemyAiStates.Frozen;
                    return true;
                }

                // This shared grapple step is Powamp's crucial distinction: it invokes the enemy's normal main behaviour, with
                // the whole frame it would otherwise see, while the grappled state remains observable, then clears that
                // state after the actor has moved its grapple anchor.
                _lifecycle.RunMainAi(enemy, frame);
                enemy.AiStates = EnemyAiStates.None;
                return true;

            case GrappleEnemyReaction.AttachAndParalyze:
                enemy.FlashTimer = enemy.HurtFlashFrames;
                enemy.AiStates = EnemyAiStates.None;
                return true;

            case null:
                throw new InvalidDataException($"Enemy {enemy.DefinitionId} names no reaction to the grapple beam.");
            default:
                throw new InvalidDataException(
                    $"Enemy {enemy.DefinitionId} has no response to grapple reaction {enemy.Definition.GrappleResponse}.");
        }
    }
}

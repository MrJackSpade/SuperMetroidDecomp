using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Enemies.Grapple;

/// <summary>The grapple beam's hold on enemies: the enemy its end catches and what a caught enemy does.</summary>
[RoomLifetime]
internal sealed partial class GrappleBehavior : IGrappleTargets
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
    /// plus the beam point's radius and selects the first interactive place; none when the endpoint catches no enemy.
    /// </summary>
    public GrappleEnemyCollision? ResolveGrappleEndpoint(Position16<RoomPixels> endpoint)
    {
        foreach (RoomEnemyOccupant grappled in _lifecycle.InteractiveEnemies)
        {
            RoomEnemyPlace enemy = grappled.Place;
            if (enemy.Invincibility.IsRunning)
                continue;

            // The endpoint tested lies in the room's grid, but an interactive enemy need not: a fly that has climbed through
            // the ceiling or a Bull braking past Samus can lie above or left of the room's origin, and the beam measures its
            // distance to one the short way, as DistanceAcrossRoomEdge.Between gives it.
            int xDistance = DistanceAcrossRoomEdge.Between(enemy.Position.X.Whole, endpoint.X);
            int yDistance = DistanceAcrossRoomEdge.Between(enemy.Position.Y.Whole, endpoint.Y);
            if (xDistance >= enemy.Radius.X + GrappleTargetDefinitions.BeamPointRadius || yDistance >= enemy.Radius.Y + GrappleTargetDefinitions.BeamPointRadius)
                continue;

            // Grappling replaces any hurt or frozen state the enemy had, so its next update runs its grappled
            // reaction; an unreactive enemy is held too, and the beam reads it as one with no interaction.
            enemy.AiStates = EnemyAiStates.Grappled;
            return new GrappleEnemyCollision(
                grappled.Definition.GrappleResponse.BeamReaction,
                enemy.Position.Whole,
                grappled.Definition.Damage);
        }

        return null;
    }

    /// <summary>How the grapple beam moves an enemy of each grapple reaction; true when it replaced main AI for the enemy frame.</summary>
    private IdentityTable<GrappleEnemyReaction, Func<RoomEnemyOccupant, RoomEnemyPlace, IEnemyAiFrame, bool>> GrappleReactions => field ??=
        new(new Dictionary<GrappleEnemyReaction, Func<RoomEnemyOccupant, RoomEnemyPlace, IEnemyAiFrame, bool>>
        {
            [GrappleEnemyReaction.None] = (_, enemy, frame) =>
            {
                enemy.AiStates = EnemyAiStates.None;
                enemy.Invincibility.End();
                enemy.Freeze.End();
                enemy.Shake.End();
                return true;
            },
            [GrappleEnemyReaction.Attach] = (_, enemy, frame) =>
            {
                if (enemy.Freeze.IsRunning)
                {
                    enemy.AiStates = EnemyAiStates.Frozen;
                }
                else
                {
                    enemy.HurtFlash.Restart(enemy.HurtFlashFrames);
                    enemy.AiStates = EnemyAiStates.None;
                }
                return true;
            },
            [GrappleEnemyReaction.Kill] = (grappled, enemy, frame) =>
            {
                // A grapple kill always ends in the small explosion. The shared death keeps the enemy's position, its drop
                // and any respawn marker; deleting the enemy outright would skip the explosion.
                _deathsAndDrops.StartGenericEnemyDeath(grappled, EnemyDeathExplosion.Small, frame.Samus);
                enemy.AiStates = EnemyAiStates.None;
                return true;
            },
            [GrappleEnemyReaction.Cancel] = (_, enemy, frame) =>
            {
                enemy.AiStates = EnemyAiStates.Frozen;
                return true;
            },
            [GrappleEnemyReaction.HurtSamus] = (_, enemy, frame) =>
            {
                enemy.AiStates = EnemyAiStates.Frozen;
                return true;
            },
            [GrappleEnemyReaction.AttachWithoutInvincibility] = (_, enemy, frame) =>
            {
                if (enemy.Freeze.IsRunning)
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
            },
            [GrappleEnemyReaction.AttachAndParalyze] = (_, enemy, frame) =>
            {
                enemy.HurtFlash.Restart(enemy.HurtFlashFrames);
                enemy.AiStates = EnemyAiStates.None;
                return true;
            },
        });

    /// <summary>
    /// Moves the enemy the grapple beam holds, as the grappled state selects. Returning true tells the
    /// ordinary scheduler that this update replaced main AI for the current enemy frame.
    /// </summary>
    internal bool RunCommonGrappleAi(RoomEnemyOccupant grappled, IEnemyAiFrame frame)
    {
        RoomEnemyPlace enemy = Ensure.NotNull(grappled).Place;
        if (!enemy.IsGrappled)
            return false;

        // A held unreactive enemy's grapple behaviour is its main behaviour, which the ordinary scheduler runs.
        return grappled.Definition.GrappleResponse is EnemyGrappleResponse.Reacting { Reaction: var reaction } &&
            GrappleReactions[reaction](grappled, enemy, frame);
    }
}

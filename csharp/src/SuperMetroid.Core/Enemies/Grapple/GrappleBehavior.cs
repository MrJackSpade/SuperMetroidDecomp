using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Enemies.Grapple;

/// <summary>The Grapple species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class GrappleBehavior : IGrappleTargets
{
    /// <summary>Provides interactive enemy order and wrapped main-AI dispatch.</summary>
    private readonly IEnemyDispatch _dispatch;
    /// <summary>Guards endpoint checks until a room is loaded.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves native enemy indexes to physical slots.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Starts the full generic death and drop sequence for grapple-killed enemies.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Binds enemy ordering, room state, physical slots, and death handling.</summary>
    internal GrappleBehavior(IEnemyDispatch dispatch, EnemyRoomBinding room, EnemySlotTable slotTable, EnemyDeathsAndDrops deathsAndDrops)
    {
        _dispatch = dispatch;
        _room = room;
        _slotTable = slotTable;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>
    /// Ports <c>EnemyGrappleBeamCollisionDetection</c> at $A0:9E9A. Extended hitboxes are
    /// intentionally ignored: the original compares the endpoint with each ordinary radius
    /// plus an eight-pixel beam-point radius and selects the first interactive slot.
    /// </summary>
    public GrappleEnemyCollision ResolveGrappleEndpoint(Position16<RoomPixels> endpoint)
    {
        _room.EnsureLoaded();
        foreach (int interactiveIndex in _dispatch.InteractiveEnemyIndexes)
        {
            RoomEnemySlot enemy = _slotTable.SlotFromNativeIndex(interactiveIndex);
            if (!enemy.InvincibilityTimer.IsZero)
                continue;

            int xDistance = Math.Abs(unchecked((short)(enemy.Position.X.Whole - endpoint.X)));
            int yDistance = Math.Abs(unchecked((short)(enemy.Position.Y.Whole - endpoint.Y)));
            if (xDistance >= enemy.Radius.X + 8 || yDistance >= enemy.Radius.Y + 8)
                continue;

            // The collision routine stores exactly one, rather than ORing it into an old
            // hurt/frozen handler. The next EnemyMain pass therefore dispatches grapple AI.
            enemy.AiStates = EnemyAiStates.Grappled;
            GrappleEnemyReaction reaction = enemy.Definition.GrappleAi switch
            {
                GrappleAiRoutine.NoInteraction => GrappleEnemyReaction.None,
                GrappleAiRoutine.Attach => GrappleEnemyReaction.Attach,
                GrappleAiRoutine.Kill => GrappleEnemyReaction.Kill,
                GrappleAiRoutine.Cancel => GrappleEnemyReaction.Cancel,
                GrappleAiRoutine.AttachWithoutInvincibility =>
                    GrappleEnemyReaction.AttachWithoutInvincibility,
                GrappleAiRoutine.AttachAndParalyze => GrappleEnemyReaction.AttachAndParalyze,
                GrappleAiRoutine.HurtSamus => GrappleEnemyReaction.HurtSamus,
                GrappleAiRoutine.None or GrappleAiRoutine.ElevatorMainAi or _ => throw new InvalidDataException(
                    $"Enemy {enemy.DefinitionId} grapple AI {enemy.Definition.GrappleAi} " +
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
    /// Executes the common grapple AI selected by handler bit zero. Returning true tells the
    /// ordinary scheduler that this call replaced main AI for the current enemy frame.
    /// </summary>
    internal bool RunCommonGrappleAi(RoomEnemySlot enemy, IEnemyAiFrame frame)
    {
        if ((enemy.AiStates & EnemyAiStates.Grappled) == 0)
            return false;

        switch (enemy.Definition.GrappleAi)
        {
            case GrappleAiRoutine.NoInteraction:
                enemy.AiStates = EnemyAiStates.None;
                enemy.InvincibilityTimer = WrappingCountdown.Start(0);
                enemy.FrozenTimer = WrappingCountdown.Start(0);
                enemy.ShakeTimer = WrappingCountdown.Start(0);
                return true;

            case GrappleAiRoutine.Attach:
                if (!enemy.FrozenTimer.IsZero)
                {
                    enemy.AiStates = EnemyAiStates.Frozen;
                }
                else
                {
                    enemy.FlashTimer = WrappingCountdown.Start(enemy.HurtFlashFrames);
                    enemy.AiStates = EnemyAiStates.None;
                }
                return true;

            case GrappleAiRoutine.Kill:
                // $A0:9FC4 always selects explosion variant zero. The shared death
                // routine preserves the actor's position/drop identity and any respawn
                // marker; directly deleting the enemy loses the complete visual tail.
                _deathsAndDrops.StartGenericEnemyDeath(enemy, deathAnimation: 0);
                enemy.AiStates = EnemyAiStates.None;
                return true;

            case GrappleAiRoutine.Cancel:
            case GrappleAiRoutine.HurtSamus:
                enemy.AiStates = EnemyAiStates.Frozen;
                return true;

            case GrappleAiRoutine.AttachWithoutInvincibility:
                if (!enemy.FrozenTimer.IsZero)
                {
                    enemy.AiStates = EnemyAiStates.Frozen;
                    return true;
                }

                // This common wrapper is Powamp's crucial distinction: it invokes the
                // enemy's normal main function while bit zero remains observable, then
                // clears that bit after the actor has moved its grapple anchor.
                _dispatch.RunMainAi(enemy, new GrappleWrappedAiFrame(frame));
                enemy.AiStates = EnemyAiStates.None;
                return true;

            case GrappleAiRoutine.AttachAndParalyze:
                enemy.FlashTimer = WrappingCountdown.Start(enemy.HurtFlashFrames);
                enemy.AiStates = EnemyAiStates.None;
                enemy.ExtraProperties = enemy.ExtraProperties.WithUntranslatedBits(
                    EnemyExtraPropertyRawBits.GrappleParalyzeBit0001, "grapple attach-and-paralyze");
                return true;

            case GrappleAiRoutine.None:
            case GrappleAiRoutine.ElevatorMainAi:
            default:
                throw new InvalidDataException(
                    $"Enemy {enemy.DefinitionId} grapple AI {enemy.Definition.GrappleAi} is not translated.");
        }
    }

    /// <summary>
    /// The frame the common grapple wrapper hands to main AI: the enemy's own camera, Samus,
    /// input and room, but no Samus projectiles, Mode 7 transform, shared bombs or VRAM queue,
    /// exactly as the wrapper's main-AI call has always received them (#633).
    /// </summary>
    internal sealed class GrappleWrappedAiFrame(IEnemyAiFrame frame) : IEnemyAiFrame
    {
        /// <summary>Camera inherited from the current enemy update.</summary>
        public Position16<RoomPixels> Camera => frame.Camera;
        /// <summary>Samus state visible to the wrapped main AI.</summary>
        public SamusState? Samus => frame.Samus;
        /// <summary>New input edges inherited from the current update.</summary>
        public SnesButton NewlyPressedControllerInput => frame.NewlyPressedControllerInput;
        /// <summary>Held controller word inherited from the current update.</summary>
        public SnesButton ControllerInput => frame.ControllerInput;
        /// <summary>Current room collision data visible to wrapped main AI.</summary>
        public RoomLevelData? Level => frame.Level;
        /// <summary>Unavailable projectile service in the common grapple wrapper.</summary>
        public SamusProjectileSystem? SamusProjectiles => null;
        /// <summary>Unavailable Mode 7 transform in the common grapple wrapper.</summary>
        public SamusMode7Transform? Mode7Transform => null;
        /// <summary>Unavailable shared bomb service in the common grapple wrapper.</summary>
        public SamusBombProjectileSystem? SharedProjectiles => null;

        /// <inheritdoc/>
        public FrameCounters Clock => frame.Clock;
    }
}

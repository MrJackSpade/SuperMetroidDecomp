using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Grapple species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class GrappleBehavior
{
    /// <summary>Stores dispatch.</summary>
    private readonly IEnemyDispatch _dispatch;
    /// <summary>Stores room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Stores slot table.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Stores deaths and drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Initializes a new grapple behavior instance.</summary>
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
        foreach (ushort nativeIndex in _dispatch.InteractiveEnemyIndexes)
        {
            RoomEnemySlot enemy = _slotTable.SlotFromNativeIndex(nativeIndex);
            if (enemy.InvincibilityTimer != 0)
                continue;

            int xDistance = Math.Abs(unchecked((short)(enemy.Position.X.Whole - endpoint.X)));
            int yDistance = Math.Abs(unchecked((short)(enemy.Position.Y.Whole - endpoint.Y)));
            if (xDistance >= enemy.Radius.X + 8 || yDistance >= enemy.Radius.Y + 8)
                continue;

            // The collision routine stores exactly one, rather than ORing it into an old
            // hurt/frozen handler. The next EnemyMain pass therefore dispatches grapple AI.
            enemy.AiHandlerBits = 1;
            GrappleEnemyReaction reaction = enemy.Definition.GrappleAiPointer switch
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
                    $"Enemy ${(int)enemy.EnemyDefinitionPointer:X4} grapple AI " +
                    $"${enemy.Definition.Bank:X2}:{(int)enemy.Definition.GrappleAiPointer:X4} " +
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
    internal bool RunCommonGrappleAi(RoomEnemySlot enemy, IEnemyAiFrame frame, byte nmiFrameCounter8)
    {
        if ((enemy.AiHandlerBits & 1) == 0)
            return false;

        switch (enemy.Definition.GrappleAiPointer)
        {
            case GrappleAiRoutine.NoInteraction:
                enemy.AiHandlerBits = 0;
                enemy.InvincibilityTimer = 0;
                enemy.FrozenTimer = 0;
                enemy.ShakeTimer = 0;
                return true;

            case GrappleAiRoutine.Attach:
                if (enemy.FrozenTimer != 0)
                {
                    enemy.AiHandlerBits = 4;
                }
                else
                {
                    enemy.FlashTimer = enemy.HurtAiTime == 0 ? (ushort)4 : enemy.HurtAiTime;
                    enemy.AiHandlerBits = 0;
                }
                return true;

            case GrappleAiRoutine.Kill:
                // $A0:9FC4 always selects explosion variant zero. The shared death
                // routine preserves the actor's position/drop identity and any respawn
                // marker; directly deleting the enemy loses the complete visual tail.
                _deathsAndDrops.StartGenericEnemyDeath(enemy, deathAnimation: 0);
                enemy.AiHandlerBits = 0;
                return true;

            case GrappleAiRoutine.Cancel:
            case GrappleAiRoutine.HurtSamus:
                enemy.AiHandlerBits = 4;
                return true;

            case GrappleAiRoutine.AttachWithoutInvincibility:
                if (enemy.FrozenTimer != 0)
                {
                    enemy.AiHandlerBits = 4;
                    return true;
                }

                // This common wrapper is Powamp's crucial distinction: it invokes the
                // enemy's normal main function while bit zero remains observable, then
                // clears that bit after the actor has moved its grapple anchor.
                _dispatch.RunMainAi(enemy, new GrappleWrappedAiFrame(frame), nmiFrameCounter8);
                enemy.AiHandlerBits = 0;
                return true;

            case GrappleAiRoutine.AttachAndParalyze:
                enemy.FlashTimer = enemy.HurtAiTime == 0 ? (ushort)4 : enemy.HurtAiTime;
                enemy.AiHandlerBits = 0;
                enemy.ExtraProperties = unchecked((ushort)(enemy.ExtraProperties | 1));
                return true;

            case GrappleAiRoutine.None:
            case GrappleAiRoutine.ElevatorMainAi:
            default:
                throw new InvalidDataException(
                    $"Enemy ${(int)enemy.EnemyDefinitionPointer:X4} grapple AI " +
                    $"${enemy.Definition.Bank:X2}:{(int)enemy.Definition.GrappleAiPointer:X4} is not translated.");
        }
    }

    /// <summary>
    /// The frame the common grapple wrapper hands to main AI: the enemy's own camera, Samus,
    /// input and room, but no Samus projectiles, Mode 7 transform, shared bombs or VRAM queue,
    /// exactly as the wrapper's main-AI call has always received them (#633).
    /// </summary>
    internal sealed class GrappleWrappedAiFrame(IEnemyAiFrame frame) : IEnemyAiFrame
    {
        /// <summary>Gets camera.</summary>
        public Position16<RoomPixels> Camera => frame.Camera;
        /// <summary>Gets samus.</summary>
        public SamusState? Samus => frame.Samus;
        /// <summary>Gets newly pressed controller input.</summary>
        public ushort NewlyPressedControllerInput => frame.NewlyPressedControllerInput;
        /// <summary>Gets controller input.</summary>
        public ushort ControllerInput => frame.ControllerInput;
        /// <summary>Gets level.</summary>
        public RoomLevelData? Level => frame.Level;
        /// <summary>Gets samus projectiles.</summary>
        public SamusProjectileSystem? SamusProjectiles => null;
        /// <summary>Gets mode7 transform.</summary>
        public SamusMode7Transform? Mode7Transform => null;
        /// <summary>Gets shared projectiles.</summary>
        public SamusBombProjectileSystem? SharedProjectiles => null;
        /// <summary>Gets vram write queue.</summary>
        public VramWriteQueue? VramWriteQueue => null;
    }
}

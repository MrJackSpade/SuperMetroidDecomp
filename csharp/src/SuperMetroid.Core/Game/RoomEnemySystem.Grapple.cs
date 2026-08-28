using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Header-driven ordinary-enemy grapple collision and the seven common bank-$A0 reactions.
/// This is shared infrastructure: Powamp consumes reaction four, while every other enemy
/// retains the behavior selected by its own 64-byte ROM definition.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort GrappleNoInteraction = 0x8000;
    private const ushort GrappleAttach = 0x8005;
    private const ushort GrappleKill = 0x800a;
    private const ushort GrappleCancel = 0x800f;
    private const ushort GrappleAttachWithoutInvincibility = 0x8014;
    private const ushort GrappleAttachAndParalyze = 0x8019;
    private const ushort GrappleHurtSamus = 0x801e;

    /// <summary>
    /// Ports <c>EnemyGrappleBeamCollisionDetection</c> at $A0:9E9A. Extended hitboxes are
    /// intentionally ignored: the original compares the endpoint with each ordinary radius
    /// plus an eight-pixel beam-point radius and selects the first interactive slot.
    /// </summary>
    public GrappleEnemyCollision ResolveGrappleEndpoint(ushort endpointX, ushort endpointY)
    {
        EnsureLoaded();
        foreach (ushort nativeIndex in _interactiveEnemyIndexes)
        {
            RoomEnemySlot enemy = SlotFromNativeIndex(nativeIndex);
            if (enemy.InvincibilityTimer != 0)
                continue;

            int xDistance = Math.Abs(unchecked((short)(enemy.XPosition - endpointX)));
            int yDistance = Math.Abs(unchecked((short)(enemy.YPosition - endpointY)));
            if (xDistance >= enemy.XRadius + 8 || yDistance >= enemy.YRadius + 8)
                continue;

            // The collision routine stores exactly one, rather than ORing it into an old
            // hurt/frozen handler. The next EnemyMain pass therefore dispatches grapple AI.
            enemy.AiHandlerBits = 1;
            GrappleEnemyReaction reaction = enemy.Definition.GrappleAiPointer switch
            {
                GrappleNoInteraction => GrappleEnemyReaction.None,
                GrappleAttach => GrappleEnemyReaction.Attach,
                GrappleKill => GrappleEnemyReaction.Kill,
                GrappleCancel => GrappleEnemyReaction.Cancel,
                GrappleAttachWithoutInvincibility =>
                    GrappleEnemyReaction.AttachWithoutInvincibility,
                GrappleAttachAndParalyze => GrappleEnemyReaction.AttachAndParalyze,
                GrappleHurtSamus => GrappleEnemyReaction.HurtSamus,
                _ => throw new NotSupportedException(
                    $"Enemy ${enemy.EnemyDefinitionPointer:X4} grapple AI " +
                    $"${enemy.Definition.Bank:X2}:{enemy.Definition.GrappleAiPointer:X4} " +
                    "is not one of the translated common reactions."),
            };
            return new GrappleEnemyCollision(
                Collided: true,
                reaction,
                nativeIndex,
                enemy.XPosition,
                enemy.YPosition,
                enemy.Definition.Damage);
        }

        return default;
    }

    /// <summary>
    /// Executes the common grapple AI selected by handler bit zero. Returning true tells the
    /// ordinary scheduler that this call replaced main AI for the current enemy frame.
    /// </summary>
    private bool RunCommonGrappleAi(
        RoomEnemySlot enemy,
        SamusState? samus,
        ushort newlyPressedControllerInput,
        ushort controllerInput,
        RoomLevelData? level,
        ushort cameraX,
        ushort cameraY,
        byte nmiFrameCounter8)
    {
        if ((enemy.AiHandlerBits & 1) == 0)
            return false;

        switch (enemy.Definition.GrappleAiPointer)
        {
            case GrappleNoInteraction:
                enemy.AiHandlerBits = 0;
                enemy.InvincibilityTimer = 0;
                enemy.FrozenTimer = 0;
                enemy.ShakeTimer = 0;
                return true;

            case GrappleAttach:
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

            case GrappleKill:
                enemy.Health = 0;
                enemy.Properties = enemy.Properties.With(EnemyProperties.Deleted);
                enemy.AiHandlerBits = 0;
                EnemiesKilled = unchecked((ushort)(EnemiesKilled + 1));
                return true;

            case GrappleCancel:
            case GrappleHurtSamus:
                enemy.AiHandlerBits = 4;
                return true;

            case GrappleAttachWithoutInvincibility:
                if (enemy.FrozenTimer != 0)
                {
                    enemy.AiHandlerBits = 4;
                    return true;
                }

                // This common wrapper is Powamp's crucial distinction: it invokes the
                // enemy's normal main function while bit zero remains observable, then
                // clears that bit after the actor has moved its grapple anchor.
                RunMainAi(
                    enemy,
                    samus,
                    newlyPressedControllerInput,
                    controllerInput,
                    level,
                    cameraX,
                    cameraY,
                    samusProjectiles: null,
                    nmiFrameCounter8);
                enemy.AiHandlerBits = 0;
                return true;

            case GrappleAttachAndParalyze:
                enemy.FlashTimer = enemy.HurtAiTime == 0 ? (ushort)4 : enemy.HurtAiTime;
                enemy.AiHandlerBits = 0;
                enemy.ExtraProperties = unchecked((ushort)(enemy.ExtraProperties | 1));
                return true;

            default:
                throw new NotSupportedException(
                    $"Enemy ${enemy.EnemyDefinitionPointer:X4} grapple AI " +
                    $"${enemy.Definition.Bank:X2}:{enemy.Definition.GrappleAiPointer:X4} is not translated.");
        }
    }
}

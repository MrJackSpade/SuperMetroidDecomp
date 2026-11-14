namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One ChozoStatue occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The ChozoStatue species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ChozoStatueEntity(ChozoStatueBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (ChozoStatuePartDefinitions.Parts[Slot.DefinitionId])
        {
            case ChozoStatuePart.NoobTubeCrack:
                ChozoStatueBehavior.InitializeN00bTubeCracks();
                break;
            case ChozoStatuePart.Chozo:
                species.InitializeChozoStatue(Slot);
                break;
            default:
                throw new InvalidOperationException($"ChozoStatue entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (ChozoStatuePartDefinitions.Parts[Slot.DefinitionId])
        {
            case ChozoStatuePart.Chozo:
                species.RunChozoStatueMainAi(Slot, frame);
                break;
            // The glass tube's cracks take no action of their own.
            case ChozoStatuePart.NoobTubeCrack:
                break;
            default:
                throw new InvalidOperationException($"ChozoStatue entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <summary>The Chozo statue animates by its typed animation; the n00b tube crack has none.</summary>
    public bool HasTypedAnimation => ChozoStatuePartDefinitions.Parts[Slot.DefinitionId] == ChozoStatuePart.Chozo;

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context)
    {
        switch (ChozoStatuePartDefinitions.Parts[Slot.DefinitionId])
        {
            case ChozoStatuePart.Chozo:
                species.AdvanceChozoStatueTypedAnimation(Slot, context);
                break;
            case ChozoStatuePart.NoobTubeCrack:
            default:
                throw new InvalidOperationException($"ChozoStatue entity bound to {Slot.DefinitionId} has no typed animation.");
        }
    }
}

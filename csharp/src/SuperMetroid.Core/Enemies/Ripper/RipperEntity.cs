namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>One Ripper occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Ripper species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class RipperEntity(RipperBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (RipperPartDefinitions.Parts[Slot.DefinitionId])
        {
            case RipperPart.GRipper:
                RipperBehavior.InitializeGRipper(Slot);
                break;
            case RipperPart.Ripper2:
                RipperBehavior.InitializeRipper2(Slot);
                break;
            case RipperPart.Ripper:
                RipperBehavior.InitializeRipper(Slot);
                break;
            default:
                throw new InvalidOperationException($"Ripper entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (RipperPartDefinitions.Parts[Slot.DefinitionId])
        {
            case RipperPart.GRipper:
                species.RunGRipperMainAi(Slot, frame);
                break;
            case RipperPart.Ripper2:
                species.RunRipper2MainAi(Slot, frame);
                break;
            case RipperPart.Ripper:
                species.RunRipperMainAi(Slot, frame);
                break;
            default:
                throw new InvalidOperationException($"Ripper entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => RipperBehavior.AdvanceRipperTypedAnimation;

    /// <summary>The grapple Ripper's and second Ripper's shared shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        Slot.DefinitionId is EnemyDefinitionId.GRipper or EnemyDefinitionId.Ripper2 && reaction == EnemyReaction.GRipperRipper2Shot;
}

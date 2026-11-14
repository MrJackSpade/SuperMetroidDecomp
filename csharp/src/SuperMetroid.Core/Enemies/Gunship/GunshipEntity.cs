namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>One Gunship occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Gunship species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class GunshipEntity(GunshipBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (GunshipPartDefinitions.Parts[Slot.DefinitionId])
        {
            case GunshipPart.ShipTop:
                species.InitializeGunshipTop(Slot);
                break;
            case GunshipPart.ShipBottomEntrance:
                species.InitializeGunshipBottom(Slot);
                break;
            default:
                throw new InvalidOperationException($"Gunship entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (GunshipPartDefinitions.Parts[Slot.DefinitionId])
        {
            case GunshipPart.ShipTop:
                species.RunGunshipMainAi(Slot, frame);
                break;
            // The ship's bottom and entrance take no action of their own; the top moves the ship.
            case GunshipPart.ShipBottomEntrance:
                break;
            default:
                throw new InvalidOperationException($"Gunship entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => (enemy, _) => species.AdvanceGunshipAnimation(enemy);
}

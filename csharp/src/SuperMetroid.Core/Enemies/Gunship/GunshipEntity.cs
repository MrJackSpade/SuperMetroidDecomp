namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>One Gunship occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Gunship species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class GunshipEntity(GunshipBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemySurroundingsInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize(EnemySpawnSurroundings surroundings)
    {
        switch (GunshipPartDefinitions.Parts[Slot.DefinitionId])
        {
            case GunshipPart.ShipTop:
                species.InitializeGunshipTop(Slot, surroundings.IntroducedSamus);
                break;
            case GunshipPart.ShipBottomEntrance:
                species.InitializeGunshipBottom(Slot, surroundings.IntroducedSamus);
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
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => GunshipBehavior.AdvanceGunshipAnimation(Slot);
}

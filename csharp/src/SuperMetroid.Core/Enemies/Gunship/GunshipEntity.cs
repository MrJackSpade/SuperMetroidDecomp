namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>One Gunship occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Gunship species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class GunshipEntity(GunshipBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (GunshipPartDefinitions.Parts[Place.DefinitionId])
        {
            case GunshipPart.ShipTop:
                species.InitializeGunshipTop(Place);
                break;
            case GunshipPart.ShipBottomEntrance:
                species.InitializeGunshipBottom(Place);
                break;
            default:
                throw new InvalidOperationException($"Gunship entity bound to {Place.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (GunshipPartDefinitions.Parts[Place.DefinitionId])
        {
            case GunshipPart.ShipTop:
                species.RunGunshipMainAi(Place, frame);
                break;
            // The ship's bottom and entrance take no action of their own; the top moves the ship.
            case GunshipPart.ShipBottomEntrance:
                break;
            default:
                throw new InvalidOperationException($"Gunship entity bound to {Place.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceGunshipAnimation(Place);
}

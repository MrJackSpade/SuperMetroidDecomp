namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>One Gunship occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Gunship species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class GunshipEntity(GunshipBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemySurroundingsInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize(EnemySpawnSurroundings surroundings)
    {
        switch (GunshipPartDefinitions.Parts[Place.DefinitionId])
        {
            case GunshipPart.ShipTop:
                species.InitializeGunshipTop(Place, surroundings.IntroducedSamus);
                break;
            case GunshipPart.ShipBottomEntrance:
                species.InitializeGunshipBottom(Place, surroundings.IntroducedSamus);
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
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => GunshipBehavior.AdvanceGunshipAnimation(Place);
}

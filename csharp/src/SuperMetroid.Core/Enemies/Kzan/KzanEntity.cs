namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>One Kzan occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Kzan species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KzanEntity(KzanBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (KzanPartDefinitions.Parts[Place.DefinitionId])
        {
            case KzanPart.Top:
                KzanBehavior.InitializeKzanTop(Place);
                break;
            case KzanPart.Bottom:
                species.InitializeKzanBottom(Place);
                break;
            default:
                throw new InvalidOperationException($"Kzan entity bound to {Place.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (KzanPartDefinitions.Parts[Place.DefinitionId])
        {
            case KzanPart.Top:
                species.RunKzanTopMainAi(Place, frame);
                break;
            case KzanPart.Bottom:
                species.RunKzanBottomMain(Place);
                break;
            default:
                throw new InvalidOperationException($"Kzan entity bound to {Place.DefinitionId}.");
        }
    }

    /// <summary>The Kzan's top animates by its typed animation; the bottom has none.</summary>
    public bool HasTypedAnimation => KzanPartDefinitions.Parts[Place.DefinitionId] == KzanPart.Top;

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context)
    {
        switch (KzanPartDefinitions.Parts[Place.DefinitionId])
        {
            case KzanPart.Top:
                KzanBehavior.AdvanceKzanTypedAnimation(Place, context);
                break;
            case KzanPart.Bottom:
            default:
                throw new InvalidOperationException($"Kzan entity bound to {Place.DefinitionId} has no typed animation.");
        }
    }
}

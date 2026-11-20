namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>The gunship's top hull (#2146), which moves the whole ship: its own AI, with the shared reactions.</summary>
/// <param name="species">The Gunship species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the top hull occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="setup">How the enemy comes to be set up, whose surroundings its initialization reads.</param>
internal sealed class GunshipTopEntity(GunshipBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The top hull as its setup builds it, as the hull is built (#2554).</summary>
    private readonly GunshipTop _top = species.InitializeGunshipTop(place, setup.SurroundingsOf(place).Samus);

    /// <summary>The state of the whole ship, which the top hull moves.</summary>
    internal GunshipState Ship => _top.Ship;

    /// <summary>The top hull's animation.</summary>
    internal GunshipPieceState Piece => _top.Piece;

    /// <summary>The ship's three pieces.</summary>
    internal GunshipPieces Pieces => _top.Pieces;

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunGunshipMainAi(Place, Ship, Pieces, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => GunshipBehavior.AdvanceGunshipAnimation(Place, Piece);
}

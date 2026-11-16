namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>
/// The glass tube's crack enemy, which the glass tunnel's population places already deleted: its place in the room's
/// list is kept, and the enemy pass clears it before any update. Its only initialization effect in the game was the
/// crack colours every tube piece draws in, which the pieces' own shading carries (#1512).
/// </summary>
/// <param name="slot">The slot the crack occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NoobTubeCrackEntity(RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization
{
    /// <summary>Places the crack, which keeps no state of its own.</summary>
    public void Initialize()
    {
    }
}

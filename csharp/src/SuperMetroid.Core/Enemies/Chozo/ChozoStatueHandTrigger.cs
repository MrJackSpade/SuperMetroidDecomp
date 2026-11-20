using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// A Chozo statue's hand as its room objects hold it (#2554): bound to its statue when the statue asks for it, so the hand's
/// trigger wakes that statue.
/// </summary>
/// <param name="statues">The Chozo statue species, which runs the trigger's effects.</param>
/// <param name="statue">The statue's place.</param>
/// <param name="state">The statue the hand belongs to.</param>
[RoomLifetime]
internal sealed class ChozoStatueHandTrigger(ChozoStatueBehavior statues, RoomEnemyPlace statue, ChozoStatueState state) : IChozoStatueHandActivation
{
    /// <summary>The Chozo statue species.</summary>
    private readonly ChozoStatueBehavior _statues = Ensure.NotNull(statues);

    /// <summary>The statue the hand belongs to.</summary>
    private readonly ChozoStatueState _state = Ensure.NotNull(state);

    /// <summary>The statue's place.</summary>
    internal RoomEnemyPlace Statue { get; } = Ensure.NotNull(statue);

    /// <inheritdoc/>
    public IReadOnlyList<RoomObjectSpawnRequest<ChozoStatueRoomObject>> ActivateChozoStatueHand() => _statues.ActivateChozoStatueHandTrigger(_state);
}

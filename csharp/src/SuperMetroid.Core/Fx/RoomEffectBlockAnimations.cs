using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>Starts the looping block animations a room's door-selected room effect lists (#1988).</summary>
internal static class RoomEffectBlockAnimations
{
    /// <summary>
    /// Starts a player for each animation the door-selected room effect lists that is among <paramref name="played"/>,
    /// in the effect's order; none when the room has no effect.
    /// </summary>
    /// <param name="fx">The room state's effect selection, from which <paramref name="door"/> picks the room's effect.</param>
    /// <param name="door">The entry door, which selects the room effect.</param>
    /// <param name="played">The block animations the room plays.</param>
    internal static List<BlockAnimationPlayer> Start(RoomFxSelection? fx, DoorDefinition? door, IReadOnlyCollection<BlockAnimation> played)
    {
        var players = new List<BlockAnimationPlayer>();
        if (fx?.Select(door) is not { } effect)
            return players;
        foreach (BlockAnimation animation in effect.BlockAnimations)
        {
            if (!played.Contains(animation))
                continue;
            var player = new BlockAnimationPlayer();
            player.Play(BlockAnimationLoops.Of(animation));
            players.Add(player);
        }
        return players;
    }
}

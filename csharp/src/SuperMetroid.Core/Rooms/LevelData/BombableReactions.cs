namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Reads what the bomb-block dispatchers do with a bombable block's reaction (#1722).</summary>
public static class BombableReactions
{
    /// <summary>Whether <paramref name="reaction"/> is one of the eight bomb-block breaks.</summary>
    public static bool IsBreak(BombableReaction reaction) => reaction is
        BombableReaction.RespawningBreak1x1 or BombableReaction.RespawningBreak2x1 or
        BombableReaction.RespawningBreak1x2 or BombableReaction.RespawningBreak2x2 or
        BombableReaction.PermanentBreak1x1 or BombableReaction.PermanentBreak2x1 or
        BombableReaction.PermanentBreak1x2 or BombableReaction.PermanentBreak2x2;
}

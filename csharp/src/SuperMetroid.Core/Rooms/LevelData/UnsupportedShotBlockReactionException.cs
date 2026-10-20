namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>A shootable block's BTS selects a reaction the shot-block dispatcher does not translate.</summary>
internal sealed class UnsupportedShotBlockReactionException : ArgumentOutOfRangeException
{
    /// <summary>Creates the failure for the BTS parameter <paramref name="paramName"/>.</summary>
    internal UnsupportedShotBlockReactionException(string paramName, string message)
        : base(paramName, message)
    {
    }
}

using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Queries the active room PLM population.</summary>
public interface IRoomPlmPresence
{
    /// <summary>Whether a PLM with <paramref name="header"/> is active.</summary>
    bool HasActiveHeader(PlmHeaderId header);
}

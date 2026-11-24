using Engine.World;

namespace Engine.Core;

/// <summary>
/// A session service with something to do each time a room's liquid has moved, as an update opens or wherever else
/// the liquid is moved. The room calls it there, right after the liquid's move, in the order the
/// <see cref="GameDefinition"/> made the services.
/// </summary>
public interface ILiquidWatcher : ISessionService
{
    /// <summary>The liquid of the room being played has just moved.</summary>
    void LiquidMoved(GameSession session, Liquid liquid);
}

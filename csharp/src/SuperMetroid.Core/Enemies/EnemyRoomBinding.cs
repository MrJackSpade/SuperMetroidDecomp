using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The loaded room's bindings that <see cref="RoomEnemySystem"/> shares with the owners it composes: the room's
/// area, its enemy definition source and the game-state services its AI calls (including the enemy random source).
/// </summary>
internal sealed class EnemyRoomBinding
{
    /// <summary>Binds the room to its enemy definitions and services and to the session whose enemy state outlives it.</summary>
    /// <param name="session">The enemy state and installed artwork that outlive each room.</param>
    /// <param name="definitions">The room's enemy definition source.</param>
    /// <param name="services">The game-state services the room's AI calls.</param>
    /// <param name="area">The area the room is in.</param>
    internal EnemyRoomBinding(EnemySession session, IRoomEnemyDefinitionSource definitions, IRoomEnemyServices services, AreaId area)
    {
        Area = area;
        Session = Ensure.NotNull(session);
        Definitions = Ensure.NotNull(definitions);
        Services = Ensure.NotNull(services);
    }

    /// <summary>The enemy state and installed artwork that outlive each room.</summary>
    internal EnemySession Session { get; }

    /// <summary>
    /// The area the room is in, which decides how long a shot's freeze lasts and how much Ridley's fireballs hurt.
    /// </summary>
    internal AreaId Area { get; }

    /// <summary>The room's enemy definition source.</summary>
    internal IRoomEnemyDefinitionSource Definitions { get; }

    /// <summary>
    /// The game-state services the room's AI calls, which the room's enemy system was built with. A host without
    /// a service supplies one that fails at the exact dependency.
    /// </summary>
    internal IRoomEnemyServices Services { get; }

    /// <summary>The game random source of the current room's services.</summary>
    internal IEnemyRandomNumbers Random => Services.Random;

}

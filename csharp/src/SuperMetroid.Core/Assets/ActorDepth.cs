namespace SuperMetroid.Core.Assets;

/// <summary>
/// Where a gameplay actor's frame sits among the room's layers (#1514): behind the whole room, between the room's back
/// tiles and its front tiles (as Samus passes behind a pillar's front tiles), or in front of the whole room. A frame is
/// drawn whole at its one depth: the import decides it as it installs the frame, from where the game draws the frame as a
/// whole, and the actor's draw shows the frame there.
/// </summary>
/// <remarks>
/// The two depths behind the room differ only where a turning floor shows (#2583): one lies behind the floor and the other
/// in front of it. In a room drawn from its background and foreground, both lie behind every part of the room, the room's
/// effect overlay lies behind or in front of both alike, and the actors at the two depths are drawn together.
/// </remarks>
public enum ActorDepth
{
    /// <summary>Behind every tile of the room's background and foreground, and behind a turning floor; only the backdrop lies behind it.</summary>
    BehindRoom,

    /// <summary>
    /// Behind every tile of the room's background and foreground, as <see cref="BehindRoom"/>, but in front of a turning
    /// floor (#2583), where the budding Kraid lint, Draygon's goop and the Tourian statues' base decoration are drawn.
    /// </summary>
    BehindRoomInFrontOfFloor,

    /// <summary>In front of the room's back tiles and behind its front tiles: where Samus and most actors are drawn.</summary>
    BehindFrontParts,

    /// <summary>In front of every tile of the room's background and foreground.</summary>
    InFront,
}

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Where a gameplay actor's frame sits among the room's layers (#1514): behind the whole room, between the room's back
/// tiles and its front tiles (as Samus passes behind a pillar's front tiles), or in front of the whole room. A frame is
/// drawn whole at its one depth: the import decides it as it installs the frame, from where the game draws the frame as a
/// whole, and the actor's draw shows the frame there.
/// </summary>
public enum ActorDepth
{
    /// <summary>Behind every tile of the room's background and foreground; only the backdrop lies behind it.</summary>
    BehindRoom,

    /// <summary>In front of the room's back tiles and behind its front tiles: where Samus and most actors are drawn.</summary>
    BehindFrontParts,

    /// <summary>In front of every tile of the room's background and foreground.</summary>
    InFront,
}

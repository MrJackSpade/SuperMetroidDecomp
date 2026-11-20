namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Kraid's room's ceiling and spike block draws (#2034): the block shape each draws, from
/// <see cref="KraidRoomObjectDrawDefinitions"/>, and the installed picture (#1557) each shows. A crumbling block shows the
/// stages of its breakup (#2127), then the picture its draw leaves.
/// </summary>
internal static class KraidRoomObjectDraws
{
    /// <summary>The room's backgrounds and clears, pictured by the installed Kraid room frames.</summary>
    internal static RoomObjectDrawFamily<KraidRoomDraw> Family { get; } =
        new RoomObjectDrawFamily<KraidRoomDraw>("Kraid room",
            frame => RoomObjectDrawFamily<KraidRoomDraw>.Defined(KraidRoomObjectDrawDefinitions.Shapes, frame));

    /// <summary>A crumbling ceiling or spike block's breakup stages, pictured by the installed Kraid room frames.</summary>
    internal static RoomObjectDrawFamily<CrumbleDraw> CrumbleFamily { get; } =
        new RoomObjectDrawFamily<CrumbleDraw>("Kraid room crumble",
            frame => RoomObjectDrawFamily<BlockBreakupStage>.Defined(KraidRoomObjectDrawDefinitions.CrumbleShapes, frame.Stage));

    /// <summary>A crumbling block showing <paramref name="stage"/> of its breakup.</summary>
    internal static RoomObjectFrameDraw<CrumbleDraw> Crumble(BlockBreakupStage stage) => CrumbleFamily.Of(new CrumbleDraw(stage));

    /// <summary>
    /// A crumbling ceiling or spike block at one stage of its breakup: the rock and its pieces still solid, the dust open.
    /// The breakup's last stage is not drawn: the block shows the background or spike picture its draw leaves instead.
    /// </summary>
    /// <param name="Stage">What the crumbling block shows.</param>
    internal readonly record struct CrumbleDraw(BlockBreakupStage Stage);
}

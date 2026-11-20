using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// Verification's native view of the Beetom identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class BeetomNativeWords
{
    /// <summary>
    /// The routine word naming each <see cref="BeetomAction"/> and, for an action that goes one way, its heading (#2277),
    /// the cartridge giving each heading a routine of its own. A hop's or lunge's routine runs both its halves and names its
    /// rise; its falling flag ($7E:780A) tells the fall (#2531).
    /// </summary>
    internal static readonly NativeWordIdentities<(BeetomAction Action, HorizontalDirection? Heading)> BeetomActions =
        new(new Dictionary<(BeetomAction Action, HorizontalDirection? Heading), ushort>
        {
            [(BeetomAction.DecideAction, null)] = 0xb814,
            [(BeetomAction.ChooseIdleCrawlOrHop, null)] = 0xb82f,
            [(BeetomAction.StartIdling, null)] = 0xb84f,
            [(BeetomAction.StartCrawling, HorizontalDirection.Left)] = 0xb85f,
            [(BeetomAction.StartCrawling, HorizontalDirection.Right)] = 0xb873,
            [(BeetomAction.StartShortHop, HorizontalDirection.Left)] = 0xb887,
            [(BeetomAction.StartShortHop, HorizontalDirection.Right)] = 0xb8a9,
            [(BeetomAction.StartLongHop, HorizontalDirection.Left)] = 0xb8cb,
            [(BeetomAction.StartLongHop, HorizontalDirection.Right)] = 0xb8ed,
            [(BeetomAction.StartLunge, null)] = 0xb90f,
            [(BeetomAction.StartDraining, HorizontalDirection.Left)] = 0xb952,
            [(BeetomAction.StartDraining, HorizontalDirection.Right)] = 0xb966,
            [(BeetomAction.StartDropping, null)] = 0xb97a,
            [(BeetomAction.StartBeingFlung, null)] = 0xb9a2,
            [(BeetomAction.Idling, null)] = 0xb9b2,
            [(BeetomAction.Crawling, HorizontalDirection.Left)] = 0xb9c1,
            [(BeetomAction.Crawling, HorizontalDirection.Right)] = 0xba24,
            [(BeetomAction.ShortHopRising, HorizontalDirection.Left)] = 0xba84,
            [(BeetomAction.ShortHopRising, HorizontalDirection.Right)] = 0xbab7,
            [(BeetomAction.LongHopRising, HorizontalDirection.Left)] = 0xbb55,
            [(BeetomAction.LongHopRising, HorizontalDirection.Right)] = 0xbb88,
            [(BeetomAction.LungeRising, HorizontalDirection.Left)] = 0xbc26,
            [(BeetomAction.LungeRising, HorizontalDirection.Right)] = 0xbc5a,
            [(BeetomAction.Draining, HorizontalDirection.Left)] = 0xbcf8,
            [(BeetomAction.Draining, HorizontalDirection.Right)] = 0xbd42,
            [(BeetomAction.Dropping, null)] = 0xbd9d,
            [(BeetomAction.BeingFlung, null)] = 0xbdc5,
        });

    /// <summary>The routine word naming <paramref name="state"/>'s action, with its heading when the action goes one way.</summary>
    internal static ushort ActionWord(BeetomEnemyState state) =>
        BeetomActions.WordsByIdentity.ContainsKey((state.Action, null)) ? BeetomActions.Word((state.Action, null)) : BeetomActions.Word((state.Action, state.Heading));
}

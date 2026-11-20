using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The gameplay a sequence's step runs (#2554): the session's runtime and its loaded room, matched once where the game
/// steps the sequence.
/// </summary>
/// <param name="Runtime">The session's gameplay runtime.</param>
/// <param name="Room">The runtime's loaded room, which every room load completes and every later one replaces.</param>
internal sealed record LoadedGameplay(SuperMetroidRuntime Runtime, RoomScope Room);

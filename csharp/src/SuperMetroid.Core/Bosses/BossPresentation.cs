using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses;

/// <summary>
/// What a boss draws with (#1395): the room's enemy presentation surfaces and the session's installed boss artwork. A boss
/// species receives them together, as one input to its components.
/// </summary>
/// <param name="Surfaces">The room's enemy CGRAM, VRAM and BG2 presentation.</param>
/// <param name="Artwork">The session's installed boss artwork and presentation catalogs.</param>
[RoomLifetime]
internal sealed record BossPresentation(EnemyPresentation Surfaces, BossArtworkBinding Artwork);

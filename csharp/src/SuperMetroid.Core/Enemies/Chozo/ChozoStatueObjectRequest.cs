using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// One PLM request made by the Chozo-statue enemy AI. Hardcoded requests carry
/// explicit room-block coordinates; ordinary requests use the block selected by the preceding
/// pixel probe. Retaining the PLM identity keeps this seam consumable by the PLM system later.
/// </summary>
[IndependentFlag("whether the request spawns the hard-coded PLM rather than one from the room list")]
public readonly record struct ChozoStatueObjectRequest(
    RoomObjectHeaderId Header,
    Position16<RoomBlocks> Block,
    bool IsHardcoded);

using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One of the two shipped Chozo statues' sites (<see cref="ChozoStatueSiteDefinitions.Sites"/>).</summary>
/// <param name="Shade">The statue's own colours, which no other enemy shows (#1512).</param>
/// <param name="Idle">The segment the statue waits in.</param>
/// <param name="Activated">The segment its activation starts at once Samus has triggered its hand.</param>
/// <param name="PlacedObjects">The room objects its placement requests, in the order it requests them, each at its block.</param>
/// <param name="Area">The area the statue's room lies in.</param>
/// <param name="HandBehavior">The behaviour of its hand's block, which a touch of the hand meets.</param>
/// <param name="Admission">What admits Samus to its hand.</param>
internal sealed record ChozoStatueSiteDefinition(
    PictureShade Shade,
    ChozoStatueSegment Idle,
    ChozoStatueSegment Activated,
    IReadOnlyList<RoomObjectSpawnRequest<ChozoStatueRoomObject>> PlacedObjects,
    AreaId Area,
    RoomBlockBehavior HandBehavior,
    ChozoHandAdmission Admission);

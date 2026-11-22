using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One body fragment: where it starts from Ridley's body while he faces left, and the pose it shows each way he faces.</summary>
/// <param name="FacingLeftOffset">Where it starts while he faces left; facing right mirrors its X.</param>
/// <param name="FacingLeftFrame">The pose it shows while he faces left.</param>
/// <param name="FacingRightFrame">The pose it shows while he faces right.</param>
internal readonly partial record struct RidleyExplosionBodyPartDefinition(
    Offset16<RoomPixels> FacingLeftOffset,
    EnemyFrameId FacingLeftFrame,
    EnemyFrameId FacingRightFrame);

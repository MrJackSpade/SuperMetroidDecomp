using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary><c>LoadFloor</c>'s scene: both map copies and the characters of one floor scene.</summary>
/// <param name="Scene">The floor scene loaded.</param>
internal sealed record EndingFloorSceneUpload(EndingFloorSceneId Scene) : EndingUpload(EndingUploadKind.FloorSceneTiles);

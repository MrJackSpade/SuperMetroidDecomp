using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>What a boss's body is shown over in one of its rooms (#1466).</summary>
/// <param name="Tileset">The room's tileset, whose palette colours the body.</param>
/// <param name="Content">What shows at each character once the room has loaded and its library background has run.</param>
/// <param name="Loaded">The artwork the room's load and library background bind, with its bytes.</param>
/// <param name="Rebound">What the room binds over its tile characters after loading.</param>
/// <param name="Animated">The frames the room's animations paint over its tile characters.</param>
internal sealed record BossRoomCharacters(TilesetDefinition Tileset, BgArtworkBindings Content, IReadOnlyList<BgCharacterUpload> Loaded,
    IReadOnlyList<BgCharacterUpload> Rebound, IReadOnlyList<AnimatedCharacterPlacement> Animated);

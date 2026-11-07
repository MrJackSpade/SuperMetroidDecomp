using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Development-tool members of <see cref="RoomObjectCollectibleDrawDefinitions"/>; never linked by player hosts.</summary>
internal static class RoomObjectCollectibleDrawDefinitionsTooling
{
    /// <summary>The exported frame of <paramref name="request"/> when it is one of the twenty-four collectible draws.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectCollectibleDrawFrame frame)
    {
        frame = RoomObjectCollectibleVisualFiles.All.FirstOrDefault(candidate => RoomObjectDraw.Of(candidate.Draw) == request);
        return frame != default;
    }
}

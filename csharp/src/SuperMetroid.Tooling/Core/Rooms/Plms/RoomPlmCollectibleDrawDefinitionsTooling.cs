namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Development-tool members of <see cref="RoomPlmCollectibleDrawDefinitions"/>; never linked by player hosts.</summary>
internal static class RoomPlmCollectibleDrawDefinitionsTooling
{
    /// <summary>The exported frame of <paramref name="request"/> when it is one of the twenty-four collectible draws.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmCollectibleDrawFrame frame)
    {
        frame = CollectiblePlmDraws.All.FirstOrDefault(candidate => PlmDraw.Of(candidate.Draw) == request);
        return frame != default;
    }
}

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>The opening flight toward Ceres: its floor's blank cell and its music.</summary>
internal static class CeresFlightSceneDefinitions
{
    /// <summary>The floor character every cell outside the gunship's front or rear view shows.</summary>
    internal const byte BlankFloorCharacter = 140;
    /// <summary>The track of the flight's music set that starts with the flight.</summary>
    internal const byte MusicTrack = 5;
    /// <summary>The queue delay the flight's track is requested with.</summary>
    internal const ushort MusicDelay = 14;
}

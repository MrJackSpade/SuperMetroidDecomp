namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>Word offsets in a DemoRoomData record at bank $82.</summary>
internal static class AttractDemoSceneRoomFields
{
    /// <summary>Room identity word in a demo room record.</summary>
    public const int Room = 0;
    /// <summary>Entry door pointer word.</summary>
    public const int Door = 2;
    /// <summary>Door slot index word.</summary>
    public const int DoorSlot = 4;
    /// <summary>Initial camera X word.</summary>
    public const int CameraX = 6;
    /// <summary>Initial camera Y word.</summary>
    public const int CameraY = 8;
    /// <summary>Samus's vertical offset from the screen top.</summary>
    public const int SamusYFromTop = 10;
    /// <summary>Samus's horizontal offset from the screen center.</summary>
    public const int SamusXFromCenter = 12;
    /// <summary>Demo duration word.</summary>
    public const int Duration = 14;
    /// <summary>Demo setup routine pointer word.</summary>
    public const int Setup = 16;
}

namespace SuperMetroid.Core.Game;

/// <summary>Saved frontend dispatcher values consumed by $82:EEB4 after file selection.</summary>
public static class SaveLoadingGameStates
{
    /// <summary>$0000 selects the opening cinematic and a new Ceres start.</summary>
    public const ushort OpeningCinematic = 0x0000;
    /// <summary>$0005 resumes through the file-select area map and saved checkpoint.</summary>
    public const ushort MainGame = 0x0005;
    /// <summary>$001F resumes at the initial Ceres-elevator arrival.</summary>
    public const ushort CeresElevatorArrival = 0x001f;
    /// <summary>$0022 resumes the Ceres-destruction cinematic.</summary>
    public const ushort CeresDestruction = 0x0022;
}

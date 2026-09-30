namespace SuperMetroid.Core.Game;

/// <summary>Saved frontend dispatcher values consumed by $82:EEB4 after file selection.</summary>
public static class SaveLoadingGameStates
{
    /// <summary>$0000 selects the opening cinematic and a new Ceres start.</summary>
    public const ushort OpeningCinematic = 0x0000;
    /// <summary>
    /// $0005 resumes through the file-select area map and saved checkpoint.
    /// Gunship landing publishes this word at $A2:A9A0-$A9A3 before saving.
    /// </summary>
    public const ushort MainGame = 0x0005;
    /// <summary>
    /// $001F resumes at the initial Ceres-elevator arrival. FlyToCeres_StartGameAtCeres
    /// publishes it at $8B:C100-$C103 before saving; $82:8026 selects the Ceres loader.
    /// </summary>
    public const ushort CeresElevatorArrival = 0x001f;
    /// <summary>
    /// $0022 resumes the Ceres-destruction cinematic, not the activated Ceres station.
    /// Escape blackout publishes it at $82:83D6-$83D9 before saving; $82:EECC-$EED4
    /// selects CeresGoesBoom_Initial and $82:802B later selects Zebes station eighteen.
    /// </summary>
    public const ushort CeresDestruction = 0x0022;
}

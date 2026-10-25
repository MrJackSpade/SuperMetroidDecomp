namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>The title demonstrations' timing and the literal staging values their setups apply.</summary>
public static class AttractDemoDefinitions
{
    /// <summary>Frames a completed demo holds its final image before the title resumes.</summary>
    public const int FinalImageHoldFrames = 90;

    /// <summary>Frames a demo spends transferring its room's enemy graphics before playback starts.</summary>
    public const int EnemyGraphicsLoadFrames = 7;

    /// <summary>Energy the low-health demo setup leaves Samus with; her maximum energy is unchanged.</summary>
    public const int LowHealthEnergy = 20;

    /// <summary>The room screen whose scroll the Charge Beam demo closes with a red boundary.</summary>
    public const int ChargeBeamRedScrollScreen = 33;

    /// <summary>Frames Kraid's body waits before its next action in the Kraid demo.</summary>
    public const int KraidFunctionTimer = 60;
}

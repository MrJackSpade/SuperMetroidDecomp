using SuperMetroid.Core.Game;

/// <summary>Cartridge identities and fixture constants for issue 428's forced Blue Suit audit.</summary>
internal static class ForcedBlueAuditDefinitions
{
    /// <summary>SHA-256 of the unheadered Japan/USA revision-zero cartridge image.</summary>
    public const string RetailRomSha256 =
        "12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72";

    /// <summary>SHA-256 of the accepted six-row original-CPU trace.</summary>
    public const string NativeTraceSha256 =
        "B5972EF5BCD2667E1F9B91DDE91DFA81E3191100B9270CE26EE82A37976D5748";

    /// <summary><c>$90:94CB</c>, the drained-falling movement handler installed by command <c>$F7</c>.</summary>
    public const ushort DrainedFallingMovementHandler = 0x94cb;

    /// <summary><c>$90:E90E</c>, the RTS movement/input handler retained by Ceres Ridley's ejection.</summary>
    public const ushort RtsMovementOrInputHandler = 0xe90e;

    /// <summary><c>$90:A337</c>, the ordinary Samus movement dispatcher.</summary>
    public const ushort NormalMovementHandler = 0xa337;

    /// <summary><c>$91:E913</c>, the ordinary Samus pose-input dispatcher.</summary>
    public const ushort NormalPoseInputHandler = 0xe913;

    /// <summary><c>$90:D106</c>, the horizontal-shinespark movement handler.</summary>
    public const ushort HorizontalShinesparkMovementHandler = 0xd106;

    /// <summary>Enemy definition <c>$A3:D73F</c>, the normal room elevator actor.</summary>
    public const ushort ElevatorEnemyDefinition = 0xd73f;

    /// <summary>Automatic Reserve mode bit used by the cartridge's reserve-tank state.</summary>
    public const ushort AutomaticReserveMode = 1;

    /// <summary>Whole-pixel horizontal speed seeded by <c>$90:CFFA</c>.</summary>
    public const ushort InitialShinesparkExtraRunSpeed = 8;

    /// <summary>Returns the native pose-input pointer represented by the translated lock owner.</summary>
    public static ushort PoseInputHandler(SamusState samus) =>
        samus.ShinesparkPoseInputLocked ? RtsMovementOrInputHandler : NormalPoseInputHandler;
}

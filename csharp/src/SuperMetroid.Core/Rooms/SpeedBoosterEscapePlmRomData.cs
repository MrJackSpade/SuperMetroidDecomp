namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Named bank-$84 identities and operands used by the Speed Booster escape PLM.
/// </summary>
public static class SpeedBoosterEscapePlmRomData
{
    /// <summary><c>$84:B7EF</c>: wait for Speed Booster, then start the lavaquake.</summary>
    public const ushort WaitForSpeedBoosterPreInstruction = 0xb7ef;

    /// <summary><c>$84:B82A</c>: wait until Samus has run far enough left.</summary>
    public const ushort WaitForSamusLeftPreInstruction = 0xb82a;

    /// <summary><c>$84:B846</c>: advance lava stages as Samus continues left.</summary>
    public const ushort AdvanceLavaPreInstruction = 0xb846;

    /// <summary>Samus X threshold encoded by <c>CMP #$0AE0</c> at $84:B82A.</summary>
    public const ushort StartFxMotionSamusX = 0x0ae0;

    /// <summary>Pinned USA/Japan packed -$00.80 velocity written by $84:B80F.</summary>
    public const ushort InitialLavaquakeVelocity = 0xff80;
}

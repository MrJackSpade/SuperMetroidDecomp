namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Named bank-$84 identities and operands used by the Speed Booster escape PLM.
/// </summary>
public static class SpeedBoosterEscapePlmRomData
{
    /// <summary>Samus X threshold encoded by <c>CMP #$0AE0</c> at $84:B82A.</summary>
    public const ushort StartFxMotionSamusX = 0x0ae0;

    /// <summary>Pinned USA/Japan packed -$00.80 velocity written by $84:B80F.</summary>
    public const ushort InitialLavaquakeVelocity = 0xff80;
}

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Named bank-$84 identities and operands used by the Speed Booster escape PLM.
/// </summary>
/// <remarks>
/// The staged table remains ROM-backed at runtime so a regional cartridge supplies its own
/// velocity words. Constants here describe code identity and record shape, not guessed room
/// behavior.
/// </remarks>
public static class SpeedBoosterEscapePlmRomData
{
    /// <summary><c>$84:B7EF</c>: wait for Speed Booster, then start the lavaquake.</summary>
    public const ushort WaitForSpeedBoosterPreInstruction = 0xb7ef;

    /// <summary><c>$84:B82A</c>: wait until Samus has run far enough left.</summary>
    public const ushort WaitForSamusLeftPreInstruction = 0xb82a;

    /// <summary><c>$84:B846</c>: advance lava stages as Samus continues left.</summary>
    public const ushort AdvanceLavaPreInstruction = 0xb846;

    /// <summary>First word of the four-entry stage table at <c>$84:B876</c>.</summary>
    public const ushort StageTable = 0xb876;

    /// <summary>Target-X, maximum-FX-Y, and packed Y-velocity words per live stage.</summary>
    public const ushort StageByteCount = 6;

    /// <summary>Byte offset of the first word after the three live stage rows.</summary>
    public const ushort TerminatorOffset = 18;

    /// <summary>Samus X threshold encoded by <c>CMP #$0AE0</c> at $84:B82A.</summary>
    public const ushort StartFxMotionSamusX = 0x0ae0;

    /// <summary>USA/Japan packed -$00.80 velocity written by $84:B80F.</summary>
    public const ushort InitialLavaquakeVelocity = 0xff80;
}

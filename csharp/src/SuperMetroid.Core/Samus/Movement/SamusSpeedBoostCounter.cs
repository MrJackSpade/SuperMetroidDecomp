namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's Speed Booster counter: one native word whose high byte is the boost stage and whose low byte counts the
/// running cadence down to the next stage. It stays one word because the cartridge decrements the whole word: a
/// cadence already at zero borrows from the stage (reachable through the queue-selection-five reset, which leaves
/// stage four with a zero cadence). The four stage tests below disagree for stages above four and are kept apart.
/// </summary>
[SnesEncodingOwner]
public readonly record struct SamusSpeedBoostCounter
{
    /// <summary>The counter span of one stage: the cadence runs below it.</summary>
    private const int StageSpan = 0x100;

    /// <summary>The counter's stage field as the active-boost test masks it.</summary>
    private const int StageFieldMask = 0x0f00;

    /// <summary>Stage four, full speed boost, in the counter word.</summary>
    private const int StageFour = 0x0400;

    /// <summary>Holds the counter word, 0 through 65535.</summary>
    public SamusSpeedBoostCounter(int rawValue) => RawValue = Ensure.BetweenInclusive(0, ushort.MaxValue, rawValue);

    /// <summary>The counter word.</summary>
    public int RawValue { get; }

    /// <summary>The boost stage, 0 through 255 (the high byte).</summary>
    public int Stage => RawValue / StageSpan;

    /// <summary>Whether the stage field masked by <c>$0F00</c> is four: active boosting for blocks and contact.</summary>
    public bool IsActivelySpeedBoosting => (RawValue & StageFieldMask) == StageFour;

    /// <summary>Whether the stage-four bit (<c>$0400</c>) is set: the echo sound and lava tests read it alone.</summary>
    public bool HasActiveStageBit => (RawValue & StageFour) != 0;

    /// <summary>Whether the high byte is exactly four.</summary>
    public bool HighByteIsStageFour => Stage == StageFour / StageSpan;

    /// <summary>
    /// Whether a crouch stores a shinespark: the stage word compared with stage four by a signed subtraction, so stage
    /// four and any larger high byte below <c>$80</c> pass.
    /// </summary>
    public bool MeetsShinesparkStoreThreshold => unchecked((short)(Stage * StageSpan - StageFour)) >= 0;

    /// <summary>
    /// One animation command's full-word decrement, which borrows from the stage when the cadence is zero.
    /// </summary>
    /// <returns>The decremented counter and whether its cadence is now zero.</returns>
    public SpeedBoostCountdown DecrementForAnimationCommand()
    {
        var next = new SamusSpeedBoostCounter(unchecked((ushort)(RawValue - 1)));
        return new SpeedBoostCountdown(next, next.RawValue % StageSpan == 0);
    }

    /// <summary>The counter one stage higher; the caller checks that the stage-four bit is clear first.</summary>
    public SamusSpeedBoostCounter AdvanceStage() => new(RawValue + StageSpan);

    /// <summary>The current stage ORed with a cadence reset word, as the cartridge stores it.</summary>
    public SamusSpeedBoostCounter WithCadenceWord(ushort resetWord) => new(Stage * StageSpan | resetWord);
}

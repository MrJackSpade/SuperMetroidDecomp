/// <summary>Native program identities used by the independent Power Bomb motion schedule.</summary>
internal static class CrocomirePowerBombFixtureData
{
    /// <summary>$A4:BDAE/BDB2/BDB6: fully open, partially open, and closed mouth Power Bomb programs.</summary>
    public static readonly ushort[] ReactionLists = [0xbdae, 0xbdb2, 0xbdb6];
    /// <summary>$A4:D600: fully open mouth component examined by $B9DA.</summary>
    public const ushort FullyOpenComponent = 0xd600;
    /// <summary>$A4:D51C: partially open mouth component examined by $B9DA.</summary>
    public const ushort PartlyOpenComponent = 0xd51c;
    /// <summary>$A4:869E: native Power Bomb reaction enable/step count, three in this revision.</summary>
    public const int StepCount = 0xa4869e;
    /// <summary>$A4:80ED: Instruction_Common_GotoY, consumes a same-bank list pointer.</summary>
    public const ushort Goto = 0x80ed;
    /// <summary>$A4:9A9B..9AD7: five-byte-spaced dust callbacks with no list operands.</summary>
    public const ushort FirstDust = 0x9a9b, LastDust = 0x9ad7;
}

using SuperMetroid.Core.Rooms;

/// <summary>Checks every live floor word on every death frame against the retail crumble list.</summary>
internal sealed class KraidLiveSpikeAudit(RoomLevelData level)
{
    private readonly ushort[] _original = Enumerable.Range(0, KraidSpikeAuditData.BlockCount)
        .Select(x => level.GetCollisionBlock(KraidSpikeAuditData.Left + x, KraidSpikeAuditData.Row).LevelWord)
        .ToArray();
    private int _age = -1;

    public void Step()
    {
        ushort first = level.GetCollisionBlock(KraidSpikeAuditData.Left, KraidSpikeAuditData.Row).LevelWord;
        if (_age < 0 && first == _original[0]) return;
        _age++;
        for (int x = 0; x < _original.Length; x++)
        {
            int localAge = _age - x * KraidSpikeAuditData.FramesPerBlock;
            ushort expected = localAge < 0 ? _original[x] : localAge < 9
                ? KraidSpikeAuditData.CrumbleWords[localAge / 3]
                : KraidSpikeAuditData.ClearedWords[x % 2];
            ushort actual = level.GetCollisionBlock(KraidSpikeAuditData.Left + x, KraidSpikeAuditData.Row).LevelWord;
            if (actual != expected)
                throw new InvalidDataException($"Kraid spike sweep age {_age}, block {x}: expected {expected:X4}, actual {actual:X4}.");
        }
    }

    public void VerifyComplete()
    {
        if (_age < KraidSpikeAuditData.BlockCount * KraidSpikeAuditData.FramesPerBlock)
            throw new InvalidDataException("Kraid death never completed its live spike sweep.");
        Console.WriteLine($"Kraid live spike sweep: all 22 words matched the ROM animation cadence across {_age + 1} frames.");
    }
}

/// <summary>Independent expectations from $84:ABA9–ABD4, checked against the player's native death-state.</summary>
internal static class KraidSpikeAuditData
{
    /// <summary>$A7:C3F4 hardcoded starting X block.</summary>
    public const int Left = 5;
    /// <summary>$A7:C3F5 hardcoded floor row.</summary>
    public const int Row = 27;
    /// <summary>$84:ABAB seeds eleven iterations, each replacing two blocks.</summary>
    public const int BlockCount = 22;
    /// <summary>Four draw entries, each with duration three frames.</summary>
    public const int FramesPerBlock = 12;
    /// <summary>Level words drawn by $84:9367, $84:936D, and $84:9373.</summary>
    public static ReadOnlySpan<ushort> CrumbleWords => [0x8180, 0x8181, 0x0182];
    /// <summary>Alternating replacement words at $84:9391 and $84:9397.</summary>
    public static ReadOnlySpan<ushort> ClearedWords => [0x0111, 0x0110];
}

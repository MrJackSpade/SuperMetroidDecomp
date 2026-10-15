namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Bank-$91 revealed-block table and native command identities.</summary>
public static class XrayRevealTableWords
{
    /// <summary>Blank metatile selected by negative-coordinate extension exits at $91:CE95/$91:CED7.</summary>
    public const ushort BlankMetatile = 0x00ff;
    /// <summary>RevealedBlockTable_0 at $91:D2D6, keyed by the level word's block-type nibble.</summary>
    public const ushort BlockTypeTable = 0xd2d6;
    /// <summary>Bank containing the revealed-block tables and commands.</summary>
    public const int Bank = 0x910000;
    /// <summary>Native end marker for both block-type and BTS tables.</summary>
    public const ushort End = 0xffff;
    /// <summary>Native BTS wildcard; matches any byte BTS in the selected block family.</summary>
    public const ushort AnyBts = 0xff00;

    /// <summary>Whether <paramref name="word"/> is the reveal tables' end marker.</summary>
    public static bool IsEnd(ushort word) => word == End;

    /// <summary>Whether a reveal table's BTS key <paramref name="value"/> selects <paramref name="bts"/>, directly or as the any-BTS wildcard.</summary>
    public static bool MatchesBts(ushort value, int bts) => value == AnyBts || value == bts;
}

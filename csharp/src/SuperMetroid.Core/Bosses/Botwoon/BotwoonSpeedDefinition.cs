namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's health-dependent movement, body-history spacing and spit speed.</summary>
public readonly record struct BotwoonSpeedDefinition(int MovementSpeed, int SegmentSpacingBytes, int SpitSpeed)
{
    /// <summary>The spacing in history entries; every retail spacing is a whole number of four-byte entries.</summary>
    public int SegmentSpacingEntries => SegmentSpacingBytes / BotwoonNavigationDefinitions.HistoryEntryBytes;
}

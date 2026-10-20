namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What a regular fake-death draw list writes.</summary>
internal enum MotherBrainFakeDeathClear
{
    /// <summary>A solid fill row.</summary>
    FillRow,

    /// <summary>A tube cleared from the ceiling, keeping its ceiling endpoint.</summary>
    CeilingTube,

    /// <summary>A tube cleared from the floor, keeping its floor endpoint.</summary>
    FloorTube,
}

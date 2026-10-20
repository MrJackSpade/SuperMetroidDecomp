namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Whether a station collision checks the station's entry pose and direction before arming it.</summary>
public enum StationSetupGate
{
    /// <summary>The entry pose and direction must match.</summary>
    Checked,

    /// <summary>The check is skipped.</summary>
    Bypassed,
}

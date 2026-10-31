namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>One step of an authored Botwoon path: a single pixel in one of the eight directions.</summary>
internal enum BotwoonPathStep
{
    /// <summary>One pixel up.</summary>
    Up,
    /// <summary>One pixel up and one right.</summary>
    UpRight,
    /// <summary>One pixel right.</summary>
    Right,
    /// <summary>One pixel down and one right.</summary>
    DownRight,
    /// <summary>One pixel down.</summary>
    Down,
    /// <summary>One pixel down and one left.</summary>
    DownLeft,
    /// <summary>One pixel left.</summary>
    Left,
    /// <summary>One pixel up and one left.</summary>
    UpLeft,
}

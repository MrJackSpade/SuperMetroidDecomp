namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// A native frame record that shows the picture of the record before it again, straight after it, which a typed beat
/// shows as one frame of both durations (#2318): the beat's first record, and the updates its earlier records show.
/// </summary>
/// <param name="FirstRecord">The native address of the beat's first record.</param>
/// <param name="UpdatesBefore">The updates the beat's records before this one show.</param>
internal readonly record struct NativeFrameShownAgain(ushort FirstRecord, int UpdatesBefore);

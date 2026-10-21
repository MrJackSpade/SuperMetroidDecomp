namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>The top, bottom, x and y of one ridley native body record result.</summary>
/// <param name="Top">The top.</param>
/// <param name="Bottom">The bottom.</param>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
internal readonly record struct RidleyNativeBodyRecordResult(ushort Top, ushort Bottom, ushort X, ushort Y);

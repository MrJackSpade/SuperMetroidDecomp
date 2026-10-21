namespace SuperMetroid.Verification.Fx.PaletteFx;

/// <summary>The definition and destination of one stream.</summary>
/// <param name="Definition">The definition.</param>
/// <param name="Destination">The destination.</param>
internal readonly record struct Stream(ushort Definition, int Destination);

namespace SuperMetroid.Verification.Enemies.FuneNamihe;

/// <summary>The namihe, active and right of one decode result.</summary>
/// <param name="Namihe">The namihe.</param>
/// <param name="Active">The active.</param>
/// <param name="Right">The right.</param>
internal readonly record struct DecodeResult(bool Namihe, bool Active, bool Right);

namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>The low, high and phase of one expected result.</summary>
/// <param name="Low">The low.</param>
/// <param name="High">The high.</param>
/// <param name="Phase">The phase.</param>
internal readonly record struct ExpectedResult(ushort Low, ushort High, ushort Phase);

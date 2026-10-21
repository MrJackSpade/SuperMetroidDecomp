namespace SuperMetroid.Verification.Samus.Drawing;

/// <summary>The open, close and frame of one initial.</summary>
/// <param name="Open">The open.</param>
/// <param name="Close">The close.</param>
/// <param name="Frame">The frame.</param>
internal readonly record struct Initial(int Open, int Close, int Frame);

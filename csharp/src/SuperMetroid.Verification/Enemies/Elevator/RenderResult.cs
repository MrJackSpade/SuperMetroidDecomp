namespace SuperMetroid.Verification.Enemies.Elevator;

/// <summary>The physical and rendered of one render result.</summary>
/// <param name="Physical">The physical.</param>
/// <param name="Rendered">The rendered.</param>
internal readonly record struct RenderResult(ushort Physical, ushort Rendered);

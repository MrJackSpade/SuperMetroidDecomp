namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One ledge of Kraid's fingernail contour (#2400): the nails it pushes back and where it pushes them.</summary>
/// <param name="Top">
/// The ledge's bound, in pixels below his body: a nail lower than it is level with this ledge, unless a lower ledge's bound
/// already holds it.
/// </param>
/// <param name="Left">How far right of his body the ledge's edge lies, which a nail level with it is pushed back from.</param>
internal readonly partial record struct KraidNailLedge(int Top, int Left);

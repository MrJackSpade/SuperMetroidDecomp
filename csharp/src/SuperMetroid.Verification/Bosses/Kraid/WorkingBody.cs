using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>The top and bottom of one working body.</summary>
/// <param name="Top">The top.</param>
/// <param name="Bottom">The bottom.</param>
internal readonly record struct WorkingBody(KraidBodyPage Top, KraidBodyPage Bottom);

using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Fx;

/// <summary>An area-specific FX bit.</summary>
/// <param name="Area">The area.</param>
/// <param name="Bit">The bit.</param>
internal readonly record struct AreaFxBit(AreaId Area, int Bit);

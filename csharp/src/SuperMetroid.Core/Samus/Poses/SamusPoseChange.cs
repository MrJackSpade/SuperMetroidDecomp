
namespace SuperMetroid.Core.Samus.Poses;

/// <summary>A change from one pose to another.</summary>
/// <param name="From">The from.</param>
/// <param name="To">The to.</param>
internal readonly record struct SamusPoseChange(SamusPoseId From, SamusPoseId To);

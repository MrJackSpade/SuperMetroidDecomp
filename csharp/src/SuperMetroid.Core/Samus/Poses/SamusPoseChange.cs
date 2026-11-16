
namespace SuperMetroid.Core.Samus.Poses;

/// <summary>A change from one pose to another.</summary>
/// <param name="From">The pose Samus changes from.</param>
/// <param name="To">The pose she changes to.</param>
internal readonly record struct SamusPoseChange(SamusPoseId From, SamusPoseId To);

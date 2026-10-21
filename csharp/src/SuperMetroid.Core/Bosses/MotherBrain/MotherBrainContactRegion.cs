
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A contact hitbox region of one Mother Brain part.</summary>
/// <param name="Part">The part.</param>
/// <param name="Region">The region.</param>
internal readonly record struct MotherBrainContactRegion(MotherBrainContactPart Part, int Region);

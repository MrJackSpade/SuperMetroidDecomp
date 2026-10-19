namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The Tourian statue actors' list entries at $86:B79F-$B878; <see cref="TourianStatueProjectileAnimation"/> plays them typed.
/// </summary>
internal abstract class TourianStatueProjectileInstructionProgramDefinitions
{
    /// <summary>Unlocking-particle program at $86:B802.</summary>
    internal const ushort Particle = 0xb802;
    /// <summary>Unlocking-particle tail program at $86:B823.</summary>
    internal const ushort Tail = 0xb823;
    /// <summary>Ascending statue-soul program at $86:B84E.</summary>
    internal const ushort Soul = 0xb84e;

}

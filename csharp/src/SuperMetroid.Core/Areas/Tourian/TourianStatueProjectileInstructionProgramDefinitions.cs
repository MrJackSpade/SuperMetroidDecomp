
namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The Tourian statue actors' list entries at $86:B79F-$B878; <see cref="TourianStatueProjectileAnimation"/> plays them typed.
/// </summary>
internal abstract class TourianStatueProjectileInstructionProgramDefinitions
{
    /// <summary>Unlocking-particle water splash program at $86:B7A1.</summary>
    internal const ushort Splash = 0xb7a1;
    /// <summary>Boss-statue eye-glow program at $86:B7B3.</summary>
    internal const ushort EyeGlow = 0xb7b3;
    /// <summary>Unlocking-particle program at $86:B802.</summary>
    internal const ushort Particle = 0xb802;
    /// <summary>Unlocking-particle tail program at $86:B823.</summary>
    internal const ushort Tail = 0xb823;
    /// <summary>Ascending statue-soul program at $86:B84E.</summary>
    internal const ushort Soul = 0xb84e;
    /// <summary>Initial base-decoration program at $86:B85A.</summary>
    internal const ushort BaseDecoration = 0xb85a;
    /// <summary>Looping Ridley-statue program at $86:B86A.</summary>
    internal const ushort Ridley = 0xb86a;
    /// <summary>Looping Phantoon-statue program at $86:B872.</summary>
    internal const ushort Phantoon = 0xb872;

}

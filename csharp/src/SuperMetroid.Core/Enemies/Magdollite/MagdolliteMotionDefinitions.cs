namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Where Magdollite's parts hide and surface, and how long it waits between throws (#2043).</summary>
internal static class MagdolliteMotionDefinitions
{
    /// <summary>How far below its origin a dormant arm or the hidden overlay waits, in pixels.</summary>
    internal const int HiddenDepth = 32;

    /// <summary>How far below its origin the head and arm sink as the parts show, in pixels.</summary>
    internal const int SunkDepth = 24;

    /// <summary>How far below its origin the head and arm come to rest as the parts hide, in pixels.</summary>
    internal const int SurfacedDepth = 4;

    /// <summary>How long the head waits after a throw before it may attack again, in updates.</summary>
    internal const int AttackCooldown = 256;
}

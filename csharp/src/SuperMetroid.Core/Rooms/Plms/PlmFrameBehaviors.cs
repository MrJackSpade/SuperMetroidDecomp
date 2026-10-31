namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The shared inert pre-instruction. Every other pre-instruction is installed by name where its family's program or setup installs it.</summary>
internal static class PlmFrameBehaviors
{
    /// <summary>The frame behaviour every spawn installs and the clear step restores: no per-frame work.</summary>
    internal static PlmFrameBehavior Inert { get; } = PlmFrameBehavior.NoOp("inert");
}

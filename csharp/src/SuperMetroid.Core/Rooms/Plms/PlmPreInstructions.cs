namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The shared inert pre-instruction. Every other pre-instruction is installed by name where its family's program or setup installs it.</summary>
internal static class PlmPreInstructions
{
    /// <summary>The return every spawn installs and <c>$84:86CA</c> restores: no pre-instruction work.</summary>
    internal static PlmPreInstruction Inert { get; } = PlmPreInstruction.NoOp("inert", PlmPreInstructionDefinitions.SpawnDefault);
}

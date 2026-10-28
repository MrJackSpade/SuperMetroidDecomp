namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>Compiled initial program/function selections for Wrecked Ship Spark.</summary>
internal static class SparkMovementDefinitions
{
    /// <summary>
    /// The word Spark's initializer ORs into its properties before Phantoon is defeated. The retail <c>ORA $0100</c> reads
    /// stack-page bytes $0100-$0101: entries 6 and 7 of the VRAM write table, which depend on the cartridge's upload-queue
    /// history. Reproducing that needs the native queue the port does not keep, so the value is exempt under the
    /// emulation scope limits (owner decision on #1318): Spark behaves as if the read returned zero.
    /// </summary>
    internal const ushort StackPagePropertyOverlay = 0x0000;

    /// <summary>
    /// $A8:E682/$A8:E688: the three authored list/function pairs plus selector
    /// three's native adjacent-word observations. Its instruction-list lookup
    /// observes the first function word $E694, which is code rather than an animation
    /// list; its function lookup observes the following LDX opcode/operand word $54AE.
    /// </summary>
    internal static readonly SparkMovementDefinition[] InitialStates =
    [
        new(SparkProgram.Active, SparkEnemyFunction.AlwaysActive),
        new(SparkProgram.Active, SparkEnemyFunction.IntermittentActive),
        new(SparkProgram.Emitter, SparkEnemyFunction.EmitFallingSparks),
        new(null, SparkEnemyFunction.AdjacentCode),
    ];
}

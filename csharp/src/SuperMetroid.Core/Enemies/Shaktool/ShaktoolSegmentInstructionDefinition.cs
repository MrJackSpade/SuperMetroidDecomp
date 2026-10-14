namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Collision-recovery and dormant-attack programs for one Shaktool segment.</summary>
internal readonly record struct ShaktoolSegmentInstructionDefinition(
    ushort CollisionInstruction);

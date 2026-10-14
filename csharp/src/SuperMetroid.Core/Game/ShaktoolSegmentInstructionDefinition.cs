namespace SuperMetroid.Core.Game;

/// <summary>Collision-recovery and dormant-attack programs for one Shaktool segment.</summary>
internal readonly record struct ShaktoolSegmentInstructionDefinition(
    ushort CollisionInstruction);

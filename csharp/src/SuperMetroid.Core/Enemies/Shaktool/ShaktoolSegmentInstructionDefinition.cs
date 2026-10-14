namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>The collision-recovery animation for one Shaktool segment.</summary>
internal readonly record struct ShaktoolSegmentInstructionDefinition(
    ShaktoolProgram CollisionProgram);

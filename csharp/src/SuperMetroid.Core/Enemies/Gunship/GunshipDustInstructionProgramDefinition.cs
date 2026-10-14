namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>One of the six authored gunship liftoff-dust animation programs.</summary>
internal readonly record struct GunshipDustInstructionProgramDefinition(
    ushort Initial,
    ushort FirstFrame,
    ushort Terminal,
    GunshipDustDurations Durations);

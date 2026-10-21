namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>The name and apply of one drained pose history command.</summary>
/// <param name="Name">The name.</param>
/// <param name="Apply">The apply.</param>
internal readonly record struct DrainedPoseHistoryCommand(string Name, Action Apply);

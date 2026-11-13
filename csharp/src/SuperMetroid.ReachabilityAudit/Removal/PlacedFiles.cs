namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>What the relocation's last step wrote.</summary>
/// <param name="Written">The target files written or merged into.</param>
/// <param name="Deleted">The source files deleted because the move left them without a type.</param>
internal readonly record struct PlacedFiles(int Written, int Deleted);

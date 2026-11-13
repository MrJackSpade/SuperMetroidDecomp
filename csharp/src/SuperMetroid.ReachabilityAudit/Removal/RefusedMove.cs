namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>A move that stays for manual relocation because a member it reaches cannot become internal.</summary>
/// <param name="Move">The move.</param>
/// <param name="Reason">The member it reaches and why that member cannot become internal as it is.</param>
internal sealed record RefusedMove(RelocationMove Move, string Reason);

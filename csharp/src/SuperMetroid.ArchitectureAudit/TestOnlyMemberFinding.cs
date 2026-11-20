namespace SuperMetroid.ArchitectureAudit;

/// <summary>One test-only way of reaching a product member and what production does with it.</summary>
/// <param name="Uses">The member, the way and every place that reaches it that way.</param>
/// <param name="Shape">Whether production's own type reaches it that way (<c>narrow</c>), uses the member otherwise (<c>unneeded</c>) or never (<c>unused</c>).</param>
internal sealed record TestOnlyMemberFinding(MemberUses Uses, string Shape);

using Microsoft.CodeAnalysis;

namespace SuperMetroid.ArchitectureAudit;

/// <summary>
/// One way a product member can be reached (the member itself, or one accessor of a property) and where it is reached:
/// how often shipped code reaches it, inside and outside the member's type, and the first check that does.
/// </summary>
/// <param name="Member">The member.</param>
/// <param name="Way">The member as a whole (<c>use</c>), its getter (<c>read</c>) or its setter (<c>write</c>).</param>
internal sealed record MemberUses(ISymbol Member, string Way)
{
    /// <summary>How many places in shipped code outside the member's own type reach it this way.</summary>
    public int Production { get; set; }

    /// <summary>How many places in shipped code inside the member's own type, or a type nested in it, read it this way.</summary>
    public int InsideReads { get; set; }

    /// <summary>How many places in shipped code inside the member's own type, or a type nested in it, write it this way.</summary>
    public int InsideWrites { get; set; }

    /// <summary>How many places in checks reach it this way.</summary>
    public int Checks { get; set; }

    /// <summary>Every place in checks that reaches it this way.</summary>
    public List<SyntaxNode> CheckSites { get; } = [];

    /// <summary>The first check that reaches it this way, as <c>file:line</c>.</summary>
    public string? FirstCheck { get; set; }
}

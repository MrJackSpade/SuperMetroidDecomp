namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>How the room lists a Powamp's halves (#2556): its balloon right before its body.</summary>
internal static class PowampPartListingDefinitions
{
    /// <summary>The balloon, then the body.</summary>
    internal static EnemyComposite<PowampMember> Listing { get; } = new([PowampMember.Balloon, PowampMember.Body]);
}

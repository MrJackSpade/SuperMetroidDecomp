namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>How the room lists a Ki-Hunter's parts (#2556): its wings right after its body.</summary>
internal static partial class KiHunterPartListingDefinitions
{
    /// <summary>The body, then its wings.</summary>
    internal static EnemyComposite<KiHunterMember> Listing { get; } = new([KiHunterMember.Body, KiHunterMember.Wings]);
}

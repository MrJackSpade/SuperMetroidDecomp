namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>How the room lists a Hibashi's parts (#2556): its collision part right after its flame.</summary>
internal static partial class HibashiPartListingDefinitions
{
    /// <summary>The flame, then its collision part.</summary>
    internal static EnemyComposite<HibashiMember> Listing { get; } = new([HibashiMember.Flame, HibashiMember.Hitbox]);
}

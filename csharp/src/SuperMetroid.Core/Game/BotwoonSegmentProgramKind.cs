namespace SuperMetroid.Core.Game;

/// <summary>What a body-segment actor shows.</summary>
internal enum BotwoonSegmentProgramKind
{
    /// <summary>An outside body link: four frames that cycle (<c>InstList_EnemyProjectile_BotwoonsBody_*</c>).</summary>
    Body,
    /// <summary>The outside tail cap: one frame (<c>InstList_EnemyProjectile_BotwoonsTail_*</c>).</summary>
    Tail,
    /// <summary>A link or tail inside the wall: nothing drawn (<c>InstList_EnemyProjectile_BotwoonsBodyTail_Hidden</c>).</summary>
    Hidden,
}

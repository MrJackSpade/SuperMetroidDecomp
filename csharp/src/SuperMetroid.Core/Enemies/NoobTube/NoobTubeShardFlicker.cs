namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>
/// The flicker a n00b-tube shard's animation last ran (#2639): none yet, the reflected flicker that places the shard at its
/// column and its mirror image across the tube on alternate updates, or the hiding flicker that hides it on alternate updates.
/// The shard is drawn whole on every update whichever it runs; the flicker places it as the game does.
/// </summary>
internal enum NoobTubeShardFlicker
{
    /// <summary>No flicker has run.</summary>
    None,

    /// <summary>The reflected flicker: the shard, then its mirror image across the tube.</summary>
    Reflected,

    /// <summary>The hiding flicker: the shard, then nothing.</summary>
    Hiding,
}

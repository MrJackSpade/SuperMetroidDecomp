namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>
/// Whether a n00b-tube piece's animation shows its mirrored frames (#2571): a shard's reflected flicker shows them on the
/// updates it draws the shard at its mirror image across the tube. The animation creates it and its runner hands it to the
/// tube pieces' behaviour with each event.
/// </summary>
internal sealed partial class NoobTubeShardMirroring
{
    /// <summary>Whether the animation shows its mirrored frames.</summary>
    internal bool ShowsMirroredFrame { get; set; }
}

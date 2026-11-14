namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>What a cue of the Skree's and Metaree's animations does: keep processing it off screen, or tell main AI the dive may start.</summary>
internal enum SkreeMetareeCueKind
{
    /// <summary>Keeps processing the enemy while it is off screen.</summary>
    ProcessOffScreen,
    /// <summary>Tells main AI the wind-up is done and the dive may start.</summary>
    SetAttackReady,
}

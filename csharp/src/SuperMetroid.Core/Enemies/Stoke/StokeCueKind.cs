namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>What a cue of Stoke's animations does: make main AI walk Stoke left or right, or spit a fireball.</summary>
internal enum StokeCueKind
{
    /// <summary>Makes main AI walk Stoke left.</summary>
    SetMovingLeft,
    /// <summary>Makes main AI walk Stoke right.</summary>
    SetMovingRight,
    /// <summary>Spits a fireball in the direction the cue names.</summary>
    SpawnFireball,
}

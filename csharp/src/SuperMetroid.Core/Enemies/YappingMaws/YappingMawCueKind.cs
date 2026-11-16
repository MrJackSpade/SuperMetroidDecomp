namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>What a cue of the Yapping Maw's animations does: hold a grabbed Samus at a mouth offset, or play the bite sound.</summary>
internal enum YappingMawCueKind
{
    /// <summary>Holds a grabbed Samus at the mouth offset of the cue's aim.</summary>
    HoldSamus,
    /// <summary>Plays the bite sound when the Yapping Maw is on screen.</summary>
    PlayBiteSound,
}

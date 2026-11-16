namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>What a signalling cue of the Fune's and Namihe's animations does: play the spit sound, or return to idle.</summary>
internal enum FuneNamiheCueKind
{
    /// <summary>Returns to the idle animation and the waiting action.</summary>
    FinishActivity,
    /// <summary>Plays the spit sound.</summary>
    SpitSound,
}

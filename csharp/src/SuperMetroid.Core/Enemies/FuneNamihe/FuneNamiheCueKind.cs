namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>What a cue of the Fune's and Namihe's programs does: one per kind of step its lists use.</summary>
internal enum FuneNamiheCueKind
{
    /// <summary>Returns to the idle animation and the waiting function.</summary>
    FinishActivity,
    /// <summary>Spits a Fune fireball to the left.</summary>
    FuneFireLeft,
    /// <summary>Spits a Fune fireball to the right.</summary>
    FuneFireRight,
    /// <summary>Spits a Namihe fireball to the left.</summary>
    NamiheFireLeft,
    /// <summary>Spits a Namihe fireball to the right.</summary>
    NamiheFireRight,
    /// <summary>Plays the spit sound.</summary>
    SpitSound,
}

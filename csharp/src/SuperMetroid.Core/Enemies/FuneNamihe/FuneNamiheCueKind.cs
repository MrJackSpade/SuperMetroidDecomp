namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>What a cue of the Fune's and Namihe's animations does: spit a Fune or Namihe fireball left or right, play the spit sound, or return to idle.</summary>
internal enum FuneNamiheCueKind
{
    /// <summary>Returns to the idle animation and the waiting action.</summary>
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

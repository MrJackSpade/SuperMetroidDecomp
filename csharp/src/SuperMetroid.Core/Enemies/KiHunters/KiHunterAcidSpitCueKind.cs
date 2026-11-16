namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>What a signalling cue of the Ki-Hunter acid's animations does: stop it as its splash plays, or delete it.</summary>
internal enum KiHunterAcidSpitCueKind
{
    /// <summary>Stops the acid's movement as its splash plays.</summary>
    StopMoving,

    /// <summary>Deletes the acid.</summary>
    Delete,
}

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>What Draygon's cannon programs do besides drawing (#1296).</summary>
internal enum DraygonCannonCue
{
    /// <summary>Links the facing's hit program for the missile wake.</summary>
    LinkHit,
    /// <summary>Installs the missile-hit pre-instruction.</summary>
    ArmMissileWake,
    /// <summary>Counts a hit; the third (or a Super Missile's) destroys the cannon.</summary>
    CountHit,
    /// <summary>Destroys the cannon: disables its firing word and writes its destroyed terrain.</summary>
    Damage,
}

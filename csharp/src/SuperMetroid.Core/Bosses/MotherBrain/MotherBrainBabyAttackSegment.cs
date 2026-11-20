namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The runs of frames her four-ring volley at the arrived Baby Metroid is made of (#2554), whose cues act on the Baby its
/// playback carries: the volleys her murder of the Baby fires once it has arrived, and her last one at it.
/// </summary>
public enum MotherBrainBabyAttackSegment
{
    /// <summary>Her damaged head counts an attack on the Baby Metroid and aims its onion rings at it.</summary>
    AttackingBabyMetroid,
    /// <summary>
    /// Her damaged head opens wide, cries unless it has attacked the Baby eleven times, and fires the first of four onion
    /// rings at the Baby: the volley's opening.
    /// </summary>
    FourOnionRingsPhase3,
    /// <summary>Her damaged head, its mouth at its widest, fires one more onion ring at the Baby.</summary>
    OnionRingShotPhase3,
    /// <summary>Her damaged head closes its mouth after its volley's last onion ring and lets her neck move again.</summary>
    OnionRingVolleyEndPhase3,
    /// <summary>Her damaged head idles after the volley, its mouth opening and closing.</summary>
    NeutralPhase3,
    /// <summary>Her damaged head holds its idling mouth open after the volley, and on a chance holds it again.</summary>
    NeutralPhase3Hold,
}

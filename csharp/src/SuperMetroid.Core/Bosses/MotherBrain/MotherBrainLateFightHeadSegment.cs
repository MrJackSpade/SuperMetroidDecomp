namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The runs of frames her late fight's four-ring volley at Samus is made of (#2554), whose cues act on the late fight its
/// playback carries: the volley her recovery from a Hyper Beam recoil and her third phase's attacks fire, and the one her
/// murder of the Baby fires before the Baby has arrived.
/// </summary>
public enum MotherBrainLateFightHeadSegment
{
    /// <summary>Her damaged head clears its count of attacks on the Baby Metroid and aims its onion rings at Samus.</summary>
    AttackingSamusFourOnionRingsPhase3,
    /// <summary>
    /// Her damaged head opens wide, cries unless it has attacked the Baby eleven times, and fires the first of four onion
    /// rings where it aimed them: the volley's opening.
    /// </summary>
    FourOnionRingsPhase3,
    /// <summary>Her damaged head, its mouth at its widest, fires one more onion ring where it aimed them.</summary>
    OnionRingShotPhase3,
    /// <summary>Her damaged head closes its mouth after its volley's last onion ring and lets her neck move again.</summary>
    OnionRingVolleyEndPhase3,
    /// <summary>Her damaged head idles after the volley, its mouth opening and closing.</summary>
    NeutralPhase3,
    /// <summary>Her damaged head holds its idling mouth open after the volley, and on a chance holds it again.</summary>
    NeutralPhase3Hold,
}

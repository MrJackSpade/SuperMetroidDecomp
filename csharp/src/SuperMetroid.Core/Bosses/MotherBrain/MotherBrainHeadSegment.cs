namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The runs of frames Mother Brain's brain (head) animations are made of, played by her dedicated brain animation rather
/// than an enemy's animation (#1948). An animation plays its own runs in order; a run another one continues into is named for
/// what it holds.
/// </summary>
public enum MotherBrainHeadSegment
{
    /// <summary>Her whole head stretches: it shakes, opens wide with a roar and a big purple breath, and drools.</summary>
    StretchingPhase2,
    /// <summary>Her whole head holds its stretched, open mouth.</summary>
    StretchingPhase2Held,
    /// <summary>Her damaged head stretches: it shakes, opens wide with a roar and a big purple breath, and drools.</summary>
    StretchingPhase3,
    /// <summary>Her damaged head holds its stretched, open mouth.</summary>
    StretchingPhase3Held,
    /// <summary>Her damaged head is thrown back by a Hyper Beam hit: it shakes and opens wide with a roar, without drool.</summary>
    HyperBeamRecoil,
    /// <summary>Her damaged head holds its open mouth after the recoil.</summary>
    HyperBeamRecoilHeld,
    /// <summary>Her head before it is damaged, held still with its eye shut.</summary>
    WholeHeadMouthClosed,
    /// <summary>Her damaged head closes its open mouth as her body falls away.</summary>
    Decapitated,
    /// <summary>Her decapitated head holds its closed mouth.</summary>
    DecapitatedHeld,
    /// <summary>Her dying head shakes and opens its mouth with a roar.</summary>
    DyingDrool,
    /// <summary>Her dying head drools once from its widest mouth; a burst of drool plays it over and over.</summary>
    DyingDroolBurst,
    /// <summary>Her dying head holds its widest mouth, and on a chance bursts into more drool.</summary>
    DyingDroolLoop,
    /// <summary>Her whole head holds its open mouth while the rainbow beam fires.</summary>
    FiringRainbowBeam,
    /// <summary>Her whole head idles, its mouth opening and closing.</summary>
    NeutralPhase2,
    /// <summary>Her whole head holds its idling mouth open, and on a chance holds it again.</summary>
    NeutralPhase2Hold,
    /// <summary>Her damaged head idles, its mouth opening and closing.</summary>
    NeutralPhase3,
    /// <summary>Her damaged head holds its idling mouth open, and on a chance holds it again.</summary>
    NeutralPhase3Hold,
    /// <summary>Her damaged head goes still as her corpse.</summary>
    Corpse,
    /// <summary>Her corpse's head slumped, still for good.</summary>
    CorpseHeld,
    /// <summary>
    /// Her whole head opens wide with an attack cry and fires the first of four onion rings at Samus: the four-ring
    /// volley's opening.
    /// </summary>
    FourOnionRingsPhase2,
    /// <summary>Her whole head, its mouth at its widest, aims and fires one more onion ring at Samus.</summary>
    OnionRingShotPhase2,
    /// <summary>Her whole head closes its mouth after a volley's last onion ring and lets her neck move again.</summary>
    OnionRingVolleyEndPhase2,
    /// <summary>
    /// Her whole head opens wide with an attack cry and fires the first of two onion rings at Samus: the two-ring volley's
    /// opening.
    /// </summary>
    TwoOnionRingsPhase2,
    /// <summary>Her damaged head counts an attack on the Baby Metroid and aims its onion rings at it.</summary>
    AttackingBabyMetroid,
    /// <summary>Her damaged head clears its count of attacks on the Baby Metroid and aims its onion rings at Samus.</summary>
    AttackingSamusFourOnionRingsPhase3,
    /// <summary>
    /// Her damaged head opens wide, cries unless this is her silent attack (<see cref="MotherBrainFightDefinitions.SilentBabyAttackCount"/>), and fires the first of her onion
    /// rings where it aimed them: the volley's opening.
    /// </summary>
    FourOnionRingsPhase3,
    /// <summary>Her damaged head, its mouth at its widest, fires one more onion ring where it aimed them.</summary>
    OnionRingShotPhase3,
    /// <summary>Her damaged head closes its mouth after its volley's last onion ring and lets her neck move again.</summary>
    OnionRingVolleyEndPhase3,
    /// <summary>Her whole head opens wide with an attack cry and lobs a bomb that leaves seven afterburns.</summary>
    BombPhase2,
    /// <summary>Her damaged head opens wide with an attack cry and lobs a bomb that leaves one afterburn.</summary>
    BombPhase3,
    /// <summary>Her whole head opens its mouth and fires its laser.</summary>
    Laser,
    /// <summary>Her whole head closes its mouth as the rainbow beam starts charging.</summary>
    ChargingRainbowBeam,
    /// <summary>Her whole head holds its closed mouth while the rainbow beam's charge gathers.</summary>
    ChargingRainbowBeamLoop,
}

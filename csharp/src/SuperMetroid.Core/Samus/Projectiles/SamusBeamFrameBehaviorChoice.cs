namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A beam frame behaviour the fire handlers select by charge state and beams, including what the Spazer and Plasma
/// combinations run (<see cref="UnauthoredBeamFrameBehaviors{TBeams, TBehavior}"/>).
/// </summary>
internal enum SamusBeamFrameBehaviorChoice
{
    /// <summary>A beam without Wave: ordinary terrain-stopping beam motion.</summary>
    NoWave,
    /// <summary>Wave or Ice-Wave: Wave movement with a three-frame trail reload.</summary>
    WaveThreeFrameTrail,
    /// <summary>Wave-Plasma and the other Wave combinations: Wave movement with a four-frame trail reload.</summary>
    WaveFourFrameTrail,
    /// <summary>
    /// The uncharged Spazer-Plasma shot's: it runs whatever an unconnected region holds, which is outside the emulation
    /// scope limit (#1500), so the shot is refused loudly.
    /// </summary>
    UnchargedSpazerPlasmaRunsUnconnectedRegion,
    /// <summary>
    /// The uncharged Spazer-Plasma-Ice shot's, the Spacetime shot: from an animation that reaches far enough, the shot
    /// turns saved progression on and off as
    /// <see cref="SuperMetroid.Core.Compatibility.SpaceTimeProgressionOverwrite{TCursor, TOverwrite}"/> decodes it.
    /// </summary>
    SpaceTimeProgressionOverwrite,
    /// <summary>
    /// The uncharged Spazer-Plasma-Wave shot's, the Chainsaw: it runs a Power Bomb's fuse over the shot. The store it
    /// starts with changes only the display's window edges, a presentation quirk the port does not reproduce (#1407).
    /// </summary>
    ChainsawPowerBomb,
    /// <summary>
    /// The charged Spazer-Plasma-Wave and Spazer-Plasma-Ice shots': they run whatever Samus's changing state holds, so
    /// the game's execution depends on that state and may crash; outside the emulation scope limit (#1500), the shot is
    /// refused when it runs.
    /// </summary>
    ChargedChainsawRunsSamusState,
    /// <summary>
    /// The uncharged all-beams shot's: it runs whatever the game's live state holds, which is outside the emulation
    /// scope limit (#1500), so the shot is refused loudly.
    /// </summary>
    UnchargedAllBeamsRunsLiveState,
    /// <summary>
    /// The charged Spazer-Plasma shot's: it runs whatever the game's live state holds, which is outside the emulation
    /// scope limit (#1500), so the shot is refused loudly.
    /// </summary>
    ChargedSpazerPlasmaRunsLiveState,
    /// <summary>
    /// The charged all-beams shot's, the Murder Beam: it starts in the middle of an unrelated part of the game. The safe
    /// left-facing Murder Beam has no animation, so the game never runs it; the other directions keep their animations
    /// and run it.
    /// </summary>
    MurderBeamMisalignedExecution,
}

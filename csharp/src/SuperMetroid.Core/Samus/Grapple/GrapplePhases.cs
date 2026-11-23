namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Reads of the grapple phases' definitions (<see cref="GrapplePhaseDefinitions"/>).</summary>
internal static partial class GrapplePhases
{
    /// <summary>What a grapple phase does beside moving the beam.</summary>
    extension(GrapplePhase phase)
    {
        /// <summary>Whether the grapple keeps its sound in the phase.</summary>
        internal bool PlaysGrappleSound => GrapplePhaseDefinitions.SoundPlaying.Contains(phase);
    }
}

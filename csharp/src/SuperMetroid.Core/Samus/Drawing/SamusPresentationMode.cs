namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>How Samus is presented: as the player's Samus in play, or as a figure a cinematic shows.</summary>
public enum SamusPresentationMode
{
    /// <summary>The player's Samus in play: her footsteps, landings and hurt make their sounds and effects.</summary>
    Gameplay,

    /// <summary>
    /// The intro cinematic's flashback Samus: her footsteps, landings, spin jumps and hurt impact make no sound, her
    /// hurt flash restores the intro's colours, and Crateria shows no footstep or landing effects for her.
    /// </summary>
    Cinematic,
}

namespace SuperMetroid.Core.Enemies;

/// <summary>The room-level effects Mother Brain's fight drives.</summary>
public interface IMotherBrainRoomEffects
{
    /// <summary>
    /// Counts a shot that hurt Mother Brain's head as a hit on her glass, at once and apart from the glass's own count of
    /// strikes on its block; a hit once the glass has gone changes nothing.
    /// </summary>
    void CountHeadShotOnGlass();

    /// <summary>
    /// Confirms, as Mother Brain's body sets up, that her room set her glass up, which her head shots count on.
    /// </summary>
    /// <exception cref="InvalidDataException">Her room set up no glass.</exception>
    void ConfirmGlassLoaded();
}

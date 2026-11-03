namespace SuperMetroid.Core.Assets;

/// <summary>
/// What a fade of the whole gameplay display leaves in its own colours (#1512): the time-up whiteout fades everything, a
/// door's fade spares the HUD, whose steps the import shades because the door keeps some of it lit, and the death fades
/// spare Samus, who stays lit while the rest of the display darkens.
/// </summary>
public enum GameplayFadeSpares
{
    /// <summary>Every picture and every sprite draw shows the fade.</summary>
    Nothing,

    /// <summary>The HUD's pictures keep their own colours, as a door's fade keeps some of them lit.</summary>
    Hud,

    /// <summary>Samus's draws keep their own colours, as the death fade keeps her lit.</summary>
    Samus,
}

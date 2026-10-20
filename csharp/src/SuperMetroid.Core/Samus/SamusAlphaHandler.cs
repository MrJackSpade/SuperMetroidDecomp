namespace SuperMetroid.Core.Samus;

/// <summary>The frame handler's alpha, <c>SamusCurrentStateHandler</c> (WRAM $0A42), as Samus commands install it.</summary>
internal enum SamusAlphaHandler
{
    /// <summary>
    /// <c>$90:E695 SamusCurrentStateHandler_Normal</c>: radius, HUD selection, block-inside reactions, pose input, bombs
    /// and projectiles.
    /// </summary>
    Normal,

    /// <summary>
    /// <c>$90:E713 SamusCurrentStateHandler_SamusIsLocked</c>, or the inert <c>RTL_90E8CD</c> of the suit and death
    /// commands: none of the normal alpha's input owners run.
    /// </summary>
    Locked,
}

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Gameplay with a blend outside a window (#1514): the frontmost thing at each pixel outside the window blends as
/// <see cref="Blend"/> says, with the layers <see cref="OverlayBehind"/> and <see cref="BackgroundBehind"/> place behind it
/// or with the blend's colour. The X-ray scope's cone shows the room unblended inside it and blends the room outside it;
/// Phantoon's translucent body, Fireflea's darkness, the waterfall rooms and the spores blend everywhere, a closed window.
/// The paint belongs to the containing scene, and this layer also draws the unchanged HUD.
/// </summary>
public sealed record GameplayBlendLayer : DisplayLayer
{
    /// <summary>The gameplay layers and actors, composed before the blend.</summary>
    public GameplaySceneLayer Gameplay { get; }
    /// <summary>The window the X-ray scope's cone lights (#1468): inside it the room shows unblended, revealing blocks when <see cref="RevealBlocks"/> says.</summary>
    public ScreenWindow Window { get; }
    /// <summary>Whether the X-ray reveal's blocks replace the room's blocks they reveal.</summary>
    public bool RevealBlocks { get; }
    /// <summary>What blends, how, and with what colour.</summary>
    public GameplayBlend Blend { get; }
    /// <summary>
    /// The room's effect overlay lying behind what blends, which the blend combines with, or null for none: the waterfall
    /// and the spores blend with it, and the X-ray with the room's liquid or fog.
    /// </summary>
    public OverlayBlendLayer? OverlayBehind { get; }
    /// <summary>
    /// Whether the room's background lies behind what blends, which the blend combines with: Phantoon's translucent body
    /// shows the background through it, and the waterfall blends with the background and its overlay.
    /// </summary>
    public bool BackgroundBehind { get; }

    /// <summary>Creates gameplay with a blend outside a window.</summary>
    /// <param name="gameplay">The gameplay layers and actors.</param>
    /// <param name="window">The window inside which the room shows unblended; <see cref="ScreenWindow.Closed"/> blends everywhere.</param>
    /// <param name="revealBlocks">Whether to show the X-ray reveal's blocks in place of the room's.</param>
    /// <param name="blend">What blends, how, and with what colour.</param>
    /// <param name="overlayBehind">The effect overlay lying behind what blends, or null for none.</param>
    /// <param name="backgroundBehind">Whether the room's background lies behind what blends.</param>
    [IndependentFlag("independent facts of the frame: the X-ray block reveal, and the background lying behind the blend")]
    public GameplayBlendLayer(GameplaySceneLayer gameplay, ScreenWindow window, bool revealBlocks, GameplayBlend blend,
        OverlayBlendLayer? overlayBehind = null, bool backgroundBehind = false)
    {
        Ensure.NotNull(gameplay);
        Ensure.NotNull(window);
        if (!Enum.IsDefined(blend.Operation))
            throw new ArgumentOutOfRangeException(nameof(blend), blend.Operation, "Undefined blend operation.");
        Gameplay = gameplay;
        Window = window;
        RevealBlocks = revealBlocks;
        Blend = blend;
        OverlayBehind = overlayBehind;
        BackgroundBehind = backgroundBehind;
    }
}

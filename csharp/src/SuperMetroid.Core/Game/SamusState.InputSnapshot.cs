namespace SuperMetroid.Core.Game;

public sealed partial class SamusState
{
    /// <summary>
    /// WRAM $0E00, joypad1_newinput_samusfilter: the Fire edge retained by $90:EAB3
    /// after drawing Samus and projectiles. Grapple's inactive function accepts either
    /// this previous-frame edge or the current NMI edge, allowing a press that first
    /// exits a non-firing pose to fire on the next frame. Demo alpha replaces it with
    /// the preceding scripted edge before running the normal HUD dispatcher.
    /// </summary>
    public ushort PreviousDrawNewInput { get; internal set; }
}

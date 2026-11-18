namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Whether a Puyo that strikes a ceiling while rising turns back on its next hop (#1592), which depends on what was drawn
/// last before the strike. The Puyo keeps hopping its way only when the last thing drawn was an enemy, or an effect actor
/// inside the area around the view that effect actors are drawn in, rather than the HUD's redraw or the room's load, and
/// the charge flare has not drawn since the HUD last redrew. Anything else turns it back.
/// </summary>
/// <remarks>
/// <para>
/// So a strike in an ordinary gameplay update always turns the Puyo back: the HUD redraws after every gameplay update's
/// draw pass, before the next update's enemies run, and nothing is drawn between. A strike during the source room's door
/// waits and fades, which draw without redrawing the HUD, turns it back only when the update before drew no enemy and no
/// effect actor in that area, or drew the charge flare since the HUD's last redraw. From the room's load to the HUD's
/// first redraw in the destination (the fade in and the first gameplay update) the strike turns it back, because the
/// room's load counts both as the last thing drawn and as a charge flare drawn.
/// </para>
/// <para>
/// Told of the HUD's redraw after each gameplay update's draw pass (<see cref="HudRedrawn"/>), of each enemy the draw
/// pass draws (<see cref="EnemyDrawn"/>), of each effect actor it draws (<see cref="EffectActorDrawn"/>) and of the
/// charge flare's draw (<see cref="ChargeFlareDrawn"/>); created with the room's enemy system at the room's load;
/// consulted by a rising Puyo's ceiling strike (<see cref="TurnsBack"/>).
/// </para>
/// <para>
/// Evidence: the Puyo's movement at <c>$A2:9BBF</c> loads the scratch word at <c>$01</c> (bytes <c>$01</c> and <c>$02</c>)
/// into its invert-direction flag, and the next hop tests only zero against nonzero. Byte <c>$01</c> is the high byte of
/// the word at <c>$00</c>, which the enemy and sprite-object draws set to a character number below 256 (zero high byte),
/// and byte <c>$02</c> is the bank of a long address, which the charge flare's draw and the room's load set and the HUD
/// clears. Its writers, inventoried statically
/// over every routine that writes <c>$00</c> to <c>$02</c>: the HUD routine at <c>$80:9B44</c> clears <c>$02</c> and
/// leaves <c>$00</c> naming its digit tilemap in bank <c>$80</c>; the enemy drawing routine at <c>$A0:94C5</c> stores the
/// enemy's tile offset in <c>$00</c>, which the enemy graphics layout keeps below 256 (the import's graphics-set layout
/// rejects a set that would not); the sprite-object drawing routine at <c>$B4:BD7C</c> stores the object's palette word
/// masked to its tile base in <c>$00</c> for an object inside its window, a base every spawner takes as zero, an enemy's
/// palette and tile offset, or a fixed palette word; the charge-beam graphics routine at <c>$90:BAFC</c> and its flare
/// component draw set <c>$02</c> to a bank whenever a flare component is drawn; the door's loading reloads the beam
/// palette at <c>$90:AC8D</c>, which stores a bank in <c>$02</c>. Every other writer stores a table's long address, and
/// runs only in rooms whose enemy populations hold no Puyo (the scrolling sky, the morph-ball eye's beam, the x-ray scope,
/// Crocomire's melting, the Dachora's palette, the Baby Metroid's and Mother Brain's spritemaps), on menus, pause or the
/// title, or only where the HUD or the room's load follows before any Puyo runs (music uploads, Samus's animation delays,
/// the minimap, grapple's beam palette, room-object drawing). Interrupts never write those bytes.
/// </para>
/// </remarks>
[CompatibilityService("a Puyo that strikes a ceiling while rising turns back unless an enemy or an effect actor in view was the last thing drawn and the charge flare has not drawn since the HUD's last redraw")]
public sealed class PuyoCeilingTurnFromLastDraw
{
    /// <summary>How far left of the view the area effect actors are drawn in begins, in pixels.</summary>
    private const int EffectActorWindowLeftMargin = 16;

    /// <summary>How wide the area effect actors are drawn in is, in pixels, from its left edge.</summary>
    private const int EffectActorWindowWidth = 288;

    /// <summary>How tall the area effect actors are drawn in is, in pixels, from the top of the view.</summary>
    private const int EffectActorWindowHeight = 272;

    /// <summary>
    /// Whether the last thing drawn was the HUD's redraw or the room's load, rather than an enemy or an effect actor
    /// inside the area around the view that effect actors are drawn in.
    /// </summary>
    private bool _lastDrawnWasHudOrRoomLoad = true;

    /// <summary>Whether the charge flare has drawn, or the room has loaded, since the HUD last redrew.</summary>
    private bool _chargeFlareDrawnSinceHudRedraw = true;

    /// <summary>Whether a Puyo striking a ceiling while rising now turns back on its next hop.</summary>
    public bool TurnsBack => _lastDrawnWasHudOrRoomLoad || _chargeFlareDrawnSinceHudRedraw;

    /// <summary>
    /// The HUD redrew after a gameplay update's draw pass: it becomes the last thing drawn, and the charge flare has not
    /// drawn since.
    /// </summary>
    public void HudRedrawn()
    {
        _lastDrawnWasHudOrRoomLoad = true;
        _chargeFlareDrawnSinceHudRedraw = false;
    }

    /// <summary>The draw pass drew an enemy, which becomes the last thing drawn.</summary>
    public void EnemyDrawn() => _lastDrawnWasHudOrRoomLoad = false;

    /// <summary>
    /// The draw pass reached an effect actor at the given place relative to the view's top left corner; one inside the
    /// area around the view that effect actors are drawn in becomes the last thing drawn, and one outside it is not drawn.
    /// </summary>
    /// <param name="fromViewLeft">How far right of the view's left edge the object lies, in pixels; negative to its left.</param>
    /// <param name="fromViewTop">How far below the view's top edge the object lies, in pixels; negative above it.</param>
    public void EffectActorDrawn(int fromViewLeft, int fromViewTop)
    {
        int fromWindowLeft = fromViewLeft + EffectActorWindowLeftMargin;
        if (fromWindowLeft is >= 0 and < EffectActorWindowWidth && fromViewTop is >= 0 and < EffectActorWindowHeight)
            _lastDrawnWasHudOrRoomLoad = false;
    }

    /// <summary>The draw pass drew a component of the charge flare.</summary>
    public void ChargeFlareDrawn() => _chargeFlareDrawnSinceHudRedraw = true;
}

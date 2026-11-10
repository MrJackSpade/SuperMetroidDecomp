namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Whether a Puyo that strikes a ceiling while rising turns back on its next hop (#1592). The strike copies a value the
/// room's latest drawing and HUD work left behind, and the Puyo's next hop turns away from Samus unless that value is
/// clear. The value has two parts. Its first part is rewritten by every enemy the draw pass draws and by every sprite
/// object it draws inside its window, with the character the picture counts from, which always lies within the first
/// 256 and so reads clear; the HUD's redraw and the room's load leave it naming one of their tables, which never reads
/// clear. Its second part is cleared by the HUD's redraw and set by the charge flare's draw and by the room's load.
/// </summary>
/// <remarks>
/// <para>
/// So a strike in an ordinary gameplay update always turns the Puyo back: the HUD redraws after every gameplay update's
/// draw pass, before the next update's enemies run, and nothing between clears the value. A strike during the source
/// room's door waits and fades, which draw without redrawing the HUD, turns it back only when the update before drew no
/// enemy and no sprite object in the window, or drew the charge flare since the HUD's last redraw. From the room's load
/// to the HUD's first redraw in the destination (the fade in and the first gameplay update) the strike turns it back.
/// </para>
/// <para>
/// Told of the HUD's redraw after each gameplay update's draw pass (<see cref="HudRedrawn"/>), of each enemy the draw
/// pass draws (<see cref="EnemyDrawn"/>), of each sprite object it draws (<see cref="SpriteObjectDrawn"/>) and of the
/// charge flare's draw (<see cref="ChargeFlareDrawn"/>); created with the room's enemy system at the room's load, which
/// loads the beams' colours and leaves both parts set; consulted by a rising Puyo's ceiling strike
/// (<see cref="TurnsBack"/>).
/// </para>
/// <para>
/// Evidence: the Puyo's movement at <c>$A2:9BBF</c> loads the scratch word at <c>$01</c> (bytes <c>$01</c> and <c>$02</c>)
/// into its invert-direction flag, and the next hop tests only zero against nonzero. Its writers, inventoried statically
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
[CompatibilityService("a Puyo that strikes a ceiling while rising turns back unless the room's last draw left the shared value clear")]
public sealed class PuyoCeilingTurnFromLastDraw
{
    /// <summary>How far left of the view the sprite-object draw's window begins, in pixels.</summary>
    private const int SpriteObjectWindowLeftMargin = 16;

    /// <summary>How wide the sprite-object draw's window is, in pixels, from its left edge.</summary>
    private const int SpriteObjectWindowWidth = 288;

    /// <summary>How tall the sprite-object draw's window is, in pixels, from the top of the view.</summary>
    private const int SpriteObjectWindowHeight = 272;

    /// <summary>
    /// Whether the value's first part was last written by the HUD's redraw or the room's load, naming a table, rather than
    /// by an enemy's or sprite object's draw, which leaves it clear.
    /// </summary>
    private bool _firstPartNamesTable = true;

    /// <summary>Whether the value's second part has been set since the HUD last cleared it.</summary>
    private bool _secondPartSet = true;

    /// <summary>Whether a Puyo striking a ceiling while rising now turns back on its next hop.</summary>
    public bool TurnsBack => _firstPartNamesTable || _secondPartSet;

    /// <summary>The HUD redrew after a gameplay update's draw pass: the first part names its digits, the second is clear.</summary>
    public void HudRedrawn()
    {
        _firstPartNamesTable = true;
        _secondPartSet = false;
    }

    /// <summary>The draw pass drew an enemy, whose picture's first character leaves the first part clear.</summary>
    public void EnemyDrawn() => _firstPartNamesTable = false;

    /// <summary>
    /// The draw pass reached a sprite object at the given place relative to the view's top left corner; one inside the
    /// draw's window leaves the first part clear, and one outside it writes nothing.
    /// </summary>
    /// <param name="fromViewLeft">How far right of the view's left edge the object lies, in pixels; negative to its left.</param>
    /// <param name="fromViewTop">How far below the view's top edge the object lies, in pixels; negative above it.</param>
    public void SpriteObjectDrawn(int fromViewLeft, int fromViewTop)
    {
        int fromWindowLeft = fromViewLeft + SpriteObjectWindowLeftMargin;
        if (fromWindowLeft is >= 0 and < SpriteObjectWindowWidth && fromViewTop is >= 0 and < SpriteObjectWindowHeight)
            _firstPartNamesTable = false;
    }

    /// <summary>The draw pass drew a component of the charge flare, which sets the second part.</summary>
    public void ChargeFlareDrawn() => _secondPartSet = true;
}

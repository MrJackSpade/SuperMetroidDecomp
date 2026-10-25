using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.World;

/// <summary>The colours a door's source-room fade keeps (#1405), as <c>GameState_A_LoadingNextRoom</c> copies them at $82:E1F1-$E264.</summary>
public static class DoorTransitionColourDefinitions
{
    /// <summary>The ordinary green visor colour $82:E52B loads when door scrolling completes.</summary>
    public static readonly RgbColor DoorCompletionVisorGreen = RgbColor.FromWord(0x3be0);

    /// <summary>The minimap's explored colours and its explored feature colour, always kept.</summary>
    public static readonly BackgroundColourRun MinimapExplored = new(0, 9, 2);

    /// <summary>The minimap's unexplored colours and its unexplored feature colour, always kept.</summary>
    public static readonly BackgroundColourRun MinimapUnexplored = new(0, 13, 2);

    /// <summary>The highlighted HUD item's background outline, background and outline, always kept.</summary>
    public static readonly BackgroundColourRun HighlightedItem = new(1, 1, 3);

    /// <summary>The minimap's room highlight, always kept.</summary>
    public static readonly BackgroundColourRun MinimapRoomHighlight = new(1, 13, 1);

    /// <summary>The HUD's background and its items' background outline, background and outline, kept between common rooms.</summary>
    public static readonly BackgroundColourRun HudItems = new(1, 4, 4);

    /// <summary>The minimap's background, kept between common rooms.</summary>
    public static readonly BackgroundColourRun MinimapBackground = new(1, 12, 1);

    /// <summary>The escape timer's first two colours, kept between common rooms while it runs.</summary>
    public static readonly SpriteColourRun TimerFirst = new(5, 1, 2);

    /// <summary>The escape timer's fourth colour, kept between common rooms while it runs.</summary>
    public static readonly SpriteColourRun TimerFourth = new(5, 4, 1);

    /// <summary>The escape timer's thirteenth colour, kept between common rooms while it runs.</summary>
    public static readonly SpriteColourRun TimerThirteenth = new(5, 13, 1);

    /// <summary>The colours every door's source fade keeps.</summary>
    public static readonly IColourSetRun[] Hud = [MinimapExplored, MinimapUnexplored, HighlightedItem, MinimapRoomHighlight];

    /// <summary>The colours a fade between two common rooms also keeps.</summary>
    public static readonly IColourSetRun[] CommonRoom = [HudItems, MinimapBackground];

    /// <summary>The colours a fade between two common rooms also keeps while the escape timer runs.</summary>
    public static readonly IColourSetRun[] EscapeTimer = [TimerFirst, TimerFourth, TimerThirteenth];
}

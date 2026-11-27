namespace Engine.World;

/// <summary>
/// Where what a room shows is drawn against the rest of it, from the back to the front: the depths a room's
/// pictures lie at. Things at one depth are drawn in the order they stand in the room. A thing with a place of its
/// own against the room's tiles takes one of these in place of the depth of the room's things.
/// </summary>
public static class RoomDepth
{
    /// <summary>The colour shown wherever nothing of the room is drawn: behind everything.</summary>
    public const int BackdropColour = -200;

    /// <summary>The picture behind the room.</summary>
    public const int Backdrop = -100;

    /// <summary>In front of the backdrop and behind the room's tiles and everything standing in the room.</summary>
    public const int BehindRoom = -20;

    /// <summary>The room's tiles, which the room gives its terrain.</summary>
    public const int Tiles = -10;

    /// <summary>
    /// In front of the room's tiles and behind everything standing in the room: where a thing puts what it shows
    /// behind the player and the rest, whatever order they stand in.
    /// </summary>
    public const int BehindThings = -5;

    /// <summary>
    /// In front of the room's tiles and behind everything standing in the room, what stands rearmost too: where
    /// something launched lies that all the room's own things are drawn over.
    /// </summary>
    public const int BehindAllThings = -7;

    /// <summary>Behind the things standing in the room at their own depth, and in front of what is behind them all.</summary>
    public const int FarBehindThings = -3;

    /// <summary>
    /// Everything standing in the room, in the order it stands there: the depth a thing has unless it says otherwise.
    /// The player is in front of it.
    /// </summary>
    public const int Things = 0;

    /// <summary>Behind the player and in front of the things standing in the room at their own depth.</summary>
    public const int JustBehindPlayer = 3;

    /// <summary>
    /// The player, whom the room gives this depth as it takes them in: so what stands behind them or in front of
    /// them does so whenever it came into the room.
    /// </summary>
    public const int Player = 5;


    /// <summary>In front of the player: what they pass behind, as a door's shutter or an enemy that stands in front of them.</summary>
    public const int InFrontOfThings = 10;

    /// <summary>In front of what stands in front of the player.</summary>
    public const int Frontmost = 12;

    /// <summary>
    /// What is launched and left in the room in play: shots, and what a shot or a death leaves. In
    /// front of everything standing in the room, what stands frontmost too, and behind the room's own front. A thing
    /// of these kinds that is told where it stands takes that place in its stead.
    /// </summary>
    public const int Launched = 13;

    /// <summary>
    /// In front of what is launched and left in the room: where something launched lies that is drawn over the rest
    /// of what is launched, as the bursts of the player's own shots are over what enemies launch. Behind the room's
    /// own front.
    /// </summary>
    public const int OverLaunched = 14;

    /// <summary>
    /// The part of the picture behind the room that lies in front of the things standing in it, which a backdrop
    /// draws again there: behind the tiles' own such part.
    /// </summary>
    public const int BackdropInFront = 15;

    /// <summary>The part of the room's tiles that lies in front of the things standing in it.</summary>
    public const int TilesInFront = 20;

    /// <summary>In front of every tile of the room, the ones in front of its things too: where a pickup lies, and an enemy whose data says so.</summary>
    public const int InFrontOfRoom = 30;

    /// <summary>The room's liquid: over the room and what is in it, under the overlays.</summary>
    public const int Liquid = 90;

    /// <summary>What is laid over the whole room.</summary>
    public const int Overlay = 100;

    /// <summary>The depth of a standing against the player.</summary>
    public static int Of(Standing standing) => standing switch
    {
        Standing.Rearmost => BehindThings,
        Standing.FarBehind => FarBehindThings,
        Standing.Behind => Things,
        Standing.JustBehind => JustBehindPlayer,
        Standing.InFront => InFrontOfThings,
        Standing.Frontmost => Frontmost,
        Standing.Hindmost => BehindAllThings,
        Standing.Foremost => Launched,
        Standing.Topmost => OverLaunched,
        _ => throw new ArgumentOutOfRangeException(nameof(standing), standing, "Undefined standing."),
    };

    /// <summary>
    /// Has <paramref name="node"/> stand so against the player, whatever it is a part of: a thing of the room, or one
    /// picture of a thing whose other pictures stand elsewhere.
    /// </summary>
    public static void Stand(Godot.Node2D node, Standing standing)
    {
        ArgumentNullException.ThrowIfNull(node);
        node.ZAsRelative = false;
        node.ZIndex = Of(standing);
    }
}

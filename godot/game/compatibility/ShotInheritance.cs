using Engine.Shared;

namespace Game.Compatibility;

/// <summary>
/// What a shot inherits of Samus's movement as the cartridge reckons it. Each move of her body records how far it
/// went, in the place of the way it set out: leftward, rightward, upward, downward, each replacing the last record
/// of its way. A shot fired a way inherits the whole pixels last recorded that way, counted within 256 and read as
/// -128 to 127, and for its part of a pixel the top part of the record before it in that order, the first taking
/// the view's own speed down instead. So a shot to the left takes part of how the view last moved down, and one to
/// the right part of her last move left. A shot upward takes a quarter of what it would inherit, and always as a
/// rise.
/// </summary>
/// <remarks>
/// The records are cleared once her shots have been fired, before she next moves; the view's speed is read as the
/// shot is fired. The cartridge reads each way's speed from storage that runs one place into the record before it.
/// </remarks>
public sealed class ShotInheritance
{
    private const int Left = 0, Right = 1, Up = 2, Down = 3;

    /// <summary>The parts of a pixel an inherited speed is counted in.</summary>
    private const int SpeedParts = 256;

    /// <summary>The whole pixels an inherited speed holds before they come round.</summary>
    private const int WholeCycle = 256;

    private readonly Pixels[] _moved = new Pixels[4];

    /// <summary>Records a move across or down: how far it set out to go, negative leftward or upward, and how far it went.</summary>
    public void Record(bool across, Pixels asked, Pixels moved) =>
        _moved[across ? (asked.IsNegative ? Left : Right) : asked.IsNegative ? Up : Down] = moved;

    /// <summary>Clears the four records.</summary>
    public void Clear() => Array.Clear(_moved);

    /// <summary>
    /// The speed a shot leaves at, fired the way given (-1, 0 or 1 across and down) at <paramref name="own"/> along
    /// each axis it flies on, with <paramref name="viewSpeedDown"/> the view's speed down on its last update.
    /// </summary>
    public PixelVelocity Launch(int across, int down, Pixels own, Pixels viewSpeedDown)
    {
        Pixels x = across > 0 ? Inherited(Right, viewSpeedDown) + own : across < 0 ? Inherited(Left, viewSpeedDown) - own : Pixels.Zero;
        Pixels y = down > 0 ? Inherited(Down, viewSpeedDown) + own : down < 0 ? Rising(viewSpeedDown) - own : Pixels.Zero;
        return new PixelVelocity(x, y);
    }

    /// <summary>The parts of a pixel, 256 to the pixel, of what the record at <paramref name="place"/> gives a shot.</summary>
    private int InheritedParts(int place, Pixels viewSpeedDown)
    {
        Pixels before = place == 0 ? viewSpeedDown : _moved[place - 1];
        return WholeInCycle(_moved[place]) * SpeedParts + before.Fraction / (Pixels.FractionsPerPixel / SpeedParts);
    }

    private Pixels Inherited(int place, Pixels viewSpeedDown) => FromParts(InheritedParts(place, viewSpeedDown));

    /// <summary>
    /// What a shot upward takes: nothing while the upward record has no whole pixel, otherwise a quarter of what it
    /// would inherit, rounded down, and taken as a rise: a quarter that reads as downward is moved a quarter of the
    /// speed's whole range further up.
    /// </summary>
    private Pixels Rising(Pixels viewSpeedDown)
    {
        if (WholeInCycle(_moved[Up]) == 0)
            return Pixels.Zero;
        int inherited = InheritedParts(Up, viewSpeedDown);
        int quarter = (int)Math.Floor(inherited / 4.0);
        return FromParts(inherited < 0 ? quarter : quarter - WholeCycle / 4 * SpeedParts);
    }

    /// <summary>The whole pixels of a record within their cycle of 256, read as -128 to 127.</summary>
    private static int WholeInCycle(Pixels moved)
    {
        int place = ((moved.Whole % WholeCycle) + WholeCycle) % WholeCycle;
        return place >= WholeCycle / 2 ? place - WholeCycle : place;
    }

    private static Pixels FromParts(int parts)
    {
        int whole = (int)Math.Floor(parts / (double)SpeedParts);
        return Pixels.FromParts(whole, (parts - whole * SpeedParts) * (Pixels.FractionsPerPixel / SpeedParts));
    }
}

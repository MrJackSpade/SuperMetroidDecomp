namespace Game.Compatibility;

/// <summary>
/// How far the two echoes of a shinespark's crash have turned about Samus, as the cartridge keeps it: in the height
/// its first echo is drawn at once that echo is let go. A crash does not start the count over. Each update the echoes
/// circle adds four, and they stop circling once the count, taken from 128 as a number that may be below nothing,
/// is nothing or more. So a crash after one that let no first echo go begins at the 128 or more the last ended on and
/// circles for one update; a crash after a first echo that left the screen, or after a room was entered, begins at
/// nothing and circles for its thirty-two; and a first echo still flying leaves its height, which shortens or ends
/// the circling of a crash begun meanwhile.
/// </summary>
/// <remarks>
/// How long the echoes circle moves the update on which she is free again. The crash moves the count on and asks of
/// it on every update it circles; the crash's end leaves her height as it lets the first echo go; that echo leaves
/// its own height on every update it flies on the screen and nothing on the update it leaves; and entering a room
/// leaves nothing. Hers for as long as she is.
/// </remarks>
public sealed class CrashOrbitTravel
{
    /// <summary>The most the count holds; one step more goes on from nothing.</summary>
    private const int Most = 65535;

    /// <summary>The count at which the echoes stop circling, and what one circling update adds.</summary>
    private const int End = 128, Step = 4;

    private int _travel;

    /// <summary>Adds one circling update to the count. Returns whether the echoes stop circling.</summary>
    public bool Advance()
    {
        int advanced = _travel + Step;
        _travel = advanced > Most ? advanced - (Most + 1) : advanced;
        return unchecked((short)(_travel - End)) >= 0;
    }

    /// <summary>The first echo leaves a height: hers as it is let go, then its own on each update it flies. A height off the room's top is counted round from its most.</summary>
    public void LeaveHeight(int height) => _travel = ((height % (Most + 1)) + Most + 1) % (Most + 1);

    /// <summary>The first echo has left the screen, or a room has been entered: the count is nothing.</summary>
    public void Clear() => _travel = 0;
}

using Engine.World;
using Godot;

namespace Game.Rooms;

/// <summary>
/// The bubbling of lava: every so many of the liquid's moves, while its surface lies within the room, one of the
/// bubbling sounds is asked for, chosen by the number chance last gave, read and not drawn. It counts with the
/// liquid's moves as each update opens, so it does not count while the room is held still.
/// </summary>
/// <remarks>
/// The first move after the room is set up only starts the count, as it only starts the cartridge's liquid. The
/// cartridge chooses by the number as it stood before lava stirred it on the same move, and the stir has changed its
/// halves round since: the digits it chose by are now the lowest of the high half.
/// </remarks>
public partial class LavaBubbling : RoomSequence, IOpeningStep
{
    /// <summary>The liquids that bubble, by their types' names.</summary>
    [Export]
    public string[] Liquids { get; set; } = ["lava"];

    /// <summary>The bubbling sounds, by name, which the number's lowest digits take in turn and round again.</summary>
    [Export]
    public Godot.Collections.Array<string> Sounds { get; set; } = [];

    /// <summary>The liquid's moves from one bubbling to the next.</summary>
    [Export]
    public int Interval { get; set; } = 112;

    /// <summary>How many digits of the number choose a sound.</summary>
    private const int ChoiceDigits = 3;

    /// <summary>Where the digits the cartridge chose by lie once lava has stirred the number: the high half's lowest.</summary>
    private const int StirredChoiceAt = 8;

    /// <summary>Whether the first move, which only starts the count, has passed.</summary>
    private bool _counting;

    /// <summary>The moves left until the next bubbling, that one counted.</summary>
    private int _left;

    /// <inheritdoc/>
    protected override void OnStarted()
    {
        if (Interval < 1)
            throw new InvalidOperationException($"The lava's bubbling \"{Name}\" comes every {Interval} moves; it must be one or more.");
        if (Sounds.Count == 0)
            throw new InvalidOperationException($"The lava's bubbling \"{Name}\" names no sounds.");
        _counting = false;
        _left = Interval;
    }

    /// <inheritdoc/>
    public void StepOpening(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        if (!_counting)
        {
            _counting = true;
            return;
        }

        if (room.Liquid is not { } liquid || !Liquids.Contains(liquid.Type.Name) || liquid.Surface.Whole < 0)
            return;
        _left--;
        if (_left > 0)
            return;
        _left = Interval;
        int choice = room.Session.Random.Last.Part(StirredChoiceAt, ChoiceDigits);
        room.Session.Sounds.Request(Sounds[choice % Sounds.Count], channel: "room");
    }

    /// <summary>Its work is all in the opening of each update; it never finishes.</summary>
    protected override bool Play(Room room, int elapsed) => false;
}

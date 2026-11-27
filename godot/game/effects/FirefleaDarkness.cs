using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Effects;

/// <summary>
/// The dark of a Fireflea room. The room's light comes from its Firefleas: it pulses gently, and each Fireflea
/// that dies leaves the room darker, until with five gone it is as dark as it gets. It is told to "darken". The
/// dark is taken from the colours of what lies behind the room, its backdrops, as the cartridge subtracts it: so
/// many of a colour's thirty-one levels from each of its three parts. The room's tiles, Samus and everything that
/// stands in the room keep their colours.
/// </summary>
public partial class FirefleaDarkness : RoomEffect
{
    /// <summary>How dark the room is with so many Firefleas gone, each out of thirty-one.</summary>
    private static readonly int[] DarknessLevels = [0, 6, 12, 18, 24, 25];

    /// <summary>How much darker the pulse makes it, step by step, each out of thirty-one.</summary>
    private static readonly int[] Pulse = [0, 1, 2, 3, 4, 5, 6, 5, 4, 3, 2, 1];

    /// <summary>How many updates each step of the pulse lasts.</summary>
    private const int PulseStepUpdates = 6;

    private int _killed;
    private int _pulse;
    private int _wait = PulseStepUpdates;

    /// <summary>The name its dark is taken from the backdrops' colours under.</summary>
    private const string Source = "FirefleaDarkness";

    /// <summary>The room it darkens, from its setting up on.</summary>
    private Room? _in;

    /// <inheritdoc/>
    protected override void OnSetup(Room room)
    {
        (_killed, _pulse, _wait) = (0, 0, PulseStepUpdates);
        _in = room;
        Darken(room);
    }

    /// <inheritdoc/>
    public override void Triggered(string what)
    {
        if (what != "darken")
        {
            // Started or stopped, its dark is given or taken back at once.
            base.Triggered(what);
            if (_in is { } room)
                Darken(room);
            return;
        }

        _killed = Math.Min(_killed + 1, DarknessLevels.Length - 1);
    }

    /// <inheritdoc/>
    protected override void OnStep(Room room)
    {
        // The step showing plays out its time whatever happens. With the room at its darkest, the step that follows
        // is the pulse's own darkest, and it is held there: the pulse does not jump to it.
        if (--_wait <= 0)
        {
            _wait = PulseStepUpdates;
            _pulse = _killed >= DarknessLevels.Length - 1 ? Array.IndexOf(Pulse, 6) : (_pulse + 1) % Pulse.Length;
        }

        Darken(room);
    }

    /// <summary>Takes the dark as it now stands from the backdrops' colours; none while it is not at work.</summary>
    private void Darken(Room room)
    {
        float dark = Math.Min(31, DarknessLevels[_killed] + Pulse[_pulse]) / 31f;
        room.ShiftBackdrops(Source, IsActive ? ColourShift.Taking(new Color(dark, dark, dark)) : ColourShift.None);
    }
}

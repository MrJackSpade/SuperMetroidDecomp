using Godot;

namespace Engine.World;

/// <summary>
/// A room-wide condition: something true of the whole room, such as heat. It owns its look, and reports that it is
/// active and any ongoing damage it does. It never reaches into the player or enemies; they ask the room and apply
/// what it reports to themselves. It can be told to "start" and "stop". The room steps it as an update ends, after
/// everything that may start it or tell it something.
/// </summary>
public abstract partial class RoomEffect : Node2D, IPlaced, ITriggerTarget
{
    /// <summary>The effect's name, which is also the name of its ongoing damage.</summary>
    [Export]
    public string EffectName { get; set; } = "";

    /// <summary>The ongoing damage it does each update while active, in hundredths of a point.</summary>
    [Export]
    public int DamageHundredths { get; set; }

    /// <summary>Whether the effect is active when the room is set up.</summary>
    [Export]
    public bool StartsActive { get; set; } = true;

    /// <inheritdoc/>
    [Export]
    public Condition? PresentWhen { get; set; }

    /// <summary>Whether the effect is active right now.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Puts the effect in its starting state.</summary>
    internal void Setup(Room room)
    {
        IsActive = StartsActive;
        OnSetup(room);
    }

    /// <summary>Called once as the room is set up.</summary>
    protected virtual void OnSetup(Room room)
    {
    }

    /// <summary>
    /// Whether the effect takes its step as an update ends, once everything that may start it or tell it something
    /// has run, and so shows on that same update what it was told. Unless its kind says so, an effect takes its step
    /// as an update opens, before anyone acts: started or told something during an update, it first shows it on the
    /// next.
    /// </summary>
    protected internal virtual bool StepsAsUpdateEnds => false;

    /// <summary>Runs one update of the effect while it is active.</summary>
    internal void Step(Room room)
    {
        if (IsActive)
            OnStep(room);
    }

    /// <summary>Called each update while the effect is active.</summary>
    protected virtual void OnStep(Room room)
    {
    }

    /// <summary>Starts or stops the effect.</summary>
    protected void SetActive(bool active) => IsActive = active;

    /// <inheritdoc/>
    public virtual void Triggered(string what)
    {
        IsActive = what switch
        {
            "start" => true,
            "stop" => false,
            _ => throw new ArgumentException($"A room effect can be told to \"start\" or \"stop\", not \"{what}\".", nameof(what)),
        };
    }
}

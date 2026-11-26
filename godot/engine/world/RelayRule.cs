using Godot;

namespace Engine.World;

/// <summary>
/// One thing a <see cref="Relay"/> does on hearing a word. Any of its parts may be left empty: it tells a thing a
/// word, puts a terrain patch in place or takes one back, changes the music, shakes the room, puts a thing into the
/// room, and records a fact.
/// </summary>
[GlobalClass]
public partial class RelayRule : Resource
{
    /// <summary>The word that sets this off.</summary>
    [Export]
    public string Hears { get; set; } = "";

    /// <summary>The thing told, if any.</summary>
    [Export]
    public NodePath Target { get; set; } = new();

    /// <summary>What it is told.</summary>
    [Export]
    public string Tells { get; set; } = "";

    /// <summary>The terrain patch put in place, by name; empty for none.</summary>
    [Export]
    public string AppliesPatch { get; set; } = "";

    /// <summary>The terrain patch taken back, by name; empty for none.</summary>
    [Export]
    public string RevertsPatch { get; set; } = "";

    /// <summary>Whether the music changes.</summary>
    [Export]
    public bool ChangesMusic { get; set; }

    /// <summary>The music it changes to, by name; empty for silence.</summary>
    [Export]
    public string Music { get; set; } = "";

    /// <summary>How hard the room shakes; zero for no shake.</summary>
    [Export]
    public int ShakeStrength { get; set; }

    /// <summary>For how many updates it shakes.</summary>
    [Export]
    public int ShakeUpdates { get; set; }

    /// <summary>The fact recorded, by name; empty for none.</summary>
    [Export]
    public string Records { get; set; } = "";
}

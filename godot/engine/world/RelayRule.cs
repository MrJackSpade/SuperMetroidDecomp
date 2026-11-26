using Godot;

namespace Engine.World;

/// <summary>
/// One thing a <see cref="Relay"/> does on hearing a word: it puts a terrain patch in place, and may shake the room.
/// </summary>
[GlobalClass]
public partial class RelayRule : Resource
{
    /// <summary>The word that sets this off.</summary>
    [Export]
    public string Hears { get; set; } = "";

    /// <summary>The terrain patch put in place, by name; empty for none.</summary>
    [Export]
    public string AppliesPatch { get; set; } = "";

    /// <summary>How hard the room shakes; zero for no shake.</summary>
    [Export]
    public int ShakeStrength { get; set; }

    /// <summary>For how many updates it shakes.</summary>
    [Export]
    public int ShakeUpdates { get; set; }
}

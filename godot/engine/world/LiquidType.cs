using Engine.Content;

namespace Engine.World;

/// <summary>
/// A kind of liquid: how it slows what moves in it, the ongoing damage it does, which equipment cancels each, and how
/// it looks.
/// </summary>
/// <remarks>
/// <code>
/// { "name": "lava", "speed_times": 1, "speed_over": 2, "slowing_cancelled_by": ["gravity suit"],
///   "damage_hundredths": 50, "damage_cancelled_by": ["gravity suit"], "picture": "liquids/lava.png" }
/// </code>
/// </remarks>
public sealed class LiquidType
{
    /// <summary>The liquid's name, which is also the name of its ongoing damage.</summary>
    public string Name { get; private init; } = "";

    /// <summary>Movement in the liquid is at this many…</summary>
    public int SpeedTimes { get; private init; } = 1;

    /// <summary>…over this many of the usual speed.</summary>
    public int SpeedOver { get; private init; } = 1;

    /// <summary>The equipment that lets its owner move in the liquid as in air.</summary>
    public IReadOnlyList<string> SlowingCancelledBy { get; private init; } = [];

    /// <summary>The ongoing damage it does each update, in hundredths of a point.</summary>
    public int DamageHundredths { get; private init; }

    /// <summary>The equipment that protects its owner from that damage.</summary>
    public IReadOnlyList<string> DamageCancelledBy { get; private init; } = [];

    /// <summary>The picture its surface is drawn with; empty for none.</summary>
    public string Picture { get; private init; } = "";

    /// <summary>The effect left where something crosses its surface, by the data file of its timeline; empty for none.</summary>
    public string SplashEffect { get; private init; } = "";

    /// <summary>The sound of something crossing its surface, by name; empty for none.</summary>
    public string SplashSound { get; private init; } = "";

    /// <summary>Reads a liquid type from its data file.</summary>
    public static LiquidType Read(DataNode data)
    {
        IReadOnlyList<string> Names(string name) => data.Has(name) ? data.List(name).Select(item => item.AsText()).ToList() : [];
        var type = new LiquidType
        {
            Name = data.Text("name"),
            SpeedTimes = data.Has("speed_times") ? data.Int("speed_times") : 1,
            SpeedOver = data.Has("speed_over") ? data.Int("speed_over") : 1,
            SlowingCancelledBy = Names("slowing_cancelled_by"),
            DamageHundredths = data.Has("damage_hundredths") ? data.Int("damage_hundredths") : 0,
            DamageCancelledBy = Names("damage_cancelled_by"),
            Picture = data.Has("picture") ? data.Text("picture") : "",
            SplashEffect = data.Has("splash") ? data.Text("splash") : "",
            SplashSound = data.Has("splash_sound") ? data.Text("splash_sound") : "",
        };
        if (type.SpeedOver <= 0 || type.SpeedTimes < 0 || type.DamageHundredths < 0)
            throw data.Problem("gives a speed share or a damage that is not a sensible number");
        return type;
    }
}

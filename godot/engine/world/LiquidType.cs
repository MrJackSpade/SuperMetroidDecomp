using Engine.Content;
using Engine.Shared;

namespace Engine.World;

/// <summary>
/// A kind of liquid: how it slows what moves in it, the ongoing damage it does, which equipment cancels each, and how
/// it looks.
/// </summary>
/// <remarks>
/// <code>
/// { "name": "lava", "speed_times": 1, "speed_over": 2, "slowing_cancelled_by": ["gravity suit"],
///   "damage_hundredths": 50, "damage_cancelled_by": ["gravity suit"],
///   "surface": { "steps": [ { "picture": "liquids/{graphics}/lava_surface_0.png", "updates": 13 } ], "loops_to": 0 },
///   "body": { "steps": [ { "picture": "liquids/{graphics}/lava_body_0.png", "updates": 13 } ], "loops_to": 0 },
///   "blend": "add", "drift": 0 }
/// </code>
/// "surface" is the timeline of the strip drawn along its surface, whose offset says how far above the surface the
/// strip's picture starts, and "body" that of the picture tiled under the strip to the bottom of the view; both are needed to draw it, and a liquid with neither is not drawn. "blend" says
/// how they are drawn over the room, as an overlay's does. "drift" is how far they move across each update in a
/// room whose liquid drifts.
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

    /// <summary>The timeline of the strip drawn along its surface; null for a liquid that is not drawn.</summary>
    public FrameTimeline? Surface { get; private init; }

    /// <summary>The timeline of the picture tiled under the strip; null for a liquid that is not drawn.</summary>
    public FrameTimeline? Body { get; private init; }

    /// <summary>How its pictures are drawn over the room.</summary>
    public Godot.CanvasItemMaterial.BlendModeEnum Blend { get; private init; } = Godot.CanvasItemMaterial.BlendModeEnum.Mix;

    /// <summary>How far its pictures move across each update, where the room's liquid drifts.</summary>
    public Pixels Drift { get; private init; }

    /// <summary>The effect left where something crosses its surface, by the data file of its timeline; empty for none.</summary>
    public string SplashEffect { get; private init; } = "";

    /// <summary>The sound of something going under its surface, by name; empty for none.</summary>
    public string SplashSound { get; private init; } = "";

    /// <summary>The sound of something coming out through its surface, by name; empty for none.</summary>
    public string SplashOutSound { get; private init; } = "";

    /// <summary>
    /// How the liquid's own pictures ripple below its surface ("ripple", a <see cref="RowWaveSteps"/>): their rows
    /// moved across by a table gone round a row at a time from the row below the surface; null for a kind that lies
    /// still.
    /// </summary>
    public RowWaveSteps? Ripple { get; private init; }

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
            Surface = data.Has("surface") ? FrameTimeline.Read(data.Get("surface")) : null,
            Body = data.Has("body") ? FrameTimeline.Read(data.Get("body")) : null,
            Blend = data.Has("blend") ? OverlayEffect.BlendOf(data, data.Text("blend")) : Godot.CanvasItemMaterial.BlendModeEnum.Mix,
            Drift = data.Has("drift") ? data.Distance("drift") : Pixels.Zero,
            SplashEffect = data.Has("splash") ? data.Text("splash") : "",
            SplashSound = data.Has("splash_sound") ? data.Text("splash_sound") : "",
            SplashOutSound = data.Has("splash_out_sound") ? data.Text("splash_out_sound") : "",
            Ripple = data.Has("ripple") ? RowWaveSteps.Read(data.Get("ripple")) : null,
        };
        if ((type.Surface is null) != (type.Body is null))
            throw data.Problem("gives one of a surface and a body, and a liquid is drawn with both or neither");
        if (type.SpeedOver <= 0 || type.SpeedTimes < 0 || type.DamageHundredths < 0)
            throw data.Problem("gives a speed share or a damage that is not a sensible number");
        return type;
    }
}

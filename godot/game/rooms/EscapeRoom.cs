using Engine.Actors;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Rooms;

/// <summary>
/// The escape as one of its rooms shows it: the room shakes without stopping, harder and corner to corner for a moment now and then,
/// and blasts and smoke go off all over what the camera shows. It goes on for as long as the room is there. A harder
/// spell is the escape's own count (<see cref="Escape.HarderShakeLeft"/>): one the room before left goes on here, and
/// no chance of another is drawn until it is over.
/// </summary>
public partial class EscapeRoom : RoomSequence
{
    /// <summary>Whether the room is out of doors, where blasts come twice as often.</summary>
    [Export]
    public bool Outdoors { get; set; }

    /// <summary>How hard the room shakes.</summary>
    [Export]
    public int ShakeStrength { get; set; } = 1;

    /// <summary>Whether the shaking turns harder, corner to corner, for a moment now and then.</summary>
    [Export]
    public bool TurnsHarder { get; set; } = true;

    /// <summary>Whether the room, as it is set up, ends whatever harder spell the room before left.</summary>
    [Export]
    public bool StartsShakeAfresh { get; set; }

    /// <summary>The effects of its blasts, by the data files of their timelines: a small blast, a puff of dust, smoke, a short cloud of dust and a big one.</summary>
    [Export(PropertyHint.File)]
    public string SmallExplosionEffect { get; set; } = "";

    /// <summary>The effect of a puff of dust.</summary>
    [Export(PropertyHint.File)]
    public string DustPuffEffect { get; set; } = "";

    /// <summary>The effect of smoke.</summary>
    [Export(PropertyHint.File)]
    public string SmokeEffect { get; set; } = "";

    /// <summary>The effect of a short cloud of dust.</summary>
    [Export(PropertyHint.File)]
    public string ShortDustCloudEffect { get; set; } = "";

    /// <summary>The effect of a big cloud of dust.</summary>
    [Export(PropertyHint.File)]
    public string BigDustCloudEffect { get; set; } = "";

    /// <summary>The sound of a small blast, by name.</summary>
    [Export]
    public string SmallExplosionSound { get; set; } = "";

    /// <summary>The sound of smoke bursting out.</summary>
    [Export]
    public string SmokeSound { get; set; } = "";

    /// <summary>How many updates a harder shake lasts.</summary>
    private const int HarderUpdates = 42;

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        Escape escape = room.Session.Service<Escape>();
        if (elapsed == 0)
        {
            // As the room is set up: a harder spell the room before left is ended, or goes on here for what is left.
            if (StartsShakeAfresh)
                escape.HarderShakeLeft = 0;
            else if (TurnsHarder && escape.HarderShakeLeft > 0)
                room.Shake(ShakeStrength + 1, escape.HarderShakeLeft, ShakeWay.Both);
        }

        // The shaking never stops; now and then, by chance, it is harder for a while. A room that never turns harder
        // neither counts a spell down nor draws for one.
        if (!TurnsHarder)
        {
            if (room.ShakeStrength == 0)
                room.Shake(ShakeStrength, 600);
        }
        else if (escape.HarderShakeLeft > 0)
        {
            if (--escape.HarderShakeLeft == 0)
                room.Shake(ShakeStrength, 600);
        }
        else
        {
            uint chance = room.Session.Random.Draw().Number & 0xFFFF;
            bool harder = ShakeStrength <= 1 ? chance >> 9 == 0 : chance >> 7 < 3;
            if (harder)
            {
                escape.HarderShakeLeft = HarderUpdates;
                room.Shake(ShakeStrength + 1, HarderUpdates, ShakeWay.Both);
            }
            else if (room.ShakeStrength == 0)
            {
                room.Shake(ShakeStrength, 600);
            }
        }

        if (room.Session.Clock.Updates % (Outdoors ? 2 : 4) != 0)
            return false;
        RandomDraw where = room.Session.Random.Draw();
        PixelRect view = room.Session.Camera.View;
        var at = PixelPosition.FromWhole(view.Left + where.Part(0, 8), view.Top + where.Part(8, 8));
        int kind = room.Session.Random.Draw().Part(0, 4);
        (string effect, string sound) = kind switch
        {
            0 => (SmallExplosionEffect, SmallExplosionSound),
            1 => (SmallExplosionEffect, ""),
            2 => (DustPuffEffect, ""),
            3 => (SmokeEffect, SmokeSound),
            4 => (SmokeEffect, ""),
            5 or 6 => (ShortDustCloudEffect, ""),
            7 => (BigDustCloudEffect, ""),
            // Half the time the draw leaves the kind open: indoors a small blast, outdoors one of four by where it falls.
            _ => (!Outdoors ? SmallExplosionEffect : (at.X.Whole / 16 % 4) switch
            {
                0 => SmallExplosionEffect,
                1 => DustPuffEffect,
                2 => SmokeEffect,
                _ => ShortDustCloudEffect,
            }, ""),
        };
        if (effect.Length > 0)
            room.Spawn(Effect.Of(FrameTimeline.Read(room.Content.ReadData(effect)), at));
        if (sound.Length > 0)
            room.Session.Sounds.Request(sound, channel: "room");
        return false;
    }
}

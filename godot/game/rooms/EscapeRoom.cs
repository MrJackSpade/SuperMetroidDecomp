using Engine.Actors;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Rooms;

/// <summary>
/// The escape as one of its rooms shows it: the room shakes with no length, harder and corner to corner for a moment now and then,
/// and blasts and smoke go off all over what the camera shows. It goes on for as long as the room is there. A harder
/// spell is the escape's own count (<see cref="Escape.HarderShakeLeft"/>): one the room before left goes on being
/// counted here, the room shaking its own way meanwhile, and no chance of another is drawn until it is over. The
/// shaking has no length only until something gives it one: indoors a shake started by anything else, a super missile
/// against a wall, runs its length and leaves the room still, the harder spells then changing a shake that does not
/// run; out of doors the length is taken from it again on every update, and the room shakes on with that shake's look.
/// </summary>
public partial class EscapeRoom : RoomSequence
{
    /// <summary>Whether the room is out of doors, where blasts come twice as often and go off only over scenery.</summary>
    [Export]
    public bool Outdoors { get; set; }

    /// <summary>How hard the room shakes.</summary>
    [Export]
    public int ShakeStrength { get; set; } = 1;

    /// <summary>What of the room the shaking throws about.</summary>
    [Export]
    public ShakeReach Reach { get; set; } = ShakeReach.Everything;

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

    /// <summary>
    /// While the room is held still, as under the X-ray scope or a refill from her reserves, the shaking's own count
    /// goes on and its chance is still drawn, as the cartridge's are; the blasts wait.
    /// </summary>
    protected override void WhileRoomHeld(Room room) => KeepShaking(room, room.Session.Service<Escape>());

    /// <summary>
    /// Now and then, by chance, the shaking is harder for a while: only its look changes, and however long it runs
    /// is left as it is. A room that never turns harder neither counts a spell down nor draws for one. Out of doors
    /// the shake then has its length taken from it, whatever gave it one.
    /// </summary>
    private void KeepShaking(Room room, Escape escape)
    {
        if (TurnsHarder)
        {
            if (escape.HarderShakeLeft > 0)
            {
                if (--escape.HarderShakeLeft == 0)
                    room.ChangeShake(ShakeStrength, ShakeWay.Across, Reach);
            }
            else
            {
                uint chance = room.Session.Random.Draw().Number & 0xFFFF;
                bool harder = ShakeStrength <= 1 ? chance >> 9 == 0 : chance >> 7 < 3;
                if (harder)
                {
                    escape.HarderShakeLeft = HarderUpdates;
                    room.ChangeShake(ShakeStrength + 1, ShakeWay.Both, Reach);
                }
            }
        }

        if (Outdoors)
            room.KeepShakeOn();
    }

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        Escape escape = room.Session.Service<Escape>();
        if (elapsed == 0)
        {
            // As the room is set up it shakes its own way, with no length. A harder spell the room before left is
            // ended, or goes on being counted here: the room shakes its own way through what is left of it.
            if (StartsShakeAfresh)
                escape.HarderShakeLeft = 0;
            room.ChangeShake(ShakeStrength, ShakeWay.Across, Reach);
            room.RunShakeOn();
        }

        KeepShaking(room, escape);
        if (room.Session.Clock.Updates % (Outdoors ? 2 : 4) != 0)
            return false;
        RandomDraw where = room.Session.Random.Draw();
        PixelRect view = room.Session.Camera.View;
        var at = PixelPosition.FromWhole(view.Left + where.Part(0, 8), view.Top + where.Part(8, 8));

        // Out of doors a blast goes off only over a block that shows scenery: over open space there is none, and no
        // number is drawn for its kind. What lies beyond the room's blocks counts as scenery, as on the cartridge.
        Terrain terrain = room.Terrain;
        bool inRoom = at.X.Whole >= 0 && at.Y.Whole >= 0 && at.X.Whole < terrain.WidthInPixels && at.Y.Whole < terrain.HeightInPixels;
        if (Outdoors && inRoom && !terrain.ShowsTileAt(at.X.Whole, at.Y.Whole))
            return false;
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

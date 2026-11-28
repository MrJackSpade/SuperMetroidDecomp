using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Rooms.Brinstar;

/// <summary>
/// The plants of Brinstar that seize Samus: each is a block of the room's terrain, in a floor or a ceiling. When she
/// comes into one, level with it, it holds her where she is and chews, hurting her at each bite, and then lets her go
/// and stays shut a while before it can seize her again. It holds her by the whole pixel: she moves as she would on
/// each update and is put back at the pixel she is held at, keeping whatever part of a pixel her own move left her,
/// as the cartridge's plant writes her position and leaves her subposition alone.
/// </summary>
/// <remarks>
/// Its data names, under "floor" and "ceiling", the kind of plant: the data file of its "block"; how many blocks to
/// the left of that block its mouth also reaches ("reach_left"); how far from where she was Samus is held
/// ("held_offset", in pixels down); its "chew", the looks it goes through once with how many "updates" each shows
/// and whether it "bites" and is heard ("sound") as the look comes; how many "chews" it takes; and the look it has
/// once it has "released" her. Each look is a patch of the room's terrain, named "plant_", the column and row of
/// the plant's block, and the look's name, with underscores between; with no look it is as the room has it. The data
/// also gives the "release_updates" it stays shut, the "damage_hundredths" of a bite, the "chew_sound", and how many
/// updates nothing else hurts her while she is held and once she is let go ("held_invincible_updates",
/// "released_invincible_updates").
/// </remarks>
public partial class SamusEaters : RoomSequence
{
    /// <summary>The data file of the plants.</summary>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>The name its bites' damage goes by.</summary>
    private const string DamageSource = "plant";

    private enum Stage
    {
        Waiting,
        Chewing,
        Shut,
    }

    /// <summary>One plant of the room: where its block is, what kind it is, and what it is doing.</summary>
    private sealed class Plant(DataNode kind, bool inCeiling, int column, int row)
    {
        public DataNode Kind { get; } = kind;

        public bool InCeiling { get; } = inCeiling;

        public int Column { get; } = column;

        public int Row { get; } = row;

        public Stage Doing { get; set; }

        public string Shown { get; set; } = "";

        /// <summary>The whole pixel she is held at.</summary>
        public (int X, int Y) Held { get; set; }

        public int Look { get; set; }

        public int UpdatesLeft { get; set; }

        public int ChewsLeft { get; set; }
    }

    private readonly List<Plant> _plants = [];
    private DataNode? _data;

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        if (elapsed == 0)
            Find(room);
        foreach (Plant plant in _plants)
            Step(room, plant);
        return false;
    }

    /// <summary>Finds the room's plants: every block of the terrain that is of a plant's type.</summary>
    private void Find(Room room)
    {
        _data = room.Content.ReadData(DataFile);
        _plants.Clear();
        int size = room.Terrain.TileSize;
        foreach ((string name, bool inCeiling) in new[] { ("floor", false), ("ceiling", true) })
        {
            DataNode kind = _data.Get(name);
            string file = kind.Text("block");
            foreach (Block block in room.Terrain.Blocks.Where(block => block.Type.File == file))
                _plants.Add(new Plant(kind, inCeiling, block.Rect.Left / size, block.Rect.Top / size));
        }
    }

    private DataNode Data() => _data ?? throw new InvalidOperationException($"The plants \"{Name}\" have not begun.");

    /// <summary>Plays one update of one plant.</summary>
    private void Step(Room room, Plant plant)
    {
        Player samus = room.Player;
        IReadOnlyList<DataNode> chew = plant.Kind.List("chew");
        switch (plant.Doing)
        {
            case Stage.Waiting:
                if (!Seizes(room, plant))
                    return;
                plant.Held = (samus.Position.X.Whole, samus.Position.Y.Whole + plant.Kind.Int("held_offset"));
                (plant.Doing, plant.Look, plant.ChewsLeft) = (Stage.Chewing, 0, plant.Kind.Int("chews"));
                Take(room, plant, chew[0]);
                return;

            case Stage.Chewing:
                // She moves as she would and is then put back at the pixel she is held at, with the part of a pixel
                // her move left her, and nothing else hurts her meanwhile.
                samus.Position = new PixelPosition(samus.Position.X.WithWhole(plant.Held.X), samus.Position.Y.WithWhole(plant.Held.Y));
                samus.BeInvincibleFor(Data().Int("held_invincible_updates"));
                if (--plant.UpdatesLeft > 0)
                    return;
                if (++plant.Look == chew.Count)
                {
                    plant.Look = 0;
                    if (--plant.ChewsLeft == 0)
                    {
                        samus.BeInvincibleFor(Data().Int("released_invincible_updates"));
                        (plant.Doing, plant.UpdatesLeft) = (Stage.Shut, Data().Int("release_updates"));
                        Show(room, plant, plant.Kind.Text("released"));
                        return;
                    }
                }

                Take(room, plant, chew[plant.Look]);
                return;

            case Stage.Shut:
                if (--plant.UpdatesLeft > 0)
                    return;
                Show(room, plant, "");
                plant.Doing = Stage.Waiting;
                return;

            default:
                throw new InvalidOperationException($"A plant of \"{Name}\" is doing something that is nothing a plant does.");
        }
    }

    /// <summary>Takes up one look of a plant's chewing: it shows, and bites and is heard if the look says so.</summary>
    private void Take(Room room, Plant plant, DataNode look)
    {
        plant.UpdatesLeft = look.Int("updates");
        Show(room, plant, look.Text("look"));
        if (look.Flag("sound"))
            room.Session.Sounds.Request(Data().Text("chew_sound"), channel: "room");
        if (look.Flag("bites"))
            room.Player.AddOngoingDamage(DamageSource, Data().Int("damage_hundredths"));
    }

    /// <summary>
    /// Whether a waiting plant seizes Samus now: the middle of her body, at her feet, her middle or her head, is in its
    /// mouth, and she is level with it: her feet at the bottom of a block for one in a floor, her head at the top of a
    /// block for one in a ceiling.
    /// </summary>
    private static bool Seizes(Room room, Plant plant)
    {
        int size = room.Terrain.TileSize;
        PixelRect body = room.Player.BodyRect;
        int reach = plant.Kind.Int("reach_left");
        var mouth = new PixelRect((plant.Column - reach) * size, plant.Row * size, (reach + 1) * size, size);
        bool level = plant.InCeiling ? body.Top % size == 0 : body.Bottom % size == 0;
        if (!level)
            return false;
        int middle = body.Left + body.Width / 2;
        return mouth.Contains(middle, body.Bottom - 1) || mouth.Contains(middle, body.Top + body.Height / 2) || mouth.Contains(middle, body.Top);
    }

    /// <summary>Shows a plant in one of its looks, by the look's name, or as the room has it for no name.</summary>
    private static void Show(Room room, Plant plant, string look)
    {
        if (plant.Shown == look)
            return;
        if (plant.Shown.Length > 0)
            room.Terrain.RevertPatch(PatchOf(plant, plant.Shown));
        if (look.Length > 0)
            room.Terrain.ApplyPatch(PatchOf(plant, look));
        plant.Shown = look;
    }

    private static string PatchOf(Plant plant, string look) => $"plant_{plant.Column}_{plant.Row}_{look}";
}

using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// The husk of a creature the giant Metroid has drained. It is solid, and lies as it fell until something disturbs
/// it: Samus coming to rest against it, after a moment, or at once a shot, a bomb or her touch. Then it crumbles to
/// dust, row by row, and is gone.
/// </summary>
/// <remarks>Its data file names the timeline it lies in, "look", and how many "rows" of it there are to crumble.</remarks>
public partial class Corpse : Enemy
{
    /// <summary>The timeline it lies in.</summary>
    [Export, DataValue("look")]
    public string LiesIn { get; set; } = "corpse";

    /// <summary>How many rows of it there are to crumble: the height of its picture.</summary>
    [Export, DataValue("rows")]
    public int Rows { get; set; } = 16;

    /// <summary>How many updates after Samus comes to rest against it that it starts to crumble.</summary>
    [Export, DataValue("disturbed_updates")]
    public int DisturbedUpdates { get; set; } = 16;

    /// <summary>The effect of the dust it crumbles into, by the data file of its timeline.</summary>
    [Export, DataValue("dust_effect")]
    public string DustEffect { get; set; } = "";

    /// <summary>The sound of it crumbling, by name.</summary>
    [Export, DataValue("dust_sound")]
    public string DustSound { get; set; } = "";

    /// <summary>Where its dust rises from, from its middle, in pixels.</summary>
    [Export]
    public Vector2I DustFrom { get; set; } = new(-14, 16);

    private RowCrumble? _crumble;

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        Crumble();
        return HitResult.Absorbed;
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part) => Crumble();

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _crumble = null;
        Play(LiesIn);
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (_crumble is null)
        {
            if (PlayerRestsAgainstIt())
                _crumble = new RowCrumble(Rows, DisturbedUpdates);
            return;
        }

        if (!_crumble.Step(LeaveDust))
            return;
        // Once it has started to go, nothing touches it.
        AlwaysActive = true;
        foreach (Collider part in GetChildren().OfType<Collider>())
            part.Enabled = false;
        Visual?.ShowRows(_crumble.Rows);
        if (_crumble.IsOver)
            Vanish();
    }

    /// <summary>Starts it crumbling at once.</summary>
    private void Crumble()
    {
        _crumble ??= new RowCrumble(Rows);
        _crumble.StartNow();
    }

    /// <summary>
    /// Leaves the puff of dust of a row reaching the ground: one of the enemies' shots on the cartridge, which holds one
    /// of the room's places while it plays and is not made with none free.
    /// </summary>
    private void LeaveDust()
    {
        RandomDraw roll = Random.Last;
        int spread = roll.Part(1, 1) * 2 + roll.Part(3, 2) * 8;
        if (DustEffect.Length > 0)
            Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(DustEffect)), Position + PixelOffset.FromWhole(DustFrom.X + spread, DustFrom.Y), holdsPlace: true));
        if (Room.Session.EnemyPasses % 8 == 0)
            Sound(DustSound);
    }
}

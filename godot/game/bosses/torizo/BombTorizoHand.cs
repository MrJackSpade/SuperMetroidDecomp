using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// The statue hand that holds the Bombs out to Samus. Once she has them it crumbles away piece by piece, and when the
/// last piece has gone Bomb Torizo wakes.
/// </summary>
/// <remarks>
/// Its data lists its "crumble": each step's "picture", how many "updates" it shows for, and the "fragment" thrown as
/// it begins, which may be empty; and its "fragments" by name, each with where it starts from the hand's place ("x",
/// "y") and the data file of its "shot". The hand's place is the top left of the block it fills. Its solid
/// colliders "Fingers" and "Palm" are what Samus can stand on until a step that shows no picture, the hand cleared
/// away, begins. They stand as part of the room, as the cartridge's hand is blocks of it: a move stopped by them is
/// stopped as by a wall, and reaches what lies beside them, the Bombs above the palm among it.
/// </remarks>
public partial class BombTorizoHand : RoomObject
{
    /// <summary>The look Bomb Torizo's body takes on as the hand begins to crumble.</summary>
    [Export, DataValue("debris_look")]
    public string DebrisLook { get; set; } = "";

    private enum Stage
    {
        Starting,
        Arming,
        Resting,
        Crumbling,
    }

    private sealed record CrumbleStep(string Picture, int Updates, string Fragment);

    private sealed record Fragment(int X, int Y, string Shot);

    private readonly List<CrumbleStep> _crumble = [];
    private readonly Dictionary<string, Fragment> _fragments = new(StringComparer.Ordinal);
    private Stage _stage;
    private int _step;
    private int _hold;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _crumble.Clear();
        _crumble.AddRange(data.List("crumble").Select(step => new CrumbleStep(step.Text("picture"), step.Int("updates"), step.Text("fragment"))));
        if (_crumble.Count == 0)
            throw data.Problem("must list at least one step of its crumbling");
        _fragments.Clear();
        foreach ((string name, DataNode fragment) in data.Get("fragments").Entries())
            _fragments[name] = new Fragment(fragment.Int("x"), fragment.Int("y"), fragment.Text("shot"));
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _stage = Stage.Starting;
        _step = -1;
        _hold = 0;
        ShowPicture(_crumble[0].Picture);
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        if (_stage == Stage.Resting && Room.Player.Inventory.Owns(Equipment.Bombs))
        {
            foreach (Torizo torizo in Room.Enemies.OfType<Torizo>())
                torizo.ShowLook(DebrisLook);
            _stage = Stage.Crumbling;
            _hold = 1;
        }

        if (_hold > 0 && --_hold > 0)
            return;

        switch (_stage)
        {
            case Stage.Starting:
                _stage = Stage.Arming;
                break;

            case Stage.Arming:
                _stage = Stage.Resting;
                break;

            case Stage.Resting:
                break;

            case Stage.Crumbling:
                if (++_step >= _crumble.Count)
                {
                    // The hand asks for the waking music as it goes; the Torizo asks for it again when it sees the hand gone.
                    Room.Session.Sounds.RequestMusic(Music.TorizoAwakening);
                    Room.Remove(this);
                    return;
                }

                CrumbleStep step = _crumble[_step];
                if (step.Fragment.Length > 0)
                    Throw(step.Fragment);

                // A step that shows nothing is the hand cleared away: what Samus stood on goes as it begins.
                if (step.Picture.Length == 0)
                {
                    foreach (Collider solid in GetChildren().OfType<Collider>())
                        solid.Enabled = false;
                }

                ShowPicture(step.Picture);
                _hold = step.Updates;
                return;

            default:
                throw new InvalidOperationException($"The Torizo's hand has no stage {_stage}.");
        }

        _hold = 1;
    }

    /// <summary>Breaks a piece off and lets it fall.</summary>
    private void Throw(string name)
    {
        if (!_fragments.TryGetValue(name, out Fragment? fragment))
            throw new InvalidOperationException($"The Torizo's hand has no fragment named \"{name}\".");
        ShotType shot = ShotType.Read(Room.Content.ReadData(fragment.Shot));
        PixelPosition at = Position + PixelOffset.FromWhole(fragment.X, fragment.Y);
        Room.Spawn(shot.Launch(at, new PixelVelocity(Pixels.Zero, Pixels.FromWhole(1)), HitSide.Enemies, this, this));
    }

    /// <summary>Shows a picture on every visual it has.</summary>
    private void ShowPicture(string picture)
    {
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Show(picture);
    }
}

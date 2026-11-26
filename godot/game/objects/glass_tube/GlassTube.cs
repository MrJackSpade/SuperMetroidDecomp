using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Input;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Objects;

/// <summary>
/// The glass tube of Maridia. Only a power bomb harms it. Once one has gone off against it, the next thing Samus
/// does starts it cracking: she is held where she is, the crack spreads, and the tube bursts into shards and
/// bubbles, shaking the room. From then on it is broken for good and the water is in. Each bubble, and each shard
/// still on the screen, draws a random number as it begins to sway: the bubbles on the ninth update after the
/// burst, the shards on the thirty-third.
/// </summary>
/// <remarks>
/// Its data names its pictures "intact", "cracking" and "shattered"; its effects "crack", "bubble" and the ten of its
/// "shards"; the terrain patch that opens it, "open_patch"; and its sounds. Its hurtbox is what a power bomb's blast
/// reaches.
/// </remarks>
public partial class GlassTube : RoomObject, IHittable
{
    /// <summary>The picture of it whole.</summary>
    [Export, DataValue("intact")]
    public string IntactPicture { get; set; } = "";

    /// <summary>The picture of it cracking.</summary>
    [Export, DataValue("cracking")]
    public string CrackingPicture { get; set; } = "";

    /// <summary>The picture of it burst.</summary>
    [Export, DataValue("shattered")]
    public string ShatteredPicture { get; set; } = "";

    /// <summary>The effect of the crack, by the data file of its timeline.</summary>
    [Export, DataValue("crack")]
    public string CrackEffect { get; set; } = "";

    /// <summary>The effect of a bubble of the air let out.</summary>
    [Export, DataValue("bubble")]
    public string BubbleEffect { get; set; } = "";

    /// <summary>The terrain patch that opens it.</summary>
    [Export, DataValue("open_patch")]
    public string OpenPatch { get; set; } = "";

    /// <summary>The sound of it bursting, by name.</summary>
    [Export, DataValue("break_sound")]
    public string BreakSound { get; set; } = "";

    /// <summary>The sound of a shot that does nothing to it.</summary>
    [Export, DataValue("dud_sound")]
    public string DudSound { get; set; } = "";

    private enum Phase
    {
        Intact,
        WaitingForSamus,
        Cracking,
        Shattered,
    }

    /// <summary>Where the crack starts, from where the tube stands, in pixels.</summary>
    private static readonly PixelOffset CrackAt = PixelOffset.FromWhole(96, 48);

    /// <summary>Where each shard starts from the crack, and how it flies.</summary>
    private static readonly (PixelOffset From, PixelVelocity Speed)[] Shards =
    [
        (PixelOffset.FromWhole(-56, 8), Speed(-1.5, 1.25)), (PixelOffset.FromWhole(-64, -12), Speed(-1.5, -1)),
        (PixelOffset.FromWhole(-20, -26), Speed(-0.625, -1.625)), (PixelOffset.FromWhole(-40, -24), Speed(-1.125, -1.125)),
        (PixelOffset.FromWhole(-64, -32), Speed(-1.125, -1.125)), (PixelOffset.FromWhole(-48, 28), Speed(-1.25, 1.75)),
        (PixelOffset.FromWhole(-24, 16), Speed(-0.375, 2.25)), (PixelOffset.FromWhole(-40, -8), Speed(-1.375, -0.375)),
        (PixelOffset.FromWhole(0, -24), Speed(0, -1.125)), (PixelOffset.FromWhole(-8, 16), Speed(-0.25, 1.5)),
    ];

    /// <summary>Where each bubble starts, from where the tube stands.</summary>
    private static readonly PixelOffset[] Bubbles =
    [
        PixelOffset.FromWhole(40, 80), PixelOffset.FromWhole(80, 72), PixelOffset.FromWhole(104, 84),
        PixelOffset.FromWhole(120, 32), PixelOffset.FromWhole(152, 64), PixelOffset.FromWhole(184, 84),
    ];

    /// <summary>How long after the burst a bubble, and a shard, begins to sway, drawing a number for where its sway starts.</summary>
    private const int BubbleSwaysAfter = 9, ShardSwaysAfter = 33;

    /// <summary>How long the burst tube stays before it is done.</summary>
    private const int ShatteredUpdates = 96;

    /// <summary>How far below the top of the view a shard is taken away.</summary>
    private const int ShardGoneBelowView = 288;

    private readonly List<string> _shardEffects = [];
    private Phase _phase;
    private int _timer;

    /// <summary>The shards still flying, each with where it started and how it flies, and how many bubbles were let out.</summary>
    private readonly List<(Effect Shard, Pixels StartedAtY, Pixels SpeedDown)> _shards = [];
    private int _bubbles;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _shardEffects.Clear();
        _shardEffects.AddRange(data.List("shards").Select(shard => shard.AsText()));
        if (_shardEffects.Count != Shards.Length)
            throw data.Problem($"must name {Shards.Length} shards");
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        if (Room.Session.Progress.Has(StoryEvents.MaridiaTubeBroken))
        {
            // It was broken before: it is open, and the water is in.
            Room.Liquid?.LetIn();
            Room.Terrain.ApplyPatch(OpenPatch);
            Room.Remove(this);
            return;
        }

        _phase = Phase.Intact;
        Show(IntactPicture);
    }

    /// <inheritdoc/>
    public bool IsHitBy(HitSide side) => side != HitSide.Enemies && _phase == Phase.Intact;

    /// <inheritdoc/>
    public HitResult TakeHit(Hit hit, Collider part)
    {
        if (_phase != Phase.Intact)
            return HitResult.Ignored;
        if (!hit.Carries(new HitKind(HitKinds.PowerBomb)))
        {
            Sound(DudSound);
            return HitResult.Absorbed;
        }

        _phase = Phase.WaitingForSamus;
        return HitResult.Hurt;
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        Player samus = Room.Player;
        switch (_phase)
        {
            case Phase.Intact:
                break;

            case Phase.WaitingForSamus:
                GameInput input = samus.Input;
                bool acted = input.WasPressed(Actions.Run) || input.WasPressed(Actions.ItemCancel) || input.WasPressed(GameAction.Left)
                    || input.WasPressed(GameAction.Right) || input.WasPressed(GameAction.Jump) || input.WasPressed(GameAction.Attack);
                if (!acted)
                    break;

                // She is stopped as she is while it breaks, and goes on from there with the speed she had.
                samus.HoldAsTheyAre();
                // The crack stays where it is until its own timeline has it fall.
                Leave(CrackEffect, CrackAt, PixelVelocity.Zero);
                Show(CrackingPicture);
                (_phase, _timer) = (Phase.Cracking, 50);
                break;

            case Phase.Cracking:
                if (--_timer > 0)
                    break;
                Sound(BreakSound);
                _shards.Clear();
                for (int shard = 0; shard < Shards.Length; shard++)
                {
                    if (Leave(_shardEffects[shard], CrackAt + Shards[shard].From, Shards[shard].Speed) is { } flying)
                        _shards.Add((flying, (Position + CrackAt + Shards[shard].From).Y, Shards[shard].Speed.Y));
                }

                _bubbles = 0;
                foreach (PixelOffset bubble in Bubbles)
                {
                    if (Leave(BubbleEffect, bubble, new PixelVelocity(Pixels.Zero, Pixels.FromWhole(-5))) is not null)
                        _bubbles++;
                }

                Room.Shake(1, 64);
                Room.Terrain.ApplyPatch(OpenPatch);
                Show(ShatteredPicture);
                (_phase, _timer) = (Phase.Shattered, ShatteredUpdates);
                break;

            case Phase.Shattered:
                StepPieces(ShatteredUpdates - (_timer - 1));
                if (--_timer > 0)
                    break;
                Room.Session.Progress.Record(StoryEvents.MaridiaTubeBroken);
                Room.Liquid?.LetIn();
                if (samus.State is HeldState)
                    samus.Release();
                Room.Remove(this);
                break;

            default:
                throw new InvalidOperationException($"The glass tube has no phase {_phase}.");
        }
    }

    private static PixelVelocity Speed(double across, double down) => new(Pixels.FromNumber(across), Pixels.FromNumber(down));

    /// <summary>
    /// What the pieces do on the update so many after the burst that touches the room's random numbers. A shard that
    /// has flown far enough below the view is taken away, where its flight has taken it by then; each bubble draws a
    /// number as it begins to sway, and so does each shard still there when its turn to sway comes.
    /// </summary>
    private void StepPieces(int sinceBurst)
    {
        if (sinceBurst <= ShardSwaysAfter)
        {
            int goneBelow = Room.Session.Camera.Top + ShardGoneBelowView;
            for (int index = _shards.Count - 1; index >= 0; index--)
            {
                (Effect shard, Pixels startedAtY, Pixels speedDown) = _shards[index];
                if ((startedAtY + speedDown * sinceBurst).Whole < goneBelow)
                    continue;
                Room.Remove(shard);
                _shards.RemoveAt(index);
            }
        }

        if (sinceBurst == BubbleSwaysAfter)
        {
            for (int bubble = 0; bubble < _bubbles; bubble++)
                Room.Session.Random.Draw();
        }

        if (sinceBurst == ShardSwaysAfter)
        {
            for (int shard = 0; shard < _shards.Count; shard++)
                Room.Session.Random.Draw();
        }
    }

    private Effect? Leave(string effect, PixelOffset from, PixelVelocity speed)
    {
        if (effect.Length == 0)
            return null;
        Effect left = Effect.Of(FrameTimeline.Read(Room.Content.ReadData(effect)), Position + from);
        left.Velocity = speed;
        Room.Spawn(left);
        return left;
    }

    private void Show(string picture)
    {
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Show(picture);
    }

    private void Sound(string sound)
    {
        if (sound.Length > 0)
            Room.Session.Sounds.Request(sound, channel: "objects");
    }
}

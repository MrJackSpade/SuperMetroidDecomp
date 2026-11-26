using Engine.Actors.Components;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Engine.Actors;

/// <summary>
/// A short-lived visual: an explosion, dust, a splash. It plays its timeline and is gone. It does not collide, deal
/// damage, change anything in play or draw on the gameplay random generator; if it did any of those it would be a
/// projectile or an enemy. A cue of its timeline that is "sound:" and a sound's name asks for that sound; one that
/// is "drift:" and two numbers, pixels across and down an update, makes that its velocity from then on.
/// </summary>
public partial class Effect : Node2D, IRoomEntity, Ui.IRecoloured
{
    /// <inheritdoc/>
    public void Recolour(string source, IReadOnlyList<Shared.ColourSwap> swaps) => _visual?.Recolour(source, swaps);

    /// <inheritdoc/>
    public bool ShowsPicturesIn(string place) => _visual?.ShowsPicturesIn(place) ?? false;

    private FrameTimeline? _timeline;
    private TimelinePlayer? _player;
    private ActorVisual? _visual;
    private Room? _room;

    /// <summary>The effect's velocity, for one that drifts or falls.</summary>
    public PixelVelocity Velocity { get; set; }

    /// <summary>Where each picture's top-left corner lies from the effect's place, in pixels.</summary>
    public Vector2I PictureOffset { get; set; }

    /// <summary>
    /// Whether it holds one of the room's places for what its enemies launch and leave for as long as it plays, as
    /// dust or a sparkle that one of them throws off does where a game counts those: with no place free it is not
    /// made. It is still only looked at, and plays with the room's other effects.
    /// </summary>
    public bool HoldsPlace { get; private init; }

    /// <summary>An effect playing <paramref name="timeline"/> once at <paramref name="at"/>.</summary>
    /// <param name="timeline">What it plays.</param>
    /// <param name="at">Where it is.</param>
    /// <param name="holdsPlace">Whether it holds one of the room's places for what its enemies launch and leave.</param>
    public static Effect Of(FrameTimeline timeline, PixelPosition at, bool holdsPlace = false)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        return new Effect { _timeline = timeline, Position = at, Name = "Effect", HoldsPlace = holdsPlace };
    }

    /// <summary>
    /// Something that shows nothing for <paramref name="updates"/> updates at <paramref name="at"/> and holds one of
    /// the room's places for what its enemies launch and leave meanwhile: what a spent drop leaves in its place.
    /// </summary>
    public static Effect HoldingPlace(PixelPosition at, int updates) =>
        Of(new FrameTimeline([new TimelineStep("", updates, [])], null), at, holdsPlace: true);

    /// <summary>
    /// Something that shows nothing at <paramref name="at"/> and holds one of the room's places for what its enemies
    /// launch and leave until whoever made it takes it out of the room: the place a part of an enemy or a fitting of
    /// the room holds where a game counts those among them. With no place free it is not made
    /// (<see cref="World.Room.Spawn"/> says so), and what it stands for goes without.
    /// </summary>
    public static Effect HoldingPlace(PixelPosition at) =>
        Of(new FrameTimeline([new TimelineStep("", 1, [])], 0), at, holdsPlace: true);

    /// <inheritdoc/>
    public new PixelPosition Position { get; set; }

    /// <inheritdoc/>
    public void EnterRoom(Room room)
    {
        _room = room;
        ZIndex = RoomDepth.Launched;
        FrameTimeline timeline = _timeline ?? throw new InvalidOperationException("The effect has no timeline.");
        _player = new TimelinePlayer(timeline);
        _visual = new ActorVisual { PictureOffset = PictureOffset };
        AddChild(_visual);
        _visual.Bind(room.Content);

        // Being left is the first update of its first picture: what comes with that picture comes now.
        Play(room, _player);
        _visual.Show(_player.Shown);
    }

    /// <summary>
    /// Plays one update of the effect's timeline. One whose timeline does not loop is gone on the update after its
    /// last picture's last, showing nothing then.
    /// </summary>
    internal void Step()
    {
        Room room = _room ?? throw new InvalidOperationException("The effect is not in a room.");
        TimelinePlayer player = _player ?? throw new InvalidOperationException("The effect is not in a room.");
        Position += Velocity.OverOneUpdate;
        if (player.IsOver)
        {
            room.Remove(this);
            return;
        }

        Play(room, player);
        _visual?.Show(player.Shown);
    }

    /// <summary>Moves its timeline on an update and acts on the cues reached.</summary>
    private void Play(Room room, TimelinePlayer player)
    {
        foreach (string cue in player.Step())
        {
            if (cue.StartsWith("sound:", StringComparison.Ordinal))
                room.Session.Sounds.Request(cue["sound:".Length..], channel: "effects");
            else if (cue.StartsWith("drift:", StringComparison.Ordinal))
                Velocity = DriftOf(cue);
        }
    }

    /// <summary>Reads the velocity a "drift:" cue names.</summary>
    private static PixelVelocity DriftOf(string cue)
    {
        string[] parts = cue.Split(':');
        System.Globalization.CultureInfo plain = System.Globalization.CultureInfo.InvariantCulture;
        if (parts.Length != 3 || !double.TryParse(parts[1], System.Globalization.NumberStyles.Float, plain, out double across)
            || !double.TryParse(parts[2], System.Globalization.NumberStyles.Float, plain, out double down))
            throw new InvalidOperationException($"An effect's timeline has the cue \"{cue}\", which is not \"drift:\" and two numbers.");
        return new PixelVelocity(Pixels.FromNumber(across), Pixels.FromNumber(down));
    }

    /// <inheritdoc/>
    public void ShowPosition() => EntityNode.Show(this, Position);
}

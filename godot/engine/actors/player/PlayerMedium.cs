using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.Actors;

/// <summary>
/// How the player moves in one medium: air, or a liquid that hinders them. It holds the pull of gravity, the speeds a
/// jump starts with, and the move values of each thing the player can be doing. It may hold each picture of the
/// player's timed changes (a turn, a change of posture) some updates longer. A player with no medium of a liquid's name moves in that liquid as in air, slowed by the liquid's
/// own share.
/// </summary>
/// <remarks>
/// <code>
/// "air": { "gravity": 0.109375, "fall_speed_limit": 5, "jump_speed": 4.875, "boosted_jump_speed": 6,
///          "wall_jump_speed": 4.625, "boosted_wall_jump_speed": 5.5, "knockback_up_speed": 5,
///          "moves": { "running": { "acceleration": 0.1875, "top_speed": 2.75, "deceleration": 0.5 } },
///          "speeds": { "bounce": 1 }, "picture_hold": 3 }
/// </code>
/// </remarks>
[GlobalClass]
public partial class PlayerMedium : Resource
{
    /// <summary>The name of air, the medium the player is in when no liquid hinders them.</summary>
    public const string Air = "air";

    /// <summary>The medium's name: "air", or the name of a liquid type.</summary>
    [Export]
    public string MediumName { get; set; } = Air;

    /// <summary>The downward speed gained each update while in the air or sinking, as a number of pixels.</summary>
    [Export]
    public double Gravity { get; set; } = 0.125;

    /// <summary>The fastest the player falls.</summary>
    [Export]
    public double FallSpeedLimit { get; set; } = 5;

    /// <summary>The upward speed a jump starts with.</summary>
    [Export]
    public double JumpSpeed { get; set; } = 4.5;

    /// <summary>The upward speed a jump starts with while the player's jump-boosting equipment is switched on.</summary>
    [Export]
    public double BoostedJumpSpeed { get; set; } = 4.5;

    /// <summary>The upward speed a wall jump starts with.</summary>
    [Export]
    public double WallJumpSpeed { get; set; } = 4;

    /// <summary>The upward speed a wall jump starts with while the player's jump-boosting equipment is switched on.</summary>
    [Export]
    public double BoostedWallJumpSpeed { get; set; } = 4;

    /// <summary>The upward speed a hit knocks the player back with.</summary>
    [Export]
    public double KnockbackUpSpeed { get; set; } = 2;

    /// <summary>The move values of each thing the player can be doing, by the state's name.</summary>
    [Export]
    public Godot.Collections.Dictionary<string, MoveValues> Moves { get; set; } = [];

    /// <summary>Any further speeds a game's own states use in this medium, by name.</summary>
    [Export]
    public Godot.Collections.Dictionary<string, double> Speeds { get; set; } = [];

    /// <summary>How many updates longer the medium holds each picture of the player's timed changes; nought in a medium that holds them no longer.</summary>
    [Export]
    public int PictureHold { get; set; }

    /// <summary>The move values of the state named <paramref name="state"/>.</summary>
    /// <exception cref="InvalidOperationException">The medium has no move values for that state.</exception>
    public MoveValues MovesFor(string state) =>
        Moves.TryGetValue(state, out MoveValues? values)
            ? values
            : throw new InvalidOperationException($"The player's medium \"{MediumName}\" has no move values for \"{state}\".");

    /// <summary>The further speed named <paramref name="name"/>.</summary>
    /// <exception cref="InvalidOperationException">The medium has no speed of that name.</exception>
    public Pixels Speed(string name) =>
        Speeds.TryGetValue(name, out double speed)
            ? Pixels.FromNumber(speed)
            : throw new InvalidOperationException($"The player's medium \"{MediumName}\" has no speed named \"{name}\".");

    /// <summary>
    /// A plain medium of air, with move values for each of the engine's states: what a player moves by when their
    /// scene and data file give them no mediums of their own.
    /// </summary>
    public static PlayerMedium Plain()
    {
        var medium = new PlayerMedium();
        MoveValues Values(double acceleration, double top, double deceleration) =>
            new() { Acceleration = acceleration, TopSpeed = top, Deceleration = deceleration };
        medium.Moves[PlayerState.StandingMoves] = Values(0.25, 0, 0.5);
        medium.Moves[PlayerState.RunningMoves] = Values(0.25, 2, 0.5);
        medium.Moves[PlayerState.TurningMoves] = Values(0.25, 0, 0.5);
        medium.Moves[PlayerState.CrouchingMoves] = Values(0.25, 0, 0.5);
        medium.Moves[PlayerState.JumpingMoves] = Values(0.25, 2, 0.5);
        medium.Moves[PlayerState.FallingMoves] = Values(0.25, 2, 0.5);
        medium.Moves[PlayerState.WallJumpingMoves] = Values(0.25, 2, 0.5);
        medium.Moves[PlayerState.CrawlingMoves] = Values(0.25, 1.5, 0.5);
        medium.Moves[PlayerState.CrawlingInAirMoves] = Values(0.25, 1.5, 0.5);
        medium.Moves[PlayerState.CrawlingJumpingMoves] = Values(0.25, 1.5, 0.5);
        medium.Moves[PlayerState.KnockedBackMoves] = Values(1.5, 1.5, 0.5);
        return medium;
    }

    /// <summary>Reads the medium named <paramref name="name"/> from a data file's group.</summary>
    public static PlayerMedium Read(string name, DataNode data)
    {
        var medium = new PlayerMedium
        {
            MediumName = name,
            Gravity = data.Distance("gravity").ToNumber(),
            FallSpeedLimit = data.Distance("fall_speed_limit").ToNumber(),
            JumpSpeed = data.Distance("jump_speed").ToNumber(),
            WallJumpSpeed = data.Has("wall_jump_speed") ? data.Distance("wall_jump_speed").ToNumber() : data.Distance("jump_speed").ToNumber(),
            KnockbackUpSpeed = data.Has("knockback_up_speed") ? data.Distance("knockback_up_speed").ToNumber() : 0,
        };
        medium.BoostedJumpSpeed = data.Has("boosted_jump_speed") ? data.Distance("boosted_jump_speed").ToNumber() : medium.JumpSpeed;
        medium.BoostedWallJumpSpeed = data.Has("boosted_wall_jump_speed") ? data.Distance("boosted_wall_jump_speed").ToNumber() : medium.WallJumpSpeed;
        foreach ((string state, DataNode values) in data.Get("moves").Entries())
            medium.Moves[state] = MoveValues.Read(values);
        if (data.Has("speeds"))
        {
            foreach ((string speed, DataNode value) in data.Get("speeds").Entries())
                medium.Speeds[speed] = value.AsPixels().ToNumber();
        }

        if (data.Has("picture_hold"))
            medium.PictureHold = data.Get("picture_hold").AsInt();

        return medium;
    }
}

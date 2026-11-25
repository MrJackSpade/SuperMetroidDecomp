using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.Ui;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A face block: a carved face in the rock that does nothing until Samus has the Morph Ball. From then on it glows,
/// in step with every face block in the room, and turns its eyes to her the first time she comes near on the side it
/// watches. Nothing touches it.
/// </summary>
/// <remarks>
/// Its timelines are "looking_ahead", "samus_left" and "samus_right". Its data lists the glow's levels under "glow",
/// in the order the glow goes through them and round again: each a change of the colours of its pictures, and of
/// whatever else shares them, written place by place as any change of colours is. "glow_updates" is how long each
/// level lasts.
/// </remarks>
public partial class FaceBlock : Enemy
{
    /// <summary>The name its glow's colours are asked under on what shows them.</summary>
    public const string GlowSource = "face_block_glow";

    /// <summary>How near Samus must come, in pixels across and down, for it to look at her.</summary>
    [Export]
    public int TriggerDistance { get; set; } = 64;

    /// <summary>The equipment Samus must have for it to wake.</summary>
    [Export, DataValue("wakes_with")]
    public string WakesWith { get; set; } = "";

    /// <summary>How many updates each level of its glow lasts.</summary>
    [Export, DataValue("glow_updates")]
    public int GlowUpdates { get; set; } = 16;

    private ColourChange[] _glowLevels = [];
    private ColourChange? _shownGlow;
    private bool _looked;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _glowLevels = [.. data.List("glow").Select(ColourChange.Read)];
        if (_glowLevels.Length == 0)
            throw data.Problem("gives the face block's glow no level");
    }

    /// <summary>
    /// The level of its glow the clock is at, the same for every face block, while Samus has what wakes it; nothing
    /// while it sleeps.
    /// </summary>
    public ColourChange? Glow =>
        IsAwake ? _glowLevels[(int)(Room.Session.Clock.Updates / GlowUpdates % _glowLevels.Length)] : null;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        Play("looking_ahead");
        _looked = false;
        _shownGlow = null;
        ShowGlow();
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        ShowGlow();
        if (!IsAwake || _looked)
            return;
        if (Math.Abs(Target.Y.Whole - Position.Y.Whole) >= TriggerDistance)
            return;
        int across = Target.X.Whole - Position.X.Whole;
        if (Math.Abs(across) >= TriggerDistance)
            return;
        Facing side = across < 0 ? Facing.Left : Facing.Right;
        if (side != Facing)
            return;
        Play(side == Facing.Left ? "samus_left" : "samus_right", restart: true);
        _looked = true;
    }

    /// <summary>Shows its glow's present level, or none while it sleeps.</summary>
    private void ShowGlow()
    {
        ColourChange glow = Glow ?? ColourChange.None;
        if (ReferenceEquals(glow, _shownGlow) || Visual is not { } visual)
            return;
        glow.ShowOn(visual, GlowSource);
        _shownGlow = glow;
    }

    /// <summary>Whether Samus has what wakes it.</summary>
    private bool IsAwake => Room.Player.Inventory.Owns(WakesWith);
}

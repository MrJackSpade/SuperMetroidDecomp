using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.Ui;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A face block: a carved face in the rock that does nothing until Samus has the Morph Ball. From then on it glows,
/// with the one glow its room's face blocks share (<see cref="FaceBlockGlow"/>), and turns its eyes to her the first
/// time she comes near on the side it watches. It is solid to her, a block wide and high; her shots and bombs
/// take no notice of it and fly on, her grapple's line is stopped by it, and nothing harms it.
/// </summary>
/// <remarks>
/// Its timelines are "looking_ahead", "samus_left" and "samus_right". Its data lists the glow's levels under "glow",
/// in the order the glow goes through them and round again: each a change of the colours of its pictures, and of
/// whatever else shares them, written place by place as any change of colours is. "glow_updates" is how long each
/// level lasts.
/// </remarks>
public partial class FaceBlock : Enemy
{
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
    private FaceBlockGlow? _glow;
    private bool _looked;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _glowLevels = [.. data.List("glow").Select(ColourChange.Read)];
    }

    /// <summary>
    /// Set up, it joins its room's glow, starts it if Samus already has what wakes it, and in any case starts the
    /// glow's present level over.
    /// </summary>
    protected override void OnEnterRoom()
    {
        Play("looking_ahead");
        _looked = false;
        _glow = FaceBlockGlow.In(Room, _glowLevels, GlowUpdates);
        if (Visual is { } visual)
            _glow.Join(visual);
        if (IsAwake)
            Glow();
        _glow.HoldLevelAnew();
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (!IsAwake)
            return;
        Glow();
        if (_looked)
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

        // Waking starts the glow's present level over.
        _glow?.HoldLevelAnew();
    }

    /// <summary>Starts its room's glow, if no block has, and shows the glow's present level.</summary>
    private void Glow()
    {
        if (_glow is { } glow && Visual is { } visual)
            glow.StartOn(visual);
    }

    /// <summary>Whether Samus has what wakes it.</summary>
    private bool IsAwake => Room.Player.Inventory.Owns(WakesWith);
}

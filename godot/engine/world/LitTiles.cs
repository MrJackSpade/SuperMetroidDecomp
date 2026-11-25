using Godot;

namespace Engine.World;

/// <summary>
/// A room's tiles drawn a second time, somewhere else: over a cover of the screen, in the colours that stay lit
/// under it (<see cref="Ui.LitColours"/>, given as its material). Whoever makes it puts it where its terrain is on
/// screen each update (<see cref="Follow"/>); it draws what the terrain draws and nothing of its own.
/// </summary>
public partial class LitTiles : Node2D
{
    /// <summary>The terrain whose tiles it draws.</summary>
    public Terrain? Of { get; set; }

    /// <summary>Puts it where its terrain is on screen now, and has it drawn again as the terrain now is.</summary>
    public void Follow()
    {
        if (Of is null)
            return;
        GlobalPosition = Of.GlobalPosition;
        QueueRedraw();
    }

    /// <inheritdoc/>
    public override void _Draw() => Of?.DrawTiles(this);
}

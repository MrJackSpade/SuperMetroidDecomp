using Engine.Actors;
using Engine.Actors.Components;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Rooms.Ceres;

/// <summary>
/// Samus set down in Ceres: she stands facing forward on a pad that, after a moment, lowers her a pixel at a time to
/// the platform below, and then she is free to move.
/// </summary>
/// <remarks>Its visuals "Pad" and "Platform" are shown while it plays and put away when it ends.</remarks>
public partial class CeresArrival : RoomSequence
{
    /// <summary>The pad's two pictures, which it flickers between.</summary>
    [Export]
    public Godot.Collections.Array<string> PadPictures { get; set; } = [];

    /// <summary>The picture of the platform she is lowered to.</summary>
    [Export(PropertyHint.File)]
    public string PlatformPicture { get; set; } = "";

    /// <summary>How many updates she waits before the pad starts down.</summary>
    private const int WaitUpdates = 60;

    /// <summary>How far below her the pad is, how far down the room the platform is, and where she is set down.</summary>
    private const int PadBelow = 28, PlatformY = 97, LandingY = 72;

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        Player samus = room.Player;
        ActorVisual pad = Part("Pad"), platform = Part("Platform");
        if (elapsed == 0)
        {
            samus.ChangeState(new FacingForwardState());
            pad.Bind(room.Content);
            platform.Bind(room.Content);
            platform.Show(PlatformPicture);
            platform.Position = new Vector2(samus.Position.X.Whole, PlatformY);
            pad.SetShown(true);
            platform.SetShown(true);
        }

        if (PadPictures.Count > 0)
            pad.Show(PadPictures[elapsed % PadPictures.Count]);
        pad.Position = new Vector2(samus.Position.X.Whole, samus.Position.Y.Whole + PadBelow);
        if (elapsed < WaitUpdates)
            return false;
        samus.Position = new PixelPosition(samus.Position.X, samus.Position.Y + Pixels.FromWhole(1));
        pad.Position = new Vector2(samus.Position.X.Whole, samus.Position.Y.Whole + PadBelow);
        if (samus.Position.Y.Whole <= LandingY)
            return false;
        samus.Position = new PixelPosition(samus.Position.X, Pixels.FromWhole(LandingY));
        pad.SetShown(false);
        platform.SetShown(false);
        samus.Release();
        return true;
    }

    private ActorVisual Part(string name) => GetNodeOrNull<ActorVisual>(name)
        ?? throw new InvalidOperationException($"The arrival \"{Name}\" has no visual named \"{name}\".");
}

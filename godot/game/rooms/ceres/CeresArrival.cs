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
    /// <summary>The pad's look, by the data file of its timeline: it flickers.</summary>
    [Export(PropertyHint.File)]
    public string PadTimeline { get; set; } = "";

    /// <summary>The look of the platform she is lowered to, by the data file of its timeline.</summary>
    [Export(PropertyHint.File)]
    public string PlatformTimeline { get; set; } = "";

    private TimelinePlayer? _pad;
    private TimelinePlayer? _platform;

    /// <summary>How many updates she waits before the pad starts down.</summary>
    private const int WaitUpdates = 60;

    /// <summary>How far below her the pad is, how far down the room the platform is, and where she is set down.</summary>
    private const int PadBelow = 28, PlatformY = 97, LandingY = 72;

    /// <summary>
    /// She faces forward on the pad from the moment the room is set up, before any update of hers is played: put in
    /// the front view where she is, at the room's top, as the cartridge puts her, and not moved down clear of its edge.
    /// </summary>
    protected override void OnStarted()
    {
        var room = (Room)GetParent();
        room.Player.ChangeState(new FacingForwardState());

        // The shaft is set up level as she arrives, as whenever its room is entered.
        CeresShaft.LeaveLevelStep(room.Session);
    }

    /// <summary>She is lowered before the view follows her, so it follows each pixel on the update it is made.</summary>
    public override bool PlaysBeforeView => true;

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        Player samus = room.Player;
        ActorVisual pad = Part("Pad"), platform = Part("Platform");
        if (elapsed == 0)
        {
            pad.Bind(room.Content);
            platform.Bind(room.Content);
            // Both lie in front of Samus and of whatever stands in front of her: the cartridge draws them with what
            // its enemies launch in front, before the enemies that stand there.
            RoomDepth.Stand(pad, Standing.Foremost);
            RoomDepth.Stand(platform, Standing.Foremost);
            _pad = new TimelinePlayer(FrameTimeline.Read(room.Content.ReadData(PadTimeline)));
            _platform = new TimelinePlayer(FrameTimeline.Read(room.Content.ReadData(PlatformTimeline)));
            platform.Position = new Vector2(samus.Position.X.Whole, PlatformY);
            pad.SetShown(true);
            platform.SetShown(true);
        }

        TimelinePlayer padLook = _pad ?? throw new InvalidOperationException("The arrival has not begun.");
        TimelinePlayer platformLook = _platform ?? throw new InvalidOperationException("The arrival has not begun.");
        padLook.Step();
        platformLook.Step();
        pad.Show(padLook.Shown);
        platform.Show(platformLook.Shown);
        // The pad is put under where she stands before she is lowered; the wait's last update is the first that lowers her.
        pad.Position = new Vector2(samus.Position.X.Whole, samus.Position.Y.Whole + PadBelow);
        if (elapsed < WaitUpdates - 1)
            return false;
        samus.Position = new PixelPosition(samus.Position.X, samus.Position.Y + Pixels.FromWhole(1));
        if (samus.Position.Y.Whole <= LandingY)
            return false;
        samus.Position = new PixelPosition(samus.Position.X, Pixels.FromWhole(LandingY));
        pad.SetShown(false);
        platform.SetShown(false);

        // She is let go still in the front view, and stays so until she is turned.
        samus.ChangeState(new StandingForwardState());
        return true;
    }

    private ActorVisual Part(string name) => GetNodeOrNull<ActorVisual>(name)
        ?? throw new InvalidOperationException($"The arrival \"{Name}\" has no visual named \"{name}\".");
}

using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Game.Rooms;
using Godot;

namespace Game;

/// <summary>
/// What Samus raises from the ground she lands and runs on, as the room's ground has it (<see cref="Ground"/>): splashes
/// on wet ground, dust on dusty ground, nothing on plain ground; and dust from her feet on a boosted run anywhere that is
/// not wet.
/// </summary>
public partial class Samus
{
    /// <summary>A landing's splash on wet ground, by the data file of its timeline.</summary>
    [Export(PropertyHint.File), DataValue("wet_landing_effect")]
    public string WetLandingEffect { get; set; } = "";

    /// <summary>How many pixels right of her middle each of a wet landing's two splashes begins: the right-hand one, then the left.</summary>
    [Export, DataValue("wet_landing_across")]
    public int[] WetLandingAcross { get; set; } = [];

    /// <summary>How many pixels above the lowest row of her body a wet landing's splashes begin.</summary>
    [Export, DataValue("wet_landing_rise")]
    public int WetLandingRise { get; set; }

    /// <summary>How many updates a wet landing's right-hand splash holds her breath's next bubble off.</summary>
    [Export, DataValue("wet_landing_bubble_hold_updates")]
    public int WetLandingBubbleHoldUpdates { get; set; } = 1;

    /// <summary>A landing's dust on dusty ground, by the data file of its timeline.</summary>
    [Export(PropertyHint.File), DataValue("dusty_landing_effect")]
    public string DustyLandingEffect { get; set; } = "";

    /// <summary>How many pixels right of her middle each of a dusty landing's two clouds begins: the right-hand one, then the left.</summary>
    [Export, DataValue("dusty_landing_across")]
    public int[] DustyLandingAcross { get; set; } = [];

    /// <summary>How many pixels above the lowest row of her body a dusty landing's clouds begin.</summary>
    [Export, DataValue("dusty_landing_rise")]
    public int DustyLandingRise { get; set; }

    /// <summary>How many updates a dusty landing's right-hand cloud holds her breath's next bubble off.</summary>
    [Export, DataValue("dusty_landing_bubble_hold_updates")]
    public int DustyLandingBubbleHoldUpdates { get; set; } = 1;

    /// <summary>The two splashes a running foot raises on wet ground, by the data files of their timelines: the leading one, then the trailing.</summary>
    [Export, DataValue("wet_footstep_effects")]
    public string[] WetFootstepEffects { get; set; } = [];

    /// <summary>The two clouds a running foot raises on a boosted run, by the data files of their timelines: the leading one, then the trailing.</summary>
    [Export, DataValue("boosted_footstep_effects")]
    public string[] BoostedFootstepEffects { get; set; } = [];

    /// <summary>How many pixels ahead of her middle a running foot's leading splash or cloud begins.</summary>
    [Export, DataValue("footstep_ahead")]
    public int FootstepAhead { get; set; }

    /// <summary>How many pixels behind her middle a running foot's trailing splash or cloud begins.</summary>
    [Export, DataValue("footstep_behind")]
    public int FootstepBehind { get; set; }

    /// <summary>How many pixels below her middle a running foot's splashes and clouds begin.</summary>
    [Export, DataValue("footstep_below")]
    public int FootstepBelow { get; set; }

    /// <summary>How many updates a boosted run's leading cloud holds the next spray of a burning liquid off.</summary>
    [Export, DataValue("boosted_footstep_spray_hold_updates")]
    public int BoostedFootstepSprayHoldUpdates { get; set; } = 1;

    /// <summary>What the room's ground is under her: none where it is plain, or where the room says nothing of it.</summary>
    private GroundKind? GroundUnder => Room.GetChildren().OfType<Ground>().SingleOrDefault()?.Under(Position);

    /// <summary>
    /// Raises what her landing raises from the ground she came down on, as the cartridge does: a pair of splashes on wet
    /// ground and a pair of dust clouds on dusty ground, neither while her feet are under a liquid, the right-hand one
    /// holding her breath's next bubble off while it shows. A landing on plain ground raises nothing and lets her
    /// breath's next bubble go at once.
    /// </summary>
    private void RaiseFromLanding()
    {
        switch (GroundUnder)
        {
            case GroundKind.Wet:
                RaiseLandingPair(WetLandingEffect, WetLandingAcross, WetLandingRise, WetLandingBubbleHoldUpdates);
                break;
            case GroundKind.Dusty:
                RaiseLandingPair(DustyLandingEffect, DustyLandingAcross, DustyLandingRise, DustyLandingBubbleHoldUpdates);
                break;
            default:
                _bubbleWait = Countdown.Over;
                break;
        }
    }

    /// <summary>Raises a landing's pair at the lowest row of her body, unless her feet are under a liquid, which leaves the bubble's wait as it stands.</summary>
    private void RaiseLandingPair(string effect, int[] across, int rise, int bubbleHold)
    {
        if (FeetAreInLiquid)
            return;
        int y = Feet.Y.Whole - rise;
        FrameTimeline timeline = FrameTimeline.Read(Room.Content.ReadData(effect));
        foreach (int x in across)
            Room.Spawn(Effect.Of(timeline, PixelPosition.FromWhole(Position.X.Whole + x, y)));
        _bubbleWait = Countdown.Of(bubbleHold);
    }

    /// <summary>
    /// Raises what a running foot raises as it comes down, as the cartridge does: a pair of splashes on wet ground,
    /// which lets the next spray of a burning liquid go at once; otherwise, on a boosted run with the bottom of her body
    /// clear of any liquid, a pair of dust clouds, the leading one holding that spray off while it shows.
    /// </summary>
    private void RaiseFromFootstep()
    {
        if (GroundUnder == GroundKind.Wet)
        {
            RaiseFootstepPair(WetFootstepEffects);
            _sprayWait = Countdown.Over;
        }
        else if (IsBoosting && !BottomIsInLiquid)
        {
            RaiseFootstepPair(BoostedFootstepEffects);
            _sprayWait = Countdown.Of(BoostedFootstepSprayHoldUpdates);
        }
    }

    /// <summary>
    /// Whether the row just below her body lies under a liquid she meets, as the cartridge tests the boundary of her body
    /// rather than its last row for a boosted run's dust.
    /// </summary>
    private bool BottomIsInLiquid =>
        Room.Liquid is { IsMet: true } liquid && liquid.IsSubmerged(PixelPosition.FromWhole(Position.X.Whole, BodyRect.Bottom));

    /// <summary>Raises a running foot's pair: the leading one ahead of her middle, the trailing one behind it.</summary>
    private void RaiseFootstepPair(string[] effects)
    {
        int y = Position.Y.Whole + FootstepBelow;
        int ahead = Position.X.Whole + Facing.Toward(Pixels.FromWhole(FootstepAhead)).Whole;
        int behind = Position.X.Whole + Facing.Opposite().Toward(Pixels.FromWhole(FootstepBehind)).Whole;
        Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(effects[0])), PixelPosition.FromWhole(ahead, y)));
        Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(effects[1])), PixelPosition.FromWhole(behind, y)));
    }
}

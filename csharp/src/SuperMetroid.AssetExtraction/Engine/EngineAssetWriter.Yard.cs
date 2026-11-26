using System.Text.Json.Nodes;
using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The Yard: the snail of Maridia.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>
    /// Writes the Yard: for each of its eight ways of crawling its crawl, its two turns and its hiding, and for each
    /// way its shell faces its flight and its landing.
    /// </summary>
    private void WriteYard()
    {
        static string? Cue(YardCue cue) => cue switch
        {
            YardCue.BeginCrawling => "crawl",
            // The Godot Yard knows the way it crawls, and so what it hides with, from the crawl itself.
            YardCue.SetDirection or YardCue.SetHidingAnimation => null,
            // It is in the air from the moment it is knocked there.
            YardCue.BeginFlight => null,
            YardCue.MoveBy move => $"move:{move.Offset.X}:{move.Offset.Y}",
            YardCue.Signal { Kind: YardCueKind.BeginTurn } => "turn",
            YardCue.Signal { Kind: YardCueKind.BeginHiding } => "hide",
            YardCue.Signal { Kind: YardCueKind.ShellClosedUpdateEnds } => "shell_closed",
            _ => throw new InvalidDataException($"The Yard's animation raises {cue}, which this does not know."),
        };

        SignedSixteenSixteen none = SignedSixteenSixteen.FromWholePixels(0);
        SignedSixteenSixteen gravity = YardFallAcceleration.Accelerated(none) - none;
        // Its fall is held below the first whole speed that gravity will not carry it up to.
        int fallLimit = 1;
        while (YardFallAcceleration.Accelerated(SignedSixteenSixteen.FromWholePixels(fallLimit) - gravity).WholePixels == fallLimit)
            fallLimit++;
        SignedSixteenSixteen fast = SignedSixteenSixteen.FromWholePixels(8);
        SignedSixteenSixteen drag = fast - YardAirborneSlowing.Slowed(fast);

        // It feels the same distance ahead whichever way it crawls.
        int lookahead = YardTurnDefinitions.Turns.Select(turn => Math.Abs(turn.Value.Lookahead.X) + Math.Abs(turn.Value.Lookahead.Y)).Distinct().Single();

        EngineEnemy yard = Enemy("enemies/yard.json", "Yard", EnemyDefinitionId.Yard).BodyColliders("Body", "Solid", "Hurtbox")
            .Value("aggressive_crawl_speed", EngineNames.Number(YardVelocityDefinitions.AggressiveCrawlSpeed))
            .Value("hiding_reach_above", YardMotionDefinitions.HidingReachAbove)
            .Value("hiding_probe_reach", YardMotionDefinitions.HidingProbeMargin)
            .Value("turn_lookahead", lookahead)
            .Value("turn_rest_updates", YardMotionDefinitions.TurnSuppressionPasses)
            .Value("landing_speed_limit", YardMotionDefinitions.LandingWholeSpeedLimit)
            .Value("gravity", EngineNames.Number(gravity))
            .Value("fall_speed_limit", fallLimit)
            .Value("air_drag", EngineNames.Number(drag))
            .Value("shot_launch_speed", YardMotionDefinitions.BeamLaunchWholeSpeed)
            // Its bounce joins only a queue holding at most two.
            .Value("bounce_sound", EngineNames.Sound(EnemySoundEffectDefinitions.YardBounceSound, SoundRequestPriority.Low))
            // The speed down a kick gives it, by the whole pixels of the camera's last measure of Samus's movement across.
            .Group("kick_speeds", EngineNames.List(YardKickDefinitions.Kicks.Select(EngineNames.Number)));
        // It is shaken off what it clings to as the shake of a super missile striking a wall begins, and at no other.
        yard.Data["falls_with_shake"] = Shake(SuperMissileDefinitions.WallImpactEarthquake, SuperMissileDefinitions.EarthquakeDuration);

        foreach (YardDirection direction in Enum.GetValues<YardDirection>())
        {
            // "OnFloorMovingLeft" is the way "floor_left".
            string way = EngineSheets.FileName(direction.ToString().Replace("On", "", StringComparison.Ordinal).Replace("Moving", "", StringComparison.Ordinal));
            YardDirectionDefinition crawl = YardDirectionDefinitions.Directions[direction];
            YardTurnDefinition turn = YardTurnDefinitions.Turns[direction];
            Animation<YardSegment> hiding = YardAnimations.StartingWith(crawl.HidingAnimation);
            if (hiding.End is not AnimationEnd.Loop { Phase: > 1 } into)
                throw new InvalidDataException($"The Yard's hiding {crawl.HidingAnimation} does not tuck its legs, shut its shell and then lead into a crawl.");
            // Its shut shell onward, which it comes back to while it keeps its shell shut.
            Animation<YardSegment> shell = Animation<YardSegment>.LeadingInto([.. hiding.Phases.Take(into.Phase).Skip(1)], [.. hiding.Phases.Skip(into.Phase)]);
            yard.Timeline($"crawl_{way}", YardAnimations.Segments, YardAnimations.StartingWith(crawl.CrawlingAnimation), Cue)
                .Timeline($"outside_turn_{way}", YardAnimations.Segments, YardAnimations.StartingWith(turn.OutsideTurnAnimation), Cue)
                .Timeline($"inside_turn_{way}", YardAnimations.Segments, YardAnimations.StartingWith(turn.InsideTurnAnimation), Cue)
                .Timeline($"hide_{way}", YardAnimations.Segments, hiding, Cue)
                .Timeline($"shell_{way}", YardAnimations.Segments, shell, Cue);
        }

        foreach (HorizontalDirection facing in new[] { HorizontalDirection.Left, HorizontalDirection.Right })
        {
            YardAirborneAnimationDefinition airborne = YardDirectionDefinitions.AirborneFacings[facing];
            yard.Timeline($"airborne_{EngineNames.Side(facing)}", YardAnimations.Segments, YardAnimations.StartingWith(airborne.VisibleAnimation), Cue)
                .Timeline($"landed_{EngineNames.Side(facing)}", YardAnimations.Segments, YardAnimations.StartingWith(airborne.HidingAnimation), Cue);
        }

        yard.Write();
    }
}

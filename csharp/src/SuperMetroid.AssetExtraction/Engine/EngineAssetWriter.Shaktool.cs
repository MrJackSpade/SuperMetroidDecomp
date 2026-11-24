using System.Text.Json.Nodes;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>Shaktool: the digging machine of Maridia.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>
    /// Writes Shaktool: its seven parts with where each starts and how it turns, what each shows and how each nods at a
    /// wall, the reach of a part about the one before it at every angle, and its head pointing each of eight ways.
    /// </summary>
    private void WriteShaktool()
    {
        static string? Cue(ShaktoolCue cue) => cue.Kind switch
        {
            ShaktoolCueKind.Lower => "lower",
            ShaktoolCueKind.Raise => "raise",
            ShaktoolCueKind.ResumeMotion => "resume",
            _ => throw new InvalidDataException($"Shaktool's animation raises {cue.Kind}, which this does not know."),
        };

        // A timeline is named as the old animation it is.
        static string Named(ShaktoolSegment segment) => EngineSheets.FileName(segment.ToString());

        // A count of angle steps that the Godot Shaktool keeps whole.
        static int WholeSteps(RotationAngle angle, string what) => angle.RawValue % RotationAngle.FractionsPerStep == 0
            ? angle.RawValue / RotationAngle.FractionsPerStep
            : throw new InvalidDataException($"Shaktool's {what} is not a whole number of angle steps.");

        EngineEnemy shaktool = Enemy("enemies/shaktool.json", "Shaktool", EnemyDefinitionId.Shaktool);
        if (shaktool.Definition.Radius.X != shaktool.Definition.Radius.Y)
            throw new InvalidDataException("A part of Shaktool is not as wide as it is tall, which the Godot Shaktool's one size cannot say.");
        shaktool
            .Value("part_half_size", shaktool.Definition.Radius.X)
            .Value("straighten_at", WholeSteps(ShaktoolOrbitDefinitions.StraighteningThreshold, "straightening threshold"))
            .Value("straighten_restart", WholeSteps(ShaktoolOrbitDefinitions.StraighteningStart, "straightening start"))
            .Value("straightening_turn", (int)ShaktoolOrbitDefinitions.StraighteningTurn.RawValue)
            .Value("convergence_shares", ShaktoolOrbitDefinitions.ConvergenceShares)
            // Each part: its angle and its turn in 256ths of an angle step, what it shows and how it nods.
            .Group("parts", EngineNames.List(ShaktoolSegmentDefinitions.Segments.Select(part => (JsonNode)new JsonObject
            {
                ["angle"] = (int)part.InitialOrbitAngle.RawValue,
                ["turn"] = (int)part.AngularVelocity.RawValue,
                ["look"] = Named(part.InitialAnimation),
                ["bob"] = Named(part.CollisionAnimation),
            })))
            // How far across a part sits from the one before it, at each angle step.
            .Group("orbit", EngineNames.List(Enumerable.Range(0, RotationAngle.StepsPerTurn)
                .Select(step => EngineNames.Number(ShaktoolOrbitDefinitions.OrbitSine.At(TrigonometryAngle.Wrapped(step))))));

        foreach (ShaktoolSegment segment in ShaktoolSegmentDefinitions.Segments.SelectMany(part => new[] { part.InitialAnimation, part.CollisionAnimation }).Distinct())
            shaktool.Timeline(Named(segment), ShaktoolAnimations.Segments, ShaktoolAnimations.StartingWith(segment), Cue);

        // The head as it points each way: one picture that stays.
        foreach (KeyValuePair<CompassDirection, EnemyFrameId> aimed in ShaktoolHeadPictureDefinitions.AimedPictures)
        {
            shaktool.Timeline($"head_{EngineSheets.FileName(aimed.Key.ToString())}", new JsonObject
            {
                ["steps"] = new JsonArray(new JsonObject { ["picture"] = shaktool.Picture(aimed.Value), ["updates"] = 1 }),
                ["loops_to"] = 0,
            });
        }

        shaktool.Write();
    }
}

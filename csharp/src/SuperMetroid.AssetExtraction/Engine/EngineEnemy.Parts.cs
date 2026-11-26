using System.Text.Json.Nodes;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>What a thing of several parts adds: where its parts are in each of its poses, and its pictures in its other colours.</summary>
internal sealed partial class EngineEnemy
{
    /// <summary>The cue that puts a thing's parts where one of its poses has them.</summary>
    internal const string PoseCue = "pose:";

    /// <summary>The name of the pose that goes with one of an enemy's frames: the frame's name as its picture's file has it.</summary>
    internal static string PoseName(EnemyFrameId frame) => EngineSheets.FileName(EnemySheetDefinitions.FrameName(frame));

    /// <summary>
    /// Gives every step of a timeline that shows a picture the cue of the pose named as the picture's file is, ahead
    /// of the step's other cues: the old game's hitboxes belong to the frame shown, and the Godot game's parts are
    /// moved by cues.
    /// </summary>
    internal static JsonObject WithPoseCues(JsonObject timeline)
    {
        foreach (JsonNode? step in timeline["steps"]!.AsArray())
        {
            string picture = step!["picture"]!.GetValue<string>();
            if (picture.Length == 0)
                continue;
            string name = picture[(picture.LastIndexOf('/') + 1)..picture.LastIndexOf('.')];
            var cues = new JsonArray { PoseCue + name };
            if (step["cues"] is JsonArray heard)
            {
                foreach (JsonNode? cue in heard)
                    cues.Add(cue!.GetValue<string>());
            }

            step["cues"] = cues;
        }

        return timeline;
    }

    /// <summary>
    /// Makes the enemy one that is met through the boxes of the picture it shows, as the old game's frame hitboxes
    /// are: every step of the timelines it has so far takes the pose of its picture, and each pose puts the scene's
    /// hurtboxes "Part1", "Part2" and so on, and its hitboxes "Part1Touch", "Part2Touch" and so on, where the old
    /// frame has its boxes: the one for a shot, the other for Samus's touch, which the old probe meets on different
    /// edges (<see cref="ShotRect"/>, <see cref="TouchRect"/>). A picture whose frame the old game gives no boxes is
    /// a pose with none. Where <paramref name="shot"/> is given, each pose says under "shots" what
    /// it names each box's reaction to a shot.
    /// </summary>
    internal EngineEnemy PosedByFrame(EnemyFrameCollision<EnemyCollisionHitbox> frames, Func<EnemyReaction, string>? shot = null)
    {
        var shown = new HashSet<string>(StringComparer.Ordinal);
        foreach ((string _, JsonNode? timeline) in _timelines)
        {
            foreach (JsonNode? step in WithPoseCues(timeline!.AsObject())["steps"]!.AsArray())
            {
                if (step!["cues"] is JsonArray cues && cues[0]!.GetValue<string>() is { } first && first.StartsWith(PoseCue, StringComparison.Ordinal))
                    shown.Add(first[PoseCue.Length..]);
            }
        }

        var poses = new JsonObject();
        foreach ((EnemyFrameId frame, EnemyCollisionHitbox[] boxes) in frames)
        {
            if (frame == EnemyFrameId.PointAtOrigin || !shown.Remove(PoseName(frame)))
                continue;
            JsonObject colliders = [], shots = [];
            foreach (EnemyCollisionHitbox box in boxes)
            {
                string part = $"Part{colliders.Count / 2 + 1}";
                colliders[part] = ShotRect(box);
                colliders[part + "Touch"] = TouchRect(box);
                if (shot is not null)
                    shots[part] = shot(box.Shot);
            }

            var pose = new JsonObject { ["colliders"] = colliders };
            if (shot is not null)
                pose["shots"] = shots;
            poses[PoseName(frame)] = pose;
        }

        foreach (string bare in shown)
            poses[bare] = new JsonObject { ["colliders"] = new JsonObject() };
        return Group("poses", poses);
    }

    /// <summary>
    /// One of the old game's frame hitboxes as the hurtbox a shot meets on the same pixels. The old probe meets a shot
    /// on the box's left and top edges and not on its right and bottom ones (EnemyHitboxProbe.Overlaps), where the
    /// Godot rectangles are half-open the other way: the hurtbox is a pixel more at the left and at the top.
    /// </summary>
    internal static JsonObject ShotRect(EnemyCollisionHitbox hitbox) => ShotRect(hitbox.Left, hitbox.Top, hitbox.Right, hitbox.Bottom);

    /// <summary>The hurtbox a shot meets on the pixels the old probe meets it on a frame box with these edges.</summary>
    internal static JsonObject ShotRect(int left, int top, int right, int bottom) => new()
    {
        ["left"] = left - 1, ["top"] = top - 1, ["width"] = right - left + 1, ["height"] = bottom - top + 1,
    };

    /// <summary>
    /// One of the old game's frame hitboxes as the hitbox Samus's touch meets on the same pixels: the old probe meets
    /// her on the box's right and bottom edges and not on its left and top ones, so the hitbox is a pixel more at
    /// the right and at the bottom.
    /// </summary>
    internal static JsonObject TouchRect(EnemyCollisionHitbox hitbox) => TouchRect(hitbox.Left, hitbox.Top, hitbox.Right, hitbox.Bottom);

    /// <summary>The hitbox Samus's touch meets on the pixels the old probe meets her on a frame box with these edges.</summary>
    internal static JsonObject TouchRect(int left, int top, int right, int bottom) => new()
    {
        ["left"] = left, ["top"] = top, ["width"] = right - left + 1, ["height"] = bottom - top + 1,
    };

    /// <summary>
    /// Cuts the pictures of one of its timelines of what it spawns (<see cref="SpawnedLook{TFrame, TEvent}"/>) again
    /// in one of their sheet's shades, into the folder a set of its pictures named <paramref name="after"/> keeps them in.
    /// </summary>
    internal EngineEnemy ShadedSpawnedSet(string after, string name, IEnumerable<EnemyProjectileFrameId> frames, string shade)
    {
        // They lie under the set's folder, and so flash in the colours written for the set (ShadedSet): what it spawns
        // is drawn in its own colours, and the sheet they are cut from holds other things' colours beside.
        _sheets.CutSpawnedShade(_tree, $"{Folder}{after}/{name}", EnemyDefinitionIdKeys.Installed.Of(_id), frames.Select(EnemySheetDefinitions.ProjectileFrameName), shade);
        return this;
    }
}

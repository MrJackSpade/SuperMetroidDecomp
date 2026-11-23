using System.Runtime.CompilerServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Her brain drawn as the room's late draw draws it (#2604): under the graphics set a retail room holding her head draws,
/// her brain shown by her head's main, and the picture read from what her own draw
/// (<see cref="MotherBrainHead.DrawMotherBrainBrainAndNeck"/>) publishes. The fixture's session draws from the installed
/// sheets alone (<see cref="Session"/>), composing nothing, so drawing her brain builds her head's sheet and no other, and
/// the picture is read by looking up the frames her head's sheet shows it in, indexed once (#2606).
/// </summary>
internal static class MotherBrainBrainDraws
{
    /// <summary>Her head's frames by the picture her head's installed sheet shows each in, indexed once per process.</summary>
    private static readonly Lazy<Dictionary<SpriteSheetFrame, EnemyFrameId[]>> headFrames = new(() =>
    {
        SpriteSheet sheet = InstalledFixtures.EnemySheets.For(SpawnedObjectFixtures.SetDrawing(EnemyDefinitionId.MotherBrainHead), EnemyDefinitionId.MotherBrainHead);
        var byPicture = new Dictionary<SpriteSheetFrame, List<EnemyFrameId>>();
        foreach (EnemyFrameId frame in MotherBrainAnimations.HeadSegments.Frames)
        {
            if (!sheet.frames.TryGetValue(EnemySheetDefinitions.FrameName(frame), out SpriteSheetFrame? picture))
                continue;
            if (!byPicture.TryGetValue(picture, out List<EnemyFrameId>? showing))
                byPicture.Add(picture, showing = []);
            showing.Add(frame);
        }
        return byPicture.ToDictionary(pictured => pictured.Key, pictured => pictured.Value.ToArray());
    });

    /// <summary>The enemy systems already drawn under the set that holds her head's artwork, with her head's frames by picture.</summary>
    private static readonly ConditionalWeakTable<RoomEnemySystem, Dictionary<SpriteSheetFrame, EnemyFrameId[]>> drawing = new();

    /// <summary>
    /// A fresh enemy session drawing from the installed sheets alone, which every system her brain is drawn in through
    /// <see cref="DrawMotherBrainBrain"/> is built over, so her brain's draw builds her head's sheet and no other.
    /// </summary>
    internal static EnemySession Session() => EnemySessionFixtures.DrawingInstalledSheets();

    /// <summary>Her brain's draws in a fixture's enemy system.</summary>
    extension(RoomEnemySystem enemies)
    {
        /// <summary>
        /// Draws her brain once, as the room's late draw does, and returns the picture it published, or null when it
        /// published none: a brain that is not live draws nothing. The system is one built over <see cref="Session"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">The published picture is not one picture of her head's frames.</exception>
        internal EnemyFrameId? DrawMotherBrainBrain(MotherBrainEnemyState state, SamusState samus)
        {
            ArgumentNullException.ThrowIfNull(state);
            Dictionary<SpriteSheetFrame, EnemyFrameId[]> frames = drawing.GetValue(enemies, system =>
            {
                SpawnedObjectFixtures.LoadSetDrawing(system, EnemyDefinitionId.MotherBrainHead);
                return headFrames.Value;
            });
            // Her head's main decides that her own draw shows her brain, as it does once her head first runs.
            if (!state.DrawBrain)
                MotherBrainHead.RunMotherBrainHeadMain(state.Head, state, samus);
            var sprites = new SpriteFrameBuffer();
            enemies.MotherBrainBehavior.Head.DrawMotherBrainBrainAndNeck(state, sprites, Position16<RoomPixels>.Zero, samus);
            sprites.FinalizeFrame();
            if (sprites.Draws.OfType<ImageSpriteDraw>().SingleOrDefault(draw => draw.Source.Kind == SpriteSourceKind.MotherBrainBrain) is not { } brain)
                return null;
            EnemyFrameId[] shown = frames.GetValueOrDefault(brain.Image) ?? [];
            return shown is [EnemyFrameId frame]
                ? frame
                : throw new InvalidOperationException($"Her brain's draw published a picture {shown.Length} of her head's frames show.");
        }
    }
}

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
/// (<see cref="MotherBrainHead.DrawMotherBrainBrainAndNeck"/>) publishes.
/// </summary>
internal static class MotherBrainBrainDraws
{
    /// <summary>The enemy systems already drawn under the set that holds her head's artwork.</summary>
    private static readonly ConditionalWeakTable<RoomEnemySystem, SpriteSheet> drawing = new();

    /// <summary>Her brain's draws in a fixture's enemy system.</summary>
    extension(RoomEnemySystem enemies)
    {
        /// <summary>
        /// Draws her brain once, as the room's late draw does, and returns the picture it published, or null when it
        /// published none: a brain that is not live draws nothing.
        /// </summary>
        /// <exception cref="InvalidOperationException">The published picture is not one picture of her head's frames.</exception>
        internal EnemyFrameId? DrawMotherBrainBrain(MotherBrainEnemyState state, SamusState samus)
        {
            ArgumentNullException.ThrowIfNull(state);
            SpriteSheet sheet = drawing.GetValue(enemies, system =>
            {
                SpawnedObjectFixtures.LoadSetDrawing(system, EnemyDefinitionId.MotherBrainHead);
                return InstalledFixtures.EnemySheets.For(SpawnedObjectFixtures.SetDrawing(EnemyDefinitionId.MotherBrainHead), EnemyDefinitionId.MotherBrainHead);
            });
            // Her head's main decides that her own draw shows her brain, as it does once her head first runs.
            if (!state.DrawBrain)
                MotherBrainHead.RunMotherBrainHeadMain(state.Head, state, samus);
            var sprites = new SpriteFrameBuffer();
            enemies.MotherBrainBehavior.Head.DrawMotherBrainBrainAndNeck(state, sprites, Position16<RoomPixels>.Zero, samus);
            sprites.FinalizeFrame();
            if (sprites.Draws.OfType<ImageSpriteDraw>().SingleOrDefault(draw => draw.Source.Kind == SpriteSourceKind.MotherBrainBrain) is not { } brain)
                return null;
            EnemyFrameId[] shown = [.. MotherBrainAnimations.HeadSegments.Frames.Where(frame =>
                sheet.frames.TryGetValue(EnemySheetDefinitions.FrameName(frame), out SpriteSheetFrame? picture) && picture == brain.Image)];
            return shown is [EnemyFrameId frame]
                ? frame
                : throw new InvalidOperationException($"Her brain's draw published a picture {shown.Length} of her head's frames show.");
        }
    }
}

using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Enemies stand in front of Samus or behind her as the game draws them (#2618): the enemy draw pass, which runs after
/// Samus is drawn, shows an enemy standing in front of her at the place taken before her drawing and one standing behind
/// her after it, so a Ceres door's frame covers her as she walks through it. The Ceres doors and walls stand in front of her.
/// </summary>
internal static class EnemyStackingVerification
{
    /// <summary>Checks the draw order of an enemy in front of Samus, Samus, and an enemy behind her.</summary>
    internal static void VerifyEnemiesStandInFrontOfAndBehindSamus()
    {
        ImportEnemyArtwork artwork = InstalledFixtures.EnemyArtwork;
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: artwork.Bosses);
        RoomEnemyGraphicsSetDefinition set = RoomEnemyGraphicsSetDefinitions.GreenPiratesShaft;
        enemies.DrawUnder(set);
        EnemyFrameId frame = EnemyDefinitionFrameDefinitions.Frames[EnemyDefinitionId.PirateGreenWalking][0];
        // The enemy listed first stands behind Samus and the one listed after it in front of her, so the order the draws
        // show in is the standings', not the list's.
        RoomEnemyPlace behind = Pirate(enemies.Places.InUpdateOrder[0], frame, new(40, 110), EnemyStacking.BehindSamus);
        RoomEnemyPlace inFront = Pirate(enemies.Places.InUpdateOrder[1], frame, new(80, 110), EnemyStacking.InFrontOfSamus);
        enemies.Drawing.Enqueue(behind, reached: true);
        enemies.Drawing.Enqueue(inFront, reached: true);

        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        SpriteDrawPlace enemiesInFront = sprites.NextPlace();
        var samus = new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.Samus),
            InstalledFixtures.EnemySheets.For(set, EnemyDefinitionId.PirateGreenWalking).Frame(EnemySheetDefinitions.FrameName(frame)),
            new Offset16<ScreenPixels>(60, 110), SpriteMirror.None) { Shading = PictureShading.Standard };
        sprites.Add(samus);
        IReadOnlyList<EnemyDrawList.PreparedEnemyDraw> prepared = enemies.Drawing.Advance(Position16<RoomPixels>.Zero);
        var front = new SpriteFrameBuffer();
        enemies.Drawing.DrawInFrontOfSamus(prepared, front, Position16<RoomPixels>.Zero);
        sprites.InsertAt(enemiesInFront, front);
        enemies.Drawing.DrawBehindSamus(prepared, sprites, Position16<RoomPixels>.Zero, RoomEnemyFrameFixtures.StandaloneSamus());
        sprites.FinalizeFrame();

        AssertEqual(3, sprites.Draws.Length, "the two enemies and Samus publish one draw each");
        AssertEqual(80, sprites.Draws[0].Origin.X, "the enemy standing in front of Samus is drawn frontmost");
        AssertTrue(ReferenceEquals(samus, sprites.Draws[1]), "Samus is drawn behind the enemy in front of her and in front of the other");
        AssertEqual(40, sprites.Draws[2].Origin.X, "the enemy standing behind Samus is drawn last");

        foreach (EnemyDefinitionId door in (EnemyDefinitionId[])[EnemyDefinitionId.CeresDoor, EnemyDefinitionId.CeresElevatorDoor,
                     EnemyDefinitionId.CeresRidleyRoomDoor, EnemyDefinitionId.CeresElevatorRoomWall])
            AssertEqual(EnemyStacking.InFrontOfSamus, RoomEnemyDefinitionCatalog.ById[door].Stacking, $"{door} stands in front of Samus");
    }

    /// <summary>Binds a green pirate showing <paramref name="frame"/> at <paramref name="at"/>, standing at <paramref name="stacking"/>.</summary>
    private static RoomEnemyPlace Pirate(RoomEnemyPlace place, EnemyFrameId frame, Position16<RoomPixels> at, EnemyStacking stacking)
    {
        place.Bind(EnemyDefinitionId.PirateGreenWalking, EnemyDefinitionFixtures.Blank);
        place.CollidesByFrameHitboxes = true;
        place.Frame = frame;
        place.Position = place.Position.WithWhole(at);
        place.Stacking = stacking;
        return place;
    }

    /// <summary>Runs the enemy stacking check.</summary>
    [VerificationCommand("--enemy-stacking")]
    internal static int EnemyStackingCommand()
    {
        Suite(nameof(VerifyEnemiesStandInFrontOfAndBehindSamus), VerifyEnemiesStandInFrontOfAndBehindSamus);
        return 0;
    }
}

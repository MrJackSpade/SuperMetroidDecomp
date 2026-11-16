using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>Checks where a ninja Space Pirate's setup puts its posts.</summary>
internal static class NinjaSpacePiratePostsVerification
{
    /// <summary>
    /// #2065: for every post distance whose half-span the setup accepts, from either start post and at a placement near
    /// the room's wrap, the production setup puts the posts where the original's sixteen-bit arithmetic does: midway
    /// between the requested posts, half the requested distance (rounded down) apart from it, then widened to the
    /// whole pixels of the spin's distance, its speed growing by 32 256ths an update until the distance reaches the
    /// half-span.
    /// </summary>
    internal static void VerifyNinjaSpacePiratePosts()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemyPlace slot = enemies.Places.InUpdateOrder[0];
        slot.Bind(EnemyDefinitionId.PirateSilverNinja, slot.Definition);
        int cases = 0;
        foreach (int x in new[] { 256, 40 })
        foreach (NinjaSpacePirateStartPost start in new[] { NinjaSpacePirateStartPost.Left, NinjaSpacePirateStartPost.Right })
        for (int distance = 0; distance <= 255; distance++)
        {
            slot.Position = slot.Position.WithWhole(new(x, 128));
            slot.SetSpawnSettings(new NinjaSpacePirateSpawnSettings(start, distance));
            NinjaSpacePirateBehavior.InitializeNinjaSpacePirate(slot);
            NinjaSpacePirateEnemyState state = slot.RequireSpeciesState<NinjaSpacePirateEnemyState>();

            bool startsAtLeft = start == NinjaSpacePirateStartPost.Left;
            int left = startsAtLeft ? x : (x - distance) & ushort.MaxValue;
            int right = startsAtLeft ? (x + distance) & ushort.MaxValue : x;
            int half = ((right - left) & ushort.MaxValue) >> 1;
            int middle = (left + half) & ushort.MaxValue;
            int speed = 0, covered = 0;
            do
            {
                speed += 32;
                covered += speed;
            }
            while (covered < half << 8);
            int widened = covered >> 8;

            string context = $"ninja at {x} starting at the {start} post {distance} pixels from the other";
            AssertEqual(middle, state.PostsMidpoint.X, $"{context}: posts' midpoint");
            AssertEqual((middle + widened) & ushort.MaxValue, state.RightPost.X, $"{context}: right post");
            AssertEqual((middle - widened) & ushort.MaxValue, state.LeftPost.X, $"{context}: left post");
            AssertEqual(startsAtLeft ? state.LeftPost.X : state.RightPost.X, slot.Position.X.Whole, $"{context}: starts at its post");
            cases++;
        }
        Console.WriteLine($"  Ninja Space Pirate posts: {cases} setups place their posts as the original's arithmetic does.");
    }
}

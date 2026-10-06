using System.Reflection;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyEnemyAnimationIsolation()
    {
        var stock = new EnemyIdentityFixture().Build();
        var edited = CreateEditedEnemyAnimationArtwork();
        AssertTrue(stock.ContentIdentity != edited.ContentIdentity, "animation replacement changes selected artwork identity");
        Suite(nameof(VerifyBoyonAnimationIsolation), () => VerifyBoyonAnimationIsolation(new(stock, golden: false), new(edited, golden: false)));
        Suite(nameof(VerifyTorizoAnimationIsolation), () => VerifyTorizoAnimationIsolation(new(stock, golden: true), new(edited, golden: true)));
        Console.WriteLine("Enemy animation isolation: RAM-only ordinary proximity/bounce, " +
            "boss attack/hitbox windows and death progression remain frame-exact with replaced display sequences and projectile art.");
    }

    private static void VerifyBoyonAnimationIsolation(EnemyAnimationFixture stock, EnemyAnimationFixture edited)
    {
        var heights = new HashSet<ushort>();
        var nativeFrames = new HashSet<ushort>();
        int differentDraws = 0, bounceSounds = 0;
        const int frames = 240;
        for (int frame = 0; frame < frames; frame++)
        {
            ushort samusX = (ushort)(frame is >= 32 and < 160 ? 128 : 384);
            stock.Samus.XPosition = edited.Samus.XPosition = samusX;
            stock.Step(frame); edited.Step(frame);
            AssertEnemyAnimationMechanics(stock, edited, frame);
            AssertAnimationValues(stock.Enemies.BoyonStates[0]!, edited.Enemies.BoyonStates[0]!, $"Boyon frame {frame}");
            heights.Add(stock.Actor.YPosition);
            nativeFrames.Add(stock.Actor.SpritemapPointer);
            if (stock.Enemies.LastBoyonSoundEffect is not null) bounceSounds++;
            var before = stock.Draw(); var after = edited.Draw();
            AssertEditedAnimationDraw(edited, after, frame);
            if (!before.LowTable.SequenceEqual(after.LowTable) || !before.HighTable.SequenceEqual(after.HighTable)) differentDraws++;
            AssertEnemyAnimationMechanics(stock, edited, frame);
        }
        AssertTrue(heights.Count > 8, "Boyon comparison exercises a moving arc, not an inert actor");
        AssertTrue(nativeFrames.Count >= 6, "Boyon comparison exercises idle and bounce frame selection");
        AssertTrue(bounceSounds > 0, "Boyon comparison executes the bounce gameplay callback");
        AssertEqual(frames, differentDraws, "Boyon's authored replacement is visible on every frame");
        Console.WriteLine($"Boyon animation isolation: {frames} paired frames, {nativeFrames.Count} native frames, " +
            $"{heights.Count} Y positions and {bounceSounds} bounce callbacks; OAM changes every frame without changing mechanics.");
    }

    private static void VerifyTorizoAnimationIsolation(EnemyAnimationFixture stock, EnemyAnimationFixture edited)
    {
        // The fixed right-orb program has 24 startup frames, six 6-frame firing
        // intervals and 12 recovery frames. Stop before Return can choose a new attack.
        const int attackFrames = 24 + 6 * 6 + 12;
        var nativeFrames = new HashSet<ushort>();
        var shotFrames = new List<int>();
        var heights = new HashSet<ushort>();
        int differentDraws = 0, hitSamples = 0, missSamples = 0;
        for (int frame = 0; frame < attackFrames; frame++)
        {
            stock.Step(frame); edited.Step(frame);
            AssertEnemyAnimationMechanics(stock, edited, frame);
            AssertAnimationValues(stock.Torizo!, edited.Torizo!, $"Golden Torizo attack frame {frame}");
            nativeFrames.Add(stock.Actor.SpritemapPointer);
            heights.Add(stock.Actor.YPosition);
            if (stock.Enemies.LastBombTorizoSoundEffect is not null) shotFrames.Add(frame);
            CompareTorizoHitWindows(stock, edited, frame, ref hitSamples, ref missSamples);
            var before = stock.Draw(); var after = edited.Draw();
            AssertEditedAnimationDraw(edited, after, frame);
            if (!before.LowTable.SequenceEqual(after.LowTable) || !before.HighTable.SequenceEqual(after.HighTable)) differentDraws++;
            AssertEnemyAnimationMechanics(stock, edited, frame);
        }
        AssertTrue(shotFrames.SequenceEqual(new[] { 24, 30, 36, 42, 48, 54 }), "six boss projectile/audio callbacks retain exact compiled attack frames");
        AssertEqual(GoldenTorizoRightOrbFrames().Length, nativeFrames.Count, "attack exercises all six native collision frames");
        AssertTrue(heights.Count > 1, "boss comparison exercises actual terrain movement");
        AssertTrue(hitSamples > 0 && missSamples > 0, "hit-window comparison exercises both physical hits and misses");
        AssertEqual(attackFrames, differentDraws, "boss replacement is visible on every attack frame");
        AssertEqual(12, stock.RandomCalls, "six orbs consume the compiled two RNG calls per spawn");

        stock.BeginDeath(); edited.BeginDeath();
        // Native death has 8 * (1+6) initial frames, 14 * (2+8*2)
        // flashing frames, then 64 waiting frames: publication is on zero-based frame 372.
        const int deathPublicationFrame = 8 * (1 + 6) + 14 * (2 + 8 * 2) + 64;
        int firstBossBitFrame = -1, deathDrawChanges = 0;
        for (int frame = 0; frame <= deathPublicationFrame + 1; frame++)
        {
            stock.Step(attackFrames + frame); edited.Step(attackFrames + frame);
            AssertEnemyAnimationMechanics(stock, edited, frame);
            AssertAnimationValues(stock.Torizo!, edited.Torizo!, $"Golden Torizo death frame {frame}");
            if (firstBossBitFrame < 0 && stock.BossBitCalls != 0) firstBossBitFrame = frame;
            if (frame == deathPublicationFrame)
            {
                AssertEqual(new BombTorizoMusicRequest(MusicCommand.SelectTrack(3), MusicCommandDelay.EightFrames),
                    stock.Enemies.LastBombTorizoMusicRequest!.Value, "death publishes the actual return-music command at its compiled frame");
                AssertTrue(stock.Enemies.EnemyProjectiles.Any(projectile => projectile.Kind == RoomEnemyProjectileKind.EnemyDeathPickup),
                    "death actually allocates pickups, rather than merely setting a debugger flag");
            }
            var before = stock.Draw(); var after = edited.Draw();
            AssertEditedAnimationDraw(edited, after, frame);
            if (!before.LowTable.SequenceEqual(after.LowTable) || !before.HighTable.SequenceEqual(after.HighTable)) deathDrawChanges++;
        }
        AssertEqual(deathPublicationFrame, firstBossBitFrame, "boss bit/drop/music publication retains exact compiled death frame");
        AssertEqual(1, stock.BossBitCalls, "death publishes the progression event exactly once");
        AssertTrue(stock.Torizo!.BossBitSet && stock.Torizo.ItemDropRequested, "death actually reaches both progression and item-drop callbacks");
        AssertTrue(deathDrawChanges > 0, "death and pickup replacement is visibly rendered, not ignored");
        Console.WriteLine($"Golden Torizo animation isolation: {attackFrames} attack frames, six timed orb callbacks, " +
            $"{hitSamples + missSamples} shot/touch samples, {deathPublicationFrame + 2} death frames; " +
            "identical motion, projectiles, damage fields, RNG and progression with different OAM.");
    }

    private static void CompareTorizoHitWindows(EnemyAnimationFixture stock, EnemyAnimationFixture edited,
        int frame, ref int hits, ref int misses)
    {
        MethodInfo collide = typeof(RoomEnemySystem).GetMethod("TryFindExtendedHitboxCallback",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (bool shot in new[] { false, true })
        for (int y = -64; y <= 32; y += 8)
        for (int x = -48; x <= 48; x += 8)
        {
            object?[] before = [stock.Actor, unchecked((ushort)(stock.Actor.XPosition + x)),
                unchecked((ushort)(stock.Actor.YPosition + y)), (ushort)2, (ushort)2, shot, (ushort)0];
            object?[] after = (object?[])before.Clone();
            after[0] = edited.Actor;
            bool hit = (bool)collide.Invoke(stock.Enemies, before)!;
            AssertEqual(hit, (bool)collide.Invoke(edited.Enemies, after)!, $"Torizo physical hit window frame {frame} at {x}/{y}");
            AssertEqual(before[^1], after[^1], $"Torizo physical callback frame {frame} at {x}/{y}");
            if (hit) hits++; else misses++;
        }
    }
}

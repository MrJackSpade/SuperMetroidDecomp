using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static int VerifyMotherBrainLaterContact()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_setMotherBrainBg2Scroll", flags)!
            .SetValue(enemies, (Action<Position16<Bg2Pixels>>)(_ => { }));
        var body = enemies.Slots[0];
        body.EnemyDefinitionPointer = EnemyDefinitionId.MotherBrainBody;
        body.Definition = RoomEnemyDefinitionCatalog.Get(body.EnemyDefinitionPointer);
        var state = new MotherBrainEnemyState(body) { Head = enemies.Slots[1], Form = 2, HitboxesEnabled = 7 };
        state.Head.Position = state.Head.Position.WithWhole(new(500, 500));
        state.NeckSegment1 = new(250, 80);
        state.NeckSegment2 = state.NeckSegment3 = new(500, 500);
        typeof(RoomEnemySystem).GetField("<MotherBrain>k__BackingField", flags)!.SetValue(enemies, state);
        var main = typeof(RoomEnemySystem).GetMethod("RunMotherBrainBodyMain", flags)!
            .CreateDelegate<Action<RoomEnemySlot, SamusState?, byte, SamusBombProjectileSystem?>>(enemies);
        foreach (string contact in new[] { "body", "neck", "no-damage side", "post-dispatch position" })
        {
            body.Position = body.Position.WithWhole(new(128, 128));
            state.Function = MotherBrainBodyFunction.FakeDeathAscentContinuePausing;
            state.FunctionTimer = contact == "post-dispatch position" ? (ushort)0 : (ushort)100;
            var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(158, 128)), Health = 500 };
            samus.Kinematics.Radius = new(8, 16);
            ushort expectedX = 20;
            if (contact == "neck") { samus.Position = samus.Position.WithWhole(new(250, 80)); expectedX = 16; }
            if (contact == "no-damage side") { samus.Position = samus.Position.WithWholeX(152); expectedX = 26; }
            if (contact == "post-dispatch position") { samus.Position = samus.Position.WithWhole(new(89, 279)); }
            main(body, samus, 0, null);
            AssertEqual((ushort)(contact == "no-damage side" ? 500 : 500 - body.Definition.Damage),
                samus.Health, $"Later-phase {contact} applies native contact damage");
            AssertEqual(expectedX, samus.Kinematics.ExtraDisplacement.X.WholePixels, $"{contact} horizontal displacement");
            AssertEqual((ushort)4, samus.Kinematics.ExtraDisplacement.Y.WholePixels, $"{contact} vertical displacement");
            AssertEqual((ushort)96, samus.InvincibilityTimer, "Native contact invincibility timer");
            AssertEqual((ushort)5, samus.KnockbackTimer, "Native contact knockback timer");
        }
        // Isolate the actual body callback in the ordinary radius collision pass.
        // Shot blocking is native behavior and must remain a dud, not body damage.
        body.Position = body.Position.WithWhole(new(128, 128));
        body.Radius = new(16, 16);
        body.SpritemapPointer = 1;
        body.Health = 1000;
        var interactive = (List<ushort>)typeof(RoomEnemySystem).GetField("_interactiveEnemyIndexes", flags)!.GetValue(enemies)!;
        interactive.Add(body.NativeIndex);
        foreach (ushort type in new ushort[] { 0x8000, 0x8100 })
        {
            var shots = new SamusProjectileSystem();
            var shot = shots.Slots[0];
            shot.Type = type; shot.Damage = 100; shot.Direction = 2;
            shot.Position = shot.Position.WithWhole(new(128, 128)); shot.Radius = new(4, 4);
            shot.InstructionPointer = 0x9000; shot.InstructionTimer = 1;
            shot.PreInstruction = type == 0x8100 ? SamusProjectilePreInstruction.Missile : SamusProjectilePreInstruction.NoWaveBeam;
            AssertEqual(1, enemies.ResolveOrdinaryProjectileHits(bus, shots, new SamusBombProjectileSystem(), new SamusState()),
                "Mother Brain body accepts the projectile collision callback");
            AssertEqual((ushort)1000, body.Health, "Body projectile callback retains body health");
            AssertTrue(shot.PackedDirection.HasLowByteLifecycleState, "Body blocks the beam or missile");
            AssertEqual((ushort?)0x3d, enemies.LastEnemyProjectileDudSoundEffect, "Body emits native dud sound");
        }
        AssertEqual(2, enemies.RoomSpriteObjects.Count(slot => slot.Kind == RoomSpriteObjectKind.EnemyProjectileDud),
            "Both beam and missile callbacks create their dud sprite");
        Console.WriteLine("Mother Brain later contact: body/neck damage, side condition, post-dispatch position and projectile duds passed.");
        return 0;
    }
}

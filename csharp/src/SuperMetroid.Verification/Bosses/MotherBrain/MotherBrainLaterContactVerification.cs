using SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.World;

/// <summary>Checks later-form Mother Brain contact and projectile blocking.</summary>
internal static class MotherBrainLaterContactVerification
{
    /// <summary>Checks body and neck knockback, side immunity, post-dispatch position, and beam or missile duds.</summary>
    internal static int VerifyMotherBrainLaterContact()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        var body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.MotherBrainBody, body.Definition);
        body.Bind(body.DefinitionId, RoomEnemyDefinitionCatalog.Get(body.DefinitionId));
        var state = new MotherBrainEnemyState(body) { Head = enemies.SlotTable.Slots[1], Form = MotherBrainForm.SecondPhase, HitboxesEnabled = MotherBrainHitboxParts.Body | MotherBrainHitboxParts.Brain | MotherBrainHitboxParts.Neck };
        state.Head.Position = state.Head.Position.WithWhole(new(500, 500));
        state.NeckJoints[1] = new(250, 80);
        state.NeckJoints[2] = state.NeckJoints[3] = new(500, 500);
        enemies.MotherBrainEncounter.MotherBrain = state;
        Action<RoomEnemySlot, SamusState?, SamusBombProjectileSystem?> main = enemies.MotherBrainBehavior.RunMotherBrainBodyMain;
        foreach (string contact in new[] { "body", "neck", "no-damage side", "post-dispatch position" })
        {
            body.Position = body.Position.WithWhole(new(128, 128));
            state.Function = MotherBrainBodyFunction.FakeDeathAscentContinuePausing;
            state.FunctionTimer = WrappingCountdown.Start(contact == "post-dispatch position" ? 0 : 100);
            var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(158, 128)), Health = 500 };
            samus.Kinematics.Radius = new(8, 16);
            ushort expectedX = 20;
            if (contact == "neck") { samus.Position = samus.Position.WithWhole(new(250, 80)); expectedX = 16; }
            if (contact == "no-damage side") { samus.Position = samus.Position.WithWholeX(152); expectedX = 26; }
            if (contact == "post-dispatch position") { samus.Position = samus.Position.WithWhole(new(89, 279)); }
            main(body, samus, null);
            AssertEqual((ushort)(contact == "no-damage side" ? 500 : 500 - body.Definition.Damage),
                samus.Health, $"Later-phase {contact} applies native contact damage");
            AssertEqual(expectedX, samus.Kinematics.ExtraDisplacement.X.WholePixels, $"{contact} horizontal displacement");
            AssertEqual(4, samus.Kinematics.ExtraDisplacement.Y.WholePixels, $"{contact} vertical displacement");
            AssertEqual((ushort)96, samus.InvincibilityTimer.RawValue, "Native contact invincibility timer");
            AssertEqual((ushort)5, samus.KnockbackTimer.RawValue, "Native contact knockback timer");
        }
        // Isolate the actual body callback in the ordinary radius collision pass.
        // Shot blocking is native behavior and must remain a dud, not body damage.
        body.Position = body.Position.WithWhole(new(128, 128));
        body.Radius = new(16, 16);
        body.Frame = EnemyFrameId.MotherBrainBodyOamStanding;
        body.Health = 1000;
        var interactive = (List<int>)enemies.ProcessingWindow.InteractiveEnemyIndexes;
        interactive.Add(body.NativeIndex);
        foreach (ushort type in new ushort[] { 0x8000, 0x8100 })
        {
            var shots = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None);
            var shot = shots.Slots[0];
            shot.Type = type; shot.Damage = 100; shot.Direction = 2;
            shot.Position = shot.Position.WithWhole(new(128, 128)); shot.Radius = new(4, 4);
            shot.Cursor = ProjectileFixtureCursors.Active; shot.InstructionTimer = WrappingCountdown.Start(1);
            shot.PreInstruction = type == 0x8100 ? SamusProjectilePreInstruction.Missile : SamusProjectilePreInstruction.NoWaveBeam;
            AssertEqual(1, enemies.Collision.ResolveOrdinaryProjectileHits(shots, new SamusBombProjectileSystem(), new SamusState()),
                "Mother Brain body accepts the projectile collision callback");
            AssertEqual(1000, body.Health, "Body projectile callback retains body health");
            AssertTrue(shot.PackedDirection.HasLowByteLifecycleState, "Body blocks the beam or missile");
            AssertEqual((SoundEffectId?)SoundEffectLibrary1Sounds.DudShot, enemies.Projectiles.LastEnemyProjectileDudSoundEffect, "Body emits native dud sound");
        }
        AssertEqual(2, enemies.RoomSpriteObjects.Count(slot => slot.Kind == RoomSpriteObjectKind.EnemyProjectileDud),
            "Both beam and missile callbacks create their dud sprite");
        Console.WriteLine("Mother Brain later contact: body/neck damage, side condition, post-dispatch position and projectile duds passed.");
        return 0;
    }

    /// <summary>Runs the focused later-form contact and dud check.</summary>
    [VerificationCommand("--mother-brain-later-contact")]
    internal static int MotherBrainLaterContactCommand()
    {
        return VerifyMotherBrainLaterContact();
    }
}

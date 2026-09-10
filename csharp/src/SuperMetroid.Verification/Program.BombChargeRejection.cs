using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyBombChargeRejection()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var level = new RoomLevelData(16, 16, new ushort[256], new byte[256], new ushort[256], new byte[8]);
        foreach (ushort charge in new ushort[] { 0, 58, 60 })
        foreach (bool bombsEquipped in new[] { false, true })
        foreach (bool powerBombSelected in new[] { false, true })
        foreach (bool freshShoot in new[] { false, true })
        {
            var samus = new SamusState
            {
                Pose = SamusPoseIds.MorphBallGroundRightPose,
                EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall |
                    (bombsEquipped ? SamusEquipmentFlags.Bombs : 0)),
                SelectedHudItem = powerBombSelected ? (ushort)3 : (ushort)0,
                PowerBombs = 2,
                XPosition = 80, YPosition = 80,
                ProjectileFlareCounter = charge,
            };
            samus.RefreshCollisionRadii(bus);
            var bombs = new SamusBombProjectileSystem();
            // Down postpones a valid full-charge spread. The no-edge rejection below
            // therefore belongs only to the ordinary-bomb/Power-Bomb helper, not to
            // releasing a spread. Equipment guards that skip that helper preserve charge.
            var result = bombs.StepFrame(bus, level, samus,
                (ushort)(SnesButton.X | SnesButton.Down), freshShoot ? (ushort)SnesButton.X : (ushort)0);
            bool helperRuns = powerBombSelected || bombsEquipped && charge < 60;
            bool cancelled = helperRuns && !freshShoot && charge != 0;
            string context = $"charge={charge}, Bombs={bombsEquipped}, PB={powerBombSelected}, edge={freshShoot}";
            AssertEqual(cancelled, result.BeamChargeConsumed, $"native bomb rejection charge bridge: {context}");
            var cancellations = result.SoundRequests!.Where(sound => sound ==
                new SamusSoundRequest(SoundEffectLibrary1Sounds.CancelAll, 9)).ToArray();
            AssertEqual(cancelled ? 1 : 0, cancellations.Length, $"single native charge cancellation sound: {context}");
            AssertEqual(helperRuns && freshShoot ? 1 : 0, bombs.BombCounter,
                $"rejection must not allocate a projectile: {context}");
        }
    }
}

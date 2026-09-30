using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal sealed partial class InstalledSamusIsolationTests
{
    private const ushort FlashChord = (ushort)(SnesButton.Down | SnesButton.L | SnesButton.R | SnesButton.X);

    private void CheckSpecialSequences()
    {
        foreach (bool left in new[] { false, true }) CheckCrystalFlash(left);
        CheckStoredShine();
        foreach (byte pose in new[] { SamusPoseIds.ShinesparkHorizontalRightPose,
            SamusPoseIds.ShinesparkHorizontalLeftPose, SamusPoseIds.ShinesparkVerticalRightPose,
            SamusPoseIds.ShinesparkVerticalLeftPose, SamusPoseIds.ShinesparkDiagonalRightPose,
            SamusPoseIds.ShinesparkDiagonalLeftPose }) CheckShinespark(pose);
        foreach (SamusSuitPickupKind kind in Enum.GetValues<SamusSuitPickupKind>()) CheckSuitPickup(kind);
        foreach (ushort suit in new[] { (ushort)0, (ushort)SamusEquipmentFlags.VariaSuit,
            (ushort)SamusEquipmentFlags.GravitySuit })
        foreach (byte source in new[] { SamusPoseIds.FacingRightNormalPose, SamusPoseIds.MorphBallGroundLeftPose,
            SamusPoseIds.SpinJumpRightPose }) CheckDeath(source, suit);
        CheckReserve();
        foreach (bool left in new[] { false, true }) CheckDrained(left);
    }

    private static SamusState PrepareFlash(SamusState samus)
    {
        samus.Health = 1; samus.MaxHealth = 1599;
        samus.Missiles = 10; samus.SuperMissiles = 10; samus.PowerBombs = 10;
        return samus;
    }

    private void CheckCrystalFlash(bool left)
    {
        Pair pair = Create(left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose);
        string context = $"Crystal Flash left={left}";
        pair.Apply(context + " setup", actor => { PrepareFlash(actor.Samus); });
        Require(!pair.Apply(context + " rejected chord", actor => actor.Samus.CrystalFlash.TryBegin(
            actor.Memory, actor.Samus, (ushort)(FlashChord | (ushort)SnesButton.A))), "Extra Flash input must be rejected");
        ushort initialY = pair.Stock.Samus.YPosition;
        Require(pair.Apply(context + " admission", actor => actor.Samus.CrystalFlash.TryBegin(
            actor.Memory, actor.Samus, FlashChord)), "Flash was not admitted");
        int frames = 0;
        while (pair.Stock.Samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive && frames < 360)
        {
            ushort frame = (ushort)frames++;
            pair.Apply(context + $" frame {frame}", actor =>
            {
                var result = actor.Samus.CrystalFlash.Step(actor.Memory, actor.Samus, frame);
                Animate(actor, frame);
                actor.Samus.CrystalFlash.UpdatePalette(actor.Memory, actor.Colors, actor.Samus, beams, actor.Crystal);
                return result;
            });
            if (frames == 10) Require(pair.Stock.Samus.YPosition == initialY - 20,
                "Flash must complete its ten-call, twenty-pixel raise");
            if (frames == 1)
            {
                foreach (Actor actor in new[] { pair.Stock, pair.Edited })
                {
                    for (int color = 0; color < CrystalFlashColorFormat.BodyColorCount; color++)
                        Require(actor.Colors.Colors[SamusPaletteRomData.CrystalFlash.BodyCgramStart + color] ==
                            actor.Crystal.ResolveBody(0, color), "Flash body colors were not applied");
                    for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
                        Require(actor.Colors.Colors[SamusPaletteRomData.CrystalFlash.BubbleCgramStart + color] ==
                            actor.Crystal.ResolveBubble(0, color), "Flash bubble colors were not applied");
                }
            }
        }
        Require(pair.Stock.Samus.CrystalFlash.Phase == CrystalFlashPhase.Inactive &&
            pair.Stock.Samus.Missiles == 0 && pair.Stock.Samus.SuperMissiles == 0 && pair.Stock.Samus.PowerBombs == 0 &&
            pair.Stock.Samus.Health == 1501 && !pair.Stock.Samus.CrystalFlashPoseInputLocked,
            context + ": resource drain, pose release or completion did not occur");
        Console.WriteLine($"  {context}: {frames} identical movement/animation/palette calls; all thirty ammo drains and release verified.");
    }

    private void CheckStoredShine()
    {
        Pair pair = Create();
        Require(pair.Apply("stored shine admission", actor => actor.Samus.Shinespark.TryStoreFromSpeedBooster(
            SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter)),
            "Stored shine was not admitted");
        for (int frame = 0; frame < 180; frame++)
            pair.Apply($"stored shine palette {frame}", actor => actor.Samus.Shinespark.UpdatePalette(
                actor.Memory, actor.Colors, actor.Samus.EquippedItems, actor.Suits, actor.Cycles));
        Require(pair.Stock.Samus.Shinespark.Phase == ShinesparkPhase.Inactive && pair.Stock.Samus.Shinespark.ShineTimer == 0,
            "Edited stored-shine colors must retain the 180-call expiration");
    }

    private void CheckShinespark(byte target)
    {
        bool left = (target & 1) == 0;
        Pair pair = Create(left ? SamusPoseIds.ShinesparkWindupLeftPose : SamusPoseIds.ShinesparkWindupRightPose);
        string context = $"shinespark pose {target:X2}";
        pair.Apply(context + " admission", actor =>
        {
            Require(actor.Samus.Shinespark.TryStoreFromSpeedBooster(
                SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter), "Spark charge rejected");
            actor.Samus.Shinespark.BeginWindup(actor.Samus);
        });
        for (ushort frame = 0; frame < 29; frame++)
            pair.Apply(context + $" windup {frame}", actor =>
            {
                var result = actor.Samus.Shinespark.Step(actor.Memory, actor.Level, actor.Samus, frame);
                Animate(actor, frame);
                return result;
            });
        Require(pair.Stock.Samus.Shinespark.Phase == ShinesparkPhase.Windup &&
            pair.Stock.Samus.Shinespark.StartStopTimer == 1, "Spark windup duration changed");
        pair.Apply(context + " launch", actor => actor.Samus.Shinespark.BeginDirectionalLaunch(actor.Memory, actor.Samus, target));
        int frames = 0;
        while (pair.Stock.Samus.Shinespark.Phase != ShinesparkPhase.Inactive && frames < 300)
        {
            ushort frame = (ushort)frames++;
            pair.Apply(context + $" motion/crash {frame}", actor =>
            {
                var result = actor.Samus.Shinespark.Step(actor.Memory, actor.Level, actor.Samus, frame,
                    gameTimeFrames: frame);
                Animate(actor, frame);
                actor.Samus.Shinespark.UpdatePalette(actor.Memory, actor.Colors, actor.Samus.EquippedItems, actor.Suits, actor.Cycles);
                return result;
            });
        }
        Require(pair.Stock.Samus.Shinespark.Phase == ShinesparkPhase.Inactive && pair.Stock.Samus.Health < 99,
            context + ": actual movement/energy drain/crash/release was not completed");
        Console.WriteLine($"  {context}: 29 windup calls and {frames} identical launch/crash/release calls.");
    }

    private void CheckSuitPickup(SamusSuitPickupKind kind)
    {
        Pair pair = Create();
        var stock = new SamusSuitPickupState(); var edited = new SamusSuitPickupState();
        stock.Begin(pair.Stock.Memory, pair.Stock.Samus, 0, 240, kind);
        edited.Begin(pair.Edited.Memory, pair.Edited.Samus, 0, 240, kind);
        int frames = 0;
        while (stock.IsActive && frames < 300)
        {
            stock.Step(pair.Stock.Memory, pair.Stock.Samus, pair.Stock.Colors);
            edited.Step(pair.Edited.Memory, pair.Edited.Samus, pair.Edited.Colors);
            new SamusMechanicsSnapshot(stock).RequireSame(new(edited), $"{kind} suit state {frames}");
            Require(stock.WindowTable.SequenceEqual(edited.WindowTable), $"{kind} suit window geometry changed");
            pair.Check($"{kind} suit transformation {frames}");
            pair.Draw($"{kind} suit transformation {frames++}");
        }
        Require(!stock.IsActive && !edited.IsActive && !pair.Stock.Samus.InputLocked,
            $"{kind} suit transformation did not release control");
        Require(pair.Stock.Samus.EquippedItems.HasAny(kind == SamusSuitPickupKind.Varia
            ? SamusEquipmentFlags.VariaSuit : SamusEquipmentFlags.GravitySuit), "Suit was not granted");
        Console.WriteLine($"  {kind} suit: {frames} identical stage/window/pose/control calls; edited suit colors applied.");
    }

    private void CheckDeath(byte source, ushort suit)
    {
        Pair pair = Create(source, suit);
        string context = $"death source {source:X2}, suit {suit:X4}";
        pair.Apply(context + " admission", actor => actor.Samus.DeathSequence.Begin(actor.Memory, actor.Samus, 0, 240), draw: false);
        var stockQueue = new VramWriteQueue(); var editedQueue = new VramWriteQueue();
        int frames = 0;
        while (pair.Stock.Samus.DeathSequence.Phase != SamusDeathSequencePhase.Complete && frames < 240)
        {
            var stockStep = pair.Stock.Samus.DeathSequence.Step(pair.Stock.Memory, pair.Stock.Samus,
                pair.Stock.Colors, stockQueue, pair.Stock.Body.DeathPalettes);
            var editedStep = pair.Edited.Samus.DeathSequence.Step(pair.Edited.Memory, pair.Edited.Samus,
                pair.Edited.Colors, editedQueue, pair.Edited.Body.DeathPalettes);
            Require(stockStep == editedStep, context + ": death timing/publication changed");
            Require(stockQueue.Entries.SequenceEqual(editedQueue.Entries), context + ": DMA cadence changed");
            pair.Check(context + $" frame {frames}");
            stockQueue.DrainTo(pair.Stock.Tiles, pair.Stock.Memory, new DeathAssets(pair.Stock.Body.DeathTiles));
            editedQueue.DrainTo(pair.Edited.Tiles, pair.Edited.Memory, new DeathAssets(pair.Edited.Body.DeathTiles));
            if (stockStep.DrawPose) pair.Draw(context + $" pose {frames}");
            if (stockStep.DrawExplosion)
            {
                var before = new SamusMechanicsSnapshot(pair.Stock.Samus);
                var a = new OamBuffer(); var b = new OamBuffer(); a.BeginFrame(); b.BeginFrame();
                pair.Stock.Samus.DeathSequence.DrawExplosion(pair.Stock.Memory, a, pair.Stock.Body.Spritemaps);
                pair.Edited.Samus.DeathSequence.DrawExplosion(pair.Edited.Memory, b, pair.Edited.Body.Spritemaps);
                before.RequireSame(new(pair.Stock.Samus), context + ": death draw mutated state");
                pair.Check(context + " explosion draw");
                draws++;
                if (!a.LowTable.SequenceEqual(b.LowTable)) changedOam++;
                if (!pair.Stock.Tiles.Bytes.SequenceEqual(pair.Edited.Tiles.Bytes)) changedTiles++;
            }
            frames++;
        }
        Require(frames == 211 && pair.Stock.Samus.DeathSequence.Phase == SamusDeathSequencePhase.Complete,
            context + ": death must retain its exact 16+60+135 call lifetime");
    }

    private void CheckReserve()
    {
        Pair pair = Create();
        pair.Apply("reserve setup", actor =>
        { actor.Samus.Health = 0; actor.Samus.ReserveEnergy = 42; actor.Samus.ReserveTankMode = 1; });
        var stock = new SamusReserveAutoRecoveryState(); var edited = new SamusReserveAutoRecoveryState();
        stock.Begin(pair.Stock.Samus); edited.Begin(pair.Edited.Samus);
        ushort initialFrame = pair.Stock.Samus.AnimationFrame;
        for (ushort frame = 0; frame < 42; frame++)
        {
            Require(stock.StepAfterNmi(pair.Stock.Samus, frame) == edited.StepAfterNmi(pair.Edited.Samus, frame),
                "Reserve refill or sound cadence changed");
            new SamusMechanicsSnapshot(stock).RequireSame(new(edited), "reserve owner");
            pair.Check($"reserve frame {frame}"); pair.Draw($"reserve frame {frame}");
            Require(pair.Stock.Samus.AnimationFrame == initialFrame, "Reserve recovery must not advance Samus animation");
        }
        Require(!stock.IsActive && pair.Stock.Samus.Health == 42 && pair.Stock.Samus.ReserveEnergy == 0 &&
            !pair.Stock.Samus.InputLocked, "Reserve recovery failed to refill and release");
    }

    private sealed class DeathAssets(SamusDeathTileAtlas tiles) : IVramAssetProvider, IInstalledArtworkTransferSource
    {
        public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data) =>
            tiles.TryResolve(sourceAddress, byteCount, out data);
        public ReadOnlyMemory<byte> Resolve(VramAssetId asset) =>
            throw new InvalidOperationException("Death fixture received an unrelated typed asset " + asset);
    }
}

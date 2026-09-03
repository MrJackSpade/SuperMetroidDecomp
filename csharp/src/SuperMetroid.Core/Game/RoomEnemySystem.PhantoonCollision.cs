using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Phantoon's extended-spritemap projectile collision and $A7:DD9B shot reaction. The
/// visible body list selects one of three cartridge hitbox sets—no-op, full body, or eye
/// only—so a radius shortcut would make the boss vulnerable during materialization and
/// would miss the authored appendage rectangles during a swoop.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort PhantoonNoOpHitboxCallback = 0x804c;
    private const ushort PhantoonTouchHitboxCallback = 0xdd95;
    private const ushort PhantoonShotHitboxCallback = 0xdd9b;
    private const ushort PhantoonDamageThreshold = 300;

    /// <summary>
    /// Runs bank-$A0's five-slot beam/missile walk for Phantoon, then dispatches the
    /// selected bank-$A7 hitbox callback. One projectile resolves per pass, matching the
    /// native early exit after the first overlapping enemy/component pair.
    /// </summary>
    public int ResolvePhantoonProjectileHits(
        ISnesAddressSpace bus,
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(projectiles);
        ArgumentNullException.ThrowIfNull(sharedProjectiles);
        EnsureLoaded();

        if (_phantoonState is not { } state)
            return 0;

        RoomEnemySlot body = state.Body;
        if (body.EnemyDefinitionPointer != PhantoonBodyDefinition ||
            body.SpritemapPointer is 0 or 0x804d ||
            body.InvincibilityTimer != 0 ||
            body.Properties.HasAny(
                EnemyProperties.Deleted |
                EnemyProperties.IgnoreSamusCollision))
        {
            return 0;
        }

        foreach (SamusProjectileSlot projectile in projectiles.Slots)
        {
            if (!projectile.IsActive)
                continue;

            ushort projectileType = projectile.Type;
            ushort projectileDamage = projectile.Damage;
            ushort family = unchecked((ushort)(projectileType & 0x0f00));
            if (family is 0x0300 or 0x0500 or 0x0700)
                continue;

            if (!body.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap))
            {
                throw new InvalidDataException(
                    "Phantoon lost the extended-spritemap property required by his hitboxes.");
            }

            if (!TryFindExtendedHitboxCallback(
                    body,
                    projectile.XPosition,
                    projectile.YPosition,
                    projectile.XRadius,
                    projectile.YRadius,
                    selectShotCallback: true,
                    out ushort hitboxShotAi))
            {
                continue;
            }

            // `$A0:9B7F` applies this before calling the selected component function. A
            // no-op shell hit therefore still marks the beam and shakes for a Super Missile.
            if (family == (ushort)SamusProjectileFamily.SuperMissile)
            {
                EarthquakeTimer = 30;
                EarthquakeType = 18;
            }

            if (hitboxShotAi == PhantoonNoOpHitboxCallback ||
                body.VariableF >= (ushort)PhantoonAiFunction.DyingFadeInOut)
            {
                projectiles.ApplyExtendedEnemyCollisionPrelude(
                    projectile.SlotIndex,
                    (body.Properties & 0x1000) != 0 ||
                        (projectile.Type & 0x0008) == 0);
                return 1;
            }

            if (hitboxShotAi != PhantoonShotHitboxCallback)
            {
                throw new InvalidDataException(
                    $"Phantoon hitbox shot AI $A7:{hitboxShotAi:X4} is not translated.");
            }

            // `NormalEnemyShotAI_NoDeathCheck_NoEnemyShotGraphic` still creates the Samus
            // projectile's own impact. "No enemy shot graphic" suppresses only common
            // sprite-object $37 on the boss, because Phantoon owns his white palette flash.
            if (!projectiles.TryStartEnemyImpact(bus, sharedProjectiles, projectile.SlotIndex))
                continue;

            ushort healthBefore = body.Health;
            byte vulnerability = ReadProjectileVulnerability(bus, body, projectileType);
            if (vulnerability == 0xff)
            {
                body.FrozenTimer = 400;
                body.AiHandlerBits |= 0x0004;
                body.InvincibilityTimer = 10;
            }
            else
            {
                int damage = (projectileDamage >> 1) * (vulnerability & 0x7f);
                if (damage != 0)
                {
                    ushort hurtTime = body.HurtAiTime == 0 ? (ushort)4 : body.HurtAiTime;
                    body.FlashTimer = unchecked((ushort)(hurtTime + 8));
                    body.AiHandlerBits |= 0x0002;
                    body.Health = damage >= body.Health
                        ? (ushort)0
                        : unchecked((ushort)(body.Health - damage));
                }
            }

            ushort appliedDamage = unchecked((ushort)(healthBefore - body.Health));
            state.LastProjectileDamage = appliedDamage;
            state.AcceptedProjectileHits++;
            ResolvePhantoonShotReaction(
                body,
                state,
                projectileType,
                appliedDamage);
            return 1;
        }

        return 0;
    }

    /// <summary>Ports the boss-owned tail of $A7:DD9B after common vulnerability damage.</summary>
    private void ResolvePhantoonShotReaction(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        ushort projectileType,
        ushort damage)
    {
        if (body.Health == 0)
        {
            state.LastCombatSoundEffect = 0x0073;
            state.Tentacles!.Parameter2 = 1;
            body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
            BeginPhantoonDeathSequence(body, state);
            return;
        }

        if ((body.AiHandlerBits & 0x0002) == 0)
            return;

        state.LastCombatSoundEffect = 0x0073;
        ushort family = unchecked((ushort)(projectileType & 0x0f00));
        PhantoonAiFunction function = (PhantoonAiFunction)body.VariableF;
        bool vulnerableWindow = function is
            PhantoonAiFunction.EyeTracksSamus or
            PhantoonAiFunction.TrackSamusDuringFlameRain;
        bool swooping = function == PhantoonAiFunction.Swooping;

        if (vulnerableWindow || swooping)
        {
            // A single Super Missile (600 retail damage) skips ordinary round accumulation
            // and enters the eight-wave rage sequence immediately.
            if (damage >= PhantoonDamageThreshold &&
                family == (ushort)SamusProjectileFamily.SuperMissile)
            {
                ClosePhantoonAfterDamage(body, state, enraged: true);
                return;
            }

            state.Tentacles!.VariableB = unchecked((ushort)(
                state.Tentacles.VariableB + damage));
            if (state.Tentacles.VariableB >= PhantoonDamageThreshold)
            {
                if (swooping)
                    body.VariableE = 1;
                else
                    ClosePhantoonAfterDamage(body, state, enraged: false);
                state.Tentacles.Parameter2 = 2;
                return;
            }

            if (vulnerableWindow)
            {
                ushort random = unchecked((ushort)(_nextRandom!() & 7));
                state.Eye!.VariableB = _bus!.ReadByte(0xa7cda5 + random);
                state.Mouth!.Parameter2 = random;
                state.Tentacles.Parameter2 = 1;
                if (state.Tentacles.VariableA == 0)
                {
                    state.Tentacles.VariableA = 1;
                    if (body.VariableE >= 16)
                        body.VariableE = 16;
                }
                return;
            }
        }

        // Native writes this otherwise-unused marker even when a valid damaging shot lands
        // outside the three reaction states. Retain it because the four records are exposed
        // to the debugger and later reverse-engineering may identify another consumer.
        state.Tentacles!.Parameter2 = 2;
    }

    private static void ClosePhantoonAfterDamage(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        bool enraged)
    {
        body.VariableF = enraged
            ? (ushort)PhantoonAiFunction.FadeOutBeforeRage
            : (ushort)PhantoonAiFunction.FadeOutWhileSwooping;
        body.VariableE = 0;
        RoomEnemySlot tentacles = state.Tentacles!;
        RoomEnemySlot eye = state.Eye!;
        tentacles.VariableA = 0;
        tentacles.VariableB = 0;
        state.SemiTransparencyLayerFlags |= 0x4000;
        InstallPhantoonInstruction(body, PhantoonInvulnerableBodyInstruction);
        InstallPhantoonInstruction(eye, PhantoonEyeClosedInstruction);
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        eye.VariableF = 0;
        tentacles.Parameter2 = 2;
    }

    private static void BeginPhantoonDeathSequence(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        body.VariableF = body.VariableF is
            (ushort)PhantoonAiFunction.Swooping or
            (ushort)PhantoonAiFunction.FadeOutWhileSwooping
                ? (ushort)PhantoonAiFunction.FinishFatalSwoop
                : (ushort)PhantoonAiFunction.DyingFadeInOut;
        state.Eye!.VariableC = 0;
        state.Eye.VariableF = 0;
        state.SemiTransparencyLayerFlags |= 0x4000;
        state.Mouth!.Parameter2 = 1;
    }

    /// <summary>Ports Phantoon's palette-only hurt AI at $A7:DD3F.</summary>
    private void ApplyPhantoonHurt(RoomEnemySlot body, PhantoonEnemyState state)
    {
        RoomEnemySlot tentacles = state.Tentacles!;
        bool restoreHealthPalette = body.FlashTimer == 8 ||
            (body.FrameCounter & 2) == 0 && (tentacles.Parameter2 & 0xff00) != 0;
        if (restoreHealthPalette)
        {
            CopyPhantoonHealthPalette(body);
            tentacles.Parameter2 &= 0x00ff;
            return;
        }

        if ((body.FrameCounter & 2) == 0 || (tentacles.Parameter2 & 0xff00) != 0)
            return;

        for (int color = 0; color < 16; color++)
            _cgram!.SetColor(112 + color, 0x7fff);
        tentacles.Parameter2 |= 0x0100;
    }

    private void CopyPhantoonHealthPalette(RoomEnemySlot body)
    {
        int healthBand = Math.Min(7, Math.Max(0, (body.Health - 1) / 312));
        int palette = PhantoonHealthPaletteTable + healthBand * 32;
        for (int color = 0; color < 16; color++)
            _cgram!.SetColor(112 + color, ReadWord(_bus!, palette + color * 2));
    }
}

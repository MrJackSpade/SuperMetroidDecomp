using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>One consistent read of a render mailbox's private publication counters.</summary>
/// <param name="Generation">Mailbox generation identifying its current publication lifetime.</param>
/// <param name="Published">Number of frames published into the mailbox.</param>
/// <param name="Taken">Number of frames consumed by the reader.</param>
/// <param name="Replaced">Number of pending frames superseded by a newer publication.</param>
/// <param name="Invalidated">Number of pending frames discarded during mailbox invalidation.</param>
/// <param name="HasPendingFrame">Whether a frame was pending when the counters were read under the mailbox lock.</param>
internal readonly record struct RenderMailboxCounterView(
    long Generation,
    long Published,
    long Taken,
    long Replaced,
    long Invalidated,
    bool HasPendingFrame);

/// <summary>
/// Verification views of private Core state whose debugger result fields were removed. Each
/// member reads the production state the removed field used to report.
/// </summary>
internal static partial class CoreAccess
{
    /// <summary>Exposes the private instruction pointers needed to verify Crateria's lightning palette timer operands.</summary>
    extension(CrateriaLightningPaletteFxProgramDefinition self)
    {
        /// <summary>
        /// The two timer-duration bytes that follow each <c>SetTimer</c> instruction: duration
        /// two after the second-timer instruction and duration one after the first-timer one.
        /// </summary>
        internal IReadOnlyList<(ushort Pointer, byte Value)> TimerMechanicsBytes =>
        [
            (unchecked((ushort)(PrivateState.Property<ushort>(self, "TimerTwoPointer") + 2)), 2),
            (unchecked((ushort)(PrivateState.Property<ushort>(self, "TimerOnePointer") + 2)), 1),
        ];
    }

    /// <summary>Exposes selected private crash-echo projectile coordinates for focused shinespark verification.</summary>
    extension(SamusShinesparkState self)
    {
        /// <summary>Angular-travel word of departing crash-echo projectile slot three.</summary>
        internal SnesAngle FirstReleasedCrashEchoAngle =>
            PrivateState.Property<SnesAngle>(PrivateState.Field<object>(self, "_firstReleasedCrashEcho"), "Angle");

        /// <summary>Angular-travel word of departing crash-echo projectile slot four.</summary>
        internal SnesAngle SecondReleasedCrashEchoAngle =>
            PrivateState.Property<SnesAngle>(PrivateState.Field<object>(self, "_secondReleasedCrashEcho"), "Angle");

        /// <summary>Departing radius (projectile X velocity) of crash-echo projectile slot three.</summary>
        internal ushort FirstReleasedCrashEchoRadius =>
            PrivateState.Property<ushort>(PrivateState.Field<object>(self, "_firstReleasedCrashEcho"), "Radius");
    }

    /// <summary>Exposes active PLM and collectible state in room-slot order for verification.</summary>
    extension(RoomPlmSystem self)
    {
        /// <summary>Block index of every active PLM, in native slot order.</summary>
        internal IReadOnlyList<int> ActivePlmBlockIndices =>
        [
            .. Group4ActivePlmSlots(self).Select(slot => PrivateState.Property<int>(slot, "BlockIndex")),
        ];

        /// <summary>Phase of every permanent-item PLM, in the same order as <see cref="RoomPlmSystem.Collectibles"/>.</summary>
        internal IReadOnlyList<CollectiblePhase> CollectiblePhases =>
        [
            .. Group4CollectibleItems(self).Select(item => PrivateState.Property<CollectiblePhase>(item, "Phase")),
        ];

        /// <summary>Presentation of every permanent-item PLM, in the same order as <see cref="RoomPlmSystem.Collectibles"/>.</summary>
        internal IReadOnlyList<CollectiblePresentation> CollectiblePresentations =>
        [
            .. Group4CollectibleItems(self).Select(item => PrivateState.Property<CollectiblePresentation>(item, "Presentation")),
        ];
    }

    /// <summary>Captures render-mailbox publication counters and pending-frame state as one lock-consistent snapshot.</summary>
    extension(LatestRenderFrameMailbox self)
    {
        /// <summary>Reads every private counter under the mailbox's own lock.</summary>
        internal RenderMailboxCounterView VerificationCounters
        {
            get
            {
                lock (PrivateState.Field<object>(self, "sync"))
                {
                    return new RenderMailboxCounterView(
                        PrivateState.Field<long>(self, "generation"),
                        PrivateState.Field<long>(self, "published"),
                        PrivateState.Field<long>(self, "taken"),
                        PrivateState.Field<long>(self, "replaced"),
                        PrivateState.Field<long>(self, "invalidated"),
                        PrivateState.Field<RenderFrameSnapshot?>(self, "pending") is not null);
                }
            }
        }
    }

    /// <summary>Exposes the cached gameplay subscreen register value used by accepted NMI uploads.</summary>
    extension(SuperMetroidRuntime self)
    {
        /// <summary>Cached gameplay TS byte the last accepted NMI uploaded with the window registers.</summary>
        internal byte CachedGameplaySubscreen =>
            (PrivateState.Field<GameplayWindowRegisterCache?>(self, "gameplayWindowRegisters")
                ?? throw new InvalidOperationException("Runtime has no gameplay window register cache."))
            .ReadByte(GameplayWindowRegisterAddresses.Subscreen);
    }

    /// <summary>Enumerates active room PLM slots in their underlying array order.</summary>
    /// <param name="plms">Room PLM system whose private slot array is inspected.</param>
    /// <returns>Active slot objects used by the Group 4 verification accessors.</returns>
    private static IEnumerable<object> Group4ActivePlmSlots(RoomPlmSystem plms) =>
        PrivateState.Field<object[]>(plms, "_slots").Where(slot => PrivateState.Property<bool>(slot, "Active"));

    /// <summary>Selects the non-null item state attached to each active PLM, preserving room-slot order.</summary>
    /// <param name="plms">Room PLM system whose active slots supply collectible item state.</param>
    /// <returns>Item objects present in active slots.</returns>
    private static IEnumerable<object> Group4CollectibleItems(RoomPlmSystem plms) =>
        Group4ActivePlmSlots(plms)
            .Select(slot => PrivateState.Property<object?>(slot, "Item"))
            .Where(item => item is not null)
            .Select(item => item!);
}

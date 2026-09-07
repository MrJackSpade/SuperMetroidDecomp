/* Original-ROM identities used by the issue #338 post-release input fixture. */
enum GrapplePoseFixture {
  /* $91:8000, NormalSamusPoseInputHandler; long-call entry point. */
  NativePoseInput = 0x918000,
  /* WRAM $0A1C/$0A1F/$0A28: Pose, MovementType, ProspectivePose. */
  Pose = 0xa1c, MovementType = 0xa1f, ProspectivePose = 0xa28,
  /* Direct-page $8B: held controller input (new input remains zero). */
  HeldInput = 0x8b,
  /* Retail configurable bindings at WRAM $09B2..$09BE. */
  Shot = 0x9b2, Jump = 0x9b4, Dash = 0x9b6, Cancel = 0x9b8,
  Select = 0x9ba, AimDown = 0x9bc, AimUp = 0x9be,
  /* Bank-$91 release jump bodies and their diagonal-up aiming counterparts. */
  ReleasedRight = 0x51, ReleasedLeft = 0x52, AimRight = 0x69, AimLeft = 0x6a,
  /* Native mutually-exclusive movement type two: normal jumping. */
  NormalJumping = 2
};

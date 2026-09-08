/* #347: original-ROM no-speed Y entry and its solid-platform scratch state. */
enum ShutterCeilingFixture {
  NativeGroundedY = 0x90923f,
  ExtraYWhole = 0xb5c, InteractiveEnemyBytes = 0x17a6, InteractiveEnemyList = 0x17ec,
  EnemyX = 0xf7a, EnemyY = 0xf7e, EnemyRadiusX = 0xf82, EnemyRadiusY = 0xf84,
  EnemyPropertiesWord = 0xf86
};

/* #347: bank-$A2 moving-up dispatcher and its cartridge-owned carry state. */
enum ShutterCarryFixture {
  NativeShutterMovingUp = 0xa2ef68,
  EnemyYFraction = 0xf80, ShutterUpFraction = 0xfb0, ShutterUpWhole = 0xfb2,
  ShutterMovingSamus = 0x7814, ShutterMinimumY = 0x781e
};

/* Original-ROM identities used by the issue #338 post-release input fixture. */
enum GrapplePoseFixture {
  /* $90:EF22: post-grapple terrain ejection, before the prospective pose applies. */
  NativePostGrappleCollision = 0x90ef22,
  /* $9B:C4F0 accepts the retained Fire edge when a spin has changed pose. */
  NativeGrappleInactive = 0x9bc4f0,
  /* WRAM grapple function and post-draw previous-new-input latch. */
  GrappleFunction = 0xd32, PreviousDrawNewInput = 0xe00,
  /* Bank-$9B inactive and extending function identities. */
  GrappleInactive = 0xc4f0, GrappleFiring = 0xc703,
  /* Native pose dispatcher, per-frame radius refresh, and block-only Y movement. */
  NativeUpdatePose = 0x91eb88, NativeSetRadius = 0x90ec22,
  NativeMoveVertical = 0x949763,
  /* Prospective pose tiers and their command words in WRAM. */
  PreviousPose = 0xa20, PreviousDirection = 0xa22,
  SpecialPose = 0xa2a, SuperSpecialPose = 0xa2c,
  PoseCommand = 0xa2e, SuperSpecialCommand = 0xa32,
  /* Direct-page displacement passed to the vertical movement routine. */
  DisplacementWhole = 0x12, DisplacementFraction = 0x14,
  /* Native room/body and collision-plane WRAM identities. */
  RoomWidth = 0x7a5, SamusX = 0xaf6, SamusY = 0xafa,
  SamusRadiusX = 0xafe, SamusRadiusY = 0xb00,
  LevelWords = 0x10002, BlockBts = 0x16402,
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

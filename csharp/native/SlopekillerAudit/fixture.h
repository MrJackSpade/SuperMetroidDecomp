/* Original unpatched Japan/USA routine identities used by the movement sequence. */
enum SlopekillerNativeEntry {
  /* $90:EC22: refresh pose-specific collision radii. */
  RefreshRadius = 0x90ec22,
  /* Native alpha input and enemy/projectile interaction before beta movement. */
  InputPhase = 0x90e695, InteractionPhase = 0xa09785,
  /* Bank-$90 beta animation and transition dispatch, then bank-$91 gamma checks. */
  AnimationPhase = 0x908000, TransitionPhase = 0x90dde9,
  CollisionPosePhase = 0x91e8b6, ApplyPosePhase = 0x91eb88,
  /* Post-pose bookkeeping and final native collision/hurt maintenance. */
  PoseHistoryPhase = 0x90eab3, HurtPhase = 0x90e9ce, CollisionPhase = 0xa09169
};

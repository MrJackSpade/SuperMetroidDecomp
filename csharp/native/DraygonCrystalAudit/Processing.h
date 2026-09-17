#pragma once

/* #422: original-CPU sound backlog produced by the actions named by the
   Processing technique. This probe runs the cartridge routines themselves;
   queue draining and door handoff are covered by the separate bank-$82 audit. */
enum NativeProcessingEntry {
  ProcessingHudTilemap = 0x809b44,
  ProcessingSpinTransition = 0x91f624,
  ProcessingPostDrawAudio = 0x90f576,
  ProcessingBeamHud = 0x90b80d,
  ProcessingBombExpiry = 0x90c128,
  ProcessingShinesparkSetup = 0x90cffa,
  ProcessingShinesparkWindup = 0x90d068,
};

enum NativeProcessingCase {
  ProcessingCaseHudSelect,
  ProcessingCaseSpinStart,
  ProcessingCaseSpaceJumpCheck,
  ProcessingCaseScrewAttackControl,
  ProcessingCaseBreakSpin,
  ProcessingCaseBreakSpinCharging,
  ProcessingCaseFirePowerBeam,
  ProcessingCaseFireWaveBeam,
  ProcessingCaseReleaseCharge,
  ProcessingCaseBombExplosion,
  ProcessingCaseShinesparkActivation,
  ProcessingCaseCount,
};

static const char *const kProcessingCaseNames[ProcessingCaseCount] = {
  "hud-select", "spin-start", "space-jump-check", "screw-attack-control",
  "break-spin", "break-spin-charging", "fire-power-beam", "fire-wave-beam",
  "release-charge", "bomb-explosion", "shinespark-activation",
};

static void seed_processing_action(void) {
  memset(g_ram, 0, sizeof(g_ram));
  samus_pose = samus_prev_pose = 1;
  samus_pose_x_dir = samus_prev_pose_x_dir = 8;
  samus_movement_type = samus_prev_movement_type = 0;
  samus_x_pos = samus_prev_x_pos = 256;
  samus_y_pos = samus_prev_y_pos = 400;
  samus_health = samus_max_health = 99;
  button_config_shoot_x = 0x40;
  game_state = 8;
  grapple_beam_function = 0xc4f0;
  fx_y_pos = lava_acid_y_pos = 0xffff;
  room_width_in_blocks = 32;
  room_height_in_blocks = 32;
  room_width_in_scrolls = 2;
  room_height_in_scrolls = 2;
  room_size_in_blocks = 32 * 32 * 2;
  for (int x = 0; x < 32; x++) level_data[26 * 32 + x] = 0x8000;
  run(RefreshRadius);
}

static void run_processing_action(int action) {
  switch (action) {
  case ProcessingCaseHudSelect:
    hud_item_index = 1;
    samus_prev_hud_item_index = 0;
    run(ProcessingHudTilemap);
    return;
  case ProcessingCaseSpinStart:
    equipped_items = 0x20;
    samus_movement_type = 3;
    run(ProcessingSpinTransition);
    return;
  case ProcessingCaseSpaceJumpCheck:
    equipped_items = 0x20 | 0x200;
    samus_prev_pose_x_dir = 8;
    samus_prev_movement_type2 = 3;
    samus_pose_x_dir = 4;
    samus_movement_type = 3;
    run(ProcessingSpinTransition);
    return;
  case ProcessingCaseScrewAttackControl:
    equipped_items = 0x20 | 0x200 | 8;
    samus_prev_pose_x_dir = 8;
    samus_prev_movement_type2 = 3;
    samus_pose_x_dir = 4;
    samus_movement_type = 3;
    run(ProcessingSpinTransition);
    return;
  case ProcessingCaseBreakSpin:
    samus_prev_movement_type = 3;
    samus_movement_type = 0;
    run(ProcessingPostDrawAudio);
    return;
  case ProcessingCaseBreakSpinCharging:
    samus_prev_movement_type = 3;
    samus_movement_type = 0;
    flare_counter = 16;
    joypad1_lastkeys = button_config_shoot_x;
    run(ProcessingPostDrawAudio);
    samus_prev_movement_type = 0;
    run(ProcessingPostDrawAudio);
    return;
  case ProcessingCaseFirePowerBeam:
    joypad1_lastkeys = joypad1_newkeys = button_config_shoot_x;
    run(ProcessingBeamHud);
    return;
  case ProcessingCaseFireWaveBeam:
    equipped_beams = 1;
    joypad1_lastkeys = joypad1_newkeys = button_config_shoot_x;
    run(ProcessingBeamHud);
    return;
  case ProcessingCaseReleaseCharge:
    equipped_beams = 0x1000;
    flare_counter = 60;
    run(ProcessingBeamHud);
    return;
  case ProcessingCaseBombExplosion:
    projectile_index = 10;
    projectile_type[5] = 0x500;
    projectile_variables[5] = 1;
    run(ProcessingBombExpiry);
    return;
  case ProcessingCaseShinesparkActivation:
    flare_counter = 16;
    run(ProcessingShinesparkSetup);
    timer_for_shinesparks_startstop = 1;
    run(ProcessingShinesparkWindup);
    return;
  default:
    Die("Unknown Processing action");
  }
}

static void print_processing_queue(int action, int power_bomb) {
  static uint8 *const queues[] = { sfx1_queue, sfx2_queue, sfx3_queue };
  printf("%s,%d", kProcessingCaseNames[action], power_bomb);
  for (int library = 0; library < 3; library++) {
    int count = ((int)sfx_writepos[library] - sfx_readpos[library]) & 15;
    printf(",%d,", count);
    for (int i = 0; i < count; i++)
      printf("%02X", queues[library][(sfx_readpos[library] + i) & 15]);
  }
  printf("\n");
}

static void processing_matrix(void) {
  printf("action,power_bomb,lib1_count,lib1,lib2_count,lib2,lib3_count,lib3\n");
  for (int action = 0; action < ProcessingCaseCount; action++)
  for (int power_bomb = 0; power_bomb < 2; power_bomb++) {
    seed_processing_action();
    power_bomb_explosion_status = power_bomb ? 0x8000 : 0;
    run_processing_action(action);
    print_processing_queue(action, power_bomb);
  }
}

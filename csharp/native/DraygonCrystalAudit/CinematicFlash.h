/* #432: original-CPU ownership matrix for cinematic Crystal Flash interruptions.
   These probes deliberately enter Crystal Flash through $90:D5A2, then invoke the
   cartridge routines that seize (or merely alter) Samus during the three documented
   cinematics.  The observed movement pointer is the property under test: unlike a
   high-level pose flag, it determines whether Flash keeps executing after the actor AI. */

enum {
  MotherBrainUnableToStand = 0x90f3c0,
  SuperMetroidBeginDrain = 0xa9f20e,
  CeresRidleyEjectionRequest = 0x90e119,
  CeresRidleyEjectionFirstFrame = 0x90e12e,
};

static void advance_cinematic_flash(int movement_calls) {
  for (int frame = 0; frame < movement_calls; frame++) {
    nmi_frame_counter_word = frame;
    if (samus_movement_handler == FlashRaising ||
        samus_movement_handler == FlashMain ||
        samus_movement_handler == FlashFinish) {
      run(0x900000 | samus_movement_handler);
    }
    run(AnimationPhase);
    unsigned stages[] = {
      TransitionPhase, CollisionPosePhase, ApplyPosePhase,
      PoseHistoryPhase, HurtPhase, CollisionPhase
    };
    for (int i = 0; i < 6; i++) run(stages[i]);
  }
}

static void seed_cinematic_flash(bool admit_flash, int movement_calls) {
  memset(g_ram, 0, sizeof(g_ram));
  memset(dma_channel_registers, 0, sizeof(dma_channel_registers));
  samus_pose = samus_prev_pose = 1;
  samus_pose_x_dir = samus_prev_pose_x_dir = 8;
  samus_x_pos = samus_prev_x_pos = 256;
  samus_y_pos = samus_prev_y_pos = 400;
  samus_health = 49; samus_max_health = 99;
  samus_missiles = samus_super_missiles = samus_power_bombs = 10;
  samus_max_missiles = samus_max_super_missiles = samus_max_power_bombs = 10;
  button_config_shoot_x = 0x40; game_state = 8;
  samus_input_handler = 0xe913; samus_movement_handler = 0xa337;
  frame_handler_alfa = 0xe695; frame_handler_beta = 0xe725;
  grapple_beam_function = 0xc4f0;
  room_width_in_blocks = 144; room_height_in_blocks = 80;
  room_width_in_scrolls = 9; room_height_in_scrolls = 5;
  room_size_in_blocks = 144 * 80 * 2;
  interactive_enemy_indexes[0] = 0xffff;
  for (int x = 0; x < 144; x++)
    level_data[32 * 144 + x] = level_data[16 * 144 + x] = 0x8000;
  fx_y_pos = lava_acid_y_pos = 0xffff;
  samus_prev_x_pos = samus_x_pos; samus_prev_y_pos = samus_y_pos;
  samus_x_speed_table_pointer = 0x9f55;
  button_config_run_b = 0x8000; button_config_jump_a = 0x80;
  button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
  button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
  run(RefreshRadius);

  joypad1_lastkeys = admit_flash ? 0x470 : 0x460;
  run(FlashEntry);
  advance_cinematic_flash(movement_calls);
}

static void print_cinematic_flash_row(
    const char *encounter, int admit_flash, int movement_calls,
    uint16 handler_before) {
  printf("%s,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",
    encounter, admit_flash, movement_calls, handler_before, samus_movement_handler,
    samus_input_handler, frame_handler_alfa, frame_handler_beta, frame_handler_gamma,
    samus_pose, samus_health, samus_missiles, samus_shine_timer,
    timer_for_shine_timer);
}

static void cinematic_flash_matrix(void) {
  printf("encounter,flash,movement_calls,handler_before,handler_after,input_handler,alpha,beta,gamma,pose,health,missiles,shine,palette\n");
  /* 240 is the final active-main sample. By 250, animation/transition dispatch has
     restored the ordinary movement and input pointers, making it the adjacent late
     timing which cannot retain a Flash suit. */
  const int timing[] = { 0, 5, 12, 240, 250 };
  for (int admit_flash = 0; admit_flash <= 1; admit_flash++) {
    for (int i = 0; i < 5; i++) {
      int calls = timing[i];

      seed_cinematic_flash(admit_flash, calls);
      uint16 before = samus_movement_handler;
      run(MotherBrainUnableToStand);
      print_cinematic_flash_row("mother-brain", admit_flash, calls, before);

      seed_cinematic_flash(admit_flash, calls);
      before = samus_movement_handler;
      cur_enemy_index = 0;
      Get_Shitroid(0)->base.frame_counter = 0;
      run(SuperMetroidBeginDrain);
      print_cinematic_flash_row("super-metroid", admit_flash, calls, before);

      seed_cinematic_flash(admit_flash, calls);
      before = samus_movement_handler;
      run(CeresRidleyEjectionRequest);
      print_cinematic_flash_row("ceres-request", admit_flash, calls, before);
      run(CeresRidleyEjectionFirstFrame);
      print_cinematic_flash_row("ceres-first-frame", admit_flash, calls, before);
    }
  }

  /* Adjacent failing Super Metroid timing: the terminal drain installs crouching
     before the delayed Flash begins. Flash then replaces that state and completes
     through its ordinary standing/input restoration instead of retaining a suit. */
  seed_cinematic_flash(false, 0);
  samus_health = 1;
  run(SuperMetroidBeginDrain);
  uint16 before = samus_movement_handler;
  joypad1_lastkeys = 0x470;
  run(FlashEntry);
  if (samus_movement_handler != FlashRaising)
    Die("Post-crouch Super Metroid timing must admit Crystal Flash");
  advance_cinematic_flash(400);
  print_cinematic_flash_row("super-metroid-before-flash", 1, 400, before);
}

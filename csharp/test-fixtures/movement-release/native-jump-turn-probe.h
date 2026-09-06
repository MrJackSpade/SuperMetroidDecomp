// Original-CPU jump/turn trajectory. Included after native-release-probe.h.
int DiagnosticJumpTurn(const char *rom) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  for (int ledge = 0; ledge < 2; ledge++)
  for (int water = 0; water < 2; water++)
  for (int left = 0; left < 2; left++)
  for (int hold = 0; hold < 2; hold++)
  for (int delay = 0; delay <= 10; delay++) {
    cpu_reset(g_snes->cpu);
    memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false;
    g_snes->cpu->sp = 0x1ff0;
    g_snes->cpu->dp = 0;
    room_width_in_blocks = 16;
    room_height_in_blocks = 32;
    interactive_enemy_indexes[0] = 0xffff;
    for (int i = 256; i < 272; i++) level_data[i] = 0x8000;
    if (ledge)
      for (int y = 12; y < 16; y++)
        for (int x = 0; x < 16; x++)
          if (left ? x >= 8 : x < 8) level_data[y * 16 + x] = 0x8000;
    fx_y_pos = water ? 8 : 0xffff;
    lava_acid_y_pos = 0xffff;
    fx_liquid_options = 0x80;
    fx_type = water ? 6 : 0;
    liquid_physics_type = water;
    equipped_items = 0x3105;
    samus_x_pos = ledge ? (left ? 123 : 133) : 128;
    samus_y_pos = 235;
    samus_x_radius = 5;
    samus_y_radius = 21;
    samus_pose = samus_prev_pose = left ? 1 : 2;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 8 : 4;
    samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913;
    samus_health = 99;
    button_config_run_b = 0x8000;
    button_config_jump_a = 0x80;
    samus_anim_frame_timer = 5;
    uint16 previous = 0;
    for (int frame = -24; frame < 180; frame++) {
      samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
      samus_momentum_routine_index = samus_special_transgfx_index = samus_hurt_switch_index = 0;
      int turn_frame = delay + (ledge ? 40 : 0);
      joypad1_lastkeys = frame < 0 || frame >= 60 ? 0 : 0x80 |
        (frame >= turn_frame && (hold || frame == turn_frame) ? (left ? 0x200 : 0x100) : 0);
      joypad1_newkeys = joypad1_lastkeys & ~previous;
      previous = joypad1_lastkeys;
      // Alpha refreshes collision radii every frame, including animation-owned poses.
      RunAsmCode(0x90ec22, 0, 0, 0, 0);
      RunAsmCode(0x918000, 0, 0, 0, 0);
      RunAsmCode(0x909c5b, 0, 0, 0, 0);
      RunAsmCode(0x90a337, 0, 0, 0, 0);
      RunAsmCode(0x908000, 0, 0, 0, 0);
      RunAsmCode(0x91e8b6, 0, 0, 0, 0);
      RunAsmCode(0x91eb88, 0, 0, 0, 0);
      if (frame >= 0)
        printf("JUMP ledge=%d water=%d left=%d hold=%d delay=%d frame=%d x=%04X%04X y=%04X%04X pose=%02X base=%04X%04X mode=%d anim=%d timer=%d\n",
          ledge, water, left, hold, delay, frame, samus_x_pos, samus_x_subpos, samus_y_pos, samus_y_subpos,
          samus_pose, samus_x_base_speed, samus_x_base_subspeed, samus_x_accel_mode,
          samus_anim_frame, samus_anim_frame_timer);
    }
  }
  return 0;
}

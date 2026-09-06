// Synthetic sand geometry driven by original 65816 instructions, not C callbacks.
int DiagnosticQuicksand(const char *rom) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  for (int deep = 0; deep < 2; deep++)
  for (int gravity = 0; gravity < 2; gravity++)
  for (int hold = 0; hold < 2; hold++)
  for (int delay = 8; delay <= 80; delay += 72) {
    cpu_reset(g_snes->cpu);
    memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false;
    g_snes->cpu->sp = 0x1ff0;
    g_snes->cpu->dp = 0;
    area_index = 4;
    room_width_in_blocks = 64;
    room_height_in_blocks = 16;
    interactive_enemy_indexes[0] = 0xffff;
    for (int y = 12; y < 16; y++)
      for (int x = 0; x < 64; x++) {
        int i = y * 64 + x;
        level_data[i] = y < 14 ? 0x3000 : 0x8000;
        BTS[i] = y < 14 ? (deep && y == 13 ? 0x83 : 0x82) : 0;
      }
    fx_y_pos = 8; lava_acid_y_pos = 0xffff;
    fx_type = 6; liquid_physics_type = 1;
    equipped_items = gravity ? 0x20 : 0;
    samus_x_pos = 128; samus_y_pos = 172;
    samus_x_radius = 5; samus_y_radius = 21;
    samus_pose = samus_prev_pose = 1;
    samus_pose_x_dir = samus_prev_pose_x_dir = 8;
    samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913;
    plm_flag = 0x8000;
    samus_health = 99;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80;
    samus_anim_frame_timer = 5;
    uint16 previous = 0;
    for (int frame = 0; frame < 180; frame++) {
      samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
      samus_momentum_routine_index = samus_special_transgfx_index = samus_hurt_switch_index = 0;
      joypad1_lastkeys = frame >= delay && frame < delay + (hold ? 60 : 8) ? 0x80 : 0;
      joypad1_newkeys = joypad1_lastkeys & ~previous;
      previous = joypad1_lastkeys;
      RunAsmCode(0x90ec22, 0, 0, 0, 0);
      RunAsmCode(0x918000, 0, 0, 0, 0);
      RunAsmCode(0x909c5b, 0, 0, 0, 0);
      RunAsmCode(0x949b60, 0, 0, 0, 0);
      RunAsmCode(0x90a337, 0, 0, 0, 0);
      RunAsmCode(0x908000, 0, 0, 0, 0);
      RunAsmCode(0x91e8b6, 0, 0, 0, 0);
      RunAsmCode(0x91eb88, 0, 0, 0, 0);
      // Sand reactions allocate one-shot PLMs. Run their native deletion lists so
      // the synthetic harness does not exhaust slots and disable later reactions.
      RunAsmCode(0x8485b4, 0, 0, 0, 0);
      printf("SAND deep=%d gravity=%d hold=%d delay=%d frame=%d y=%04X%04X pose=%02X speed=%04X%04X dir=%d extra=%04X%04X\n",
        deep, gravity, hold, delay, frame, samus_y_pos, samus_y_subpos, samus_pose,
        samus_y_speed, samus_y_subspeed, samus_y_dir, extra_samus_y_displacement, extra_samus_y_subdisplacement);
    }
  }
  return 0;
}

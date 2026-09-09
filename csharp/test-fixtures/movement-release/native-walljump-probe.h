// #473: ordinary solid-wall input window. Include after native-release-probe.h.
int DiagnosticWalljump(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  FILE *f = fopen(output, "wx");
  if (!f) return 4;
  fprintf(f, "history,left,delay,frame,input,x,y,pose,animation\n");
  for (int history = 0; history < 2; history++)
  for (int left = 0; left < 2; left++)
  for (int delay = 0; delay <= 12; delay++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
    room_width_in_blocks = 16; room_height_in_blocks = 32;
    interactive_enemy_indexes[0] = 0xffff;
    for (int y = 0; y <= 16; y++) {
      level_data[y * 16 + (left ? 8 : 7)] = 0x8000;
      if (y == 16) for (int x = 0; x < 16; x++) level_data[y * 16 + x] = 0x8000;
    }
    fx_y_pos = lava_acid_y_pos = 0xffff;
    samus_x_pos = left ? 122 : 134; samus_y_pos = 160;
    samus_pose = samus_prev_pose = left ? 0x19 : 0x1a;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 8 : 4;
    samus_movement_type = samus_prev_movement_type = samus_prev_movement_type2 = 3;
    // Isolate $90:9D35's history gate without changing the current spin pose.
    samus_last_different_pose_movement_type = history ? 3 : 0;
    samus_y_dir = 2; samus_anim_frame_timer = 1;
    samus_x_speed_table_pointer = 0x9f55; samus_input_handler = 0xe913;
    samus_health = 99; button_config_run_b = 0x8000; button_config_jump_a = 0x80;
    uint16 previous = 0;
    for (int frame = 0; frame < 30; frame++) {
      uint16 input = (left ? 0x200 : 0x100) | (frame >= delay ? 0x80 : 0);
      samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
      samus_momentum_routine_index = samus_special_transgfx_index = samus_hurt_switch_index = 0;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      RunAsmCode(0x90ec22, 0, 0, 0, 0); RunAsmCode(0x90e90f, 0, 0, 0, 0);
      RunAsmCode(0x909c5b, 0, 0, 0, 0); RunAsmCode(0x90a337, 0, 0, 0, 0);
      RunAsmCode(0x908000, 0, 0, 0, 0); RunAsmCode(0x91e8b6, 0, 0, 0, 0);
      RunAsmCode(0x91eb88, 0, 0, 0, 0); RunAsmCode(0x90eab3, 0, 0, 0, 0);
      fprintf(f, "%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%04X\n", history, left, delay, frame, input,
        samus_x_pos, samus_x_subpos, samus_y_pos, samus_y_subpos, samus_pose, samus_anim_frame);
    }
  }
  fclose(f); return 0;
}

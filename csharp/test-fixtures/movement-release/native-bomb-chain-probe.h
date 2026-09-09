// #412: real controller-selected bombs, short chains and low-ceiling trajectories.
// Include after native-release-probe.h and dispatch before SDL initialization.
int DiagnosticBombChainVariant(const char *rom, const char *output, int repeated) {
  bool triple = repeated == 2;
  bool horizontal = repeated == 3;
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  FILE *f = fopen(output, "wx");
  if (!f) return 4;
  fprintf(f, "left,ceiling,travel,spacing,frame,input,x,y,pose,bombDirection,ySpeed,yDirection,baseSpeed,bombCount");
  for (int b = 0; b < 5; b++) fprintf(f, ",b%dType,b%dFuse,b%dX,b%dY,b%dList,b%dTimer,b%dSprite", b,b,b,b,b,b,b);
  fprintf(f, "\n");
  for (int left = 0; left < 2; left++)
  for (int ceiling = 0; ceiling < 2; ceiling++)
  for (int travel = 0; travel < (horizontal ? 12 : triple ? 17 : 3); travel++)
  for (int schedule = 0; schedule < (horizontal ? 13 : repeated == 1 ? 9 : 6); schedule++) {
    int spacing = horizontal ? 70 + 2 * schedule : triple ? 50 + schedule : repeated ? 48 + schedule : schedule ? 36 + 4 * schedule : 0;
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 16; room_height_in_blocks = 32;
    room_width_in_scrolls = 1; room_height_in_scrolls = 2;
    interactive_enemy_indexes[0] = 0xffff;
    for (int x = 0; x < 16; x++) level_data[(ceiling ? 12 : 0) * 16 + x] = level_data[16 * 16 + x] = 0x8000;
    for (int y = 0; y <= 16; y++) level_data[y * 16] = level_data[y * 16 + 15] = 0x8000;
    fx_y_pos = lava_acid_y_pos = 0xffff; equipped_items = 0x1004; samus_health = 99;
    samus_x_pos = samus_prev_x_pos = 128; samus_y_pos = samus_prev_y_pos = 249;
    samus_pose = samus_prev_pose = left ? 0x41 : 0x1d;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_movement_type = samus_prev_movement_type = samus_prev_movement_type2 = 4;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    uint16 previous = 0;
    for (int frame = 0; frame < (repeated == 1 ? 600 : 180); frame++) {
      uint16 input = (horizontal ? !frame || frame == 52 || frame == spacing : triple ? !frame || frame == spacing || frame == 68 + travel : repeated ? frame % spacing == 0 : (!frame || (spacing && (frame == spacing || frame == 2 * spacing)))) ? 0x40 : 0;
      if (horizontal && frame >= 74 && frame < 75 + travel) input |= left ? 0x200 : 0x100;
      if (!horizontal && !triple && frame >= 46 && frame < 50 && travel) input |= travel == 1 ? 0x200 : 0x100;
      samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
      samus_momentum_routine_index = samus_special_transgfx_index = samus_hurt_switch_index = 0;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      RunAsmCode(0x90ec22, 0, 0, 0, 0); RunAsmCode(0x90e90f, 0, 0, 0, 0);
      RunAsmCode(0x909c5b, 0, 0, 0, 0);
      // Normal alpha cooldown/ball HUD production, then all live projectile slots.
      RunAsmCode(0x90ac1c, 0, 0, 0, 0);
      RunAsmCode(0x90bf9d, 0, 0, 0, 0);
      RunAsmCode(0x90aece, 0, 0, 0, 0); RunAsmCode(0xa09785, 0, 0, 0, 0);
      samus_contact_damage_index = 0;
      RunAsmCode(0x900000 | samus_movement_handler, 0, 0, 0, 0);
      RunAsmCode(0x908000, 0, 0, 0, 0); RunAsmCode(0x90dde9, 0, 0, 0, 0);
      RunAsmCode(0x91e8b6, 0, 0, 0, 0); RunAsmCode(0x91eb88, 0, 0, 0, 0);
      RunAsmCode(0x90eab3, 0, 0, 0, 0); RunAsmCode(0x90e9ce, 0, 0, 0, 0);
      RunAsmCode(0xa09169, 0, 0, 0, 0);
      fprintf(f, "%d,%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%04X,%04X%04X,%04X,%04X%04X,%04X",
        left, ceiling, travel, spacing, frame, input, samus_x_pos, samus_x_subpos,
        samus_y_pos, samus_y_subpos, samus_pose, bomb_jump_dir, samus_y_speed, samus_y_subspeed,
        samus_y_dir, samus_x_base_speed, samus_x_base_subspeed, bomb_counter);
      for (int b = 5; b < 10; b++) fprintf(f, ",%04X,%04X,%04X,%04X,%04X,%04X,%04X",
        projectile_type[b], projectile_variables[b], projectile_x_pos[b], projectile_y_pos[b],
        projectile_bomb_instruction_ptr[b], projectile_bomb_instruction_timers[b], projectile_spritemap_pointers[b]);
      fprintf(f, "\n");
    }
  }
  fclose(f); return 0;
}

int DiagnosticBombChains(const char *rom, const char *output) {
  return DiagnosticBombChainVariant(rom, output, 0);
}

int DiagnosticRepeatedBombChains(const char *rom, const char *output) {
  return DiagnosticBombChainVariant(rom, output, 1);
}

int DiagnosticTripleBombChains(const char *rom, const char *output) {
  return DiagnosticBombChainVariant(rom, output, 2);
}

int DiagnosticHorizontalBombChains(const char *rom, const char *output) {
  return DiagnosticBombChainVariant(rom, output, 3);
}

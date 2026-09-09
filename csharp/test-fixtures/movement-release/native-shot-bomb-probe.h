// #458: real bomb placement, unmorph, timed shot, and optional tunnel entry.
// Include after native-release-probe.h; dispatch before SDL startup.
int DiagnosticShotBomb(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  FILE *f = fopen(output, "wx");
  if (!f) return 4;
  fprintf(f, "left,tunnel,shot,shotFrame,frame,input,x,y,pose,anim,animTimer,jump,ySpeed,yDirection,invinc,bombs,shots,fuse,bombType\n");
  for (int left = 0; left < 2; left++)
  for (int tunnel = 0; tunnel < 2; tunnel++)
  for (int shot = 0; shot < 2; shot++)
  for (int shotFrame = 35; shotFrame <= 60; shotFrame++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 16; room_height_in_blocks = 32;
    room_size_in_blocks = 16 * 32 * 2;
    room_width_in_scrolls = 1; room_height_in_scrolls = 2;
    interactive_enemy_indexes[0] = 0xffff;
    for (int x = 0; x < 16; x++) level_data[x] = level_data[16 * 16 + x] = 0x8000;
    for (int y = 0; y <= 16; y++) level_data[y * 16] = level_data[y * 16 + 15] = 0x8000;
    if (tunnel) for (int x = 9; x < 15; x++) level_data[14 * 16 + (left ? 15-x : x)] = 0x8000;
    fx_y_pos = lava_acid_y_pos = 0xffff; equipped_items = 0x1004;
    samus_health = samus_max_health = 99;
    samus_x_pos = samus_prev_x_pos = 128; samus_y_pos = samus_prev_y_pos = 249;
    samus_pose = samus_prev_pose = left ? 0x41 : 0x1d;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_movement_type = samus_prev_movement_type = samus_prev_movement_type2 = 4;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337;
    grapple_beam_function = 0xc4f0;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    uint16 previous = 0;
    for (int frame = 0; frame < 100; frame++) {
      uint16 input = !frame || shot && frame == shotFrame ? 0x40 : 0;
      if (frame >= 10 && frame <= 20 || frame == 24) input |= 0x800;
      if (tunnel && (frame == shotFrame - 9 || frame == shotFrame + 1)) input |= 0x400;
      if (tunnel && frame >= shotFrame + 8 && frame < shotFrame + 21) input |= left ? 0x200 : 0x100;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      RunAsmCode(0x90e695, 0, 0, 0, 0);
      RunAsmCode(0xa09785, 0, 0, 0, 0);
      samus_contact_damage_index = 0;
      RunAsmCode(0x900000 | samus_movement_handler, 0, 0, 0, 0);
      RunAsmCode(0x908000, 0, 0, 0, 0); RunAsmCode(0x90dde9, 0, 0, 0, 0);
      RunAsmCode(0x91e8b6, 0, 0, 0, 0); RunAsmCode(0x91eb88, 0, 0, 0, 0);
      RunAsmCode(0x90eab3, 0, 0, 0, 0); RunAsmCode(0x90e9ce, 0, 0, 0, 0);
      RunAsmCode(0xa09169, 0, 0, 0, 0);
      fprintf(f, "%d,%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%04X,%04X,%04X,%04X%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",
        left,tunnel,shot,shotFrame,frame,input,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
        samus_pose,samus_anim_frame,samus_anim_frame_timer,bomb_jump_dir,samus_y_speed,samus_y_subspeed,samus_y_dir,
        projectile_invincibility_timer,bomb_counter,projectile_counter,projectile_variables[5],projectile_type[5]);
    }
  }
  fclose(f); return 0;
}

// #415: earned-charge, down-aim turnaround sweep. Original CPU routines only.
// Include after native-release-probe.h. No SDL or player save is involved.
int DiagnosticAerialSpread(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "left,delay,frame,input,pose,x,xsub,y,ysub,charge,spread,bombs\n");
  for (int left = 0; left < 2; left++)
  for (int delay = 0; delay < 10; delay++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = room_height_in_blocks = 64;
    room_width_in_scrolls = room_height_in_scrolls = 4;
    for (int y = 32; y < 36; y++) for (int x = 16; x < 48; x++) level_data[y * 64 + x] = 0x8000;
    interactive_enemy_indexes[0] = 0xffff;
    fx_y_pos = lava_acid_y_pos = 0xffff;
    samus_pose = samus_prev_pose = left ? 2 : 1;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_x_pos = 512; samus_y_pos = 490;
    equipped_items = 0x1004; equipped_beams = 0x1000;
    samus_health = samus_max_health = 99;
    button_config_shoot_x = 0x40; button_config_jump_a = 0x80;
    button_config_run_b = 0x8000; button_config_down = 0x400;
    samus_input_handler = 0xe913; grapple_beam_function = 0xc4f0;
    samus_anim_frame_timer = 5; game_state = 8;
    uint16 previous = 0;
    for (int frame = 0; frame < 260; frame++) {
      uint16 input = 0x40;
      if (frame >= 70 && frame < 125) input |= 0x80;
      if (frame >= 76 && frame < 105 && frame != 78 + delay) input |= 0x400;
      if (frame >= 78 && frame < 125) input |= left ? 0x100 : 0x200;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      RunAsmCode(0x90e695, 0, 0, 0, 0);
      RunAsmCode(0xa09785, 0, 0, 0, 0);
      RunAsmCode(0x90a337, 0, 0, 0, 0);
      RunAsmCode(0x908000, 0, 0, 0, 0);
      RunAsmCode(0x91e8b6, 0, 0, 0, 0);
      RunAsmCode(0x91eb88, 0, 0, 0, 0);
      fprintf(f, "%d,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",
        left,delay,frame,input,samus_pose,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
        flare_counter,bomb_spread_charge_timeout_counter,bomb_counter);
      if (delay == 6 && frame >= 105) for (int slot = 0; slot < 5; slot++) {
        int i = slot + 5;
        if (!projectile_bomb_instruction_ptr[i]) fprintf(f, "bomb,%d,0\n", slot);
        else fprintf(f, "bomb,%d,1,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",
          slot,projectile_type[i],projectile_x_pos[i],projectile_bomb_x_subpos[i],projectile_y_pos[i],projectile_bomb_y_subpos[i],
          projectile_bomb_x_speed[i],projectile_bomb_y_speed[i],projectile_timers[i],projectile_variables[i],
          projectile_x_radius[i],projectile_y_radius[i],projectile_bomb_instruction_ptr[i],projectile_bomb_instruction_timers[i],projectile_spritemap_pointers[i]);
      }
    }
  }
  fclose(f); return 0;
}

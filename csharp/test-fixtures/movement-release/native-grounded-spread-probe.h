// #414: retail CPU bomb producer/projectile/overlap calls. No SDL, SRAM or cheats.
// Include after native-release-probe.h. Output must stay private.
int DiagnosticGroundedSpread(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "floor,hold,keepDown,frame,input,charge,spread,count,slot,type,x,xsub,y,ysub,vx,vy,vysub,fuse,radiusX,radiusY,list,listTimer,sprite,bombJump\n");
  const int holds[] = {0,1,63,64,127,128,191,192};
  for (int floor = 0; floor < 2; floor++)
  for (int h = 0; h < 8; h++)
  for (int keep = 0; keep <= (holds[h] == 192); keep++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 64; room_height_in_blocks = 128;
    room_width_in_scrolls = 4; room_height_in_scrolls = 8; room_size_in_blocks = 16384;
    interactive_enemy_indexes[0] = 0xffff;
    if (floor) for (int x = 0; x < 64; x++) level_data[40 * 64 + x] = 0x8000;
    samus_x_pos = samus_y_pos = 512; samus_x_radius = samus_y_radius = 7;
    samus_pose = 0x1d; samus_pose_x_dir = 8; equipped_items = 0x1004;
    samus_y_accel = 0; samus_y_subaccel = 0x1c00;
    button_config_shoot_x = 0x40; flare_counter = 60; game_state = 8;
    samus_health = samus_max_health = 99;
    for (int frame = 0; frame < holds[h] + 150; frame++) {
      uint16 input = 0x40 | ((frame < holds[h] || keep) ? 0x400 : 0);
      joypad1_lastkeys = input; joypad1_newkeys = 0;
      RunAsmCode(0x90ac1c, 0, 0, 0, 0);
      RunAsmCode(0x90bf9d, 0, 0, 0, 0);
      RunAsmCode(0x90aece, 0, 0, 0, 0);
      RunAsmCode(0xa09785, 0, 0, 0, 0);
      for (int slot = 0; slot < 5; slot++) {
        int i = slot + 5;
        fprintf(f, "%d,%d,%d,%d,%04X,%04X,%04X,%04X,%d,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",
          floor,holds[h],keep,frame,input,flare_counter,bomb_spread_charge_timeout_counter,bomb_counter,slot,
          projectile_type[i],projectile_x_pos[i],projectile_bomb_x_subpos[i],projectile_y_pos[i],projectile_bomb_y_subpos[i],
          projectile_bomb_x_speed[i],projectile_bomb_y_speed[i],projectile_timers[i],projectile_variables[i],
          projectile_x_radius[i],projectile_y_radius[i],projectile_bomb_instruction_ptr[i],projectile_bomb_instruction_timers[i],
          projectile_spritemap_pointers[i],bomb_jump_dir);
      }
    }
  }
  fclose(f); return 0;
}

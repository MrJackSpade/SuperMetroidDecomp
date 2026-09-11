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
  // Deliberately overlap each bomb at its jump-producing fuse value. The ordinary
  // control differs only in the type sign bit; it proves geometry is accepted.
  fprintf(f, "overlap,slot,ordinary,xDelta,bombJump\n");
  for (int slot = 0; slot < 5; slot++)
  for (int ordinary = 0; ordinary < 2; ordinary++)
  for (int dx = -1; dx <= 1; dx++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    samus_x_pos = samus_y_pos = 512; samus_x_radius = samus_y_radius = 7;
    RunAsmCode(0x90d849, 0, 0, 0, 0);
    projectile_variables[slot + 5] = 8;
    if (ordinary) projectile_type[slot + 5] &= 0x7fff;
    samus_x_pos += dx;
    RunAsmCode(0xa09785, 0, 0, 0, 0);
    fprintf(f, "overlap,%d,%d,%d,%04X\n", slot, ordinary, dx, bomb_jump_dir);
  }
  fprintf(f, "admission,pb,occupied,down,fresh,held,charge,spread,count,ammo,type\n");
  for (int pb = 0; pb < 2; pb++)
  for (int occupied = 0; occupied < 2; occupied++)
  for (int down = 0; down < 2; down++)
  for (int fresh = 0; fresh < 2; fresh++)
  for (int held = 0; held < 2; held++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = room_height_in_blocks = 64;
    samus_x_pos = samus_y_pos = 512; samus_pose = 0x1d;
    equipped_items = 0x1004; button_config_shoot_x = 0x40; game_state = 8;
    if (occupied) {
      joypad1_lastkeys = joypad1_newkeys = 0x40;
      RunAsmCode(0x90bf9d, 0, 0, 0, 0);
      RunAsmCode(0x90aece, 0, 0, 0, 0);
      // Let the producer's cooldown expire without deleting the seeded bomb.
      joypad1_lastkeys = joypad1_newkeys = 0;
      for (int n = 0; n < 16; n++) {
        RunAsmCode(0x90ac1c, 0, 0, 0, 0);
        RunAsmCode(0x90bf9d, 0, 0, 0, 0);
        RunAsmCode(0x90aece, 0, 0, 0, 0);
      }
    }
    samus_power_bombs = 2; hud_item_index = pb ? 3 : 0; flare_counter = 60;
    joypad1_lastkeys = (held ? 0x40 : 0) | (down ? 0x400 : 0);
    joypad1_newkeys = fresh ? 0x40 : 0;
    RunAsmCode(0x90ac1c, 0, 0, 0, 0);
    RunAsmCode(0x90bf9d, 0, 0, 0, 0);
    fprintf(f, "admission,%d,%d,%d,%d,%d,%04X,%04X,%04X,%04X,%04X\n",
      pb,occupied,down,fresh,held,flare_counter,bomb_spread_charge_timeout_counter,
      bomb_counter,samus_power_bombs,projectile_type[5]);
  }
  fclose(f); return 0;
}

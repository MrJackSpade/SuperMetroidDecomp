#include "native-bounded-cpu.h"

int DiagnosticWallSand(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"probe,left,kind,ydir,damage,collision,amount,x,y,yspeed,gravity\n");
  for(int probe=0;probe<2;probe++) for(int left=0;left<2;left++)
  for(int kind=0;kind<2;kind++) for(int ydir=0;ydir<4;ydir++) for(int damage=0;damage<2;damage++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    room_width_in_blocks=16; room_height_in_blocks=16; room_size_in_blocks=512;
    area_index=4; samus_x_pos=left ? 117 : 139; samus_y_pos=136;
    samus_x_subpos=0x4000; samus_y_subpos=0x3456;
    samus_x_radius=5; samus_y_radius=12; samus_y_dir=ydir;
    samus_y_speed=5; samus_y_subspeed=0x4000; samus_y_accel=1; samus_y_subaccel=0x3000;
    samus_contact_damage_index=damage; samus_collision_direction=left ? 0 : 1;
    int block=8*16+(left ? 6 : 9);
    level_data[block]=0x3000; BTS[block]=kind ? 0x83 : 0x80;
    *(uint16 *)(g_ram+0x12)=left ? -7 : 7; *(uint16 *)(g_ram+0x14)=0;
    ProbeRunBounded(probe ? 0x94967f : 0x94971e);
    int32 amount=((uint32)*(uint16 *)(g_ram+0x12)<<16)|*(uint16 *)(g_ram+0x14);
    fprintf(f,"%d,%d,%d,%d,%d,%d,%08X,%04X%04X,%04X%04X,%04X%04X,%04X%04X\n",
      probe,left,kind,ydir,damage,samus_collision_flag,amount<0 ? -amount : amount,
      samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
      samus_y_speed,samus_y_subspeed,samus_y_accel,samus_y_subaccel);
  }
  fclose(f);return 0;
}

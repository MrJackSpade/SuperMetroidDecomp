#include "native-bounded-cpu.h"
int DiagnosticReportedGroundShot(const char *rom) {
 int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
 cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
 g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
 room_width_in_blocks=112; room_height_in_blocks=16;
 room_width_in_scrolls=7; room_height_in_scrolls=1; room_size_in_blocks=112*16*2;
 level_data[10*112+104]=0x1709; BTS[10*112+104]=0x12;
 level_data[10*112+105]=0x1708; BTS[10*112+105]=0x13;
 level_data[10*112+106]=0x132f; BTS[10*112+106]=0x13;
 level_data[10*112+107]=0x832f;
 projectile_type[0]=0x8000; projectile_x_pos[0]=1646;
 projectile_y_pos[0]=166; projectile_x_radius[0]=8; projectile_y_radius[0]=4;
 projectile_dir[0]=2; projectile_bomb_x_speed[0]=1024;
 for(int frame=2;frame<16;frame++) {
  projectile_bomb_x_speed[0]+=16;
  ProbeRunBounded(0x94a23b);
  printf("GROUND_SHOT frame=%d x=%u sub=%u y=%u type=%04X carry=%u\n",frame,projectile_x_pos[0],projectile_bomb_x_subpos[0],projectile_y_pos[0],projectile_type[0],g_snes->cpu->c);
  if(projectile_type[0]!=0x8000) return frame==4 && projectile_x_pos[0]==1666 && projectile_bomb_x_subpos[0]==24576 && projectile_y_pos[0]==166 && projectile_type[0]==0x8700 ? 0 : 11;
 }
 return 10;
}


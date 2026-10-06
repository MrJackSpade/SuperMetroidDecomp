#include "native-bounded-cpu.h"
// Recorded Construction Zone beam: y=131, radius=8/4, leftward motion.
int DiagnosticReportedBlocks(const char *rom) {
 int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
 for(int stopped=0; stopped<2; stopped++) {
  cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
  g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
  room_width_in_blocks=16; room_height_in_blocks=32;
  room_width_in_scrolls=1; room_height_in_scrolls=2; room_size_in_blocks=16*32*2;
  for(int x=4;x<=11;x++) level_data[8*16+x]=(x>=6&&x<=9)?0xc000:0x8000;
  projectile_type[0]=0x8000; projectile_x_pos[0]=181; projectile_bomb_x_subpos[0]=61440;
  projectile_y_pos[0]=stopped?134:131; projectile_x_radius[0]=8; projectile_y_radius[0]=4;
  projectile_dir[0]=7; projectile_bomb_x_speed[0]=(uint16)-1296;
  for(int frame=1;frame<=(stopped?1:20);frame++) {
   projectile_bomb_x_speed[0]-=16;
   ProbeRunBounded(0x94a23b);
   printf("SHOT_BLOCK stopped=%d step=%d x=%u sub=%u y=%u type=%04X carry=%u\n",stopped,frame,projectile_x_pos[0],projectile_bomb_x_subpos[0],projectile_y_pos[0],projectile_type[0],g_snes->cpu->c);
   if(projectile_type[0]!=(stopped?0x8700:0x8000)) return 10;
  }
  int actors=0; for(int slot=0;slot<40;slot++) actors+=plm_header_ptr[slot]!=0;
  printf("ACTORS stopped=%d count=%d\n",stopped,actors);
  if(!stopped && (actors<4 || projectile_x_pos[0]!=67 || projectile_bomb_x_subpos[0]!=36864)) return 11;
 }
 return 0;
}

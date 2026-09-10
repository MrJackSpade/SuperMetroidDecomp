#include "native-bounded-cpu.h"

// #455 isolation boundary: actual $A0:A8F0, not a replacement collision model.
// Actor AI/carry and controller-driven pose transitions are intentionally outside
// this capture. The sweep distinguishes existing penetration from approach.
int DiagnosticKagoSolidity(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  const uint16 definitions[] = {0xd5ff,0xd83f,0xdfff,0xe03f};
  const uint16 poses[] = {1,2,0x1d,0x41,0x25,0x26,0x27,0x28,0x35,0x36,0x3b,0x3c};
  const uint16 fractions[] = {0,0x8000,0xffff};
  fprintf(f,"definition,pose,direction,gap,fraction,mode,collided,distance,subdistance,ysub,index\n");
  for (int actor=0;actor<4;actor++)
  for (int p=0;p<12;p++)
  for (int direction=0;direction<4;direction++)
  for (int gap=-2;gap<=2;gap++)
  for (int fraction=0;fraction<3;fraction++)
  for (int mode=0;mode<3;mode++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0;
    samus_pose=poses[p]; samus_x_pos=samus_y_pos=1024;
    samus_x_subpos=samus_y_subpos=fractions[fraction];
    ProbeRunBounded(0x90ec22); // Read the cartridge pose radius, including compact poses.
    EnemyData *enemy=gEnemyData(0);
    enemy->enemy_ptr=definitions[actor];
    enemy->x_width=*(uint16*)RomPtr(0xa00000+definitions[actor]+8);
    enemy->y_height=*(uint16*)RomPtr(0xa00000+definitions[actor]+10);
    enemy->x_pos=enemy->y_pos=1024;
    int distance=(direction<2 ? enemy->x_width+samus_x_radius : enemy->y_height+samus_y_radius)+gap;
    if (direction<2) enemy->x_pos+=direction==0 ? -distance : distance;
    else enemy->y_pos+=direction==2 ? -distance : distance;
    enemy->properties=mode==0 ? 0x8000 : 0;
    enemy->frozen_timer=mode==2 ? 1 : 0;
    interactive_enemy_indexes_write_ptr=2;
    interactive_enemy_indexes[0]=0; interactive_enemy_indexes[1]=0xffff;
    for(int d=0;d<4;d++) enemy_index_colliding_dirs[d]=0xffff;
    samus_collision_direction=direction;
    *(uint16*)&g_ram[0x12]=2; *(uint16*)&g_ram[0x14]=0x8000;
    ProbeRunBounded(0xa0a8f0);
    fprintf(f,"%04X,%02X,%d,%d,%04X,%d,%04X,%04X,%04X,%04X,%04X\n",
      definitions[actor],poses[p],direction,gap,fractions[fraction],mode,
      g_snes->cpu->a,*(uint16*)&g_ram[0x12],*(uint16*)&g_ram[0x14],samus_y_subpos,
      enemy_index_colliding_dirs[direction]);
  }
  fclose(f); return 0;
}

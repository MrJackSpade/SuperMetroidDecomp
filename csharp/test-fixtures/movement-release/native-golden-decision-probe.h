// #617: original-CPU health/stun decisions, including RNG consumption and link writes.
#include "native-bounded-cpu.h"
int DiagnosticGoldenDecisions(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  const uint16 healths[] = {1927,1928,1929,10799,10800,10801};
  const uint16 seeds[] = {0,1,2,3,0x31,0x61,0xff,0x100,0x1234,0xffff};
  const uint32 routines[] = {0xaad474,0xaad49b};
  fprintf(f,"routine,health,flags,seed,cursor,random,link,counter\n");
  for (int r=0;r<2;r++) for(int h=0;h<6;h++)
  for(int flag=0;flag<2;flag++) for(int s=0;s<10;s++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    Enemy_Torizo *enemy = Get_Torizo(0);
    enemy->base.health=healths[h]; enemy->toriz_parameter_2=flag ? 0x2000 : 0;
    enemy->toriz_var_00=0x1234; enemy->toriz_var_09=9; random_number=seeds[s];
    ProbeRunBoundedRegisters(routines[r],0,0,0xd000);
    fprintf(f,"%04X,%u,%u,%u,%u,%u,%u,%u\n",routines[r]&0xffff,healths[h],
      flag?0x2000:0,seeds[s],g_snes->cpu->y,random_number,enemy->toriz_var_00,enemy->toriz_var_09);
  }
  fclose(f); return 0;
}

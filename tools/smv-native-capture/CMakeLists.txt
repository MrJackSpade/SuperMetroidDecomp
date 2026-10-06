cmake_minimum_required(VERSION 3.20)
project(RidleyNativeCapture C CXX)
set(CMAKE_CXX_STANDARD 17)
add_subdirectory(zlib)
add_executable(ridley_capture capture.cpp
  snes9x-160/filter/snes_ntsc.c
  snes9x-160/apu/apu.cpp
  snes9x-160/apu/bapu/dsp/sdsp.cpp
  snes9x-160/apu/bapu/smp/smp.cpp
  snes9x-160/apu/bapu/smp/smp_state.cpp
  snes9x-160/bsx.cpp
  snes9x-160/c4.cpp
  snes9x-160/c4emu.cpp
  snes9x-160/cheats.cpp
  snes9x-160/cheats2.cpp
  snes9x-160/clip.cpp
  snes9x-160/conffile.cpp
  snes9x-160/controls.cpp
  snes9x-160/cpu.cpp
  snes9x-160/cpuexec.cpp
  snes9x-160/cpuops.cpp
  snes9x-160/crosshairs.cpp
  snes9x-160/dma.cpp
  snes9x-160/dsp.cpp
  snes9x-160/dsp1.cpp
  snes9x-160/dsp2.cpp
  snes9x-160/dsp3.cpp
  snes9x-160/dsp4.cpp
  snes9x-160/fxinst.cpp
  snes9x-160/fxemu.cpp
  snes9x-160/gfx.cpp
  snes9x-160/globals.cpp
  snes9x-160/logger.cpp
  snes9x-160/memmap.cpp
  snes9x-160/obc1.cpp
  snes9x-160/msu1.cpp
  snes9x-160/ppu.cpp
  snes9x-160/stream.cpp
  snes9x-160/sa1.cpp
  snes9x-160/sa1cpu.cpp
  snes9x-160/screenshot.cpp
  snes9x-160/sdd1.cpp
  snes9x-160/sdd1emu.cpp
  snes9x-160/seta.cpp
  snes9x-160/seta010.cpp
  snes9x-160/seta011.cpp
  snes9x-160/seta018.cpp
  snes9x-160/snapshot.cpp
  snes9x-160/snes9x.cpp
  snes9x-160/spc7110.cpp
  snes9x-160/srtc.cpp
  snes9x-160/tile.cpp
  snes9x-160/sha256.cpp
  snes9x-160/bml.cpp
  snes9x-160/movie.cpp
  snes9x-160/libretro/libretro.cpp
)
target_include_directories(ridley_capture PRIVATE snes9x-160 snes9x-160/libretro snes9x-160/apu snes9x-160/apu/bapu zlib ${CMAKE_CURRENT_BINARY_DIR}/zlib)
target_compile_definitions(ridley_capture PRIVATE __WIN32__ __LIBRETRO__ RIGHTSHIFT_IS_SAR ALLOW_CPU_OVERCLOCK HAVE_STDINT_H ZLIB _CRT_SECURE_NO_WARNINGS _CRT_NONSTDC_NO_WARNINGS NOMINMAX)
target_link_libraries(ridley_capture PRIVATE zlibstatic)

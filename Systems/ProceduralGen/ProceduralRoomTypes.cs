using System;

namespace Casiland.Systems.ProceduralGen;


[Flags]
enum ProceduralRoomTypes
{
    Normal,
    Start,
    Boss,
    Shop,
    Crashout,
    Treasure,
}
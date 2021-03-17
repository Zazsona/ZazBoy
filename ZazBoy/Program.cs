using System;
using ZazBoy.Console;

namespace ZazBoy
{
    class Program
    {
        static void Main(string[] args)
        {
            GameBoy gameBoy = GameBoy.Instance();
            gameBoy.SetPowerOn(true);
        }
    }
}

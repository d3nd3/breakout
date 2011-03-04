using System;

namespace Breakout
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (BreakoutGame game = new BreakoutGame())
            {
                game.Run();
            }
        }
    }
#endif
}


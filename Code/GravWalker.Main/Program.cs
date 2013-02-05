using System;

namespace GravWalker
{
#if WINDOWS || LINUX || XBOX 
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (GravWalker game = new GravWalker())
            {
                game.Run();
            }
        }
    }
#endif
#if WINRT
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var factory = new MonoGame.Framework.GameFrameworkViewSource<GravWalker>();
            Windows.ApplicationModel.Core.CoreApplication.Run(factory);
        }
    }
#endif
}


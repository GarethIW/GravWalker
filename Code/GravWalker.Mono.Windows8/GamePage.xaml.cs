using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MonoGame.Framework;


namespace GravWalker
{
    /// <summary>
    /// The root page used to display the game.
    /// </summary>
    public sealed partial class GamePage : SwapChainBackgroundPanel
    {
        readonly GravWalker _game;

        public GamePage(string launchArguments)
        {
            this.InitializeComponent();

            // Create the game.
            _game = XamlGame<GravWalker>.Create(launchArguments, Window.Current.CoreWindow, this);
        }
    }
}

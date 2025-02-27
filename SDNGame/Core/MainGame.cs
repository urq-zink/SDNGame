using SDNGame.Core.GameScenes;

namespace SDNGame.Core
{
    public class MainGame : Game
    {
        protected override void Initialize()
        {
            SetScene(new LoadingScene(this, 2f, new MainMenuScene(this)));
        }

        protected override void LoadContent()
        {

        }
    }
}

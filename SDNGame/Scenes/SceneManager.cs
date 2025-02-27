using SDNGame.Core;
using SDNGame.Scenes.Transitioning;

namespace SDNGame.Scenes
{
    public class SceneManager
    {
        private readonly Game _game;
        private Scene _currentScene;
        private Scene _nextScene;
        private Transition _outgoingTransition;
        private Transition _incomingTransition;
        private bool _isTransitioning;

        public Scene CurrentScene => _currentScene;

        public SceneManager(Game game)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
        }

        public void SetScene(Scene scene, Transition? outgoing = null, Transition? incoming = null)
        {
            ArgumentNullException.ThrowIfNull(scene);

            if (_isTransitioning)
            {
                _nextScene = scene;
                return;
            }

            _nextScene = scene;
            _outgoingTransition = outgoing ?? new FadeTransition(_game, 0.5f, false);
            _incomingTransition = incoming ?? new FadeTransition(_game, 0.5f, true);
            _outgoingTransition.Start();
            _isTransitioning = true;

            if (_outgoingTransition.Duration <= 0)
            {
                CompleteOutgoingTransition();
            }
        }

        public void Update(double deltaTime)
        {
            if (_isTransitioning)
            {
                if (!_outgoingTransition.IsComplete)
                {
                    _outgoingTransition.Update((float)deltaTime);
                    if (_outgoingTransition.IsComplete)
                    {
                        CompleteOutgoingTransition();
                    }
                }
                else if (!_incomingTransition.IsComplete)
                {
                    _incomingTransition.Update((float)deltaTime);
                    if (_incomingTransition.IsComplete)
                    {
                        CompleteTransition();
                    }
                }
            }
            else
            {
                _currentScene?.Update(deltaTime);
            }
        }

        public void Draw(double deltaTime)
        {
            if (_isTransitioning)
            {
                if (!_outgoingTransition.IsComplete)
                {
                    _currentScene?.Draw(deltaTime);
                    _outgoingTransition.Draw((float)deltaTime);
                }
                else if (!_incomingTransition.IsComplete)
                {
                    _currentScene?.Draw(deltaTime);
                    _incomingTransition.Draw((float)deltaTime);
                }
            }
            else
            {
                _currentScene?.Draw(deltaTime);
            }
        }

        private void CompleteOutgoingTransition()
        {
            _currentScene?.OnExit();
            _currentScene?.Dispose();
            _outgoingTransition.Dispose();

            _currentScene = _nextScene;
            _currentScene.Initialize();
            _currentScene.LoadContent();
            _currentScene.OnEnter();
            _incomingTransition.Start();
        }

        private void CompleteTransition()
        {
            _incomingTransition.Dispose();
            _isTransitioning = false;
            _nextScene = null;

            if (_nextScene != null)
            {
                SetScene(_nextScene);
            }
        }
    }
}
using Silk.NET.Input;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json;
using System.IO;

namespace SDNGame.Input
{
    public class InputManager
    {
        private readonly IInputContext _inputContext;
        private readonly Dictionary<string, Action> _actionBindings = new();
        private readonly Dictionary<string, HashSet<InputBinding>> _inputMappings = new();
        private readonly Dictionary<Key, bool> _keyStates = new();
        private readonly Dictionary<Key, bool> _prevKeyStates = new();
        private readonly Dictionary<MouseButton, bool> _mouseButtonStates = new();
        private readonly Dictionary<MouseButton, bool> _prevMouseButtonStates = new();
        private readonly Dictionary<Button, bool> _gamepadButtonStates = new();
        private readonly Dictionary<Button, bool> _prevGamepadButtonStates = new();

        public Vector2 MousePosition { get; private set; }
        public Vector2 PreviousMousePosition { get; private set; }
        public Vector2 MouseDelta => MousePosition - PreviousMousePosition;
        public float ScrollDelta { get; private set; }
        public Vector2 LeftStick { get; private set; }
        public Vector2 RightStick { get; private set; }
        public float LeftTrigger { get; private set; }
        public float RightTrigger { get; private set; }

        public InputManager(IInputContext inputContext)
        {
            _inputContext = inputContext;

            // Initialize keyboard input
            foreach (var keyboard in _inputContext.Keyboards)
            {
                keyboard.KeyDown += OnKeyDown;
                keyboard.KeyUp += OnKeyUp;
            }

            // Initialize mouse input
            foreach (var mouse in _inputContext.Mice)
            {
                mouse.MouseDown += OnMouseDown;
                mouse.MouseUp += OnMouseUp;
                mouse.MouseMove += OnMouseMove;
                mouse.Scroll += OnMouseScroll;
            }

            // Initialize gamepad input
            foreach (var gamepad in _inputContext.Gamepads)
            {
                gamepad.ButtonDown += OnGamepadButtonDown;
                gamepad.ButtonUp += OnGamepadButtonUp;
                gamepad.ThumbstickMoved += OnThumbstickMoved;
                gamepad.TriggerMoved += OnTriggerMoved;
            }

            LoadDefaultBindings();
        }

        // Input binding structure
        public struct InputBinding
        {
            public Key? KeyboardKey { get; set; }
            public MouseButton? MouseButton { get; set; }
            public Button? GamepadButton { get; set; }
        }

        // Bind an action to a name
        public void BindAction(string actionName, Action action)
        {
            _actionBindings[actionName] = action;
        }

        // Map an input to an action
        public void MapInput(string actionName, InputBinding binding)
        {
            if (!_inputMappings.ContainsKey(actionName))
                _inputMappings[actionName] = new HashSet<InputBinding>();
            _inputMappings[actionName].Add(binding);
        }

        // Check if an action is triggered
        public bool IsActionPressed(string actionName) => CheckActionState(actionName, isPressed: true);
        public bool IsActionReleased(string actionName) => CheckActionState(actionName, isPressed: false);
        public bool IsActionHeld(string actionName) => CheckActionState(actionName, isHeld: true);

        private bool CheckActionState(string actionName, bool isPressed = false, bool isHeld = false)
        {
            if (!_inputMappings.TryGetValue(actionName, out var bindings)) return false;

            foreach (var binding in bindings)
            {
                if (binding.KeyboardKey.HasValue)
                {
                    bool current = _keyStates.GetValueOrDefault(binding.KeyboardKey.Value);
                    bool previous = _prevKeyStates.GetValueOrDefault(binding.KeyboardKey.Value);
                    if (isPressed && current && !previous) return true;
                    if (isHeld && current) return true;
                    if (!isPressed && !isHeld && !current && previous) return true;
                }
                if (binding.MouseButton.HasValue)
                {
                    bool current = _mouseButtonStates.GetValueOrDefault(binding.MouseButton.Value);
                    bool previous = _prevMouseButtonStates.GetValueOrDefault(binding.MouseButton.Value);
                    if (isPressed && current && !previous) return true;
                    if (isHeld && current) return true;
                    if (!isPressed && !isHeld && !current && previous) return true;
                }
                if (binding.GamepadButton.HasValue)
                {
                    bool current = _gamepadButtonStates.GetValueOrDefault(binding.GamepadButton.Value);
                    bool previous = _prevGamepadButtonStates.GetValueOrDefault(binding.GamepadButton.Value);
                    if (isPressed && current && !previous) return true;
                    if (isHeld && current) return true;
                    if (!isPressed && !isHeld && !current && previous) return true;
                }
            }
            return false;
        }

        // Update method to process actions
        public void Update()
        {
            foreach (var action in _actionBindings)
            {
                if (IsActionPressed(action.Key))
                    action.Value.Invoke();
            }

            // Update previous states
            foreach (var key in _keyStates.Keys) _prevKeyStates[key] = _keyStates[key];
            foreach (var button in _mouseButtonStates.Keys) _prevMouseButtonStates[button] = _mouseButtonStates[button];
            foreach (var button in _gamepadButtonStates.Keys) _prevGamepadButtonStates[button] = _gamepadButtonStates[button];
        }

        // Direct input checks (for legacy or specific use cases)
        public bool IsKeyPressed(Key key) => _keyStates.GetValueOrDefault(key) && !_prevKeyStates.GetValueOrDefault(key);
        public bool IsKeyHeld(Key key) => _keyStates.GetValueOrDefault(key);
        public bool IsKeyReleased(Key key) => !_keyStates.GetValueOrDefault(key) && _prevKeyStates.GetValueOrDefault(key);

        public bool IsMouseButtonPressed(MouseButton button) => _mouseButtonStates.GetValueOrDefault(button) && !_prevMouseButtonStates.GetValueOrDefault(button);
        public bool IsMouseButtonHeld(MouseButton button) => _mouseButtonStates.GetValueOrDefault(button);
        public bool IsMouseButtonReleased(MouseButton button) => !_mouseButtonStates.GetValueOrDefault(button) && _prevMouseButtonStates.GetValueOrDefault(button);

        public bool IsGamepadButtonPressed(Button button) => _gamepadButtonStates.GetValueOrDefault(button) && !_prevGamepadButtonStates.GetValueOrDefault(button);
        public bool IsGamepadButtonHeld(Button button) => _gamepadButtonStates.GetValueOrDefault(button);
        public bool IsGamepadButtonReleased(Button button) => !_gamepadButtonStates.GetValueOrDefault(button) && _prevGamepadButtonStates.GetValueOrDefault(button);

        // Event handlers
        private void OnKeyDown(IKeyboard keyboard, Key key, int scancode)
        {
            _keyStates[key] = true;
        }

        private void OnKeyUp(IKeyboard keyboard, Key key, int scancode)
        {
            _keyStates[key] = false;
        }

        private void OnMouseDown(IMouse mouse, MouseButton button)
        {
            _mouseButtonStates[button] = true;
        }

        private void OnMouseUp(IMouse mouse, MouseButton button)
        {
            _mouseButtonStates[button] = false;
        }

        private void OnMouseMove(IMouse mouse, Vector2 position)
        {
            PreviousMousePosition = MousePosition;
            MousePosition = position;
        }

        private void OnMouseScroll(IMouse mouse, ScrollWheel scroll)
        {
            ScrollDelta = scroll.Y;
        }

        private void OnGamepadButtonDown(IGamepad gamepad, Button button)
        {
            _gamepadButtonStates[button] = true;
        }

        private void OnGamepadButtonUp(IGamepad gamepad, Button button)
        {
            _gamepadButtonStates[button] = false;
        }

        private void OnThumbstickMoved(IGamepad gamepad, Thumbstick thumbstick)
        {
            if (thumbstick.Index == 0) LeftStick = new Vector2(thumbstick.X, thumbstick.Y);
            else if (thumbstick.Index == 1) RightStick = new Vector2(thumbstick.X, thumbstick.Y);
        }

        private void OnTriggerMoved(IGamepad gamepad, Trigger trigger)
        {
            if (trigger.Index == 0) LeftTrigger = trigger.Position;
            else if (trigger.Index == 1) RightTrigger = trigger.Position;
        }

        // Load default bindings (example)
        private void LoadDefaultBindings()
        {
            MapInput("Jump", new InputBinding { KeyboardKey = Key.Space });
            // MapInput("Jump", new InputBinding { GamepadButton = Button.A });
            MapInput("MoveLeft", new InputBinding { KeyboardKey = Key.A });
            MapInput("MoveRight", new InputBinding { KeyboardKey = Key.D });
            MapInput("Fire", new InputBinding { MouseButton = MouseButton.Left });
            MapInput("Pause", new InputBinding { KeyboardKey = Key.Escape });
        }

        // Save/Load bindings to/from file
        public void SaveBindings(string filePath)
        {
            var serializableMappings = new Dictionary<string, List<InputBinding>>();
            foreach (var kvp in _inputMappings)
                serializableMappings[kvp.Key] = new List<InputBinding>(kvp.Value);
            File.WriteAllText(filePath, JsonSerializer.Serialize(serializableMappings));
        }

        public void LoadBindings(string filePath)
        {
            if (!File.Exists(filePath)) return;
            var loadedMappings = JsonSerializer.Deserialize<Dictionary<string, List<InputBinding>>>(File.ReadAllText(filePath));
            _inputMappings.Clear();
            foreach (var kvp in loadedMappings)
                _inputMappings[kvp.Key] = new HashSet<InputBinding>(kvp.Value);
        }
    }
}
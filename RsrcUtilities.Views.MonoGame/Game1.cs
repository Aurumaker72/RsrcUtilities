using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using RsrcUtilities.Controls;
using RsrcUtilities.Geometry.Enums;
using Rectangle = RsrcUtilities.Geometry.Structs.Rectangle;

namespace RsrcUtilities.Views.MonoGame;

public class Game1 : Game
{
    private bool _allowPanning;
    private Dialog _dialog;
    private DialogPainter _dialogPainter;
    private readonly GraphicsDeviceManager _graphics;
    private bool _isPanning;
    private KeyboardState _keyboardState;
    private KeyboardState _lastKeyboardState;
    private MouseState _lastMouseState;

    private MouseState _mouseState;
    private OrthographicCamera _orthographicCamera;
    private SpriteBatch _spriteBatch;
    private Vector2 _startPanCameraPosition;
    private Vector2 _startPanMousePosition;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        _graphics.SynchronizeWithVerticalRetrace = false;
        IsFixedTimeStep = false;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _dialogPainter = new DialogPainter(GraphicsDevice, Content);
        _orthographicCamera = new OrthographicCamera(GraphicsDevice)
        {
            MinimumZoom = 0.01f,
            MaximumZoom = 5f,
            Origin = Vector2.Zero
        };

        _dialog = new Dialog
        {
            Identifier = "IDD_ABOUTBOX",
            Width = 200,
            Height = 100
        };
        var root = new TreeNode<Control>(new Button
        {
            Identifier = "IDC_BUTTON",
            Rectangle = new Rectangle(10, 10, 80, 30),
            Caption = "Hello World!",
            HorizontalAlignment = HorizontalAlignments.Stretch,
            VerticalAlignment = VerticalAlignments.Stretch
        });

        _dialog.Root = root;
    }

    protected override void Update(GameTime gameTime)
    {
        var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _mouseState = Mouse.GetState();
        _keyboardState = Keyboard.GetState();

        if (_allowPanning) HandlePanning();

        if (_keyboardState.IsKeyDown(Keys.Up)) _orthographicCamera.ZoomIn(10f * deltaSeconds);
        if (_keyboardState.IsKeyDown(Keys.Down)) _orthographicCamera.ZoomIn(-10f * deltaSeconds);

        _lastMouseState = _mouseState;
        _lastKeyboardState = _keyboardState;
        base.Update(gameTime);
    }

    private void HandlePanning()
    {
        if (_mouseState.LeftButton == ButtonState.Pressed && _lastMouseState.LeftButton != ButtonState.Pressed)
        {
            _startPanMousePosition = _mouseState.Position.ToVector2();
            _startPanCameraPosition = _orthographicCamera.Position;
            _isPanning = true;
        }
        else if (_mouseState.LeftButton == ButtonState.Released)
        {
            _isPanning = false;
        }

        if (_isPanning)
        {
            var delta = (_mouseState.Position.ToVector2() - _startPanMousePosition) / _orthographicCamera.Zoom;
            _orthographicCamera.Position = _startPanCameraPosition - delta;
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin(transformMatrix: _orthographicCamera.GetViewMatrix(), samplerState: SamplerState.PointClamp);
        var dialogTexture = _dialogPainter.GetTexture2D(_dialog);

        _spriteBatch.Draw(dialogTexture, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, Vector2.One,
            SpriteEffects.None, 0f);

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using RsrcUtilities.Controls;
using RsrcUtilities.Layout.Implementations;
using RsrcUtilities.Layout.Interfaces;
using RsrcUtilities.Views.MonoGame.Extensions;

namespace RsrcUtilities.Views.MonoGame;

public class DialogPainter
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly ILayoutEngine _layoutEngine = new DefaultLayoutEngine();
    private readonly SpriteBatch _spriteBatch;
    private readonly SpriteFont _spriteFont;
    private RenderTarget2D? _renderTarget2D;

    public DialogPainter(GraphicsDevice graphicsDevice, ContentManager contentManager)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = new SpriteBatch(graphicsDevice);
        _spriteFont = contentManager.Load<SpriteFont>("MS Shell Dlg");
    }

    public Texture2D GetTexture2D(Dialog dialog)
    {
        var flattened = _layoutEngine.DoLayout(dialog);

        if (_renderTarget2D == null || dialog.Width != _renderTarget2D.Width || dialog.Height != _renderTarget2D.Height)
            _renderTarget2D = new RenderTarget2D(_graphicsDevice, dialog.Width, dialog.Height, false,
                SurfaceFormat.Color,
                DepthFormat.None);

        _graphicsDevice.SetRenderTarget(_renderTarget2D);
        _graphicsDevice.Clear(new Color(240, 240, 240));

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        foreach (var pair in flattened)
        {
            RectangleF rectangleF = new(pair.Value.X, pair.Value.Y, pair.Value.Width, pair.Value.Height);
            if (pair.Key is Button button)
            {
                _spriteBatch.FillRectangle(rectangleF.InflateCopy(1), new Color(173, 173, 173));
                _spriteBatch.FillRectangle(rectangleF, new Color(225, 225, 225));
                var captionSize = _spriteFont.MeasureString(button.Caption);
                _spriteBatch.DrawString(_spriteFont, button.Caption, rectangleF.Center.ToVector2() - captionSize / 2,
                    Color.Black);
            }
            else if (pair.Key is TextBox textBox)
            {
                _spriteBatch.FillRectangle(rectangleF.InflateCopy(1), new Color(122, 122, 122));
                _spriteBatch.FillRectangle(rectangleF, new Color(255, 255, 255));
                var captionSize = _spriteFont.MeasureString("Text");
                _spriteBatch.DrawString(_spriteFont, "Text", rectangleF.Center.ToVector2() - captionSize / 2,
                    Color.Black);
            }
            else if (pair.Key is GroupBox groupBox)
            {
                _spriteBatch.DrawRectangle(rectangleF, new Color(220, 220, 220));
                var captionSize = _spriteFont.MeasureString(groupBox.Caption);
                _spriteBatch.DrawString(_spriteFont, groupBox.Caption, rectangleF.GetCorners()[0],
                    Color.Black);
            }
            else
            {
                ;
            }
        }

        _spriteBatch.End();
        _graphicsDevice.SetRenderTarget(null);
        return _renderTarget2D;
    }
}
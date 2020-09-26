using System.Collections.Generic;

using SFML.Graphics;
using SFML.System;

namespace PRR {
    public class BitmapFont {
        public readonly Texture texture;
        public readonly Vector2i characterSize;
        public readonly Dictionary<char, Vector2f[]> characters = new Dictionary<char, Vector2f[]>();
        public BitmapFont(Image fontImage, string mappings, Vector2i characterSize) {
            this.characterSize = characterSize;
            int index = 0;
            for(uint y = 0; y < fontImage.Size.Y; y++) {
                for(uint x = 0; x < fontImage.Size.X; x++)
                    fontImage.SetPixel(x, y, fontImage.GetPixel(x, y) == Color.Black ? Color.Transparent : Color.White);
            }
            texture = new Texture(fontImage);
            for(int y = 0; y < fontImage.Size.Y; y += characterSize.Y) {
                for(int x = 0; x < fontImage.Size.X; x += characterSize.X) {
                    if(mappings.Length <= index + 1) break;
                    Vector2f[] texCoords = new Vector2f[4];
                    // Clockwise
                    texCoords[0] = new Vector2f(x, y); // top left
                    texCoords[1] = new Vector2f(x + characterSize.X, y); // top right
                    texCoords[2] = new Vector2f(x + characterSize.X, y + characterSize.Y); // bottom right
                    texCoords[3] = new Vector2f(x, y + characterSize.Y); // bottom left
                    characters.Add(mappings[index++], texCoords);
                }
            }
        }
    }
    public class BitmapText {
        public Dictionary<Vector2i, RenderCharacter> text;
        readonly Vertex[] _backgroundQuads;
        readonly Vertex[] _foregroundQuads;
        public RenderTexture renderTexture { get; }
        public void RebuildRenderTexture(Color background) {
            renderTexture.Clear(background);

            uint index = 0;
            foreach((Vector2i key, RenderCharacter value) in text) {
                int xChar = key.X * _charWidth;
                int yChar = key.Y * _charHeight;
                Vector2f position = new Vector2f(xChar, yChar);
    
                _backgroundQuads[index].Position = position;
                _backgroundQuads[index + 1].Position = position + new Vector2f(_charWidth, 0f);
                _backgroundQuads[index + 2].Position = position + new Vector2f(_charWidth, _charHeight);
                _backgroundQuads[index + 3].Position = position + new Vector2f(0f, _charHeight);
    
                _backgroundQuads[index].Color = value.background;
                _backgroundQuads[index + 1].Color = value.background;
                _backgroundQuads[index + 2].Color = value.background;
                _backgroundQuads[index + 3].Color = value.background;
    
                if(_font.characters.TryGetValue(value.character, out Vector2f[] texCoords)) {
                    _foregroundQuads[index].Position = _backgroundQuads[index].Position;
                    _foregroundQuads[index + 1].Position = _backgroundQuads[index + 1].Position;
                    _foregroundQuads[index + 2].Position = _backgroundQuads[index + 2].Position;
                    _foregroundQuads[index + 3].Position = _backgroundQuads[index + 3].Position;
                    
                    _foregroundQuads[index].TexCoords = texCoords[0];
                    _foregroundQuads[index + 1].TexCoords = texCoords[1];
                    _foregroundQuads[index + 2].TexCoords = texCoords[2];
                    _foregroundQuads[index + 3].TexCoords = texCoords[3];
    
                    _foregroundQuads[index].Color = value.foreground;
                    _foregroundQuads[index + 1].Color = value.foreground;
                    _foregroundQuads[index + 2].Color = value.foreground;
                    _foregroundQuads[index + 3].Color = value.foreground;
                }
                else {
                    _foregroundQuads[index].TexCoords = new Vector2f();
                    _foregroundQuads[index + 1].TexCoords = new Vector2f();
                    _foregroundQuads[index + 2].TexCoords = new Vector2f();
                    _foregroundQuads[index + 3].TexCoords = new Vector2f();
                }
    
                index += 4;
            }
            renderTexture.Draw(_backgroundQuads, 0, (uint)(text.Count * 4), PrimitiveType.Quads);
            renderTexture.Draw(_foregroundQuads, 0, (uint)(text.Count * 4), PrimitiveType.Quads,
                new RenderStates(_font.texture));

            renderTexture.Display();
        }

        readonly byte _charWidth;
        readonly byte _charHeight;
        public readonly uint imageWidth;
        public readonly uint imageHeight; // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        readonly uint _textWidth; // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        readonly uint _textHeight;
        readonly BitmapFont _font;
        public BitmapText(BitmapFont font, Vector2i size) {
            _font = font;
            _charWidth = (byte)font.characterSize.X;
            _charHeight = (byte)font.characterSize.Y;
            _textWidth = (uint)size.X;
            _textHeight = (uint)size.Y;
            imageWidth = _textWidth * _charWidth;
            imageHeight = _textHeight * _charHeight;
            renderTexture = new RenderTexture(imageWidth, imageHeight);
            _backgroundQuads = new Vertex[4 * _textWidth * _textHeight];
            _foregroundQuads = new Vertex[4 * _textWidth * _textHeight];
        }
    }
}

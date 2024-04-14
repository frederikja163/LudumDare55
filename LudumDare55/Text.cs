using System.Numerics;
using JAngine.Rendering;
using JAngine.Rendering.Gui;
using JAngine.Rendering.OpenGL;

namespace LudumDare55;

public sealed class Text
{
    private readonly List<GuiElement> _characters = new List<GuiElement>();
    private readonly GuiElement _parent;
    private readonly Texture _texture;
    private string _text = "";
    
    public Text(GuiElement element, string text)
    {
        _parent = element;
        _texture = TextureManager.GetFontAtlas();
        Value = text;
    }

    public event Action? TextChanged;
    
    public string Value
    {
        get => _text;
        set
        {
            _text = value;
            _parent.Width = value.Length * _parent.Height;
            for (int i = 0; i < value.Length; i++)
            {
                GuiElement character;
                if (i >= _characters.Count)
                {
                    character = new GuiElement(Application.Window, _texture)
                    {
                        Height = _parent.Height,
                        TextureSize = Vector2.One / 16f,
                        BackgroundColor = Vector4.One,
                        Y = _parent.Y,
                    };
                    _characters.Add(character);
                }
                else
                {
                    character = _characters[i];
                }

                character.X = Position.Left(_parent) + _parent.Height * i;
                character.Width = _parent.Height;
                character.TextureOffset = new Vector2(((value[i] % 16)) / 16f, (16 - value[i] / 16 - 1) / 16f);
            }

            for (int i = value.Length; i < _characters.Count; i++)
            {
                _characters[i].Width = Size.Pixels(0);
            }
            TextChanged?.Invoke();
        }
    }
}
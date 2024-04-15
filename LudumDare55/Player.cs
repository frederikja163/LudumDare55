using System.Numerics;
using JAngine.Rendering;
using JAngine.Rendering.Gui;
using JAngine.Rendering.OpenGL;

namespace LudumDare55;

public class Player
{
    public const float PlayerSize = 0.3f;
    private static int _wave;
    private static Text _waveText;
    public static int Wave
    {
        get => _wave;
        set
        {
            _wave = value;
            _waveText.Value = $"Wave: {value}";
        }
    }
    private static int _score;
    private static Text _scoreText;
    public static int Score
    {
        get => _score;
        set
        {
            _score = value;
            _scoreText.Value = $"Score: {value}";
        }
    }

    public static event Action OnShapeChange;
    public static ShapeType Type { get; private set; }
    public static Col Col { get; private set; }
    
    private readonly Text _text;
    private readonly GuiElement _player;
    private readonly BufferDataReference<ShapeInstance> _shapeRef;
    private ShapeType _lastType = ShapeType.Circle;
    private Col _lastCol = Col.White;
    
    public Player()
    {
        _player = Shape.CreateShape(out _shapeRef);
        _player.Width = Size.Percentage(PlayerSize);
        _player.Height = _player.Width;
        
        GuiElement textField = new GuiElement(_player)
        {
            BackgroundColor = new Vector4(0, 0, 0, 0),
            Height = Size.Pixels(15f),
        };
        _text = new Text(textField, "");
        GuiElement scoreText = new GuiElement(_player)
        {
            BackgroundColor = new Vector4(0, 0, 0, 0),
            Height = Size.Pixels(10),
            Y = Position.Bottom(textField) - 15,
        };
        _scoreText = new Text(scoreText, "Score: 0");
        GuiElement waveText = new GuiElement(_player)
        {
            BackgroundColor = new Vector4(0, 0, 0, 0),
            Height = Size.Pixels(10),
            Y = Position.Top(textField) + 15,
        };
        _waveText = new Text(waveText, "Wave: 0");

        Application.Window.OnTextTyped += OnWindowOnTextTyped;
        
        Application.Window.AddKeyBinding(Key.Backspace | Key.Release, DeleteLast);
        Application.Window.AddKeyBinding(Key.Delete | Key.Release, DeleteLast);
        Application.Window.AddKeyBinding(Key.Enter | Key.Release, ClearText);
        Application.Window.AddKeyBinding(Key.Escape | Key.Release, ClearText);
        Application.Window.AddKeyBinding(Key.Space | Key.Press, () => ObjectPool.TickSpeed = 10);
        Application.Window.AddKeyBinding(Key.Space | Key.Release, () => ObjectPool.TickSpeed = 1);

        _text.TextChanged += OnTextChanged;
    }

    private void OnTextChanged()
    {
        if (Enum.TryParse<ShapeType>(_text.Value, true, out ShapeType type))
        {
            Type = type;
            _text.Value = "";
            OnShapeChanged();
        }

        if (Enum.TryParse<Col>(_text.Value, true, out Col col))
        {
            _player.BackgroundColor = Application.Colors[col];
            Col = col;
            _text.Value = "";
            OnShapeChanged();
        }
    }

    private void OnShapeChanged()
    {
        Task.Run(async () =>
        {
            if (_lastCol != Col || _lastType != Type)
            {
                for (int i = 0; i <= Application.AnimationTime; i++)
                {
                    _shapeRef.Data = new ShapeInstance(_lastType, Type, i / Application.AnimationTime);
                    _player.BackgroundColor = Vector4.Lerp(Application.Colors[_lastCol], Application.Colors[Col], i / Application.AnimationTime);
                    Thread.Sleep(1000 / ObjectPool.TicksPerSecond);
                }
            }
            _lastCol = Col;
            _lastType = Type;
            foreach (Enemy enemy in Enemy.GetEnemies(Col, Type))
            {
                ObjectPool.SpawnProjectile(enemy, Col, Type);
            }
            OnShapeChange?.Invoke();
        });
    }

    private void DeleteLast()
    {
        if (!string.IsNullOrEmpty(_text.Value))
        {
            _text.Value = _text.Value.Remove(_text.Value.Length - 1);
        }
    }
    
    private void ClearText()
    {
        _text.Value = "";
    }

    private void OnWindowOnTextTyped(Window _, char c)
    {
        if (char.IsLetter(c))
        {
            _text.Value += c;
        }
    }
}
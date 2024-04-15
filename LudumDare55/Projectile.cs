using System.Numerics;
using JAngine.Rendering.Gui;
using JAngine.Rendering.OpenGL;

namespace LudumDare55;

public sealed class Projectile : IUpdateable
{
    private readonly GuiElement _element;
    private readonly BufferDataReference<ShapeInstance> _shapeRef;
    private bool _disabled = true;
    private int _spawnTick;
    private int _fadeBegin = -1;
    private Enemy _enemy;

    public Projectile()
    {
        _element = Shape.CreateShape(out _shapeRef);
    }
    
    public bool TrySpawn(int tick, Enemy enemy, Col color, ShapeType shape)
    {
        if (_disabled)
        {
            _disabled = false;
            _spawnTick = tick;
            _fadeBegin = -1;
            _enemy = enemy;
            _element.BackgroundColor = Application.Colors[color];
            _shapeRef.Data = new ShapeInstance(shape);
            return true;
        }

        return false;
    }
    
    public void BeginFade()
    {
        _fadeBegin = -2;
    }
    
    public void Update(int tick)
    {
        if (_disabled)
        {
            return;
        }
        

        if (_fadeBegin == -2)
        {
            _fadeBegin = tick;
        }

        if (_fadeBegin == -1)
        {
            float lifetime = (tick - _spawnTick) / (float)Application.AnimationTime;
            Vector2 pos = Vector2.Lerp(Vector2.One * 0.5f, _enemy.Pos, MathF.Min(1, lifetime));
            _element.X = Position.Percentage(pos.X);
            _element.Y = Position.Percentage(pos.Y);
            _element.Width = Size.Percentage(float.Lerp(Player.PlayerSize, _enemy.Scale, MathF.Min(1, lifetime)));
            _element.Height = _element.Width;
            if (tick - _spawnTick >= Application.AnimationTime)
            {
                _enemy.BeginFade();
                Player.Score += 1;
                _fadeBegin = _spawnTick + (int)Application.AnimationTime;
            }

            return;
        }

        if (_fadeBegin > 0)
        {
            float fadeTime = (tick - _fadeBegin) / Application.AnimationTime;
            _element.BackgroundColor = _element.BackgroundColor with { W = float.Lerp(1, 0, fadeTime) };
            if (fadeTime > 1f)
            {
                _disabled = true;
                _element.Width = Size.Pixels(0);
            }
        }
    }
}
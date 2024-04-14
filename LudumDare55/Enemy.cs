using System.Diagnostics;
using System.Numerics;
using JAngine.Rendering.Gui;
using JAngine.Rendering.OpenGL;

namespace LudumDare55;

public sealed class Enemy : IUpdateable
{
    private static readonly Dictionary<(Col, ShapeType), List<Enemy>> _enemies = new Dictionary<(Col, ShapeType), List<Enemy>>();
    public static int AliveEnemies { get; private set; }

    public static IEnumerable<Enemy> GetEnemies(Col col, ShapeType type)
    {
        return GetEnemyList(col, type).ToList();
    }
    
    private static List<Enemy> GetEnemyList(Col col, ShapeType shapeType)
    {
        if (!_enemies.TryGetValue((col, shapeType), out List<Enemy>? enemies))
        {
            enemies = new List<Enemy>();
            _enemies.Add((col, shapeType), enemies);
        }

        return enemies;
    }
    
    public const float EnemyStartSize = 0.05f;
    
    private readonly GuiElement _element;
    private readonly BufferDataReference<ShapeInstance> _shapeRef;
    private Vector2 _position;
    private int _spawnTick;
    private bool _disabled = true;
    private int _aliveTicks;
    private Col _col;
    private ShapeType _type;
    private int _fadeBegin = -1;

    public Enemy()
    {
        _element = Shape.CreateShape(out _shapeRef);
        ObjectPool.OnPlayerDied += () =>
        {
            if (_disabled)
            {
                return;
            }

            BeginFade();
        };
    }
    
    public Vector2 Pos { get; private set; }
    public float Scale { get; private set; }

    public bool TrySpawn(int tick, int aliveTicks, Col col, ShapeType type)
    {
        if (_disabled)
        {
            _aliveTicks = aliveTicks;
            _spawnTick = tick;
            _col = col;
            _type = type;
            _disabled = false;
            _fadeBegin = -1;
            AliveEnemies++;
            
            _element.BackgroundColor = Application.Colors[col];
            _shapeRef.Data = new ShapeInstance(type);
            
            
            float angle = Random.Shared.NextSingle() * MathF.PI * 2;
            _position = (new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 1.2f + Vector2.One) * 0.5f;
            
            GetEnemyList(_col, _type).Add(this);
            
            return true;
        }

        return false;
    }

    public void BeginFade()
    {
        _fadeBegin = -2;
        GetEnemyList(_col, _type).Remove(this);
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
            float lifetime = (tick - _spawnTick) / (float)_aliveTicks;
            Pos = Vector2.Lerp(_position, Vector2.One * 0.5f, MathF.Min(1, lifetime));
            _element.X = Position.Percentage(Pos.X);
            _element.Y = Position.Percentage(Pos.Y);
            Scale = float.Lerp(EnemyStartSize, Player.PlayerSize, MathF.Min(1, lifetime));
            _element.Width = Size.Percentage(Scale);
            _element.Height = _element.Width;
            if (tick - _spawnTick >= _aliveTicks)
            {
                GetEnemyList(_col, _type).Remove(this);
                _fadeBegin = _spawnTick + _aliveTicks;
                ObjectPool.PlayerDied();
            }
        }

        if (_fadeBegin > 0)
        {
            float fadeTime = (tick - _fadeBegin) / 10f;
            _element.BackgroundColor = _element.BackgroundColor with { W = float.Lerp(1, 0, fadeTime) };
            if ((tick - _fadeBegin) > 10f)
            {
                _disabled = true;
                _element.Width = Size.Pixels(0);
                AliveEnemies -= 1;
            }
        }
    }
}
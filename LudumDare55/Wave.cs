namespace LudumDare55;

public interface IWave : IUpdateable
{
    event Action WaveDone;

    void Start(int tick);
    
    int WaveNumber { get; }
}

public class Wave : IWave
{
    public static int WaveCount { get; private set; }
    private const int DefaultTicksBetweenSpawn = 100;
    private const int Speed = 1600;
    
    private readonly ShapeType _maxShape;
    private readonly Col _maxCol;
    private readonly int _shapeCount;
    private readonly float _speedScaling;
    private int _nextSpawnTick = 0;
    private int _spawnedEnemies = 0;

    public Wave(Col maxCol, ShapeType maxShape, int shapeCount, float speedScaling)
    {
        _maxShape = maxShape;
        _maxCol = maxCol;
        _shapeCount = shapeCount;
        _speedScaling = speedScaling;
    }
    
    public Wave(int maxShape, int maxCol, int shapeCount, float speedScaling) : this((Col)maxCol, (ShapeType)maxShape, shapeCount, speedScaling)
    {
    }

    public event Action? WaveDone;

    public void Start(int tick)
    {
        _nextSpawnTick = tick;
        _spawnedEnemies = 0;
        WaveDone = null;
    }

    public int WaveNumber { get; } = WaveCount++;

    public void Update(int tick)
    {
        while (tick >= _nextSpawnTick)
        {
            if (++_spawnedEnemies > _shapeCount)
            {
                if (Enemy.AliveEnemies == 0)
                {
                    WaveDone?.Invoke();
                }
                return;
            }

            Spawn();
            _nextSpawnTick = (int)(_nextSpawnTick + DefaultTicksBetweenSpawn / _speedScaling);
        }
    }

    private void Spawn()
    {
        ShapeType type = (ShapeType)RandomBelow((int)_maxShape);
        Col col = (Col)RandomBelow((int)_maxCol);
        ObjectPool.SpawnEnemy((int)(Speed/_speedScaling), col, type);

        static int RandomBelow(int maxValue)
        {
            return Random.Shared.Next() % (maxValue + 1);
        }
    }
}

public class WaitWave : IWave
{
    private readonly int _ticks;
    private int _startTicks = 0;
    public int WaveNumber { get; } = Wave.WaveCount;
    
    public WaitWave(int ticks = 0) : base()
    {
        _ticks = ticks;
    }

    public void Update(int tick)
    {
        if (_startTicks + _ticks > tick)
        {
            WaveDone?.Invoke();
        }
    }

    public event Action? WaveDone;
    public void Start(int tick)
    {
        _startTicks = tick;
    }
}

public class TutorialWave : IWave
{
    private readonly string _message;
    private readonly Col _color;
    private readonly ShapeType _shape;
    private int _startTick;
    public int WaveNumber { get; } = Wave.WaveCount;

    public TutorialWave(string message, Col color, ShapeType shape)
    {
        _message = message;
        _color = color;
        _shape = shape;
    }
    
    public void Update(int tick)
    {
        if (tick - _startTick > 60)
        {
            ObjectPool.Pause = true;
        }
    }

    public event Action? WaveDone;
    public void Start(int tick)
    {
        Notification.StartNotification(_message);
        Player.OnShapeChange += PlayerOnShapeChange;
        ObjectPool.SpawnEnemy(100, _color, _shape);
        _startTick = tick;
    }

    private void PlayerOnShapeChange()
    {
        if (Player.Col == _color && Player.Type == _shape)
        {
            ObjectPool.Pause = false;
            Notification.StopNotification();
            Player.OnShapeChange -= PlayerOnShapeChange;
            WaveDone?.Invoke();
        }
    }
}
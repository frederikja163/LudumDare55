using System.Diagnostics;

namespace LudumDare55;

public static class ObjectPool
{
    public static int TickSpeed
    {
        get => _tickSpeed;
        set => _tickSpeed = value;
    }

    public static bool Pause
    {
        get
        {
            lock (Enemies)
            {
                return _pause;
            }
        }
        set
        {
            lock (Enemies)
            {
                _pause = value;
            }
        }
    }

    public static readonly int TicksPerSecond = 60;

    private static int _ticks = 0;
    private static readonly List<Enemy> Enemies = new List<Enemy>();
    private static readonly List<Projectile> Projectiles = new List<Projectile>();
    private static IWave? _wave;
    private static int _tickSpeed = 1;
    private static bool _pause;

    static ObjectPool()
    {
        new Thread(() =>
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            while (true)
            {
                lock (Enemies)
                {
                    _ticks += TickSpeed;
                    OnUpdate?.Invoke(_ticks);
                }

                do {
                    TimeSpan delay = TimeSpan.FromSeconds(1f / TicksPerSecond) - stopwatch.Elapsed;
                    if (delay > TimeSpan.Zero)
                    {
                        Thread.Sleep(delay);
                    }
                    stopwatch.Restart();
                }
                while (Pause) ;
            }
        }).Start();
    }

    public static event Action<int> OnUpdate;
    public static event Action OnPlayerDied;
    
    public static void PlayerDied()
    {
        OnPlayerDied?.Invoke();
        _wave = null;
    }

    public static void SpawnEnemy(int aliveTicks, Col color, ShapeType shape)
    {
        lock (Enemies)
        {
            if (!Enemies.Exists(e => e.TrySpawn(_ticks, aliveTicks, color, shape)))
            {
                Enemy enemy = new Enemy();
                if (!enemy.TrySpawn(_ticks, aliveTicks, color, shape))
                {
                    throw new Exception();
                }

                Enemies.Add(enemy);
            }
        }
    }

    public static void SpawnProjectile(Enemy enemy, Col color, ShapeType shape)
    {
        lock (Enemies)
        {
            if (!Projectiles.Exists(e => e.TrySpawn(_ticks, enemy, color, shape)))
            {
                Projectile projectile = new Projectile();
                if (!projectile.TrySpawn(_ticks, enemy, color, shape))
                {
                    throw new Exception();
                }

                Projectiles.Add(projectile);
            }
        }
    }

    public static void SetWave(IWave wave)
    {
        lock (Enemies)
        {
            wave.Start(_ticks);
            _wave = wave;
        }
    }
}
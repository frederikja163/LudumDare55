using System.Numerics;
using JAngine;
using JAngine.Rendering;
using JAngine.Rendering.Gui;
using JAngine.Rendering.OpenGL;

namespace LudumDare55;

public enum Col {
    White = 0,
    Red = 1,
    Blue = 2,
    Green = 3,
    Yellow = 4,
    Cyan = 5,
    Purple = 6,
}
public sealed class Application
{
    public static IReadOnlyDictionary<Col, Vector4> Colors { get; }= new Dictionary<Col, Vector4>()
    {
        { Col.White, new Vector4(1, 1, 1, 1) },
        { Col.Red, new Vector4(1, 0, 0, 1) },
        { Col.Blue, new Vector4(0, 0, 1, 1) },
        { Col.Green, new Vector4(0, 1, 0, 1) },
        { Col.Yellow, new Vector4(1, 1, 0, 1) },
        { Col.Cyan, new Vector4(0, 1, 1, 1) },
        { Col.Purple, new Vector4(0.8f, 0, 1f, 1) },
    };

    public const float AnimationTime = 10;
    public static GuiElement Background { get; }
    public static Window Window { get; }

    static Application()
    {
        
        Window = new Window("Shape summoner", 1280, 720);
        
        Background = new GuiElement(Window)
        {
            BackgroundColor = new Vector4(0.1f, 0.1f, 0.1f, 1f),
        };
        Window.OnWindowResize += (_, width, height) =>
        {
            float size = MathF.Max(width, height);
            Background.Width = Size.Pixels(size);
            Background.Height = Size.Pixels(size);
        };
    }
    
    private readonly Player _player;
    private readonly List<IWave> _waves = [
        new TutorialWave("Write circle to kill the circle summon", Col.White, ShapeType.Circle),
        new Wave(Col.White,  ShapeType.Circle, 3, 1f),
        new TutorialWave("Write square to change into a square and shoot squares", Col.White, ShapeType.Square),
        new Wave(Col.White, ShapeType.Square, 5, 1f),
        new TutorialWave("You can also change color, try red", Col.Red, ShapeType.Square),
        new Wave(Col.Red, ShapeType.Square, 10, 1f),
        new TutorialWave("Blue triangle is up next", Col.Blue, ShapeType.Triangle),
        new Wave(Col.Blue, ShapeType.Triangle, 15, 1f),
        new TutorialWave("Every 5th wave will be easier but faster", Col.Red, ShapeType.Square),
        new Wave(Col.Red, ShapeType.Square, 20, 2f),
        
        new TutorialWave("Green star - You can press space to speed up", Col.Green, ShapeType.Star),
        new Wave(Col.Green, ShapeType.Star, 20, 1.25f),
        new TutorialWave("Yellow heart - Press escape or enter to clear", Col.Yellow, ShapeType.Heart),
        new Wave(Col.Yellow, ShapeType.Heart, 20, 1.25f),
        new TutorialWave("Cyan hexagon", Col.Cyan, ShapeType.Hexagon),
        new Wave(Col.Cyan, ShapeType.Hexagon, 20, 1.25f),
        new TutorialWave("Purple cross - You almost made it to wave 10!", Col.Purple, ShapeType.Cross),
        new Wave(Col.Purple, ShapeType.Cross, 20, 1.25f),
        new TutorialWave("Now we are gonna go fast", Col.Red, ShapeType.Square),
        new Wave(Col.Red, ShapeType.Square, 30, 2.5f),
        
        new TutorialWave("No more colors or shapes left", Col.Purple, ShapeType.Cross),
        new Wave(Col.Purple, ShapeType.Cross, 1, 1.5f),
    ];

    public Application()
    {
        _player = new Player();

        bool waitingForSpace = true;
        Window.AddKeyBinding(Key.Space | Key.Press, () =>
        {
            lock (this)
            {
                waitingForSpace = false;
            }
        });

        ObjectPool.OnPlayerDied += () =>
        {
            Task.Run(async () =>
            {
                lock (this)
                {
                    waitingForSpace = true;
                }
                bool localCopy;
                do
                {
                    Notification.StartNotification("You died! Press space to try again.");
                    await Task.Delay(1000);
                    Notification.StopNotification();
                    await Task.Delay(1000);
                    lock (this)
                    {
                        localCopy = waitingForSpace;
                    }
                } while (localCopy);

                StartWave(0);
            });
        };

        StartWave(0);
    }

    private void StartWave(int i)
    {
        while (i >= _waves.Count)
        {
            _waves.Add(new WaitWave(180));
            if (i % 10 != 0)
            {
                _waves.Add(new Wave(Col.Purple, ShapeType.Cross, (int)MathF.Floor((i + 1f) / 10) * 10 + 10,
                    (int)(i / 10f) * 0.25f + 1));
            }
            else
            {
                _waves.Add(new Wave(Col.Blue, ShapeType.Triangle, (int)MathF.Floor((i + 1f) / 10) * 10 + 10, (int)(i / 10f) * 0.5f + 2));
            }
        }
        ObjectPool.SetWave(_waves[i]);
        _waves[i].WaveDone += StartWavePlusOne;

        void StartWavePlusOne()
        {
            StartWave(i + 1);
        }
    }

    public void Run()
    {
        Window.Run();
    }
}
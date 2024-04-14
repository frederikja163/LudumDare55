using JAngine;
using JAngine.Rendering;
using JAngine.Rendering.Gui;
using JAngine.Rendering.OpenGL;

namespace LudumDare55;

public enum ShapeType
{
    Circle = 0,
    Square = 1,
    Triangle = 2,
    Star = 3,
    Heart = 4,
    Hexagon = 5,
    Cross = 6,
    Pentagon = 7,
    Octagon = 8,
}

public readonly struct ShapeInstance
{
    [ShaderAttribute("vShapeType1")]
    public readonly int ShapeType1;
    [ShaderAttribute("vShapeType2")]
    public readonly int ShapeType2;
    [ShaderAttribute("vShapeLerp")]
    public readonly float ShapeLerp;

    public ShapeInstance(ShapeType shapeType1, ShapeType shapeType2, float lerp)
    {
        ShapeType1 = (int)shapeType1;
        ShapeType2 = (int)shapeType2;
        ShapeLerp = lerp;
    }
    
    public ShapeInstance(ShapeType shapeType1)
    {
        ShapeType1 = (int)shapeType1;
        ShapeType2 = (int)shapeType1;
        ShapeLerp = 0f;
    }
}

public sealed class Shape
{
    
    private static readonly Shader? _shader;

    static Shape()
    {
        VertexShader vert = Resource.Load<VertexShader>(Application.Window, "LudumDare55.Shaders.Shape.vert");
        FragmentShader frag = Resource.Load<FragmentShader>(Application.Window, "LudumDare55.Shaders.Shape.frag");
        _shader = new Shader(Application.Window, "Shape", vert, frag);
        vert.Dispose();
        frag.Dispose();
    }

    public static GuiElement CreateShape(out BufferDataReference<ShapeInstance> shapeRef)
    {
        GuiElement element = new GuiElement(Application.Background, null, _shader)
        {
            Width = Size.Pixels(0),
        };
        shapeRef = element.AddExtraAttributes(new ShapeInstance(ShapeType.Circle));
        
        return element;
    }
}
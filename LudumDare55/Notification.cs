using System.Numerics;
using JAngine.Rendering.Gui;

namespace LudumDare55;

public static class Notification
{
    private static readonly Text _text;
    private static readonly GuiElement _textField;

    static Notification()
    {
        _textField = new GuiElement(Application.Background)
        {
            BackgroundColor = new Vector4(0, 0, 0, 0),
            Height = Size.Pixels(20),
            Y = Position.Percentage(0.75f),
        };
        _text = new Text(_textField, "");
    }

    public static void StartNotification(string message)
    {
        _text.Value = message;
    }

    public static void StopNotification()
    {
        _text.Value = "";
    }
}
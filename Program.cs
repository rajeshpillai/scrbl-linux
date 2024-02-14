using Gtk;
using Cairo;
using System;
using System.Collections.Generic;

public class DrawingApp
{
    static void Main()
    {
        Application.Init();
        new DrawingWindow();
        Application.Run();
    }
}

public class DrawingWindow : Window
{
    private DrawingArea drawingArea;
    private List<Tuple<double, double>> points = new List<Tuple<double, double>>();
    private DrawMode currentMode = DrawMode.Pen;

    public DrawingWindow() : base("Drawing Tools Example")
    {
        SetDefaultSize(800, 600);
        SetPosition(WindowPosition.Center);
        DeleteEvent += delegate { Application.Quit(); };

        Box vbox = new Box(Orientation.Vertical, 0); // Updated to use Box with Orientation
        Box hbox = new Box(Orientation.Horizontal, 0); // Updated to use Box with Orientation

        Button penButton = new Button("Pen");
        Button lineButton = new Button("Line");
        Button freehandButton = new Button("Freehand");

        penButton.Clicked += (sender, e) => currentMode = DrawMode.Pen;
        lineButton.Clicked += (sender, e) => currentMode = DrawMode.Line;
        freehandButton.Clicked += (sender, e) => currentMode = DrawMode.Freehand;

        // Updated PackStart calls with required arguments
        hbox.PackStart(penButton, expand: false, fill: false, padding: 0);
        hbox.PackStart(lineButton, expand: false, fill: false, padding: 0);
        hbox.PackStart(freehandButton, expand: false, fill: false, padding: 0);

        vbox.PackStart(hbox, expand: false, fill: false, padding: 0);

        drawingArea = new DrawingArea();
        drawingArea.AddEvents((int)Gdk.EventMask.ButtonPressMask | (int)Gdk.EventMask.ButtonReleaseMask | (int)Gdk.EventMask.PointerMotionMask);
        drawingArea.Drawn += OnDrawingAreaDrawn;
        drawingArea.ButtonPressEvent += OnButtonPressEvent;
        drawingArea.ButtonReleaseEvent += OnButtonReleaseEvent;
        drawingArea.MotionNotifyEvent += OnMotionNotifyEvent;

        vbox.PackStart(drawingArea, expand: true, fill: true, padding: 0);

        Add(vbox);
        ShowAll();
    }

    private void OnDrawingAreaDrawn(object o, DrawnArgs args)
    {
        Context cr = args.Cr;

        if (currentMode == DrawMode.Line && points.Count == 2)
        {
            cr.MoveTo(points[0].Item1, points[0].Item2);
            cr.LineTo(points[1].Item1, points[1].Item2);
            cr.Stroke();
        }
        else if (currentMode == DrawMode.Freehand || currentMode == DrawMode.Pen)
        {
            if (points.Count > 1)
            {
                cr.MoveTo(points[0].Item1, points[0].Item2);
                for (int i = 1; i < points.Count; i++)
                {
                    cr.LineTo(points[i].Item1, points[i].Item2);
                }
                cr.Stroke();
            }
        }
    }

    private void OnButtonPressEvent(object o, ButtonPressEventArgs args)
    {
        points.Clear();
        points.Add(Tuple.Create(args.Event.X, args.Event.Y));
        if (currentMode != DrawMode.Freehand)
        {
            // For non-freehand modes, draw immediately on button press to show feedback for single clicks or starting points
            drawingArea.QueueDraw();
        }
    }

    private void OnButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
    {
        if (currentMode == DrawMode.Line)
        {
            points.Add(Tuple.Create(args.Event.X, args.Event.Y));
            drawingArea.QueueDraw();
        }
    }

    private void OnMotionNotifyEvent(object o, MotionNotifyEventArgs args)
    {
        if (currentMode == DrawMode.Freehand && args.Event.State.HasFlag(Gdk.ModifierType.Button1Mask))
        {
            points.Add(Tuple.Create(args.Event.X, args.Event.Y));
            drawingArea.QueueDraw();
        }
    }

    private enum DrawMode
    {
        Pen,
        Line,
        Freehand
    }
}


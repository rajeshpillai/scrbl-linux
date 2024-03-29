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
    private List<Drawable> drawings = new List<Drawable>();
    private Drawable currentDrawing;
    private DrawMode currentMode = DrawMode.Pen;

    private PointD? startPoint = null; // Track the start point for line drawing.

    public DrawingWindow() : base("Drawing Tools Example")
    {
        SetDefaultSize(800, 600);
        SetPosition(WindowPosition.Center);
        DeleteEvent += delegate { Application.Quit(); };

        Box vbox = new Box(Orientation.Vertical, 0);
        Box hbox = new Box(Orientation.Horizontal, 0);

        Button penButton = new Button("Pen");
        Button lineButton = new Button("Line");
        Button freehandButton = new Button("Freehand");

        penButton.Clicked += (sender, e) => currentMode = DrawMode.Pen;
        lineButton.Clicked += (sender, e) => currentMode = DrawMode.Line;
        freehandButton.Clicked += (sender, e) => currentMode = DrawMode.Freehand;

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

        foreach (var drawing in drawings)
        {
            drawing.Draw(cr);
        }
    }

    private void OnButtonPressEvent(object o, ButtonPressEventArgs args)
    {
       startPoint = new PointD(args.Event.X, args.Event.Y); // Set start point on button press.
       if (currentMode == DrawMode.Line)
       {
          currentDrawing = new Drawable(DrawMode.Line);
          currentDrawing.Points.Add(startPoint.Value);
          drawings.Add(currentDrawing);
      } 
      else
      {
        // Handle other modes as before
        currentDrawing = new Drawable(currentMode);
        currentDrawing.Points.Add(new PointD(args.Event.X, args.Event.Y));
        drawings.Add(currentDrawing);
      }
    }

    private void OnButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
    {
      if (currentMode == DrawMode.Line && startPoint.HasValue)
      {
        // On releasing the button, finalize the line with the end point.
        currentDrawing.Points.Add(new PointD(args.Event.X, args.Event.Y));
        drawingArea.QueueDraw();
        startPoint = null; // Reset the start point.
      }
    }

    private void OnMotionNotifyEvent(object o, MotionNotifyEventArgs args)
    {
      if (currentMode == DrawMode.Freehand || currentMode == DrawMode.Pen)
      {
        if (args.Event.State.HasFlag(Gdk.ModifierType.Button1Mask))
        {
            currentDrawing.Points.Add(new PointD(args.Event.X, args.Event.Y));
            drawingArea.QueueDraw();
        }
      }
      else if (currentMode == DrawMode.Line && startPoint.HasValue)
      {
        // Update the temporary line drawing
        if (currentDrawing.Points.Count > 1)
        {
            // Replace the temporary end point with the new position
            currentDrawing.Points[1] = new PointD(args.Event.X, args.Event.Y);
        }
        else
        {
            // Add the temporary end point for the first time
            currentDrawing.Points.Add(new PointD(args.Event.X, args.Event.Y));
        }
        drawingArea.QueueDraw();
      }
    }
    
    private enum DrawMode
    {
        Pen,
        Line,
        Freehand
    }

    private class Drawable
    {
        public List<PointD> Points { get; private set; } = new List<PointD>();
        public DrawMode Mode { get; private set; }

        public Drawable(DrawMode mode)
        {
            Mode = mode;
        }

        public void Draw(Context cr)
        {
            if (Points.Count < 1) return;

            switch (Mode)
            {
                case DrawMode.Pen:
                case DrawMode.Freehand:
                    cr.MoveTo(Points[0]);
                    foreach (var point in Points)
                    {
                        cr.LineTo(point);
                    }
                    break;
                case DrawMode.Line:
                    if (Points.Count >= 2)
                    {
                        cr.MoveTo(Points[0]);
                        cr.LineTo(Points[1]);
                    }
                    break;
            }
            cr.Stroke();
        }
    }
}


using Gtk;
using System;
using Cairo;

class Program
{
    static void Main()
    {
        Application.Init();
        MainWindow win = new MainWindow();
        win.Show();
        Application.Run();
    }
}

class MainWindow : Window
{
    public MainWindow() : base("Drawing Example")
    {
        SetDefaultSize(400, 300);
        SetPosition(WindowPosition.Center);
        DeleteEvent += delegate { Application.Quit(); };

        DrawingArea drawingArea = new DrawingArea();
        drawingArea.Drawn += OnDrawingAreaDrawn;
        Add(drawingArea);

        ShowAll();
    }

    private void OnDrawingAreaDrawn(object o, DrawnArgs args)
    {
        Context cr = args.Cr;

        cr.LineWidth = 9;
        cr.SetSourceRGB(0.7, 0.2, 0.0);

        int width = Allocation.Width;
        int height = Allocation.Height;

        cr.Translate(width / 2, height / 2);
        cr.Arc(0, 0, 50, 0, 2 * Math.PI);
        cr.StrokePreserve();

        cr.SetSourceRGB(0.3, 0.4, 0.6);
        cr.Fill();
    }
}


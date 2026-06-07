using System.Windows.Media;
using System.Windows.Threading;
using UserControl = System.Windows.Controls.UserControl;

namespace ICare.Views;

public partial class OverviewView : UserControl
{
    private Config _config;
    private Timer _timer;
    private DispatcherTimer _uiTimer;

    public OverviewView(Config config, Timer timer)
    {
        _config = config;
        _timer = timer;
        InitializeComponent();

        WorkLabel.Text = $"{_config.WorkSec / 60}";
        BreakLabel.Text = $"{_config.BreakSec}";

        _uiTimer = new DispatcherTimer();
        _uiTimer.Interval = TimeSpan.FromMilliseconds(100);
        _uiTimer.Tick += (s, e) => UpdateUI();
        _uiTimer.Start();
    }

    private void UpdateUI()
    {
        var remaining = _timer.BreakDate - DateTime.Now;
        if (remaining.TotalSeconds < 0) remaining = TimeSpan.Zero;

        CountdownLabel.Text = $"{(int)remaining.TotalMinutes}:{remaining.Seconds:D2}";
        SkipStatusLabel.Text = _timer.SkipNext ? "Skipped" : "Scheduled";
    
        WorkLabel.Text = $"{_config.WorkSec / 60}";
        BreakLabel.Text = $"{_config.BreakSec}";

        double progress = remaining.TotalSeconds / _config.WorkSec;
        progress = Math.Clamp(progress, 0, 1);
        DrawArc(progress);
    }
    
    private void DrawArc(double progress)
    {
        double radius = 82;
        double cx = 90, cy = 90;

        if (progress >= 0.999)
        {
            TimerArc.Data = new EllipseGeometry(new System.Windows.Point(cx, cy), radius, radius);
            return;
        }
        
        double angle = progress * 360 - 90;
        double radians = angle * Math.PI / 180;

        double x = cx + radius * Math.Cos(radians);
        double y = cy + radius * Math.Sin(radians);

        bool isLargeArc = progress > 0.5;

        var figure = new PathFigure
        {
            StartPoint = new System.Windows.Point(cx, cy - radius),
            IsClosed = false
        };

        figure.Segments.Add(new ArcSegment
        {
            Point = new System.Windows.Point(x, y),
            Size = new System.Windows.Size(radius, radius),
            IsLargeArc = isLargeArc,
            SweepDirection = SweepDirection.Clockwise
        });

        TimerArc.Data = new PathGeometry(new[] { figure });
    }
}
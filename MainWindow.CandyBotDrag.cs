using System.Windows;
using System.Windows.Input;

namespace DJBookingSystem
{
    public partial class MainWindow : Window
    {
        private bool _isDraggingCandyBot = false;
        private Point _candyBotDragStartPoint;
        private const double CLICK_THRESHOLD = 5.0; // Pixels - movement less than this is considered a click

        // CandyBot Drag Start
        private void CandyBot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDraggingCandyBot = true;
            _candyBotDragStartPoint = e.GetPosition(this);
            CandyBotFloatingAvatar.CaptureMouse();
            e.Handled = true;
        }

        // CandyBot Drag Move
        private void CandyBot_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingCandyBot && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(this);
                double deltaX = currentPosition.X - _candyBotDragStartPoint.X;
                double deltaY = currentPosition.Y - _candyBotDragStartPoint.Y;

                // Calculate new margin
                var currentMargin = CandyBotFloatingAvatar.Margin;
                double newLeft = currentMargin.Left + deltaX;
                double newTop = currentMargin.Top + deltaY;

                // Allow movement ANYWHERE in the window - from title bar to bottom
                double minLeft = -10; // Allow slight overflow
                double minTop = -40; // Allow to go up to title bar
                double maxLeft = this.ActualWidth - CandyBotFloatingAvatar.ActualWidth + 10;
                double maxTop = this.ActualHeight - CandyBotFloatingAvatar.ActualHeight + 10;

                newLeft = System.Math.Max(minLeft, System.Math.Min(newLeft, maxLeft));
                newTop = System.Math.Max(minTop, System.Math.Min(newTop, maxTop));

                CandyBotFloatingAvatar.Margin = new Thickness(newLeft, newTop, 0, 0);
                CandyBotFloatingAvatar.HorizontalAlignment = HorizontalAlignment.Left;
                CandyBotFloatingAvatar.VerticalAlignment = VerticalAlignment.Top;

                _candyBotDragStartPoint = currentPosition;
                
                e.Handled = true;
            }
        }

        // CandyBot Drag End
        private void CandyBot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDraggingCandyBot)
            {
                _isDraggingCandyBot = false;
                CandyBotFloatingAvatar.ReleaseMouseCapture();
                
                // Calculate total drag distance
                Point endPoint = e.GetPosition(this);
                double dragDistance = System.Math.Sqrt(
                    System.Math.Pow(endPoint.X - _candyBotDragStartPoint.X, 2) +
                    System.Math.Pow(endPoint.Y - _candyBotDragStartPoint.Y, 2)
                );

                // Only treat as click if drag distance is very small
                if (dragDistance < CLICK_THRESHOLD)
                {
                    // Right-click menu will handle interactions now
                    // No action on left-click
                    System.Diagnostics.Debug.WriteLine("[CandyBot] Click detected - use right-click menu for actions");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[CandyBot] Dragged {dragDistance:F2} pixels to new position: ({CandyBotFloatingAvatar.Margin.Left:F0}, {CandyBotFloatingAvatar.Margin.Top:F0})");
                }
                
                e.Handled = true;
            }
        }
    }
}

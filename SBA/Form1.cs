using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace SBA;

public partial class SBA : Form
{
    private List<Solution> ships;
    private int quaySize;

    public SBA(List<Solution> ships, int quaySize)
    {
        InitializeComponent();
        this.BackColor = Color.White;
        this.ships = ships;
        this.quaySize = quaySize;
        this.Load += SBA_Load;
        this.Paint += SBA_Paint;
    }

    private void SBA_Paint(object sender, PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;
        Pen axisPen = new Pen(Color.Black, 2);
        Font labelFont = new Font("Arial", 10);
        StringFormat labelFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        if (ships.Count == 0)
        {
            graphics.DrawString(
                "No ships to display",
                labelFont,
                Brushes.Black,
                ClientSize.Width / 2,
                ClientSize.Height / 2,
                labelFormat
            );
            return;
        }

        int maxPosition = quaySize; // Maximum position on X axis
        int maxTime = ships.Max(s => s.departureTime); // Maximum time on Y axis

        // Draw x-axis
        int margin = 50;
        // Calculate the length of the lines
        int xAxisLength = ClientSize.Width - 2 * margin;
        int yAxisLength = ClientSize.Height - 2 * margin;
        float[] xPoints = new float[quaySize + 1];
        float[] yPoints = new float[maxTime + 1];

        // Draw x-axis
        graphics.DrawLine(
            axisPen,
            margin,
            ClientSize.Height - margin,
            margin + xAxisLength,
            ClientSize.Height - margin
        );

        // Draw y-axis
        graphics.DrawLine(axisPen, margin, margin, margin, margin + yAxisLength);

        int cellSize = xAxisLength / maxPosition;
        // Set the size of each cell in the grid
        //int cellSize = quaySize * ships.Count;

        // Draw coordinate numbers along x-axis
        for (int i = 0; i <= quaySize; i++)
        {
            float x = ((margin * 0.83f) + i * cellSize);
            xPoints[i] = x + 7f;
            graphics.DrawString(i.ToString(), Font, Brushes.Black, x, ClientSize.Height - margin);
            if (i != 0)
            {
                graphics.DrawLine(
                    new Pen(Brushes.DarkRed, 5),
                    x + 7f,
                    (float)ClientSize.Height - margin,
                    x + 7f,
                    ((float)ClientSize.Height - margin) - 1.2f * quaySize
                );
            }
        }

        cellSize = yAxisLength / maxTime;
        // Draw coordinate numbers along y-axis
        for (int i = 0; i <= maxTime; i++)
        {
            //float y = ClientSize.Height - (margin + 0.83f * cellSize);
            if (i == 0)
            {
                yPoints[i] = ClientSize.Height - margin;
            }
            else
            {
                float y = ClientSize.Height - ((margin * 0.83f) + i * cellSize);
                graphics.DrawString(i.ToString(), Font, Brushes.Black, margin - 20, y);
                graphics.DrawLine(
                    new Pen(Brushes.DarkRed, 5),
                    margin,
                    y + 7f,
                    (float)margin + 10,
                    y + 7f
                );
                yPoints[i] = y + 7f;
            }
        }

        foreach (Solution ship in ships)
        {
            // Draw ship
            float x = xPoints[ship.startPosition];
            float y = yPoints[ship.departureTime];
            float width = xPoints[ship.endPosition] - xPoints[ship.startPosition];
            float height = (yPoints[ship.departureTime] - yPoints[ship.arrivalTime]) * -1;
            //graphics.FillRectangle(new SolidBrush(Color.Beige), x, y, width, height);
            DrawRoundedRectangle(graphics, axisPen, Brushes.Beige, x, y, width, height, 30);

            // Draw ship label
            graphics.DrawString(
                ship.ID.ToString(),
                labelFont,
                Brushes.Black,
                x + width / 2,
                y + height / 2,
                labelFormat
            );
        }
    }

    private void SBA_Load(object sender, EventArgs e)
    {
        this.Invalidate();
    }

    private void DrawRoundedRectangle(
        Graphics graphics,
        Pen borderPen,
        Brush fillBrush,
        float x,
        float y,
        float width,
        float height,
        int cornerRadius
    )
    {
        // Create a path with rounded corners
        GraphicsPath path = new GraphicsPath();
        path.AddArc(x, y, cornerRadius, cornerRadius, 180, 90); // Top-left corner
        path.AddArc(x + width - cornerRadius, y, cornerRadius, cornerRadius, 270, 90); // Top-right corner
        path.AddArc(
            x + width - cornerRadius,
            y + height - cornerRadius,
            cornerRadius,
            cornerRadius,
            0,
            90
        ); // Bottom-right corner
        path.AddArc(x, y + height - cornerRadius, cornerRadius, cornerRadius, 90, 90); // Bottom-left corner
        path.CloseFigure();

        // Fill the rounded rectangle
        graphics.FillPath(fillBrush, path);

        // Draw the rounded rectangle borders
        graphics.DrawPath(borderPen, path);
    }
}

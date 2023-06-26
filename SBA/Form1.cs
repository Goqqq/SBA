using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

        int maxPosition = quaySize; // Maximum position on X axis
        int maxTime = ships.Max(s => s.departureTime); // Maximum time on Y axis

        //for (int i = 0; i < numOfCells; i++)
        //{
        //    // Vertical
        //    graphics.DrawLine(axisPen, i * cellSize, 0, i * cellSize, numOfCells * cellSize);
        //    // Horizontal
        //    graphics.DrawLine(axisPen, 0, i * cellSize, numOfCells * cellSize, i * cellSize);
        //}

        // Draw x-axis
        int margin = 50;
        // Calculate the length of the lines
        int xAxisLength = ClientSize.Width - 2 * margin;
        int yAxisLength = ClientSize.Height - 2 * margin;
        float[] xPoints = new float[quaySize + 1];

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
            xPoints[i] = x;
            if (i % 5 == 0)
            {
                graphics.DrawString(
                    i.ToString(),
                    Font,
                    Brushes.Black,
                    x,
                    ClientSize.Height - margin
                );
            }
            if (i != 0)
            {
                graphics.DrawLine(
                    axisPen,
                    x + 7f,
                    (float)ClientSize.Height - margin,
                    x + 7f,
                    ((float)ClientSize.Height - margin) - 1.2f * quaySize
                );
            }
        }

        // Draw coordinate numbers along y-axis
        for (int i = 0; i <= maxTime; i++)
        {
            if (i != 0)
            {
                float y = ((margin * 0.83f) + i * cellSize);
                //float y = ClientSize.Height - (margin +  0.83f * cellSize);
                if (i % 5 == 0)
                {
                    graphics.DrawString(
                        i.ToString(),
                        Font,
                        Brushes.Black,
                        ClientSize.Height - margin,
                        y
                    );
                }
                graphics.DrawLine(
                    axisPen,
                    y + 7f,
                    (float)ClientSize.Width - margin,
                    y + 7f,
                    ((float)ClientSize.Width - margin) - 1.2f * quaySize
                );
            }
        }
    }
    //int gridSize = 10; // Number of divisions in the grid
    //      int margin = 20; // Margin around the grid

    //      // Calculate the size of the grid
    //      int gridWidth = this.ClientSize.Width - margin;
    //      int gridHeight = this.ClientSize.Height - margin;

    //      // Calculate the size of each grid cell
    //      int cellWidth = gridWidth / gridSize;
    //      int cellHeight = gridHeight / gridSize;

    //      // Draw X and Y axes lines
    //      graphics.DrawLine(axisPen, margin, margin, margin, gridHeight + margin); // Y axis line
    //      graphics.DrawLine(
    //          axisPen,
    //          margin,
    //          gridHeight + margin,
    //          gridWidth + margin,
    //          gridHeight + margin
    //      ); // X axis line

    //      // Draw X-axis labels
    //      for (int i = 0; i < gridSize; i++)
    //      {
    //          int labelX = margin + (i) * cellWidth;
    //          string label = (i + 1).ToString();
    //          graphics.DrawString(
    //              label,
    //              labelFont,
    //              Brushes.Black,
    //              labelX,
    //              gridHeight + margin,
    //              labelFormat
    //          );
    //      }

    //      // Draw Y-axis labels
    //      for (int i = 0; i < gridSize; i++)
    //      {
    //          int labelY = gridHeight + margin - (i) * cellHeight;
    //          string label = (i + 1).ToString();
    //          graphics.DrawString(label, labelFont, Brushes.Black, margin, labelY, labelFormat);
    //      }

    //      foreach (Solution ship in ships)
    //      {
    //          int id = ship.ID; // ID of the ship
    //          int startPosition = ship.startPosition; // Position on X axis
    //          int endPosition = ship.endPosition; // Position on X axis
    //          int arrivalTime = ship.arrivalTime; // Time on Y axis
    //          int departureTime = ship.departureTime; // Time on Y axis

    //          // Calculate the coordinates and size of the ship's rectangle
    //          int x = 20 * startPosition; // Multiply by a scale factor to adjust the position on X axis
    //          int y = 20 * arrivalTime; // Multiply by a scale factor to adjust the position on Y axis
    //          int width = endPosition - startPosition; // Width of the ship's rectangle
    //          int height = departureTime - arrivalTime; // Height of the ship's rectangle

    //          Rectangle shipRect = new Rectangle(x, y, width, height);

    //          // Draw the ship's rectangle
    //          using SolidBrush brush = new SolidBrush(Color.Blue);
    //          graphics.FillRectangle(brush, shipRect);
    //}
}

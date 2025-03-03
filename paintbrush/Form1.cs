using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace paintbrush
{
    public partial class Form1 : Form
    {
        private bool isDrawing = false;
        private Point lastPoint;
        private Color currentColor = Color.Black;
        private int brushSize = 1;
        private Bitmap mainBitmap;
        private Graphics graphics;
        private string currentTool = "brush";
        private Stack<Bitmap> undoStack = new Stack<Bitmap>();
        private Stack<Bitmap> redoStack = new Stack<Bitmap>();
        private Point startPoint;

        public Form1()
        {
            InitializeComponent();
            mainBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            graphics = Graphics.FromImage(mainBitmap);
            graphics.Clear(Color.White);
            pictureBox1.Image = mainBitmap;

            // Subscribe to mouse events
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseUp += PictureBox1_MouseUp;


            // Connect button click events with their handlers
            clearbutton.Click += clearbutton_Click;
            colorbutton.Click += colorbutton_Click;
            undobutton.Click += undobutton_Click;
            redobutton.Click += redobutton_Click;
            eraserbutton.Click += eraserbutton_Click;
            rectanglebutton4.Click += rectanglebutton4_Click;  // Fix the name but keep the handler
            circlebutton.Click += circlebutton_Click;
            linebutton.Click += linebutton_Click;
            numericUpDown1.ValueChanged += numericUpDown1_ValueChanged;
            pencilbutton.Click += pencilbutton_Click;
            brushbutton.Click += brushbutton_Click;
            spraybutton.Click += spraybutton_Click;
            squarebutton.Click += squarebutton_Click;
            fillbutton.Click += fillbutton_Click;


            // Set initial numeric up down values
            numericUpDown1.Minimum = 1;
            numericUpDown1.Maximum = 50;
            numericUpDown1.Value = 1;
        }

        // Rest of the code remains the same - just rename rectanglebutton5_Click to match the new name
        private void rectanglebutton4_Click(object sender, EventArgs e)
        {
            currentTool = "rectangle";
        }

        // All other methods remain exactly the same as in your code
        private void SaveState()
        {
            Bitmap bmp = new Bitmap(mainBitmap);
            undoStack.Push(bmp);
            redoStack.Clear();
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (currentTool == "fill")
            {
                SaveState();
                FillArea(e.Location, currentColor);
                pictureBox1.Refresh();
                return;
            }
            isDrawing = true;
            lastPoint = e.Location;
            startPoint = e.Location;
            SaveState();
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDrawing) return;

            switch (currentTool)
            {
                
                case "eraser":
                    using (var pen = new Pen(currentTool == "brush" ? currentColor : Color.White, brushSize))
                    {
                        graphics.DrawLine(pen, lastPoint, e.Location);
                    }
                    lastPoint = e.Location;
                    break;
                case "brush":
                    using (var pen = new Pen(currentColor, brushSize))
                    {
                        graphics.DrawLine(pen, lastPoint, e.Location);
                    }
                    lastPoint = e.Location;
                    break;

                case "pencil":
                    using (var thinPen = new Pen(currentColor, 1)) // Thin stroke for pencil
                    {
                        graphics.DrawLine(thinPen, lastPoint, e.Location);
                    }
                    lastPoint = e.Location;
                    break;

                case "spray":
                    Random rnd = new Random();
                    for (int i = 0; i < 20; i++) // Create spray effect
                    {
                        int dx = rnd.Next(-brushSize, brushSize);
                        int dy = rnd.Next(-brushSize, brushSize);
                        graphics.FillRectangle(new SolidBrush(currentColor), e.X + dx, e.Y + dy, 1, 1);
                    }
                    break;
                
            }
            pictureBox1.Refresh();
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isDrawing) return;

            switch (currentTool)
            {
                case "rectangle":
                    using (var pen = new Pen(currentColor, brushSize))
                    {
                        var rect = GetRectangle(startPoint, e.Location);
                        graphics.DrawRectangle(pen, rect);
                    }
                    break;

                case "circle":
                    using (var pen = new Pen(currentColor, brushSize))
                    {
                        var rect = GetRectangle(startPoint, e.Location);
                        graphics.DrawEllipse(pen, rect);
                    }
                    break;

                case "line":
                    using (var pen = new Pen(currentColor, brushSize))
                    {
                        graphics.DrawLine(pen, startPoint, e.Location);
                    }
                    break;
                case "square":
                    using (var pen = new Pen(currentColor, brushSize))
                    {
                        int size = Math.Max(Math.Abs(startPoint.X - e.Location.X), Math.Abs(startPoint.Y - e.Location.Y));
                        var rect = new Rectangle(startPoint.X, startPoint.Y, size, size);
                        graphics.DrawRectangle(pen, rect);
                    }
                    break;
            }

            isDrawing = false;
            pictureBox1.Refresh();
        }

        private Rectangle GetRectangle(Point start, Point end)
        {
            return new Rectangle(
                Math.Min(start.X, end.X),
                Math.Min(start.Y, end.Y),
                Math.Abs(start.X - end.X),
                Math.Abs(start.Y - end.Y));
        }

        private void clearbutton_Click(object sender, EventArgs e)
        {
            SaveState();
            graphics.Clear(Color.White);
            pictureBox1.Refresh();
        }

        private void colorbutton_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                currentColor = colorDialog.Color;
                currentTool = "brush";
            }
        }

        private void undobutton_Click(object sender, EventArgs e)
        {
            if (undoStack.Count > 0)
            {
                redoStack.Push(new Bitmap(mainBitmap));
                mainBitmap = undoStack.Pop();
                graphics.Dispose();
                graphics = Graphics.FromImage(mainBitmap);
                pictureBox1.Image = mainBitmap;
            }
        }

        private void redobutton_Click(object sender, EventArgs e)
        {
            if (redoStack.Count > 0)
            {
                undoStack.Push(new Bitmap(mainBitmap));
                mainBitmap = redoStack.Pop();
                graphics.Dispose();
                graphics = Graphics.FromImage(mainBitmap);
                pictureBox1.Image = mainBitmap;
            }
        }

        private void eraserbutton_Click(object sender, EventArgs e)
        {
            currentTool = "eraser";
        }

        private void circlebutton_Click(object sender, EventArgs e)
        {
            currentTool = "circle";
        }

        private void linebutton_Click(object sender, EventArgs e)
        {
            currentTool = "line";
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            brushSize = (int)numericUpDown1.Value;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            graphics.Dispose();
            mainBitmap.Dispose();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void pencilbutton_Click(object sender, EventArgs e)
        {
            currentTool = "pencil";
        }

        private void brushbutton_Click(object sender, EventArgs e)
        {
            currentTool = "brush";
        }

        private void spraybutton_Click(object sender, EventArgs e)
        {
            currentTool = "spray";
        }

        private void squarebutton_Click(object sender, EventArgs e)
        {
            currentTool = "square";
        }


        private void exitbutton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void FillArea(Point startPoint, Color fillColor)
        {
            Color targetColor = mainBitmap.GetPixel(startPoint.X, startPoint.Y);
            if (targetColor.ToArgb() == fillColor.ToArgb()) return;

            Queue<Point> pixels = new Queue<Point>();
            pixels.Enqueue(startPoint);

            while (pixels.Count > 0)
            {
                Point point = pixels.Dequeue();
                if (point.X < 0 || point.X >= mainBitmap.Width || point.Y < 0 || point.Y >= mainBitmap.Height)
                    continue;
                if (mainBitmap.GetPixel(point.X, point.Y) != targetColor)
                    continue;

                mainBitmap.SetPixel(point.X, point.Y, fillColor);
                pixels.Enqueue(new Point(point.X - 1, point.Y));
                pixels.Enqueue(new Point(point.X + 1, point.Y));
                pixels.Enqueue(new Point(point.X, point.Y - 1));
                pixels.Enqueue(new Point(point.X, point.Y + 1));
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
            saveDialog.Title = "Save Paint Image";
            saveDialog.DefaultExt = "png";

            // Show the dialog and save the image if the user clicks OK
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Save the image to the specified format
                    string extension = System.IO.Path.GetExtension(saveDialog.FileName).ToLower();
                    System.Drawing.Imaging.ImageFormat format;

                    switch (extension)
                    {
                        case ".jpg":
                        case ".jpeg":
                            format = System.Drawing.Imaging.ImageFormat.Jpeg;
                            break;
                        case ".bmp":
                            format = System.Drawing.Imaging.ImageFormat.Bmp;
                            break;
                        default:
                            format = System.Drawing.Imaging.ImageFormat.Png;
                            break;
                    }

                    // Save the image
                    mainBitmap.Save(saveDialog.FileName, format);

                    // Show a success message
                    MessageBox.Show("Image saved successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving image: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void fillbutton_Click(object sender, EventArgs e)
        {
            currentTool = "fill";
        }
        
    }
}
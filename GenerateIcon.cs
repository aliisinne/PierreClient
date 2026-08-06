using System;
using System.Drawing;
using System.Drawing.Text;

class Program
{
    static void Main()
    {
        int size = 256;
        using (Bitmap bmp = new Bitmap(size, size))
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                
                // Draw green background with rounded corners
                Color greenColor = ColorTranslator.FromHtml("#00E676");
                g.Clear(Color.Transparent);
                
                int cornerRadius = 40;
                using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddArc(0, 0, cornerRadius, cornerRadius, 180, 90);
                    path.AddArc(size - cornerRadius, 0, cornerRadius, cornerRadius, 270, 90);
                    path.AddArc(size - cornerRadius, size - cornerRadius, cornerRadius, cornerRadius, 0, 90);
                    path.AddArc(0, size - cornerRadius, cornerRadius, cornerRadius, 90, 90);
                    path.CloseFigure();
                    
                    using (SolidBrush brush = new SolidBrush(greenColor))
                    {
                        g.FillPath(brush, path);
                    }
                }

                // Draw 'P'
                using (Font font = new Font("Segoe UI", 140, FontStyle.Bold))
                {
                    SizeF textSize = g.MeasureString("P", font);
                    float x = (size - textSize.Width) / 2;
                    float y = (size - textSize.Height) / 2;
                    using (SolidBrush textBrush = new SolidBrush(Color.White))
                    {
                        g.DrawString("P", font, textBrush, x + 5, y);
                    }
                }
            }
            bmp.Save("PierreLauncher/Assets/icon.png", System.Drawing.Imaging.ImageFormat.Png);
        }
        Console.WriteLine("Icon created successfully!");
    }
}

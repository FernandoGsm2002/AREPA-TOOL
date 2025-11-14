using System.Drawing.Drawing2D;

namespace TT_Tool.Controls
{
    /// <summary>
    /// Botón moderno con bordes redondeados y efectos
    /// </summary>
    public class ModernButton : Button
    {
        private int borderRadius = 15;
        private Color borderColor = Color.Transparent;
        private int borderSize = 0;

        public int BorderRadius
        {
            get => borderRadius;
            set { borderRadius = value; Invalidate(); }
        }

        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        public int BorderSize
        {
            get => borderSize;
            set { borderSize = value; Invalidate(); }
        }

        private Color hoverColor = Color.FromArgb(88, 72, 211);
        private bool isHovering = false;

        public Color HoverColor
        {
            get => hoverColor;
            set { hoverColor = value; }
        }

        public ModernButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Size = new Size(150, 40);
            BackColor = Color.FromArgb(224, 224, 224);
            ForeColor = Color.FromArgb(66, 66, 66);
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            isHovering = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHovering = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF rectSurface = ClientRectangle;
            RectangleF rectBorder = RectangleF.Inflate(rectSurface, -borderSize, -borderSize);

            if (borderRadius > 2)
            {
                using (GraphicsPath pathSurface = GetFigurePath(rectSurface, borderRadius))
                using (GraphicsPath pathBorder = GetFigurePath(rectBorder, borderRadius - borderSize))
                using (Pen penSurface = new Pen(Parent?.BackColor ?? BackColor, 2))
                using (Pen penBorder = new Pen(borderColor, borderSize))
                {
                    pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    Region = new Region(pathSurface);

                    // Superficie del botón con efecto hover
                    Color currentColor = isHovering ? hoverColor : BackColor;
                    pevent.Graphics.FillPath(new SolidBrush(currentColor), pathSurface);

                    // Borde
                    if (borderSize >= 1)
                        pevent.Graphics.DrawPath(penBorder, pathBorder);

                    // Borde padre
                    pevent.Graphics.DrawPath(penSurface, pathSurface);
                }
            }
            else
            {
                Region = new Region(rectSurface);

                if (borderSize >= 1)
                {
                    using (Pen penBorder = new Pen(borderColor, borderSize))
                    {
                        penBorder.Alignment = PenAlignment.Inset;
                        pevent.Graphics.DrawRectangle(penBorder, 0, 0, Width - 1, Height - 1);
                    }
                }
            }
            
            // Llamar a base.OnPaint AL FINAL para que dibuje el texto e imagen ENCIMA
            base.OnPaint(pevent);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (Parent != null)
            {
                Parent.BackColorChanged += Container_BackColorChanged;
            }
        }

        private void Container_BackColorChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private GraphicsPath GetFigurePath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float curveSize = radius * 2F;

            path.StartFigure();
            path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
            path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
            path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
            path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}



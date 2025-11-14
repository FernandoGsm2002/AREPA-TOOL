using System.Drawing.Drawing2D;

namespace TT_Tool.Controls
{
    /// <summary>
    /// ComboBox moderno con estilo flat y bordes redondeados
    /// </summary>
    public class ModernComboBox : ComboBox
    {
        private Color borderColor = Color.FromArgb(224, 224, 224);
        private Color buttonColor = Color.FromArgb(108, 92, 231);
        private Color borderFocusColor = Color.FromArgb(108, 92, 231);
        private int borderSize = 1;
        private int borderRadius = 8;
        private bool isFocused = false;

        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        public Color ButtonColor
        {
            get => buttonColor;
            set { buttonColor = value; Invalidate(); }
        }

        public Color BorderFocusColor
        {
            get => borderFocusColor;
            set { borderFocusColor = value; Invalidate(); }
        }

        public int BorderSize
        {
            get => borderSize;
            set { borderSize = value; Invalidate(); }
        }

        public int BorderRadius
        {
            get => borderRadius;
            set { borderRadius = value; Invalidate(); }
        }

        public ModernComboBox()
        {
            FlatStyle = FlatStyle.Flat;
            BackColor = Color.White;
            ForeColor = Color.FromArgb(66, 66, 66);
            Font = new Font("Segoe UI", 9F);
            DropDownStyle = ComboBoxStyle.DropDownList;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            
            if (m.Msg == 0xF) // WM_PAINT
            {
                using (Graphics g = Graphics.FromHwnd(Handle))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    
                    Rectangle rectBorder = new Rectangle(0, 0, Width - 1, Height - 1);
                    Color currentBorderColor = isFocused ? borderFocusColor : borderColor;

                    using (Pen penBorder = new Pen(currentBorderColor, borderSize))
                    using (GraphicsPath path = GetRoundedRectangle(rectBorder, borderRadius))
                    {
                        g.DrawPath(penBorder, path);
                    }

                    // Dibujar botón dropdown
                    Rectangle rectButton = new Rectangle(Width - 30, 0, 30, Height);
                    using (SolidBrush brushButton = new SolidBrush(buttonColor))
                    {
                        g.FillRectangle(brushButton, rectButton);
                    }

                    // Dibujar flecha
                    Point[] arrow = new Point[]
                    {
                        new Point(Width - 18, Height / 2 - 2),
                        new Point(Width - 12, Height / 2 + 4),
                        new Point(Width - 6, Height / 2 - 2)
                    };
                    using (Pen penArrow = new Pen(Color.White, 2))
                    {
                        g.DrawLines(penArrow, arrow);
                    }
                }
            }
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            isFocused = true;
            Invalidate();
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            isFocused = false;
            Invalidate();
        }

        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float r = radius * 2f;

            path.StartFigure();
            path.AddArc(rect.X, rect.Y, r, r, 180, 90);
            path.AddArc(rect.Right - r, rect.Y, r, r, 270, 90);
            path.AddArc(rect.Right - r, rect.Bottom - r, r, r, 0, 90);
            path.AddArc(rect.X, rect.Bottom - r, r, r, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}

using System.Drawing.Drawing2D;

namespace TT_Tool.Controls
{
    /// <summary>
    /// RadioButton moderno con estilo personalizado
    /// </summary>
    public class ModernRadioButton : RadioButton
    {
        private Color checkedColor = Color.FromArgb(108, 92, 231);
        private Color unCheckedColor = Color.FromArgb(200, 200, 200);

        public Color CheckedColor
        {
            get => checkedColor;
            set { checkedColor = value; Invalidate(); }
        }

        public Color UnCheckedColor
        {
            get => unCheckedColor;
            set { unCheckedColor = value; Invalidate(); }
        }

        public ModernRadioButton()
        {
            MinimumSize = new Size(0, 21);
            Padding = new Padding(10, 0, 0, 0);
            Font = new Font("Segoe UI", 9F);
            ForeColor = Color.FromArgb(66, 66, 66);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics graphics = pevent.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            float rbBorderSize = 16F;
            float rbCheckSize = 10F;
            RectangleF rectRbBorder = new RectangleF()
            {
                X = 0.5F,
                Y = (Height - rbBorderSize) / 2,
                Width = rbBorderSize,
                Height = rbBorderSize
            };
            RectangleF rectRbCheck = new RectangleF()
            {
                X = rectRbBorder.X + ((rectRbBorder.Width - rbCheckSize) / 2),
                Y = (Height - rbCheckSize) / 2,
                Width = rbCheckSize,
                Height = rbCheckSize
            };

            // Dibujar borde del radio button
            using (Pen penBorder = new Pen(Checked ? checkedColor : unCheckedColor, 1.6F))
            {
                graphics.DrawEllipse(penBorder, rectRbBorder);
            }

            // Dibujar círculo interior si está checked
            if (Checked)
            {
                using (SolidBrush brushRbCheck = new SolidBrush(checkedColor))
                {
                    graphics.FillEllipse(brushRbCheck, rectRbCheck);
                }
            }

            // Dibujar texto
            using (SolidBrush brushText = new SolidBrush(ForeColor))
            {
                StringFormat stringFormat = new StringFormat
                {
                    LineAlignment = StringAlignment.Center
                };
                graphics.DrawString(Text, Font, brushText, 
                    rbBorderSize + 8, Height / 2, stringFormat);
            }
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckedChanged(e);
            Invalidate();
        }
    }
}

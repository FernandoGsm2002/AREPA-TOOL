using System.Drawing.Drawing2D;

namespace TT_Tool.Controls
{
    /// <summary>
    /// TabControl moderno con estilo personalizado
    /// </summary>
    public class ModernTabControl : TabControl
    {
        private Color activeTabColor = Color.FromArgb(66, 133, 244);
        private Color inactiveTabColor = Color.FromArgb(240, 242, 245);
        private Color activeTextColor = Color.White;
        private Color inactiveTextColor = Color.FromArgb(100, 100, 100);

        public ModernTabControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.OptimizedDoubleBuffer | 
                     ControlStyles.ResizeRedraw | 
                     ControlStyles.UserPaint, true);
            
            DrawMode = TabDrawMode.OwnerDrawFixed;
            SizeMode = TabSizeMode.Fixed;
            ItemSize = new Size(150, 40);
            Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Parent?.BackColor ?? Color.White);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            for (int i = 0; i < TabCount; i++)
            {
                Rectangle tabRect = GetTabRect(i);
                bool isSelected = (i == SelectedIndex);

                // Dibujar fondo de la pestaña
                using (SolidBrush brush = new SolidBrush(isSelected ? activeTabColor : inactiveTabColor))
                {
                    e.Graphics.FillRectangle(brush, tabRect);
                }

                // Dibujar texto
                using (SolidBrush textBrush = new SolidBrush(isSelected ? activeTextColor : inactiveTextColor))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    e.Graphics.DrawString(TabPages[i].Text, Font, textBrush, tabRect, sf);
                }
            }

            // Dibujar el contenido de la pestaña seleccionada
            if (SelectedTab != null)
            {
                Rectangle contentRect = new Rectangle(0, ItemSize.Height, Width, Height - ItemSize.Height);
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(240, 242, 245)))
                {
                    e.Graphics.FillRectangle(brush, contentRect);
                }
            }
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            Invalidate();
        }
    }
}

#region Disclaimer / License
// Copyright (C) 2008, Kenneth Skovhede
// http://www.hexad.dk, opensource@hexad.dk
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
// 
#endregion
using System;
using System.Windows.Forms;
using System.Drawing;

namespace OSGeo.MapGuide.Maestro.ResourceEditors.GeometryStyleEditors
{

	public class ColorComboBox
		: CustomCombo
	{
		private bool m_allowTransparent = false;
		private SpecialCell m_currentColor;
        private ColorDialog m_colorPicker;
        private static ColorDialog m_sharedColorPicker;

		public ColorComboBox()
			: base()
		{
			if (!this.DesignMode)
			{
				m_currentColor = new SpecialCell(SpecialCell.CellTypes.CurrentColor);
				base.CustomRender = new RenderCustomItem(ColorComboRender);

                //ResetColors();
			}
		}

        public void ResetColors()
        {
            base.Items.Clear();

            if (m_allowTransparent)
                base.Items.Add(SpecialCell.Transparent);

            foreach (Color c in KnownColors)
                base.Items.Add(c);
            base.Items.Add(m_currentColor);
            base.Items.Add(SpecialCell.MoreColors);
        }

		[ System.ComponentModel.Browsable(false), System.ComponentModel.ReadOnly(true) ]
		public bool AllowTransparent
		{
			get { return m_allowTransparent; }
			set { m_allowTransparent = value; }
		}

		[ System.ComponentModel.Browsable(false), System.ComponentModel.ReadOnly(true) ]
		public bool TransparentSelected
		{
			get { return this.SelectedItem == SpecialCell.Transparent; }
			set 
			{ 
				if (value)
					this.SelectedItem = SpecialCell.Transparent;
				else
					this.SelectedItem = m_currentColor;
			}
		}

		[ System.ComponentModel.Browsable(false), System.ComponentModel.ReadOnly(true) ]
		public Color CurrentColor
		{
			get 
			{
				if (this.SelectedItem == null)
					return m_currentColor.Color;
				else if (this.SelectedItem.GetType() == typeof(SpecialCell))
					return ((SpecialCell)this.SelectedItem).Color;
				else
					return (Color)this.SelectedItem;
			}
			set
			{
                if (Array.IndexOf(KnownColors, value) > 0)
                    this.SelectedItem = value;
                else if (value == SpecialCell.Transparent.Color && this.AllowTransparent)
                    this.SelectedItem = SpecialCell.Transparent;
                else
                {
                    m_currentColor.Color = value;
                    this.SelectedItem = m_currentColor;
                }
			}
	
		}

		protected override void OnMeasureItem(MeasureItemEventArgs e)
		{
			if (e.Index < 0)
				return;

			if (this.Items[e.Index].GetType() == typeof(SpecialCell))
			{
				SpecialCell c = (SpecialCell)this.Items[e.Index];
				if (c.CellType == SpecialCell.CellTypes.CurrentColor)
					e.ItemHeight = 0;
				else if (c.CellType == SpecialCell.CellTypes.Transparent && !m_allowTransparent)
					e.ItemHeight = 0;
				else
					base.OnMeasureItem(e);
			}
			else
				base.OnMeasureItem (e);
		}



		protected override void OnSelectedIndexChanged(EventArgs e)
		{
            if (this.SelectedItem != null && this.SelectedItem.ToString() == "More colors...")
            {
                ColorDialog dlg = this.ColorPicker;

                dlg.FullOpen = true;
                dlg.SolidColorOnly = true;
                dlg.AllowFullOpen = true;
                dlg.Color = m_currentColor.Color;
                this.SelectedItem = m_currentColor;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    if (Array.IndexOf(KnownColors, dlg.Color) > 0)
                        this.SelectedItem = dlg.Color;
                    else
                        m_currentColor.Color = dlg.Color;
                }
            }
            else
                m_currentColor.Color = this.CurrentColor; 
			base.OnSelectedIndexChanged (e);
		}


		private const int MARGIN = 2;

		private readonly Color[] KnownColors = 
		{
			Color.Black,
			Color.White,
			Color.DarkRed,
			Color.DarkGreen,
			Color.Goldenrod,
			Color.DarkBlue,
			Color.DarkMagenta,
			Color.DarkCyan,
			Color.LightGray,
			Color.Gray,
			Color.Red,
			Color.Green,
			Color.Yellow,
			Color.Blue,
			Color.Magenta,
			Color.Cyan
		};

		[ System.ComponentModel.Browsable(false), System.ComponentModel.ReadOnly(true) ]
		public new ComboBox.ObjectCollection Items
		{
			get 
			{
				return base.Items;
			}
		}

		private class SpecialCell
		{
			public enum CellTypes
			{
				Transparent,
				MoreColors,
				CurrentColor
			}

			private CellTypes m_cellType;
			private Color m_color;
			public SpecialCell(CellTypes celltype)
			{
				m_cellType = celltype;
                if (celltype == CellTypes.Transparent)
                    m_color = System.Drawing.Color.Transparent;
			}

			public Color Color
			{
				get { return m_color; }
				set { m_color = value; }
			}

			public CellTypes CellType
			{
				get { return m_cellType; }
			}

			public override string ToString()
			{
				if (m_cellType == CellTypes.MoreColors)
					return "More colors...";
				else if (m_cellType == CellTypes.Transparent)
					return "Transparent";
				else
					return base.ToString ();
			}

			public static readonly SpecialCell MoreColors = new SpecialCell(CellTypes.MoreColors);
			public static readonly SpecialCell Transparent = new SpecialCell(CellTypes.Transparent);

		}

		private bool ColorComboRender(DrawItemEventArgs e, object value)
		{
			Color color;
			if (value == null)
				return false;

			if (value.GetType() == typeof(SpecialCell))
			{
				if (value == m_currentColor)
					color = ((SpecialCell)value).Color;
				else
					return false;
			}
			else if (value.GetType() != typeof(Color))
				return false;
			else
				color = (Color)value;

			if (!this.Enabled)
			{
				int median = (color.R + color.G + color.B) / 3;
				median = Math.Max(Math.Min(median, 205), 50);
				color = Color.FromArgb(median, median, median);
			}


			e.DrawBackground();

			Rectangle r = new Rectangle(e.Bounds.X + MARGIN, e.Bounds.Y + MARGIN, e.Bounds.Width - MARGIN * 2, e.Bounds.Height - MARGIN * 2);
			using (Brush b = new SolidBrush(color))
				e.Graphics.FillRectangle(b, r);
			e.Graphics.DrawRectangle(Pens.Black, r);

			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
				e.DrawFocusRectangle();
			
			return true;
		}

        public ColorDialog ColorPicker
        {
            get 
            {
                if (m_colorPicker == null)
                {
                    if (m_sharedColorPicker == null)
                        m_sharedColorPicker = new ColorDialog();

                    m_colorPicker = m_sharedColorPicker;
                }
                return m_colorPicker; 
            }
            set { m_colorPicker = value; }
        }
    }
}
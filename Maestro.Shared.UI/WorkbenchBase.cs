﻿#region Disclaimer / License
// Copyright (C) 2010, Jackie Ng
// http://trac.osgeo.org/mapguide/wiki/maestro, jumpinjackie@gmail.com
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Maestro.Shared.UI
{
    public partial class WorkbenchBase : Form
    {
        MenuStrip menu;
        ToolStripContainer toolStripContainer;
        ToolStrip toolbar;
        
        StatusStrip status;
        ToolStripStatusLabel statusLabel;

        ZonedContainer contentPanel;

        ContextMenuStrip ctxToolbar;

        private IWorkbenchInitializer _workbenchInitializer;

        protected WorkbenchBase(IWorkbenchInitializer init)
        {
            InitializeComponent();
            _workbenchInitializer = init;
            _toolstrips = new Dictionary<string, ToolStrip>();
            _toolstripRegions = new Dictionary<string, ToolbarRegion>();

            this.Icon = _workbenchInitializer.GetIcon();

            contentPanel = new ZonedContainer(_workbenchInitializer);
            contentPanel.Dock = DockStyle.Fill;

            contentPanel.ViewActivated += new ViewContentActivateEventHandler(OnViewActivated);

            menu = _workbenchInitializer.GetMainMenu(this);

            toolStripContainer = new ToolStripContainer();
            toolStripContainer.ContentPanel.Controls.Add(contentPanel);
            toolStripContainer.Dock = DockStyle.Fill;

            this.Controls.Add(toolStripContainer);

            ctxToolbar = new ContextMenuStrip();
            menu.ContextMenuStrip = ctxToolbar;
            toolStripContainer.TopToolStripPanel.ContextMenuStrip = ctxToolbar;
            toolStripContainer.BottomToolStripPanel.ContextMenuStrip = ctxToolbar;
            toolStripContainer.LeftToolStripPanel.ContextMenuStrip = ctxToolbar;
            toolStripContainer.RightToolStripPanel.ContextMenuStrip = ctxToolbar;

            toolbar = _workbenchInitializer.GetMainToolStrip(this);
            toolbar.Stretch = true;

            AddToolbar("Base", toolbar, ToolbarRegion.Top, true);
            
            status = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            status.Items.Add(statusLabel);

            this.Controls.Add(menu);
            this.Controls.Add(status);

            // Use the Idle event to update the status of menu and toolbar items.
            Application.Idle += OnApplicationIdle;
        }

        protected virtual void OnViewActivated(object sender, IViewContent content) { }

        private Dictionary<string, ToolStrip> _toolstrips;
        private Dictionary<string, ToolbarRegion> _toolstripRegions;

        /// <summary>
        /// Adds the toolbar.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="toolbar">The toolbar.</param>
        /// <param name="region">The region.</param>
        /// <param name="canToggleVisibility">if set to <c>true</c> [can toggle visibility].</param>
        public void AddToolbar(string name, ToolStrip toolbar, ToolbarRegion region, bool canToggleVisibility)
        {
            _toolstrips.Add(name, toolbar);
            _toolstripRegions.Add(name, region);

            if (canToggleVisibility)
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = name;
                item.Tag = name;
                item.Checked = true;
                item.CheckOnClick = true;
                item.Click += delegate
                {
                    SetToolbarVisibility(name, item.Checked);
                };
                ctxToolbar.Items.Add(item);
            }

            switch (region)
            {
                case ToolbarRegion.Top:
                    toolStripContainer.TopToolStripPanel.Controls.Add(toolbar);
                    break;
                case ToolbarRegion.Bottom:
                    toolStripContainer.BottomToolStripPanel.Controls.Add(toolbar);
                    break;
                case ToolbarRegion.Left:
                    toolStripContainer.LeftToolStripPanel.Controls.Add(toolbar);
                    break;
                case ToolbarRegion.Right:
                    toolStripContainer.RightToolStripPanel.Controls.Add(toolbar);
                    break;
            }
        }

        /// <summary>
        /// Sets the toolbar visibility.
        /// </summary>
        /// <param name="toolbarName">Name of the toolbar.</param>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetToolbarVisibility(string toolbarName, bool visible)
        {
            ToolStrip strip = GetToolbar(toolbarName);
            if (strip != null)
            {
                ToolbarRegion region = _toolstripRegions[toolbarName];
                if (visible)
                {
                    switch (region)
                    {
                        case ToolbarRegion.Bottom:
                            toolStripContainer.BottomToolStripPanel.Controls.Add(strip);
                            break;
                        case ToolbarRegion.Left:
                            toolStripContainer.LeftToolStripPanel.Controls.Add(strip);
                            break;
                        case ToolbarRegion.Right:
                            toolStripContainer.RightToolStripPanel.Controls.Add(strip);
                            break;
                        case ToolbarRegion.Top:
                            toolStripContainer.TopToolStripPanel.Controls.Add(strip);
                            break;
                    }
                }
                else
                {
                    switch (region)
                    {
                        case ToolbarRegion.Bottom:
                            toolStripContainer.BottomToolStripPanel.Controls.Remove(strip);
                            break;
                        case ToolbarRegion.Left:
                            toolStripContainer.LeftToolStripPanel.Controls.Remove(strip);
                            break;
                        case ToolbarRegion.Right:
                            toolStripContainer.RightToolStripPanel.Controls.Remove(strip);
                            break;
                        case ToolbarRegion.Top:
                            toolStripContainer.TopToolStripPanel.Controls.Remove(strip);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the toolbar.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public ToolStrip GetToolbar(string name)
        {
            if (_toolstrips.ContainsKey(name))
                return _toolstrips[name];
            return null;
        }

        /// <summary>
        /// Gets the toolbar names.
        /// </summary>
        /// <value>The toolbar names.</value>
        public ICollection<string> ToolbarNames
        {
            get { return _toolstrips.Keys; }
        }

        /// <summary>
        /// Sets the status label.
        /// </summary>
        /// <param name="text">The text.</param>
        public void SetStatusLabel(string text)
        {
            statusLabel.Text = text;
        }

        /// <summary>
        /// Sets the title.
        /// </summary>
        /// <param name="title">The title.</param>
        public void SetTitle(string title)
        {
            this.Text = title;
        }

        /// <summary>
        /// Gets the active view in the document region
        /// </summary>
        public IViewContent ActiveDocumentView
        {
            get { return contentPanel.ActiveDocumentView; }
        }

        /// <summary>
        /// Shows the content.
        /// </summary>
        /// <param name="vc">The vc.</param>
        internal void ShowContent(IViewContent vc)
        {
            switch (vc.DefaultRegion)
            {
                case ViewRegion.Bottom:
                case ViewRegion.Left:
                case ViewRegion.Right:
                case ViewRegion.Document:
                    contentPanel.AddContent(vc);
                    var vcb = vc as ViewContentBase;
                    if (vcb != null)
                        vcb.IsAttached = true;
                    break;
                case ViewRegion.Floating:
                    throw new NotImplementedException();
                case ViewRegion.Dialog:
                    throw new NotImplementedException();
            }
        }

        protected internal void CheckContainerStatus()
        {
            contentPanel.CheckContainerStatus();
        }

        void OnApplicationIdle(object sender, EventArgs e)
        {
            // Use the Idle event to update the status of menu and toolbar.
            // Depending on your application and the number of menu items with complex conditions,
            // you might want to update the status less frequently.
            _workbenchInitializer.UpdateMenuItemStatus(menu, _toolstrips.Values);
        }
    }

    /// <summary>
    /// Defines the valid regions a toolbar can reside on a workbench
    /// </summary>
    public enum ToolbarRegion
    {
        /// <summary>
        /// On the top
        /// </summary>
        Top,
        /// <summary>
        /// On the left
        /// </summary>
        Left,
        /// <summary>
        /// On the right
        /// </summary>
        Right,
        /// <summary>
        /// On the bottom
        /// </summary>
        Bottom
    }
}

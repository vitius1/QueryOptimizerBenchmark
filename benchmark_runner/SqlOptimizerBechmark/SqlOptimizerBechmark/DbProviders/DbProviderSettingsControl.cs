﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SqlOptimizerBechmark.DbProviders
{
    public partial class DbProviderSettingsControl : UserControl
    {
        private DbProvider provider;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DbProvider Provider
        {
            get => provider;
            set
            {
                this.provider = value;
                BindControls();
            }
        }

        public event EventHandler Changed;

        protected virtual void OnChanged()
        {
            if (Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        public DbProviderSettingsControl()
        {
            InitializeComponent();
        }

        protected virtual void BindControls()
        {

        }

        protected void NotifyChanged()
        {
            OnChanged();
        }
    }
}

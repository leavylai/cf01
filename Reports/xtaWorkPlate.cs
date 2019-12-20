﻿using System;
using System.IO;
using cf01.ModuleClass;

namespace cf01.Reports
{
    public partial class xtaWorkPlate : DevExpress.XtraReports.UI.XtraReport
    {
        public xtaWorkPlate()
        {
            InitializeComponent();
        }

        void BindImage()
        {           
            string art_path = GetCurrentColumnValue("picture_name").ToString();
            if (File.Exists(art_path))
            {
                xrPictureBox1.ImageUrl = art_path;
            }
            else
            {
                xrPictureBox1.ImageUrl = null;
            }
        }

        private void xrPictureBox1_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            BindImage();
        }     
    }
}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RetrievalSystem
{
    public partial class DetailForm : Form
    {
        public DetailForm(string text)
        {
            InitializeComponent();

            txtAbtract.Text = text;
        }
    }
}

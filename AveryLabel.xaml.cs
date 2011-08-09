using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LabelPrinter
{
    /// <summary>
    /// Interaction logic for AveryLabel.xaml
    /// </summary>
    public partial class AveryLabel : UserControl
    {
        public AveryLabel() : this("Line1","Line2","Line3","12345")
        {
        }

        public AveryLabel(string line1, string line2, string line3, string zipCode)
        {
            InitializeComponent();


            //set visual elements
            this.txtLine1.Text = line1;
            this.txtLine2.Text = line2;
            this.txtLine3.Text = line3;
            this.txtLine4.Text = zipCode;
        }
    }
}

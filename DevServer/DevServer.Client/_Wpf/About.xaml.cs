using System;
using System.Windows;
//+
namespace DevServer.Client
{
    public partial class AboutWindow : Window
    {
        //- @Ctor -//
        public AboutWindow()
        {
            InitializeComponent();
            //+
            btnBlogLink.Click += delegate
            {
                System.Diagnostics.Process.Start("http://www.netfxharmonics.com");
            };
            btnLinkedInLink.Click += delegate
            {
                System.Diagnostics.Process.Start("http://www.linkedin.com/in/davidbetz");
            };
        }
    }
}
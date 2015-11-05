#region

using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace EscInstaller.UserControls
{
    /// <summary>
    ///     Interaction logic for UcSpeakerLibrary.xaml
    /// </summary>
    public partial class UcSpeakerLibrary : UserControl
    {
        public UcSpeakerLibrary()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                if (AppDomain.CurrentDomain.BaseDirectory.Contains("Blend 4"))
                {
                    // load styles resources
                    var rd = new ResourceDictionary();
                    rd.Source = new Uri(Path.Combine(Environment.CurrentDirectory, "../../Style/GrayedOutImagge.xaml"),
                        UriKind.Absolute);
                    Resources.MergedDictionaries.Add(rd);

                    // load any other resources this control needs such as Converters
                    //Resources.Add("booleanNOTConverter", new BooleanNOTConverter());
                }
            }
            InitializeComponent();
        }
    }
}
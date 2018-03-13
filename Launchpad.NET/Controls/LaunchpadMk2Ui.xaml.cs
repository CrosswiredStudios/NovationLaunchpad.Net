using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Launchpad.NET.Controls
{
    public sealed partial class LaunchpadMk2Ui
    {
        /// <summary>
        /// The launchpad this control is bound to
        /// </summary>
        public static readonly DependencyProperty LaunchpadMk2Property =
            DependencyProperty.Register(
                "LaunchpadMk2", typeof(LaunchpadMk2),
                typeof(LaunchpadMk2Ui), null
            );

        // The property wrapper, so that callers can use this property through a simple ExampleClassInstance.IsSpinning usage rather than requiring property system APIs
        public LaunchpadMk2 LaunchpadMk2
        {
            get { return (LaunchpadMk2)GetValue(LaunchpadMk2Property); }
            set => SetValue (LaunchpadMk2Property, value);
        }

        public LaunchpadMk2Ui()
        {
            this.InitializeComponent();
        }
    }
}

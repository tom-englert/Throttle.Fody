using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using Throttle;

using TomsToolbox.Desktop;

namespace SampleApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _index;

        public MainWindow()
        {
            InitializeComponent();

            Test();
            Test();
            Test();
            Test();
        }

        [Throttled(typeof(DispatcherThrottle), (int)DispatcherPriority.Input)]
        public async void Test()
        {
            await Task.Delay(TimeSpan.FromSeconds(2));

            Title = $"Test {++_index}";
        }

        private async void Self_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            Title = "Test - Loaded";
        }
    }
}

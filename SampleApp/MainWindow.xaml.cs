using System.Windows;

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

        [Throttle(typeof(DispatcherThrottle))]
        public void Test()
        {
            Title = $"Test{++_index}";
        }
    }
}

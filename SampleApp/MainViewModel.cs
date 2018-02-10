using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

using PropertyChanged;

using Throttle;

using TomsToolbox.Core;
using TomsToolbox.Desktop;
using TomsToolbox.Wpf;

namespace SampleApp
{
    [AddINotifyPropertyChangedInterface]
    public class MainViewModel
    {
        public ObservableCollection<string> Items { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Events { get; } = new ObservableCollection<string>();

        public MainViewModel()
        {
            Items.CollectionChanged += (_, __) => Items_CollectionChanged();

            Items.AddRange(Enumerable.Range(1, 5)
                .Select(i => "Item " + i));
        }

        private void Items_CollectionChanged()
        {
            Events.Add("Items_CollectionChanged");

            Refresh1();
            Refresh2();
        }

        public string Text { get; set; }

        public ICommand LoadedCommand => new DelegateCommand(() => Events.Add("Window loaded"));

        [Throttled(typeof(DispatcherThrottle))]
        private void Refresh1()
        {
            Events.Add("Refresh 1 called");
        }

        [Throttled(typeof(DispatcherThrottle), (int)DispatcherPriority.Input)]
        private void Refresh2()
        {
            Events.Add("Refresh 2 called");
        }

        [Throttled(typeof(TomsToolbox.Desktop.Throttle), 500)]
        private void OnTextChanged()
        {
            Events.Add($"Start searching for \"{Text}\"");
        }
    }
}

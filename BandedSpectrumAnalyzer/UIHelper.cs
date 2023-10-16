using System.Windows;
using System.Windows.Data;

namespace BandedSpectrumAnalyzer
{
    public static class UIHelper
    {
        public static void Bind(object dataSource, string sourcePath, FrameworkElement destinationObject, DependencyProperty dp)
        {
            Binding binding = new Binding();
            binding.Source = dataSource;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            binding.Path = new PropertyPath(sourcePath);
            destinationObject.SetBinding(dp, binding);
        }
    }
}

using System.Windows;
using System.Windows.Controls;

namespace Folders2Md5.Core
{
    public interface IApplicationStyle
    {
        /// <summary>
        /// </summary>
        void Load();

        /// <summary>
        ///     Accent of application style.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SetAccent(object sender, SelectionChangedEventArgs e);

        /// <summary>
        ///     Theme of application style.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        void SetTheme(object sender, RoutedEventArgs routedEventArgs);

        /// <summary>
        /// </summary>
        void SaveStyle();
    }
}
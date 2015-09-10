using System;
using System.IO;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Folders2Md5.Core
{
    public interface IToast
    {
        void Show(string status, string message);
    }

    public class Toast : IToast
    {
        private bool _showToast;

        /// <summary>
        ///     Initialisiert eine neue Instanz der <see cref="T:System.Object" />-Klasse.
        /// </summary>
        public Toast()
        {
            ValidateOsVersion();
        }

        public void Show(string status, string message)
        {
            //C:\Program Files (x86)\Windows Kits\8.1\References\CommonConfiguration\Neutral\Windows.winmd
            if(!_showToast)
            {
                return;
            }

            // Get a toast XML template
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

            // Fill in the text elements
            var stringElements = toastXml.GetElementsByTagName("text");

            //stringElements[0].AppendChild(toastXml.CreateTextNode("Folders2Md5"));
            stringElements[1].AppendChild(toastXml.CreateTextNode(status));
            stringElements[2].AppendChild(toastXml.CreateTextNode(message));

            // Specify the absolute path to an image
            var imagePath = "file:///" + Path.GetFullPath("md5.png");
            var imageElements = toastXml.GetElementsByTagName("image");
            var image = imageElements[0].Attributes.GetNamedItem("src");
            if(image != null)
            {
                image.NodeValue = imagePath;
            }

            var toastNode = toastXml.SelectSingleNode("/toast");
            var node = (XmlElement) toastNode;
            node?.SetAttribute("duration", "long");

            // Create the toast and attach event listeners
            var toast = new ToastNotification(toastXml)
            {
                ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(3600)
            };
            //toast.Activated += ToastActivated;
            //toast.Dismissed += ToastDismissed;
            //toast.Failed += ToastFailed;

            // Show the toast. Be sure to specify the AppUserModelId on your application's shortcut!
            ToastNotificationManager.CreateToastNotifier(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Name).Show(toast);
        }

        private void ValidateOsVersion()
        {
            var vs = Environment.OSVersion.Version;
            _showToast = ((vs.Major == 6 && vs.Minor >= 2) || (vs.Major > 6));
        }

        /*
        private void ToastActivated(ToastNotification sender, object e)
        {
            Dispatcher.Invoke(() =>
            {
                Activate();
                Output.Text = "The user activated the toast.";
            });
        }

        private void ToastDismissed(ToastNotification sender, ToastDismissedEventArgs e)
        {
            String outputText = "";
            switch (e.Reason)
            {
                case ToastDismissalReason.ApplicationHidden:
                    outputText = "The app hid the toast using ToastNotifier.Hide";
                    break;

                case ToastDismissalReason.UserCanceled:
                    outputText = "The user dismissed the toast";
                    break;

                case ToastDismissalReason.TimedOut:
                    outputText = "The toast has timed out";
                    break;
            }

            Dispatcher.Invoke(() => { Output.Text = outputText; });
        }

        private void ToastFailed(ToastNotification sender, ToastFailedEventArgs e)
        {
            Dispatcher.Invoke(() => { Output.Text = "The toast encountered an error."; });
        }
        */
    }
}
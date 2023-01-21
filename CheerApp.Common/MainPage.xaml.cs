using Xamarin.Forms;

namespace CheerApp.Common
{
    public partial class MainPage : ContentPage
    {

        public string Message
        {
            get
            {
                return textLabel.Text;
            }
            set
            {
                textLabel.Text = value;
            }
        }

        public MainPage()
        {
            InitializeComponent();
        }
    }
}

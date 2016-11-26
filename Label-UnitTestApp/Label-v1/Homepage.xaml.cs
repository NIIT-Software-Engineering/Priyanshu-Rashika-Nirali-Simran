using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System.Diagnostics.CodeAnalysis;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Label_v1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Database database;
        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
            database = new Database();
            database.LoadDatabase();
        }

        private void TextileButtonClicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Screen2_CreateLabel),database);
        }

        private void FoodButtonClicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Screen2_CreateLabel),database);
        }

        private void FootwearButtonClicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Screen2_CreateLabel),database);
        }

        private void ExamButtonClicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Screen2_CreateLabel),database);
        }

        private void TextileImageTapped(object sender, TappedRoutedEventArgs e)
        {
            TextileButtonClicked(sender, e);
        }

        private void FoodImageTapped(object sender, TappedRoutedEventArgs e)
        {
            FoodButtonClicked(sender, e);
        }

        private void FootwearImageTapped(object sender, TappedRoutedEventArgs e)
        {
            FootwearButtonClicked(sender, e);
        }

        private void ExamImageTapped(object sender, TappedRoutedEventArgs e)
        {
            ExamButtonClicked(sender, e);
        }

        private void ListLabels(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Screen3_ListLabels), database);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}

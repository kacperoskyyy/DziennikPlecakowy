namespace DziennikPlecakowy
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        private void OnLogoutClicked(object sender, EventArgs e)
        {
            SecureStorage.Default.Remove("auth_token");
            Preferences.Remove("auth_token");
            Application.Current.MainPage = new NavigationPage(new Views.LoginPage());
        }
    }
}
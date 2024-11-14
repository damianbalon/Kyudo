using KyudoApp.MAIU.Model;
using KyudoApp.MAIU.Views;
using SQLite;
using System.Globalization;
namespace KyudoApp.MAIU
{
    public partial class App : Application
    {
        private readonly SqliConnection _conection;
        public App(SqliConnection conection)
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzU3MDI0MUAzMjM3MmUzMDJlMzBGeTBvSUF4b0RteE1vNGgraXQwTVRTWDk0SGpQRWkrdk90dmVaaENseWRJPQ==\r\n\r\n");
            InitializeComponent();
            CultureInfo.CurrentUICulture = CultureInfo.CreateSpecificCulture("pl-PL");

            MainPage = new AppShell();
            _conection = conection;
        }

        protected override async void OnStart()
        {
            ISQLiteAsyncConnection database =  _conection.CreateConnection();
            await database.CreateTableAsync<Trening>();
            
            base.OnStart();
        }
    }
}

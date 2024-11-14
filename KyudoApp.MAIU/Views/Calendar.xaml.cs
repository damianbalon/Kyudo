using System;
using System.IO;
using System.Threading.Tasks;
using KyudoApp.MAIU.Model;
using Microsoft.Maui.Controls;
using SQLite;
using Syncfusion.Maui.Calendar;

namespace KyudoApp.MAIU.Views
{
    public partial class Calendar : ContentPage
    {
       
        private readonly System.Timers.Timer _timer;
        private SQLiteAsyncConnection _database;

        public Calendar()
        {
            InitializeComponent();

            try
            {
                // Inicjalizacja bazy danych
                string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "kyudo.db3");
                Console.WriteLine($"Œcie¿ka do bazy danych: {dbPath}");

                _database = new SQLiteAsyncConnection(dbPath);

                // Próbujemy utworzyæ tabelê, jeœli nie istnieje
                var result = _database.CreateTableAsync<Trening>().Result;
                Console.WriteLine($"Status tworzenia tabeli Trening: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"B³¹d inicjalizacji bazy danych: {ex.Message}");
                DisplayAlert("B³¹d", "Wyst¹pi³ problem z inicjalizacj¹ bazy danych.", "OK");
            }

            // Timer do aktualizacji daty i godziny
            UpdateDateTime();
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (s, e) => UpdateDateTime();
            _timer.Start();
            CheckTrainingDays();
            _timer.Elapsed += (s, e) => CheckTrainingDays();
        }


        private void CheckTrainingDays()
        {
            var trainingDays = _database.Table<Trening>().ToListAsync().Result;

            this.CalendarMain.MonthView.SpecialDayPredicate = (date) =>
            {
                foreach (Trening t in trainingDays)
                {
                    if(date == t.Data)
                    {
                        CalendarIconDetails iconDetails = new CalendarIconDetails();
                        iconDetails.Icon = CalendarIcon.Diamond;
                        iconDetails.Fill = Colors.Red;
                        return iconDetails;
                    }
                    
                }
                return null;

            };
        }

       

        private void UpdateDateTime()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DateLabel.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _timer.Stop();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _timer.Start();
        }

        private async void GoToStats(object sender, EventArgs e)
        {
            try
            {
                // Przekazujemy datê z wybranego dnia w kalendarzu
                DateTime selectedDate = CalendarMain.SelectedDate ?? DateTime.Now.Date;
                Console.WriteLine($"Wybrana data: {selectedDate}");

                // Przechodzimy do widoku statystyk i ustawiamy datê pocz¹tkow¹ (2000 rok)
                await Navigation.PushAsync(new Stats(selectedDate, _database));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"B³¹d w GoToStats: {ex.Message}");
                await DisplayAlert("B³¹d", $"Wyst¹pi³ problem przy przejœciu do statystyk: {ex.Message}", "OK");
            }
        }


        private async void GoToNotesList(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NotesList(_database));
        }

        private async void GoInDay(object sender, EventArgs e)
        {
            try
            {
                // Próbujemy ustawiæ datê na podstawie wybranego dnia lub dzisiejszej daty
                DateTime selectedDate = CalendarMain.SelectedDate ?? DateTime.Now.Date;
                Console.WriteLine($"Wybrana data: {selectedDate}");

                // Pobranie wszystkich rekordów i znalezienie rekordu rêcznie w pêtli (dla diagnostyki)
                var treningList = await _database.Table<Trening>().ToListAsync();
                Trening existingTrening = null;
                foreach (var trening in treningList)
                {
                    if (trening.Data.Date == selectedDate.Date)
                    {
                        existingTrening = trening;
                        break;
                    }
                }

                if (existingTrening == null)
                {

                    await Navigation.PushAsync(new AddTraining(_database, selectedDate));
                }
                else
                {
                    await Navigation.PushAsync(new Training(_database,existingTrening));

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"B³¹d w GoInDay: {ex.Message}");
                await DisplayAlert("B³¹d", $"Wyst¹pi³ problem przy zapisie lub odczycie z bazy danych: {ex.Message}", "OK");
            }
        }

    }
}

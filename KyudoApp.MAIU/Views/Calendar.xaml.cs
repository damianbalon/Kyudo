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
                Console.WriteLine($"�cie�ka do bazy danych: {dbPath}");

                _database = new SQLiteAsyncConnection(dbPath);

                // Pr�bujemy utworzy� tabel�, je�li nie istnieje
                var result = _database.CreateTableAsync<Trening>().Result;
                Console.WriteLine($"Status tworzenia tabeli Trening: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"B��d inicjalizacji bazy danych: {ex.Message}");
                DisplayAlert("B��d", "Wyst�pi� problem z inicjalizacj� bazy danych.", "OK");
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
                // Przekazujemy dat� z wybranego dnia w kalendarzu
                DateTime selectedDate = CalendarMain.SelectedDate ?? DateTime.Now.Date;
                Console.WriteLine($"Wybrana data: {selectedDate}");

                // Przechodzimy do widoku statystyk i ustawiamy dat� pocz�tkow� (2000 rok)
                await Navigation.PushAsync(new Stats(selectedDate, _database));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"B��d w GoToStats: {ex.Message}");
                await DisplayAlert("B��d", $"Wyst�pi� problem przy przej�ciu do statystyk: {ex.Message}", "OK");
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
                // Pr�bujemy ustawi� dat� na podstawie wybranego dnia lub dzisiejszej daty
                DateTime selectedDate = CalendarMain.SelectedDate ?? DateTime.Now.Date;
                Console.WriteLine($"Wybrana data: {selectedDate}");

                // Pobranie wszystkich rekord�w i znalezienie rekordu r�cznie w p�tli (dla diagnostyki)
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
                Console.WriteLine($"B��d w GoInDay: {ex.Message}");
                await DisplayAlert("B��d", $"Wyst�pi� problem przy zapisie lub odczycie z bazy danych: {ex.Message}", "OK");
            }
        }

    }
}

using Microsoft.Maui.Controls;
using KyudoApp.MAIU.Model;
using System;
using SQLite;
using Plugin.LocalNotification;

namespace KyudoApp.MAIU.Views
{
    public partial class AddTraining : ContentPage
    {
        private readonly SQLiteAsyncConnection _database;
        DateTime selectedDate_;
        public AddTraining(SQLiteAsyncConnection database, DateTime selectedDate)
        {
            InitializeComponent();
            _database = database;

            selectedDate_ = selectedDate;
        }

        private async void OnSaveTrainingClicked(object sender, EventArgs e)
        {
            try
            {
                // Pobierz dane z p�l wej�ciowych
                var start = StartTimePicker.SelectedTime;
                var end = EndTimePicker.SelectedTime;
                var date = selectedDate_;
                var location = LocationEntry.Text;

                if (start < end)
                {
                    // Tworzymy nowy obiekt treningu i zapisujemy do bazy danych
                    var newTraining = new Trening
                    {
                        StartTime = start.Value,
                        EndTime = end.Value,
                        Data = date,
                        Miejsce = location
                    };

                    // Zapisz trening do bazy danych
                    await _database.InsertAsync(newTraining);

                   /* // Ustaw powiadomienie z odpowiednimi szczeg�ami
                    var request = new NotificationRequest
                    {
                        NotificationId = newTraining.Id, // Zak�adaj�c, �e 'Id' jest autogenerowane po wstawieniu do bazy danych
                        Title = "Dzisiaj trening o " + start.Value.ToString(@"hh\:mm"),
                        Description = $"Trening odb�dzie si� w miejscu: {location}",
                        Schedule = new NotificationRequestSchedule
                        {
                            NotifyTime = DateTime.Now.AddSeconds(10), // Przypomnienie 30 minut przed startem
                        }
                    };
                    LocalNotificationCenter.Current.Show(request);*/

                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await DisplayAlert("B��d", "Godzina rozpocz�cia musi by� wcze�niejsza ni� godzina zako�czenia", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"B��d podczas zapisu treningu: {ex.Message}");
                await DisplayAlert("B��d", "Wyst�pi� problem podczas zapisu treningu", "OK");
            }
        }

    }

}

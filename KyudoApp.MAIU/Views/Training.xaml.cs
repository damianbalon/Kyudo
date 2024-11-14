using KyudoApp.MAIU.Model;
using Microsoft.Maui.Controls;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KyudoApp.MAIU.Views
{
    public partial class Training : ContentPage
    {
        private readonly SQLiteAsyncConnection _database;
        private readonly List<PointF> hitPoints = new();
        private readonly int treningId;
        private readonly DateTime dateTime;
        private readonly Trening _trening;
        private Notatki _notatki;


        public Training(SQLiteAsyncConnection database, Trening trening)
        {
            InitializeComponent();
            _database = database;
            treningId = trening.Id;
            dateTime = trening.Data;
            _trening = trening;

            // Tworzenie tabeli, jeœli nie istnieje
            var result = _database.CreateTableAsync<Notatki>().Result;

            // Ustawienie danych w UI
            DataLabel.Text = trening.Data.ToString("dddd, dd MMMM yyyy");
            MiejsceLabel.Text = trening.Miejsce;

            CzasStart.Text = trening.StartTime.ToString(@"hh\:mm");
            CzasKoniec.Text = trening.EndTime.ToString(@"hh\:mm");



            // Ustawiamy Drawable dla HitOverlay
            HitOverlay.Drawable = new HitOverlayDrawable(hitPoints);

            WriteNote();
        }




        private async void OnTargetTapped(object sender, TappedEventArgs e)
        {
            // Uzyskanie pozycji klikniêcia
            var point = e.GetPosition(HitOverlay);

            if (point.HasValue)
            {
                hitPoints.Add(new PointF((float)point.Value.X, (float)point.Value.Y));
                HitOverlay.Invalidate();  // Odœwie¿anie widoku, aby pokazaæ nowy punkt trafienia
            }
        }

        private async void ResetHits(object sender, EventArgs e)
        {
            await SaveHitPointsToDatabaseAsync();

            // Czyszczenie listy trafieñ i odœwie¿anie widoku
            hitPoints.Clear();
            HitOverlay.Invalidate();
            await DisplayHitPoints();
        }

        private async Task DisplayHitPoints()
        {
            var points = await _database.Table<Wynik>()
                                         .Where(w => w.IdTreningu == treningId)
                                         .OrderBy(w => w.Data)
                                         .ToListAsync();

            var pointsText = new StringBuilder("Zapisane punkty trafieñ:\n");
            foreach (var point in points)
            {
                pointsText.AppendLine($"Data: {point.Data}, X: {point.X:F2}, Y: {point.Y:F2}");
            }

        }

        private async void RemoveShote(object sender, EventArgs e)
        {
            if (hitPoints.Count > 0)
            {
                hitPoints.RemoveAt(hitPoints.Count - 1);
                HitOverlay.Invalidate();

                await DisplayHitPoints();
            }
            else
            {
                await DisplayAlert("Brak trafieñ", "Nie ma ¿adnych trafieñ do cofniêcia.", "OK");
            }
        }

        private async Task SaveHitPointsToDatabaseAsync()
        {
            await _database.CreateTableAsync<Wynik>();

            foreach (var point in hitPoints)
            {
                var wynik = new Wynik
                {
                    IdTreningu = treningId,
                    Data = dateTime,
                    X = point.X,
                    Y = point.Y
                };

                await _database.InsertAsync(wynik);  // Asynchroniczny zapis punktu trafienia
            }
        }

        private async void GoBack(object sender, EventArgs e)
        {
            Navigation.RemovePage(Navigation.NavigationStack.Last());
        }

        private async void GoToStatsPage(object sender, EventArgs e)
        {
            // Przechodzimy do strony statystyk zwi¹zanej z tym treningiem

            await Navigation.PushAsync(new Stats(_trening, _database)
            {
                //IsCalendarVisible = false
            });

        }

        private async void DeleteTraining(object sender, EventArgs e)
        {
            bool confirmDelete = await DisplayAlert("Potwierdzenie",
                                                    "Czy na pewno chcesz usun¹æ ten trening wraz ze wszystkimi wynikami i notatkami?",
                                                    "Tak",
                                                    "Nie");

            if (confirmDelete)
            {
                try
                {


                    // Usuniêcie wszystkich wyników powi¹zanych z treningiem
                    await _database.Table<Wynik>()
                                   .Where(w => w.IdTreningu == treningId)
                                   .DeleteAsync();

                    // Usuniêcie notatki powi¹zanej z treningiem
                    await _database.Table<Notatki>()
                                   .Where(n => n.IdTreningu == treningId)
                                   .DeleteAsync();

                    // Usuniêcie treningu
                    await _database.DeleteAsync(_trening);

                    await DisplayAlert("Sukces", "Trening zosta³ usuniêty.", "OK");

                    // Powrót do poprzedniej strony po usuniêciu
                    await Navigation.PopAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"B³¹d przy usuwaniu treningu: {ex.Message}");
                    await DisplayAlert("B³¹d", "Wyst¹pi³ b³¹d podczas usuwania treningu.", "OK");
                }
            }
        }

        private async void NotesEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // Sprawdzamy, czy notatka istnieje w bazie danych
                var notatka = await _database.Table<Notatki>()
                                             .Where(n => n.IdTreningu == treningId)
                                             .FirstOrDefaultAsync();

                // Jeœli notatka nie istnieje, tworzymy now¹
                if (notatka == null)
                {
                    notatka = new Notatki
                    {
                        IdTreningu = treningId,
                        Tresc = NotesEditor.Text // Ustawiamy treœæ na tekst z edytora
                    };
                    // Wstawiamy now¹ notatkê do bazy danych
                    await _database.InsertAsync(notatka);
                }
                else
                {
                    // Jeœli notatka istnieje, aktualizujemy jej treœæ
                    notatka.Tresc = NotesEditor.Text;
                    // Zapisujemy zmienion¹ notatkê
                    await _database.UpdateAsync(notatka);
                }
            }
            catch (Exception ex)
            {
                // Obs³uga b³êdów (np. jeœli wyst¹pi problem z baz¹ danych)
                Console.WriteLine($"B³¹d: {ex.Message}");
            }
        }


        private async void WriteNote()
        {
            try
            {
                _notatki = await _database.Table<Notatki>()
                                            .Where(n => n.IdTreningu == treningId)
                                            .FirstOrDefaultAsync();

                if (_notatki == null)
                {
                    _notatki = new Notatki
                    {
                        IdTreningu = treningId,
                        Tytul = "Trening dnia: " + _trening.Data.ToString() + " W: " + _trening.Miejsce,
                        
                    };
                    await _database.InsertAsync(_notatki);
                }
                else
                {
                    NotesEditor.Text = _notatki.Tresc;
                }
            }
            catch (Exception ex)
            {
                NotesEditor.Text = $"Error in WriteNote: {ex.Message}";
                Console.WriteLine($"Error in WriteNote: {ex.Message}");
            }
        }

    }

    public class HitOverlayDrawable : IDrawable
    {
        private readonly List<PointF> hitPoints;

        public HitOverlayDrawable(List<PointF> points)
        {
            hitPoints = points;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            DrawMato(canvas, dirtyRect);

            // Ustawienie koloru na czerwony i rysowanie trafieñ jako ma³ych kropek
            foreach (var point in hitPoints)
            {
                // Sprawdzamy, czy punkt trafienia znajduje siê w obrêbie mato
                bool isInsideTarget = IsPointInsideMato(point, dirtyRect);

                if (isInsideTarget)
                {
                    // Trafienie w mato (kolor czerwony)
                    canvas.FillColor = Colors.Red;
                    canvas.FillCircle(point.X, point.Y, 5);
                }
                else
                {
                    // Trafienie poza mato (kolor niebieski)
                    canvas.FillColor = Colors.Blue;
                    canvas.FillRectangle(point.X, point.Y, 10, 10);
                }
            }
        }

        // Funkcja sprawdzaj¹ca, czy punkt trafienia znajduje siê w obrêbie mato
        private bool IsPointInsideMato(PointF point, RectF dirtyRect)
        {
            float centerX = dirtyRect.Width / 2;
            float centerY = dirtyRect.Height / 2;

            // Definicja promieni poszczególnych okrêgów w mato
            float nakashiroRadius = 12.5f / 1.5f;
            float ichiNoKuroRadius = nakashiroRadius + 12.5f / 1.5f;
            float niNoShiroRadius = ichiNoKuroRadius + 30f / 1.5f;
            float niNoKuroRadius = niNoShiroRadius + 15f / 1.5f;
            float sanNoShiroRadius = niNoKuroRadius + 30f / 1.5f;
            float sotoKuroRadius = 125f / 1.5f;

            // Obliczanie odleg³oœci punktu od centrum mato
            float distance = (float)Math.Sqrt(Math.Pow(point.X - centerX, 2) + Math.Pow(point.Y - centerY, 2));

            // Sprawdzanie, czy punkt mieœci siê w obrêbie mato
            return distance <= sotoKuroRadius;  // Mo¿na zmieniæ to na odpowiedni promieñ
        }


        private void DrawMato(ICanvas canvas, RectF dirtyRect)
        {
            float centerX = dirtyRect.Width / 2;
            float centerY = dirtyRect.Height / 2;

            // Promienie okrêgów mato
            float nakashiroRadius = 12.5f / 1.5f;
            float ichiNoKuroRadius = nakashiroRadius + 12.5f / 1.5f;
            float niNoShiroRadius = ichiNoKuroRadius + 30f / 1.5f;
            float niNoKuroRadius = niNoShiroRadius + 15f / 1.5f;
            float sanNoShiroRadius = niNoKuroRadius + 30f / 1.5f;
            float sotoKuroRadius = 125f / 1.5f;

            // Rysowanie kolejnych pierœcieni mato
            canvas.FillColor = Colors.Black;
            canvas.FillEllipse(centerX - sotoKuroRadius, centerY - sotoKuroRadius, sotoKuroRadius * 2, sotoKuroRadius * 2);

            canvas.FillColor = Colors.White;
            canvas.FillEllipse(centerX - sanNoShiroRadius, centerY - sanNoShiroRadius, sanNoShiroRadius * 2, sanNoShiroRadius * 2);

            canvas.FillColor = Colors.Black;
            canvas.FillEllipse(centerX - niNoKuroRadius, centerY - niNoKuroRadius, niNoKuroRadius * 2, niNoKuroRadius * 2);

            canvas.FillColor = Colors.White;
            canvas.FillEllipse(centerX - niNoShiroRadius, centerY - niNoShiroRadius, niNoShiroRadius * 2, niNoShiroRadius * 2);

            canvas.FillColor = Colors.Black;
            canvas.FillEllipse(centerX - ichiNoKuroRadius, centerY - ichiNoKuroRadius, ichiNoKuroRadius * 2, ichiNoKuroRadius * 2);

            canvas.FillColor = Colors.White;
            canvas.FillEllipse(centerX - nakashiroRadius, centerY - nakashiroRadius, nakashiroRadius * 2, nakashiroRadius * 2);
        }
    }

}



using System;
using KyudoApp.MAIU.Model;
using SQLite;
using Microsoft.Maui.Controls;

namespace KyudoApp.MAIU.Views
{
    public partial class NoteDetailPage : ContentPage
    {
        private readonly SQLiteAsyncConnection _database;
        private Notatki _notatki;
        private readonly int _treningId;

        public NoteDetailPage(SQLiteAsyncConnection database, int treningId)
        {
            InitializeComponent();
            _database = database;
            _treningId = treningId;
            LoadNoteAsync();
        }

        private async void LoadNoteAsync()
        {
            try
            {
                // Pobieranie istniej¹cej notatki lub tworzenie nowej
                _notatki = await _database.Table<Notatki>()
                                           .Where(n => n.IdTreningu == _treningId)
                                           .FirstOrDefaultAsync();
                if (_notatki == null)
                {
                    _notatki = new Notatki
                    {
                        IdTreningu = _treningId,
                        Tresc = string.Empty // Pusta treœæ dla nowej notatki
                    };
                    await _database.InsertAsync(_notatki);
                }

                NotesEditor.Text = _notatki.Tresc;
            }
            catch (Exception ex)
            {
                await DisplayAlert("B³¹d", $"B³¹d podczas ³adowania notatki: {ex.Message}", "OK");
            }
        }

        private async void NotesEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_notatki != null)
            {
                try
                {
                    _notatki.Tresc = e.NewTextValue; // Aktualizacja treœci notatki w pamiêci
                    await _database.UpdateAsync(_notatki); // Zapis zmian w bazie danych
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"B³¹d podczas zapisywania notatki: {ex.Message}");
                }
            }
        }
    }
}

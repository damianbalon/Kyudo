using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using KyudoApp.MAIU.Model;
using SQLite;
using Microsoft.Maui.Controls;

namespace KyudoApp.MAIU.Views
{
    public partial class NotesList : ContentPage
    {
        public ObservableCollection<Notatki> Notes { get; set; }
        private ObservableCollection<Notatki> AllNotes { get; set; }
        public Command<Notatki> NoteSelectedCommand { get; }

        private SQLiteAsyncConnection database_;

        public NotesList(SQLiteAsyncConnection database)
        {
            database_ = database;
            Notes = new ObservableCollection<Notatki>();
            AllNotes = new ObservableCollection<Notatki>();
            NoteSelectedCommand = new Command<Notatki>(OnNoteSelected);
            BindingContext = this;
            var result = database_.CreateTableAsync<Notatki>().Result;
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadNotes();
        }

        private async Task LoadNotes()
        {
            var notesFromDb = await database_.Table<Notatki>().ToListAsync();
            Notes.Clear();
            AllNotes.Clear();

            foreach (var note in notesFromDb)
            {
                Notes.Add(note);
                AllNotes.Add(note);
            }
        }

        private async void OnNoteSelected(Notatki note)
        {
            if (note != null)
            {
                await Navigation.PushAsync(new NoteDetailPage(database_, note.IdTreningu ?? 0));
            }
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string searchText = e.NewTextValue?.ToLower() ?? string.Empty;

                if (string.IsNullOrEmpty(searchText))
                {
                    // Jeœli tekst wyszukiwania jest pusty, za³aduj wszystkie notatki z bazy
                    await LoadNotes();
                }
                else
                {
                    // Wyszukiwanie notatek w bazie danych po tytule
                    var filteredNotes = await database_.Table<Notatki>()
                        .Where(n => n.Tytul.ToLower().Contains(searchText))
                        .ToListAsync();

                    Notes.Clear();
                    foreach (var note in filteredNotes)
                    {
                        Notes.Add(note);
                    }
                }
            }
            catch (Exception ex)
            {
                // Logowanie b³êdów
                Console.WriteLine($"B³¹d: {ex.Message}");
            }
        }
    }
}

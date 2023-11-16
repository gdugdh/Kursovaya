using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kursovaya
{
    /// <summary>
    /// Логика взаимодействия для SelectMovie.xaml
    /// </summary>
    public partial class SelectMovie : Page
    {
        public SelectMovie()
        {
            InitializeComponent();
            GenerateGenres();
            GenerateMovies("", new List<string>());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            //Window mainWindow = (MainWindow)Application.Current.MainWindow;
            //Window parentWindow = Window.GetWindow(this);

            //frame.Navigate(new Uri("SelectTicket.xaml?movieId=" + 54, UriKind.Relative));


            Button btn = (Button)sender;
            MainWindow.idSelectMovie = Convert.ToInt32(btn.Tag);

            Window parentWindow = Window.GetWindow(this);

            Frame frame = LogicalTreeHelper.FindLogicalNode(parentWindow, "MainFrame") as Frame;
            //frame.Navigate(new MovieDetail());
            frame.Navigate(new SessionSelection());
        }

        private void GenerateGenres()
        {
            using (MovieEntities context = new MovieEntities())
            {
                foreach (var genre in context.Genre.ToList())
                {
                    CheckBox checkBox = new CheckBox();
                    checkBox.Content = genre.name;
                    GenresFilter.Children.Add(checkBox);
                }

            }
        }

        private void GenerateMovies(string movieName, List<string> genres)
        {
            using (MovieEntities context = new MovieEntities())
            {
                var results = context.Movie.ToList();
                if (genres.Count != 0)
                {
                    var query = from movie in context.Movie
                                where genres.All(checkedGenre => movie.MovieGenres.Any(g => g.Genre.name == checkedGenre))
                                select movie;
                    results = query.ToList();
                }

                // Регистро-зависимый поиск
                results = results.Where(m => m.name.Contains(movieName)).ToList();

                foreach (var item in results)
                {
                    Button button = new Button
                    {
                        Background = Brushes.Transparent,
                        BorderBrush = Brushes.Transparent,
                        Tag = item.id
                    };
                    button.Click += Button_Click;

                    // Создаем StackPanel
                    StackPanel stackPanel = new StackPanel
                    {
                        Width = 150,
                        Orientation = Orientation.Vertical
                    };

                    // Создаем изображение
                    Image image = new Image
                    {
                        Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(@"C:\Users\gdugd\source\repos\Kursovaya\Kursovaya\img\logo.jpg")),
                        Height = 200,
                        Width = 150
                    };

                    // Создаем TextBlock для названия
                    TextBlock titleTextBlock = new TextBlock
                    {
                        Text = item.name,
                        Margin = new Thickness(30, 0, 0, 0),
                        TextAlignment = TextAlignment.Center
                    };

                    var genres3 = context.Genre.Where(g => g.MovieGenres.Any(mg => mg.id_movie == item.id)).Select(a => a.name).ToList();

                    // Создаем TextBlock для описания
                    TextBlock descriptionTextBlock = new TextBlock
                    {
                        Text = string.Join(", ", genres3),
                        //Text = "123",
                        Margin = new Thickness(10, 5, 0, 0),
                        TextWrapping = TextWrapping.Wrap
                    };

                    // Добавляем элементы в StackPanel
                    stackPanel.Children.Add(image);
                    stackPanel.Children.Add(titleTextBlock);
                    stackPanel.Children.Add(descriptionTextBlock);

                    // Добавляем StackPanel в кнопку
                    button.Content = stackPanel;

                    // Добавляем кнопку в окно
                    Movies.Children.Add(button);
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            List<string> checkedGanres = new List<string>(); 
            foreach (UIElement element in GenresFilter.Children)
            {
                if (element is CheckBox checkBox)
                {
                    bool isChecked = checkBox.IsChecked ?? false;
                    
                    if (isChecked)
                    {
                        checkedGanres.Add(checkBox.Content.ToString());
                    }
                }
            }
            Movies.Children.Clear();
            GenerateMovies(nameInput.Text, checkedGanres);
        }
    }
}

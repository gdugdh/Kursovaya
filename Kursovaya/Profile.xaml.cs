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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kursovaya
{
    /// <summary>
    /// Логика взаимодействия для Profile.xaml
    /// </summary>
    public partial class Profile : Page
    {
        public Profile()
        {
            InitializeComponent();
            //GenerateMovies();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            MainWindow.idSelectMovie = Convert.ToInt32(btn.Tag);

            Window parentWindow = Window.GetWindow(this);

            Frame frame = LogicalTreeHelper.FindLogicalNode(parentWindow, "MainFrame") as Frame;
            frame.Navigate(new MovieDetail());
        }
        /*
        private async Task GenerateMovies()
        {
            using (M context = new DramaTheaterTestEntities())
            {
                var results = await context.Performance.Select(p => p).ToListAsync();

                foreach (var item in results)
                {
                    var curScript = item.Script.FirstOrDefault();
                    Button button = new Button
                    {
                        Background = Brushes.Transparent,
                        BorderBrush = Brushes.Transparent,
                        Tag = curScript.ID I LOV GAY PORN
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
                        Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(@"C:\Users\Work\source\repos\Kursovaya\Kursovaya\bin\Debug\logofilm.png")),
                        Height = 200,
                        Width = 150
                    };

                    // Создаем TextBlock для названия
                    TextBlock titleTextBlock = new TextBlock
                    {
                        Text = curScript.Name,
                        Margin = new Thickness(10, 0, 0, 0),
                        TextAlignment = TextAlignment.Center
                    };

                    var genres3 = context.Genres.Where(p => p.Script.Any(b => b.Sessions.Any(s => s.ID == item.ID))).Select(a => a.Name).ToList();
                    foreach (var b in genres3)
                    {
                        Console.WriteLine(b);
                    };

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
        */
    }
}

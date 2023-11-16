using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ZXing.QrCode.Internal;

namespace Kursovaya
{
    /// <summary>
    /// Логика взаимодействия для MovieDetail.xaml
    /// </summary>
    public partial class MovieDetail : Page
    {

        private string[] imagePaths = { @"C:\Users\gdugd\source\repos\Kursovaya\Kursovaya\img\deadpool-11.jpg", @"C:\Users\gdugd\source\repos\Kursovaya\Kursovaya\img\16742908331746990.jpg", @"C:\Users\gdugd\source\repos\Kursovaya\Kursovaya\img\deadpool-11.jpg", };
        private int currentImageIndex = 0;
        private DispatcherTimer timer;
        private bool isAnimating = false;

        public MovieDetail()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3); // Интервал между сменой изображений
            timer.Tick += Timer_Tick;
            timer.Start();

            LoadImage();

            sessionListBox.ItemsSource = sessions;
            CalendarDateRange allowedDates = new CalendarDateRange(
                new DateTime(2023, 10, 1), // начальная дата
                new DateTime(2023, 12, 31) // конечная дата
            ); 
            calendar.BlackoutDates.Add(allowedDates);
            calendar.SelectedDatesChanged += Calendar_SelectedDatesChanged;

            AddActorCard("Actor 1", "Character A", @"C:\Users\gdugd\source\repos\Kursovaya\Kursovaya\img\Rayan.jpg");
            AddActorCard("Actor 1", "Character A", @"C:\Users\gdugd\source\repos\Kursovaya\Kursovaya\img\Rayan.jpg");
            AddActorCard("Actor 1", "Character A", @"C:\Users\gdugd\source\repos\Kursovaya\Kursovaya\img\Rayan.jpg");
            AddActorCard("Actor 1", "Character A", @"C:\Users\gdugd\source\repos\Kursovaya\Kursovaya\img\Rayan.jpg");
        }

        private void LoadImage()
        {
            if (currentImageIndex < imagePaths.Length)
            {
                BitmapImage image = new BitmapImage(new Uri(imagePaths[currentImageIndex]));
                imageControl.Source = image;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!isAnimating)
            {
                currentImageIndex = (currentImageIndex + 1) % imagePaths.Length;
                AnimateImageTransition();
            }
        }

        private void AnimateImageTransition()
        {
            isAnimating = true;
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(1),
                FillBehavior = FillBehavior.Stop
            };

            animation.Completed += (s, e) =>
            {
                currentImageIndex = (currentImageIndex + 1) % imagePaths.Length;
                LoadImage();

                DoubleAnimation fadeInAnimation = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(1)
                };
                imageControl.BeginAnimation(Image.OpacityProperty, fadeInAnimation);
                isAnimating = false;
            };

            imageControl.BeginAnimation(Image.OpacityProperty, animation);
        }

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var selectedDate in e.RemovedItems)
            {
                if (selectedDate is DateTime date)
                {
                    if (date < calendar.DisplayDateStart || date > calendar.DisplayDateEnd)
                    {
                        // Если выбрана запрещенная дата, снимите выделение
                        calendar.SelectedDates.Remove(date);
                    }
                }
            }
        }

        private List<string> sessions = new List<string>
        {
            "Сеанс 1 - 10:00",
            "Сеанс 2 - 13:00",
            "Сеанс 3 - 16:00",
            "Сеанс 4 - 19:00",
            "Сеанс 5 - 22:00"
        };

        private void BuyTicketButton_Click(object sender, RoutedEventArgs e)
        {
            // Получите выбранный сеанс
            string selectedSession = sessionListBox.SelectedItem as string;

            if (string.IsNullOrEmpty(selectedSession))
            {
                MessageBox.Show("Выберите сеанс перед покупкой билета.");
            }
            else
            {
                // Выполните действия для покупки билета на выбранный сеанс
                MessageBox.Show("Куплен билет на " + selectedSession);
            }
        }

        private void AddActorCard(string actorName, string characterName, string imagePath)
        {
            // Создайте контейнер для карточки актера
            StackPanel actorCard = new StackPanel
            {
                Width = 200,
                Height = 300,
                Margin = new Thickness(10),
                Background = System.Windows.Media.Brushes.LightGray,
            };

            // Создайте изображение актера
            Image actorImage = new Image
            {
                Source = new BitmapImage(new System.Uri(imagePath, System.UriKind.RelativeOrAbsolute)),
                Width = 150,
                Height = 200,
                Margin = new Thickness(10),
            };

            // Создайте текстовые блоки для имени актера и роли
            TextBlock actorNameText = new TextBlock
            {
                Text = actorName,
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold
            };

            TextBlock characterNameText = new TextBlock
            {
                Text = characterName,
                TextAlignment = TextAlignment.Center
            };

            // Добавьте элементы в карточку актера
            actorCard.Children.Add(actorImage);
            actorCard.Children.Add(actorNameText);
            actorCard.Children.Add(characterNameText);

            // Добавьте карточку актера в WrapPanel
            actorWrapPanel.Children.Add(actorCard);
        }
    }
}

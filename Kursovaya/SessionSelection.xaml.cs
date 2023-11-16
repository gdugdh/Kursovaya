using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
using System.Xml.Linq;

namespace Kursovaya
{
    /// <summary>
    /// Логика взаимодействия для SessionSelection.xaml
    /// </summary>
    public partial class SessionSelection : Page
    {
        BitmapImage starImage = new BitmapImage(new Uri("C:\\Users\\gdugd\\source\\repos\\Kursovaya\\Kursovaya\\img\\star.png"));
        BitmapImage fillStarImage = new BitmapImage(new Uri("C:\\Users\\gdugd\\source\\repos\\Kursovaya\\Kursovaya\\img\\fill star.png"));
        int SelectRating = 5;
        public SessionSelection()
        {
            InitializeComponent();
            LoadData();
            GenerateComments();
        }

        private void LoadData()
        {
            using (MovieEntities context = new MovieEntities())
            {
                Movie curMovie = context.Movie.Where(m => m.id == MainWindow.idSelectMovie).FirstOrDefault();
                MovieName.Text = curMovie.name;

                List<Session> sessions = context.Session.Where(s => s.id_movie==MainWindow.idSelectMovie).ToList();
                List<string> datesSession = new List<string>();
                foreach (Session session in sessions)
                {
                    string curDate = session.date.ToString().Substring(0, 10);
                    if (!datesSession.Contains(curDate))
                    {
                        datesSession.Add(curDate);
                    }
                }

                foreach (string dateSession in datesSession)
                {
                    Button btnDate = new Button() { 
                        Content = dateSession, 
                        Margin = new Thickness(0, 10, 10, 0) 
                    };
                    btnDate.Click += SelectDate;
                    DatesSession.Children.Add(btnDate);
                }

                string actors = "";
                foreach (Person person in context.Person.Where(p => p.PersonInMovie.Any(pm => pm.Movie.id == curMovie.id)).ToList()) 
                {
                    actors += person.full_name + ", ";
                }
                Actors.Text = actors;

                DurationMovie.Text = curMovie.duration.ToString("hh\\:mm");
                descriptionText.Text = curMovie.description;
            }
        }

        private void SelectDate(object sender, RoutedEventArgs e)
        {
            using (MovieEntities context = new MovieEntities()) { 
                string selectDate = (string)((Button)sender).Content;

                TitleSession.Text = "Сеансы " + selectDate;
                TitleSession.Visibility = Visibility.Visible;

                string[] partDate = selectDate.Split('.');
                DateTime today = new DateTime(Convert.ToInt32(partDate[2]), Convert.ToInt32(partDate[1]), Convert.ToInt32(partDate[0]));
                DateTime tomorrow = today.AddDays(1);
                List<Session> sessions = context.Session.Where(s => s.date >= today && s.date < tomorrow).ToList();  // OrderBy(s => s.date, Ascending)

                sessionInSelectDate.Children.Clear();
                foreach (var session in sessions)
                {
                    Button btnSession = new Button()
                    {
                        Content = String.Format("{0} - {1}", session.Hall.name, session.date.ToString("HH:mm")),
                        Tag = session.id,
                        Margin = new Thickness(0, 10, 10, 0)
                    };
                    btnSession.Click += SelectSession;
                    sessionInSelectDate.Children.Add(btnSession);
                }
            }
        }

        private void SelectSession(object sender, RoutedEventArgs e)
        {
            MainWindow.idSelectSession = Convert.ToInt32(((Button)sender).Tag);

            Window parentWindow = Window.GetWindow(this);
            Frame frame = LogicalTreeHelper.FindLogicalNode(parentWindow, "MainFrame") as Frame;
            frame.Navigate(new SelectTicket());
        }

        private void GenerateComments()
        {
            using (MovieEntities context = new MovieEntities())
            {
                List<Review> comments = context.Review.Where(r => r.id_movie == MainWindow.idSelectMovie).ToList();

                foreach (Review comment in comments)
                {
                    // Создаем родительский Grid
                    Grid parentGrid = new Grid();
                    parentGrid.Margin = new Thickness(0, 0, 0, 20);
                    parentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(85) });
                    parentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(10) });
                    parentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(500) });
                    parentGrid.HorizontalAlignment = HorizontalAlignment.Left;
                    if (comment.id_author == MainWindow.sessionUser.id)
                    {
                        parentGrid.Background = Brushes.Wheat;
                        CommentInput.Visibility = Visibility.Hidden;
                        CommentData.Visibility = Visibility.Hidden;
                    }

                    // Создаем Image и устанавливаем его Source
                    Image userImage = new Image();
                    userImage.Source = new BitmapImage(new Uri("C:\\Users\\gdugd\\source\\repos\\Kursovaya\\Kursovaya\\img\\profile.png"));
                    Grid.SetColumn(userImage, 0);

                    // Создаем StackPanel (внутренний StackPanel)
                    StackPanel innerStackPanel = new StackPanel();
                    innerStackPanel.Orientation = Orientation.Horizontal;

                    // Создаем TextBlock с именем пользователя
                    TextBlock userNameTextBlock = new TextBlock();
                    userNameTextBlock.FontSize = 18;
                    userNameTextBlock.FontWeight = FontWeights.DemiBold;
                    userNameTextBlock.Text = comment.User.full_name;

                    // Создаем StackPanel для звездочек
                    StackPanel starStackPanel = new StackPanel();
                    starStackPanel.Orientation = Orientation.Horizontal;
                    starStackPanel.Margin = new Thickness(10, 0, 0, 0);

                    // Создаем пять Image для звездочек
                    for (int i = 0; i < 5; i++)
                    {
                        Image star = new Image();
                        star.Source = starImage;
                        if (i < comment.rating) star.Source = fillStarImage;
                        star.Width = 14;
                        star.Height = 18;
                        star.Margin = new Thickness(1, 0, 1, 0);
                        starStackPanel.Children.Add(star);
                    }

                    // Создаем TextBlock для текстовых комментариев
                    TextBlock commentTextBlock1 = new TextBlock();
                    commentTextBlock1.FontSize = 15;
                    commentTextBlock1.Text = comment.text;
                    commentTextBlock1.TextWrapping = TextWrapping.Wrap;

                    // Создаем TextBlock для даты

                    TextBlock dateTextBlock = new TextBlock();
                    dateTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 170));
                    dateTextBlock.FontSize = 14;

                    CultureInfo cultureInfo = new CultureInfo("ru-RU");
                    string customFormat = "dd MMMM yyyy, HH:mm";
                    dateTextBlock.Text = comment.date.ToString(customFormat, cultureInfo); // "14 ноября 2022, 21:49";

                    dateTextBlock.Margin = new Thickness(0, 3, 0, 0);
                    dateTextBlock.HorizontalAlignment = HorizontalAlignment.Left;

                    // Добавляем все элементы в StackPanel (второй столбец Grid)
                    innerStackPanel.Children.Add(userNameTextBlock);
                    innerStackPanel.Children.Add(starStackPanel);

                    StackPanel outerStackPanel = new StackPanel();
                    outerStackPanel.Margin = new Thickness(3, 3, 3, 3);
                    outerStackPanel.Children.Add(innerStackPanel);
                    outerStackPanel.Children.Add(commentTextBlock1);
                    outerStackPanel.Children.Add(dateTextBlock);

                    Grid.SetColumn(outerStackPanel, 2);

                    // Добавляем все элементы на родительский Grid
                    parentGrid.Children.Add(userImage);
                    parentGrid.Children.Add(outerStackPanel);

                    // Добавляем родительский Grid на окно или другой контейнер
                    CommentsMovie.Children.Add(parentGrid);
                }
            }
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            int ratingNumber = Convert.ToInt32(((Image)sender).Tag);
            int i = 0;
            foreach (Image img in stars.Children)
            {
                i++;
                if (i <= ratingNumber)
                {
                    img.Source = fillStarImage;
                } else
                {
                    img.Source = starImage;
                }
            }
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {

            int i = 0;
            foreach (Image img in stars.Children)
            {
                i++;
                if (i <= SelectRating)
                {
                    img.Source = fillStarImage;
                }
                else
                {
                    img.Source = starImage;
                }
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectRating = Convert.ToInt32(((Image)sender).Tag);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (MovieEntities context = new MovieEntities())
            {
                Review comment = new Review()
                {
                    id_author = MainWindow.sessionUser.id,
                    text = CommentInput.Text,
                    rating = SelectRating,
                    date = DateTime.Now,
                    id_movie = MainWindow.idSelectMovie
                };

                context.Review.Add(comment);
                context.SaveChanges();

                CommentInput.Visibility = Visibility.Hidden;
                CommentData.Visibility = Visibility.Hidden;

                Grid parentGrid = new Grid();
                parentGrid.Margin = new Thickness(0, 0, 0, 20);
                parentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(85) });
                parentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(10) });
                parentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(500) });
                parentGrid.HorizontalAlignment = HorizontalAlignment.Left;
                if (comment.id_author == MainWindow.sessionUser.id)
                {
                    parentGrid.Background = Brushes.Wheat;
                }

                // Создаем Image и устанавливаем его Source
                Image userImage = new Image();
                userImage.Source = new BitmapImage(new Uri("C:\\Users\\gdugd\\source\\repos\\Kursovaya\\Kursovaya\\img\\profile.png"));
                Grid.SetColumn(userImage, 0);

                // Создаем StackPanel (внутренний StackPanel)
                StackPanel innerStackPanel = new StackPanel();
                innerStackPanel.Orientation = Orientation.Horizontal;

                // Создаем TextBlock с именем пользователя
                TextBlock userNameTextBlock = new TextBlock();
                userNameTextBlock.FontSize = 18;
                userNameTextBlock.FontWeight = FontWeights.DemiBold;
                userNameTextBlock.Text = MainWindow.sessionUser.full_name;

                // Создаем StackPanel для звездочек
                StackPanel starStackPanel = new StackPanel();
                starStackPanel.Orientation = Orientation.Horizontal;
                starStackPanel.Margin = new Thickness(10, 0, 0, 0);

                // Создаем пять Image для звездочек
                for (int i = 0; i < 5; i++)
                {
                    Image star = new Image();
                    star.Source = starImage;
                    if (i < comment.rating) star.Source = fillStarImage;
                    star.Width = 14;
                    star.Height = 18;
                    star.Margin = new Thickness(1, 0, 1, 0);
                    starStackPanel.Children.Add(star);
                }

                // Создаем TextBlock для текстовых комментариев
                TextBlock commentTextBlock1 = new TextBlock();
                commentTextBlock1.FontSize = 15;
                commentTextBlock1.Text = comment.text;
                commentTextBlock1.TextWrapping = TextWrapping.Wrap;

                // Создаем TextBlock для даты

                TextBlock dateTextBlock = new TextBlock();
                dateTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 170));
                dateTextBlock.FontSize = 14;

                CultureInfo cultureInfo = new CultureInfo("ru-RU");
                string customFormat = "dd MMMM YYYY, HH:mm";
                dateTextBlock.Text = comment.date.ToString(customFormat, cultureInfo); // "14 ноября 2022, 21:49";

                dateTextBlock.Margin = new Thickness(0, 3, 0, 0);
                dateTextBlock.HorizontalAlignment = HorizontalAlignment.Left;

                // Добавляем все элементы в StackPanel (второй столбец Grid)
                innerStackPanel.Children.Add(userNameTextBlock);
                innerStackPanel.Children.Add(starStackPanel);

                StackPanel outerStackPanel = new StackPanel();
                outerStackPanel.Margin = new Thickness(3, 3, 3, 3);
                outerStackPanel.Children.Add(innerStackPanel);
                outerStackPanel.Children.Add(commentTextBlock1);
                outerStackPanel.Children.Add(dateTextBlock);

                Grid.SetColumn(outerStackPanel, 2);

                // Добавляем все элементы на родительский Grid
                parentGrid.Children.Add(userImage);
                parentGrid.Children.Add(outerStackPanel);

                // Добавляем родительский Grid на окно или другой контейнер
                CommentsMovie.Children.Add(parentGrid);
            }
        }
    }
}

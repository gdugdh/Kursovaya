using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для SelectHallChange.xaml
    /// </summary>
    public partial class SelectHallChange : Page
    {
        public SelectHallChange()
        {
            InitializeComponent();
            GenerateButton();
        }

        private void GenerateButton ()
        {
            using (MovieEntities context = new MovieEntities())
            { 
                foreach (Hall hall in context.Hall.ToList())
                {
                    Button btn = new Button()
                    {
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        Content = hall.name,
                        Tag = hall.id.ToString(),
                        Background = Brushes.MediumSeaGreen,
                        Foreground = Brushes.White,
                        Width = 180,
                        Height = 30,
                        BorderBrush = Brushes.Transparent
                    };

                    btn.Click += createHallButton;

                    hallsButtons.Children.Add(btn);
                }
            }
        }

        private void createHallButton(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            MainWindow.idHall = Convert.ToInt32(btn.Tag);
            Console.WriteLine("ID HALL: " + btn.Tag);

            Window parentWindow = Window.GetWindow(this);

            Frame frame = LogicalTreeHelper.FindLogicalNode(parentWindow, "MainFrame") as Frame;
            frame.Navigate(new CreateHall());
        }
    }
}

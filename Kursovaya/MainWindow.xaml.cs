using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Globalization;
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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 
    /*
    public partial class MovieEntities1 : DbContext
    {
        private static MovieEntities _context;

        public MovieEntities1() : base("name=MovieEntities") { }

        public static MovieEntities GetContext()
        {
            if (_context == null) _context = new MovieEntities(); 
            return _context; 
        }
    }*/

    public partial class MainWindow : Window
    {

        public static int idSelectSession { get; set; }
        public static int idSelectMovie { get; set; }
        public static int idHall { get; set; }
        public static User sessionUser { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            // // Вызываем метод для создания мест в зале
            //MainFrame.Navigate(new SelectMovie());
            MainFrame.Navigate(new SelectHallChange());
            // MainFrame.Navigate(new Profile());
            // idSelectMovie = 2;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SelectMovie());
        }

        private void Logout(object sender, RoutedEventArgs e)
        {
            Authorization authorization = new Authorization();
            authorization.Show();
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MyTickets());
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Profile());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
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
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Page
    {
        public Registration()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<TextBox> Fields = new List<TextBox> { userFullName, userLogin, userPassword, userEmail, userPhone };
            foreach (TextBox item in Fields) {
                if (item.Text == null) {
                    WarningText.Text = "Пожалуйста заполните все поля";
                    return;
                }
            }

            using (MovieEntities context = new MovieEntities())
            {
                User newUser = new User
                {
                    full_name = Fields[0].Text,
                    login = Fields[1].Text,
                    password = Fields[2].Text,
                    email = Fields[3].Text,
                    phone = Fields[4].Text,
                    is_admin = false
                };
                try
                {
                    context.User.Add(newUser);
                    context.SaveChanges();
                } catch
                {
                    WarningText.Text = "Такой логин уже существует";
                    return;
                }
                LoginSuccess(newUser);
            }
        }
        private void LoginSuccess(User dataUser)
        {
            MainWindow.sessionUser = dataUser;
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            Window parentWindow = Window.GetWindow(this);

            if (parentWindow != null)
            {
                // Закрыть родительское окно
                parentWindow.Close();
            }
        }
    }
}

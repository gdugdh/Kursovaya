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
using static System.Net.Mime.MediaTypeNames;

namespace Kursovaya
{
    /// <summary>
    /// Логика взаимодействия для Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();
            
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // Показать анимацию загрузки
           // loadingControl.IsVisible = true;

            try
            {
                await Task.Run(() =>
                {
                    using (MovieEntities context = new MovieEntities())
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            var DataUser = context.User.Where(p => p.login == userLogin.Text && p.password == userPassword.Text).FirstOrDefault();

                            if (DataUser != null)
                            {
                                // Вернуться на главный поток для обновления интерфейса
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    LoginSuccess(DataUser);
                                });
                            }
                            else
                            {
                                // Вернуться на главный поток для обновления интерфейса
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    WarningText.Text = "Ваш пароль или логин не правильны";
                                });
                            }
                        });
                    }
                });
            }
            finally
            {
                // Скрыть анимацию загрузки после выполнения операции
                //loadingControl.IsVisible = false;
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            Frame frame = LogicalTreeHelper.FindLogicalNode(parentWindow, "MainFrame") as Frame;
            frame.Navigate(new Registration());
        }
    }
}

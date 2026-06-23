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
using System.Windows.Shapes;
using PlanPractice.Logic;

namespace PlanPractice.UI
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private bool isRegistrationMode = false;
        private const int MIN_PASSWORD_LENGHT= 6;
        private DataBaseManager Db;
        public AuthWindow(DataBaseManager db)
        {
            InitializeComponent();
            Db = db;
        }

        private void Action_Button_Click(object sender, RoutedEventArgs e)
        {
            //Db.OpenConnection();

            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;

            if (isRegistrationMode)
            {
                if (string.IsNullOrEmpty(LoginTextBox.Text) || string.IsNullOrEmpty(PasswordBox.Password) || string.IsNullOrEmpty(PasswordBox2.Password))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля!", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (PasswordBox.Password != PasswordBox2.Password)
                {
                    MessageBox.Show("Пароли не совпадают!", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (PasswordBox.Password.Length < MIN_PASSWORD_LENGHT)
                {
                    MessageBox.Show("Минимальная длинна пароля 6 символов!", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if(Db.Register(login, password))
                {
                    MessageBox.Show("Успешная регистрация аккаунта!", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Information);
                    Db.Login(login, password);
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Данный логин занят, попробуйте снова!", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                if (Db.Login(login, password))
                {
                    MessageBox.Show("Успешный вход в аккаунт!", "Вход в аккаунт", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль, попробуйте снова!", "Вход в аккаунт", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void ToggleMode_Button_Click(object sender, RoutedEventArgs e)
        {
            isRegistrationMode = !isRegistrationMode;

            if (isRegistrationMode)
            {
                TitleText.Text = "Регистрация нового пользователя";
                ActionText.Text = "Зарегистрироваться";
                ToggleMode_Text.Text = "Уже есть аккаунт? Войти";
                RepeatPasswordPanel.Visibility = Visibility.Visible;
            }
            else
            {
                TitleText.Text = "Авторизация";
                ActionText.Text = "Войти";
                ToggleMode_Text.Text = "Нет аккаунта? Зарегистрироваться";
                RepeatPasswordPanel.Visibility = Visibility.Collapsed;
            }
        }
    }
}

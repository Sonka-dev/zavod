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

namespace YP_Demo1_Mod4
{
    /// <summary>
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        private readonly int currentUserId;
        public AdminWindow(int userId)
        {
            InitializeComponent();
            currentUserId = userId;
            try
            {
                using (var db = new YP_Demo1Entities2())
                {
                    Users current = db.Users.First(u => u.idUser == currentUserId);
                    WelcomeText.Text = "Здравствуйте, " + current.login + " · " + current.post;
                    AdminPanel.Visibility = current.post == "Admin"
                        ? Visibility.Visible : Visibility.Collapsed;
                }
                if (AdminPanel.Visibility == Visibility.Visible) LoadUsers();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка базы данных: " + ex.Message); }
        }

        private void LoadUsers()
        {
            using (var db = new YP_Demo1Entities2())
                UsersGrid.ItemsSource = db.Users.OrderBy(u => u.login).ToList();
        }

        private void UsersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Users user = UsersGrid.SelectedItem as Users;
            if (user == null) return;
            UserLogin.Text = user.login;
            UserPassword.Clear();
            UserRole.SelectedIndex = user.post == "Admin" ? 0 : 1;
            UserBlocked.IsChecked = user.IsBlocked;
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            UsersGrid.SelectedItem = null;
            UserLogin.Clear();
            UserPassword.Clear();
            UserRole.SelectedIndex = 1;
            UserBlocked.IsChecked = false;
        }

        private string SelectedRole
        {
            get { return ((ComboBoxItem)UserRole.SelectedItem).Content.ToString(); }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            string login = UserLogin.Text.Trim();
            string password = UserPassword.Password;
            if (login.Length == 0 || password.Length == 0)
            {
                MessageBox.Show("Логин и пароль обязательны для нового пользователя.");
                return;
            }
            try
            {
                using (var db = new YP_Demo1Entities2())
                {
                    if (db.Users.Any(u => u.login == login))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует.");
                        return;
                    }
                    db.Users.Add(new Users
                    {
                        login = login,
                        password = password,
                        post = SelectedRole,
                        IsBlocked = UserBlocked.IsChecked == true
                    });
                    db.SaveChanges();
                }
                LoadUsers();
                ClearForm_Click(sender, e);
                MessageBox.Show("Пользователь добавлен.");
            }
            catch (Exception ex) { MessageBox.Show("Ошибка базы данных: " + ex.Message); }
        }

        private void SaveUser_Click(object sender, RoutedEventArgs e)
        {
            Users selected = UsersGrid.SelectedItem as Users;
            if (selected == null) { MessageBox.Show("Выберите пользователя в списке."); return; }
            string login = UserLogin.Text.Trim();
            if (login.Length == 0) { MessageBox.Show("Логин обязателен."); return; }
            try
            {
                using (var db = new YP_Demo1Entities2())
                {
                    if (db.Users.Any(u => u.login == login && u.idUser != selected.idUser))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует.");
                        return;
                    }
                    Users user = db.Users.First(u => u.idUser == selected.idUser);
                    user.login = login;
                    user.post = SelectedRole;
                    if (UserPassword.Password.Length > 0) user.password = UserPassword.Password;
                    user.IsBlocked = UserBlocked.IsChecked == true;
                    db.SaveChanges();
                }
                LoadUsers();
                MessageBox.Show("Данные пользователя сохранены.");
            }
            catch (Exception ex) { MessageBox.Show("Ошибка базы данных: " + ex.Message); }
        }
    }
}

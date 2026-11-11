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
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;
using System.Data.SqlTypes;
using System.Diagnostics.Eventing.Reader;

namespace YP_Demo1_Mod4
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public YP_Demo1Entities2 db = new YP_Demo1Entities2();
        Random _ran = new Random();
        public MainWindow()
        {
            InitializeComponent();
            Image[] images = { img1, img2, img3, img4 };
            currentIndexes["img1"] = 0;
            currentIndexes["img2"] = 0;
            currentIndexes["img3"] = 0;
            currentIndexes["img4"] = 0;
            GenerateCupcha(images);
        }
        public string[] pathImages = { "1.png", "2.png", "3.png", "4.png" }; //путь к картнкам капчи
        private Dictionary<string, int> currentIndexes = new Dictionary<string, int>(); //храним индексы для кадого поля картинки
        private int[] _correctIndexes = new int[4] { 0, 1, 2, 3}; //массив с правильными индексами картинок

        int countChanges = 3;
        void GenerateCupcha(Image[] images)
        {
            foreach (var image in images)
            {
                int index = _ran.Next(pathImages.Length);

                LoadImage(image, pathImages[index]);
            }
        }

        private void SwitchImage(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if(btn == null) return;
            string imageName = btn.Tag.ToString();

            Image targetImage = this.FindName(imageName) as Image;
            if (targetImage == null) return;

            int currentIndex = currentIndexes[imageName];
            currentIndex = (currentIndex + 1) % pathImages.Length;

            currentIndexes[imageName] = currentIndex;

            LoadImage(targetImage, pathImages[currentIndex]);
        }

        private void LoadImage(Image imageControl, string path)
        {
            if (!File.Exists(path)) return; // Пропускаем, если файла нет

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // Чтобы файл не блокировался
            bitmap.EndInit();

            imageControl.Source = bitmap;
        }

        private void Autorization_Click(object sender, RoutedEventArgs e)
        {
            string login = loginTxt.Text;
            string pass = passTxt.Text;
            if (login != "" && pass != "")
            {
                Users user = FindUser(login, pass);
                if(user != null && user.IsBlocked == false)
                {
                    if (!CheckCapcha())
                    {
                        MessageBox.Show($"Капча собрана неверно. осталось попыток {countChanges}");
                        countChanges--;
                        if(countChanges == 0)
                        {
                            BlockUser(user);
                            MessageBox.Show("Вы были заблокированы. Обратитесь к администратору");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Авторизация пройдена");
                        switch (user.post.ToLower())
                        {
                            case "admin":
                                AdminWindow win = new AdminWindow(user.idUser);
                                win.Show();
                                this.Close();
                                break;
                            default:
                                break;
                        }
                    }
                    
                }
                else if(user == null)
                {
                    MessageBox.Show("Пользователя с таким логином и паролем не найдено");
                }
                else if (user.IsBlocked == true)
                {
                    MessageBox.Show("Вы были заблокированы. Обратитесь к администратору");
                }
                
            }
            else
            {
                MessageBox.Show("Заполните поля login и password");
            }
        }

        Users FindUser(string login, string password)
        {
            var user = (from u in db.Users
                        where u.login == login && u.password == password
                        select u).FirstOrDefault();
            return user;
        }

        bool CheckCapcha()
        {
            bool isChecked = false;
            isChecked = currentIndexes["img1"] == _correctIndexes[0] &&
                        currentIndexes["img2"] == _correctIndexes[1] &&
                        currentIndexes["img3"] == _correctIndexes[2] &&
                        currentIndexes["img4"] == _correctIndexes[3];
            return isChecked;
        }

        void BlockUser(Users user)
        {
            var curUser = db.Users.FirstOrDefault(u => u.idUser == user.idUser);
            curUser.IsBlocked = true;
            db.SaveChanges();
        }
    }
}

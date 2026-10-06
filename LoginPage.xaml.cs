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

namespace KharisovaObuv
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();

        }


        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTB.Text;
            var user = KharisovaObuvEntities.GetContext().Users.FirstOrDefault(x => x.Login == login);
            if (user!=null)
            {
                UserSession.CurrentUser = user;
                MessageBox.Show("Добро пожаловать, " + user.FirstName);
                Manager.MainFrame.Navigate(new ProductPage());
            }
            else
            {
                MessageBox.Show("Пользователь не найден");
            }

        }

        private void GuestBtn_Click(object sender, RoutedEventArgs e)
        {
            UserSession.CurrentUser = null;
            Manager.MainFrame.Navigate(new ProductPage());
        }
    }
}

using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KharisovaObuv
{
    public partial class OrdersPage : Page
    {
        public OrdersPage()
        {
            InitializeComponent();

            LoadOrders();
        }


        private void LoadOrders()
        {
            var orders = KharisovaObuvEntities
                .GetContext()
                .Orders
                .Select(x => new OrderView
                {
                    OrderID = x.OrderID,
                    OrderDate = x.OrderDate,

                    ClientFullName =
                        x.Users.LastName + " " +
                        x.Users.FirstName + " " +
                        x.Users.MiddleName,

                    TotalAmount = x.TotalAmount
                })
                .ToList();


            OrdersDG.ItemsSource = orders;
        }


        private void AddOrderBtn_Click(
            object sender,
            RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(
                new OrderEditPage());
        }


        private void OpenOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersDG.SelectedItem == null)
            {
                MessageBox.Show("Выберите заказ");
                return;
            }

            OrderView selectedOrder =
                OrdersDG.SelectedItem as OrderView;

            Manager.MainFrame.Navigate(
                new OrderDetailsPage(
                    selectedOrder.OrderID));
        }

        private void DeleteOrderBtn_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (OrdersDG.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите заказ");

                return;
            }


            OrderView selectedOrder =
                OrdersDG.SelectedItem as OrderView;


            var order = KharisovaObuvEntities
                .GetContext()
                .Orders
                .FirstOrDefault(x =>
                    x.OrderID == selectedOrder.OrderID);


            if (order == null)
                return;


            var result = MessageBox.Show(
                "Удалить заказ № " +
                order.OrderID + "?",
                "Подтверждение",
                MessageBoxButton.YesNo);


            if (result != MessageBoxResult.Yes)
                return;


            var orderItems = KharisovaObuvEntities
                .GetContext()
                .OrderItems
                .Where(x =>
                    x.OrderID == order.OrderID)
                .ToList();


            KharisovaObuvEntities
                .GetContext()
                .OrderItems
                .RemoveRange(orderItems);


            KharisovaObuvEntities
                .GetContext()
                .Orders
                .Remove(order);


            KharisovaObuvEntities
                .GetContext()
                .SaveChanges();


            MessageBox.Show(
                "Заказ удалён");


            LoadOrders();
        }
    }
}
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KharisovaObuv
{
    public partial class OrderDetailsPage : Page
    {
        // ID заказа, который сейчас просматриваем
        private int orderId;


        public OrderDetailsPage(int id)
        {
            InitializeComponent();

            // Проверяем авторизацию
            if (UserSession.CurrentUser == null)
            {
                MessageBox.Show(
                    "Для просмотра заказов необходимо авторизоваться");

                Manager.MainFrame.GoBack();
                return;
            }


            // Обычный авторизованный пользователь
            // не должен просматривать список чужих заказов
            if (UserSession.CurrentUser.RoleID != 2 &&
                UserSession.CurrentUser.RoleID != 3)
            {
                MessageBox.Show(
                    "У вас нет доступа к просмотру заказов");

                Manager.MainFrame.GoBack();
                return;
            }


            // Запоминаем номер заказа
            orderId = id;


            // Загружаем заказ
            LoadOrder();


            // Редактировать заказ может только администратор
            if (UserSession.CurrentUser.RoleID == 2)
            {
                EditOrderBtn.Visibility =
                    Visibility.Visible;
            }
            else
            {
                EditOrderBtn.Visibility =
                    Visibility.Collapsed;
            }
        }


        // Загрузка информации о заказе
        private void LoadOrder()
        {
            var order = KharisovaObuvEntities
                .GetContext()
                .Orders
                .FirstOrDefault(x =>
                    x.OrderID == orderId);


            // Если заказ не найден
            if (order == null)
            {
                MessageBox.Show(
                    "Заказ не найден");

                return;
            }


            // Заголовок
            OrderTitleTB.Text =
                "Заказ № " +
                order.OrderID;


            // ФИО клиента
            ClientTB.Text =
                "Клиент: " +
                order.Users.LastName + " " +
                order.Users.FirstName + " " +
                order.Users.MiddleName;


            // Дата заказа
            DateTB.Text =
                "Дата: " +
                order.OrderDate
                .ToString("dd.MM.yyyy");


            // Получаем состав заказа
            var items = KharisovaObuvEntities
                .GetContext()
                .OrderItems
                .Where(x =>
                    x.OrderID == orderId)
                .Select(x =>
                    new OrderItemView
                    {
                        StockItemID =
                            x.StockItemID,

                        ProductName =
                            x.StockItems
                            .Products
                            .ProductName,

                        SizeValue =
                            x.StockItems
                            .Sizes
                            .SizeValue,

                        Quantity =
                            x.Quantity,

                        UnitPrice =
                            x.UnitPrice
                    })
                .ToList();


            // Показываем позиции заказа
            OrderItemsDG.ItemsSource =
                items;


            // Итоговая сумма
            TotalTB.Text =
                "Итого: " +
                order.TotalAmount
                .ToString("0.00") +
                " руб.";
        }


        // Кнопка редактирования
        private void EditOrderBtn_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Дополнительная проверка:
            // редактировать может только администратор
            if (UserSession.CurrentUser == null ||
                UserSession.CurrentUser.RoleID != 2)
            {
                MessageBox.Show(
                    "Редактирование доступно только администратору");

                return;
            }


            MessageBox.Show(
                "Переходим к редактированию заказа № " +
                orderId);
        }
    }
}
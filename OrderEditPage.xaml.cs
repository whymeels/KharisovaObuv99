using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KharisovaObuv
{
    public partial class OrderEditPage : Page
    {
        // Товар, который может быть передан из каталога
        private Products selectedProduct;

        // Временный список позиций текущего заказа
        private List<OrderItemView> currentOrderItems =
            new List<OrderItemView>();


        // Открытие пустой страницы создания заказа
        public OrderEditPage()
        {
            InitializeComponent();

            // Гость не может оформлять заказ
            if (UserSession.CurrentUser == null)
            {
                MessageBox.Show(
                    "Для оформления заказа необходимо авторизоваться");

                Manager.MainFrame.GoBack();
                return;
            }

            LoadData();
        }


        // Открытие страницы с уже выбранным товаром из каталога
        public OrderEditPage(Products product)
        {
            InitializeComponent();

            // Гость не может оформлять заказ
            if (UserSession.CurrentUser == null)
            {
                MessageBox.Show(
                    "Для оформления заказа необходимо авторизоваться");

                Manager.MainFrame.GoBack();
                return;
            }

            LoadData();

            selectedProduct = product;

            // Автоматически выбираем переданный товар
            ProductCB.SelectedItem = selectedProduct;
        }


        // Загрузка данных на страницу
        private void LoadData()
        {
            // Загружаем пользователей
            ClientCB.ItemsSource =
                KharisovaObuvEntities
                .GetContext()
                .Users
                .ToList();

            // В ComboBox будет показываться ФИО
            ClientCB.DisplayMemberPath = "FullName";


            // Загружаем товары
            ProductCB.ItemsSource =
                KharisovaObuvEntities
                .GetContext()
                .Products
                .ToList();


            // Текущая дата
            OrderDateDP.SelectedDate =
                DateTime.Today;


            // Если вошёл обычный авторизованный пользователь,
            // заказ оформляется только на него
            if (UserSession.CurrentUser.RoleID == 1)
            {
                ClientCB.SelectedItem =
                    UserSession.CurrentUser;

                ClientCB.IsEnabled = false;
            }
        }


        // Выбор товара
        private void ProductCB_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (ProductCB.SelectedItem == null)
                return;


            Products product =
                ProductCB.SelectedItem as Products;


            // Получаем доступные размеры выбранного товара
            var sizes = KharisovaObuvEntities
                .GetContext()
                .StockItems
                .Where(x =>
                    x.ProductID == product.ProductID &&
                    x.QuantityAvailable > 0)
                .Select(x => x.Sizes)
                .ToList();


            SizeCB.ItemsSource = sizes;

            // Сбрасываем предыдущий размер
            SizeCB.SelectedIndex = -1;

            // Пока размер не выбран
            StockTB.Text = "В наличии: -";
        }


        // Выбор размера
        private void SizeCB_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (ProductCB.SelectedItem == null ||
                SizeCB.SelectedItem == null)
                return;


            Products product =
                ProductCB.SelectedItem as Products;

            Sizes size =
                SizeCB.SelectedItem as Sizes;


            // Ищем товарную позицию:
            // товар + размер
            var stockItem = KharisovaObuvEntities
                .GetContext()
                .StockItems
                .FirstOrDefault(x =>
                    x.ProductID == product.ProductID &&
                    x.SizeID == size.SizeID);


            if (stockItem != null)
            {
                StockTB.Text =
                    "В наличии: " +
                    stockItem.QuantityAvailable;
            }
        }


        // Добавление позиции в заказ
        private void AddItemBtn_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ProductCB.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите товар");

                return;
            }


            if (SizeCB.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите размер");

                return;
            }


            int quantity;


            if (!int.TryParse(
                    QuantityTB.Text,
                    out quantity) ||
                quantity <= 0)
            {
                MessageBox.Show(
                    "Введите корректное количество");

                return;
            }


            Products product =
                ProductCB.SelectedItem as Products;

            Sizes size =
                SizeCB.SelectedItem as Sizes;


            // Находим конкретный StockItemID
            var stockItem = KharisovaObuvEntities
                .GetContext()
                .StockItems
                .FirstOrDefault(x =>
                    x.ProductID == product.ProductID &&
                    x.SizeID == size.SizeID);


            if (stockItem == null)
            {
                MessageBox.Show(
                    "Товарная позиция не найдена");

                return;
            }


            // Нельзя добавить больше, чем есть на складе
            if (quantity >
                stockItem.QuantityAvailable)
            {
                MessageBox.Show(
                    "Недостаточно товара. В наличии: " +
                    stockItem.QuantityAvailable);

                return;
            }


            // Проверяем, есть ли эта позиция уже в заказе
            var existingItem =
                currentOrderItems
                .FirstOrDefault(x =>
                    x.StockItemID ==
                    stockItem.StockItemID);


            // Если уже есть - увеличиваем количество
            if (existingItem != null)
            {
                if (existingItem.Quantity +
                    quantity >
                    stockItem.QuantityAvailable)
                {
                    MessageBox.Show(
                        "Нельзя добавить больше, чем есть в наличии");

                    return;
                }

                existingItem.Quantity += quantity;
            }
            else
            {
                // Создаём новую строку заказа
                OrderItemView item =
                    new OrderItemView();

                item.StockItemID =
                    stockItem.StockItemID;

                item.ProductName =
                    product.ProductName;

                item.SizeValue =
                    size.SizeValue;

                item.Quantity =
                    quantity;

                item.UnitPrice =
                    product.Price;


                currentOrderItems.Add(item);
            }


            // Обновляем таблицу
            OrderItemsDG.ItemsSource = null;

            OrderItemsDG.ItemsSource =
                currentOrderItems;


            UpdateTotal();
        }


        // Удаление позиции из заказа
        private void DeleteItemBtn_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (OrderItemsDG.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите позицию для удаления");

                return;
            }


            OrderItemView selectedItem =
                OrderItemsDG.SelectedItem
                as OrderItemView;


            currentOrderItems.Remove(
                selectedItem);


            OrderItemsDG.ItemsSource = null;

            OrderItemsDG.ItemsSource =
                currentOrderItems;


            UpdateTotal();
        }


        // Изменение количества прямо в таблице
        private void OrderItemsDG_CellEditEnding(
            object sender,
            DataGridCellEditEndingEventArgs e)
        {
            // Колонка количества имеет индекс 2
            if (e.Column.DisplayIndex != 2)
                return;


            OrderItemView item =
                e.Row.Item as OrderItemView;


            TextBox textBox =
                e.EditingElement as TextBox;


            int newQuantity;


            if (!int.TryParse(
                    textBox.Text,
                    out newQuantity) ||
                newQuantity <= 0)
            {
                MessageBox.Show(
                    "Введите корректное количество");

                e.Cancel = true;
                return;
            }


            // Получаем остаток для этой товарной позиции
            var stockItem =
                KharisovaObuvEntities
                .GetContext()
                .StockItems
                .FirstOrDefault(x =>
                    x.StockItemID ==
                    item.StockItemID);


            // Проверяем остаток
            if (stockItem != null &&
                newQuantity >
                stockItem.QuantityAvailable)
            {
                MessageBox.Show(
                    "В наличии только: " +
                    stockItem.QuantityAvailable);

                e.Cancel = true;
                return;
            }


            item.Quantity = newQuantity;


            // После редактирования обновляем сумму
            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    OrderItemsDG.Items.Refresh();

                    UpdateTotal();
                }));
        }


        // Пересчёт общей суммы заказа
        private void UpdateTotal()
        {
            decimal total =
                currentOrderItems
                .Sum(x => x.LineTotal);


            TotalTB.Text =
                "Итого: " +
                total.ToString("0.00") +
                " руб.";
        }


        // Сохранение заказа
        private void SaveOrderBtn_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Проверяем клиента
            if (ClientCB.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите клиента");

                return;
            }


            // Проверяем дату
            if (OrderDateDP.SelectedDate == null)
            {
                MessageBox.Show(
                    "Выберите дату заказа");

                return;
            }


            // В заказе должна быть хотя бы одна позиция
            if (currentOrderItems.Count == 0)
            {
                MessageBox.Show(
                    "Добавьте хотя бы одну позицию");

                return;
            }


            Users client =
                ClientCB.SelectedItem as Users;


            // Создаём заказ
            Orders order =
                new Orders();


            order.OrderDate =
                OrderDateDP.SelectedDate.Value;


            order.UserID =
                client.UserID;


            order.TotalAmount =
                currentOrderItems
                .Sum(x => x.LineTotal);


            // Добавляем заказ
            KharisovaObuvEntities
                .GetContext()
                .Orders
                .Add(order);


            // Сохраняем, чтобы появился OrderID
            KharisovaObuvEntities
                .GetContext()
                .SaveChanges();


            // Добавляем позиции заказа
            foreach (var item in currentOrderItems)
            {
                OrderItems orderItem =
                    new OrderItems();


                orderItem.OrderID =
                    order.OrderID;


                orderItem.StockItemID =
                    item.StockItemID;


                orderItem.Quantity =
                    item.Quantity;


                orderItem.UnitPrice =
                    item.UnitPrice;


                KharisovaObuvEntities
                    .GetContext()
                    .OrderItems
                    .Add(orderItem);


                // Находим складскую позицию
                var stockItem =
                    KharisovaObuvEntities
                    .GetContext()
                    .StockItems
                    .FirstOrDefault(x =>
                        x.StockItemID ==
                        item.StockItemID);


                // Уменьшаем остаток
                if (stockItem != null)
                {
                    stockItem.QuantityAvailable -=
                        item.Quantity;
                }
            }


            // Сохраняем позиции и остатки
            KharisovaObuvEntities
                .GetContext()
                .SaveChanges();


            MessageBox.Show(
                "Заказ успешно сохранён");


            // Возвращаемся назад
            Manager.MainFrame.GoBack();
        }
    }
}
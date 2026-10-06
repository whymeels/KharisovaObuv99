using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KharisovaObuv
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        public ProductPage()
        {
            InitializeComponent();


            // Кнопка "Заказы" доступна
            // только администратору и менеджеру
            if (UserSession.CurrentUser != null &&
                (UserSession.CurrentUser.RoleID == 2 ||
                 UserSession.CurrentUser.RoleID == 3))
            {
                OrdersBtn.Visibility =
                    Visibility.Visible;
            }
            else
            {
                OrdersBtn.Visibility =
                    Visibility.Collapsed;
            }


            // Если пользователь вошёл как гость
            if (UserSession.CurrentUser == null)
            {
                // Гостю недоступны поиск,
                // сортировка и фильтрация
                SearchTB.IsEnabled = false;

                SortCB.IsEnabled = false;

                FilterCB.IsEnabled = false;


                // Гость не может оформлять заказ
                AddToOrderBtn.Visibility =
                    Visibility.Collapsed;

                OpenOrderBtn.Visibility =
                    Visibility.Collapsed;
            }


            // Получаем категории из базы
            var categories =
                KharisovaObuvEntities
                .GetContext()
                .Categories
                .ToList();


            // Добавляем пункт "Все категории"
            categories.Insert(
                0,
                new Categories
                {
                    CategoryName =
                        "Все категории"
                });


            // Передаём категории в ComboBox
            FilterCB.ItemsSource =
                categories;


            // По умолчанию:
            // все категории
            FilterCB.SelectedIndex = 0;


            // По умолчанию:
            // без сортировки
            SortCB.SelectedIndex = 0;


            // Загружаем каталог
            UpdateProducts();
        }


        // Обновление списка товаров
        private void UpdateProducts()
        {
            // Получаем все товары
            var currentProducts =
                KharisovaObuvEntities
                .GetContext()
                .Products
                .ToList();


            // Фильтрация по категории
            if (FilterCB.SelectedIndex > 0)
            {
                Categories selectedCategory =
                    FilterCB.SelectedItem
                    as Categories;


                if (selectedCategory != null)
                {
                    currentProducts =
                        currentProducts
                        .Where(p =>
                            p.CategoryID ==
                            selectedCategory.CategoryID)
                        .ToList();
                }
            }


            // Поиск по названию товара
            if (!string.IsNullOrWhiteSpace(
                    SearchTB.Text))
            {
                string searchText =
                    SearchTB.Text
                    .ToLower();


                currentProducts =
                    currentProducts
                    .Where(p =>
                        p.ProductName
                        .ToLower()
                        .Contains(searchText))
                    .ToList();
            }


            // Цена по возрастанию
            if (SortCB.SelectedIndex == 1)
            {
                currentProducts =
                    currentProducts
                    .OrderBy(p =>
                        p.Price)
                    .ToList();
            }


            // Цена по убыванию
            if (SortCB.SelectedIndex == 2)
            {
                currentProducts =
                    currentProducts
                    .OrderByDescending(p =>
                        p.Price)
                    .ToList();
            }


            // Показываем товары
            ProductListView.ItemsSource =
                currentProducts;
        }


        // Поиск
        private void SearchTB_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            UpdateProducts();
        }


        // Сортировка
        private void SortCB_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }


        // Фильтрация
        private void FilterCB_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }


        // Выбор товара в каталоге
        private void ProductListView_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            // Гость не может
            // добавлять товары в заказ
            if (UserSession.CurrentUser == null)
            {
                AddToOrderBtn.Visibility =
                    Visibility.Collapsed;

                return;
            }


            // Если товар выбран,
            // показываем кнопку
            if (ProductListView.SelectedItem != null)
            {
                AddToOrderBtn.Visibility =
                    Visibility.Visible;
            }
            else
            {
                AddToOrderBtn.Visibility =
                    Visibility.Collapsed;
            }
        }


        // Добавление выбранного товара в заказ
        private void AddToOrderBtn_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Дополнительная проверка
            if (UserSession.CurrentUser == null)
            {
                MessageBox.Show(
                    "Для оформления заказа необходимо авторизоваться");

                return;
            }


            Products selectedProduct =
                ProductListView.SelectedItem
                as Products;


            if (selectedProduct == null)
            {
                MessageBox.Show(
                    "Выберите товар");

                return;
            }


            // Открываем страницу создания заказа
            // и передаём выбранный товар
            Manager.MainFrame.Navigate(
                new OrderEditPage(
                    selectedProduct));
        }


        // Открытие текущего заказа
        private void OpenOrderBtn_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (UserSession.CurrentUser == null)
            {
                MessageBox.Show(
                    "Для оформления заказа необходимо авторизоваться");

                return;
            }


            Manager.MainFrame.Navigate(
                new OrderEditPage());
        }


        // Открытие списка готовых заказов
        private void OrdersBtn_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Пользователь должен быть авторизован
            if (UserSession.CurrentUser == null)
            {
                MessageBox.Show(
                    "Для просмотра заказов необходимо авторизоваться");

                return;
            }


            // Только администратор и менеджер
            if (UserSession.CurrentUser.RoleID != 2 &&
                UserSession.CurrentUser.RoleID != 3)
            {
                MessageBox.Show(
                    "У вас нет доступа к списку заказов");

                return;
            }


            // Переходим к списку заказов
            Manager.MainFrame.Navigate(
                new OrdersPage());
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZXing;

namespace Kursovaya
{
    /// <summary>
    /// Логика взаимодействия для MyTickets.xaml
    /// </summary>
    public partial class MyTickets : Page
    {
        int idMovie;
        int idSession;
        Ticket selectedTicket;

        private bool mousePressing = false;
        private Point previousMousePosition;
        private int zoom = 100;
        private double zc = 0;

        private double scrX = 0.5;
        private double scrY = 0.5;
        private double deltaW = 0;
        private double deltaH = 0;

        private List<Place> places = new List<Place>() { };
        private List<ElementHall> elements = new List<ElementHall>() { };
        private List<PriceCategory> priceCategories = new List<PriceCategory>() { };

        public MyTickets()
        {
            InitializeComponent();
            idMovie = 2;
            idSession = 1;
            using (MovieEntities context = new MovieEntities())
            {
                selectedTicket = context.Ticket.FirstOrDefault();
            }

            PrintMovieData();
            GetHallElement();
            GeneratePriceCategoryTypesSP();
            zc = (zoom / 100.0);
            deltaW = CanvasHall.Width * (zc - 1);
            deltaH = CanvasHall.Height * (zc - 1);
            RenderingHall();
        }

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null; // Сбросить курсор при уходе с Canvas
            mousePressing = false;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand; // Изменить курсор при клике на Canvas
            mousePressing = true;
            previousMousePosition = e.GetPosition(sender as IInputElement);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = e.GetPosition(sender as IInputElement);

            if (mousePressing && e.LeftButton == MouseButtonState.Pressed)
            {
                double deltaX = previousMousePosition.X - currentMousePosition.X;
                double deltaY = previousMousePosition.Y - currentMousePosition.Y;
                previousMousePosition = currentMousePosition;

                if (deltaX != 0) scrX += deltaX / deltaW;
                if (scrX > 1) scrX = 1;
                if (scrX < 0) scrX = 0;


                if (deltaY != 0) scrY += deltaY / deltaH;
                if (scrY > 1) scrY = 1;
                if (scrY < 0) scrY = 0;

                RenderingHall();
            }
            else
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void InZoom_Click(object sender, RoutedEventArgs e)
        {
            if (zoom < 150)
            {
                zoom += 10;
                zc = (zoom / 100.0);
                deltaW = CanvasHall.Width * (zc - 1);
                deltaH = CanvasHall.Height * (zc - 1);
                RenderingHall();
            }
        }

        private void OutZoom_Click(object sender, RoutedEventArgs e)
        {
            if (zoom != 100)
            {
                zoom -= 10;
                zc = (zoom / 100.0);
                deltaW = CanvasHall.Width * (zc - 1);
                deltaH = CanvasHall.Height * (zc - 1);
                RenderingHall();
            }
        }

        private void RenderingHall()
        {
            CanvasHall.Children.Clear();

            foreach (Place place in places)
            {
                SolidColorBrush clr = (SolidColorBrush)(new BrushConverter().ConvertFrom(priceCategories[place.id_price_category - 1].color));
                Button btn = new Button()
                {
                    Width = place.width * zc,
                    Height = place.height * zc,
                    Background = clr,
                    Foreground = clr,
                    Content = place.column.ToString(),
                    Style = (Style)FindResource("RoundedButtonStyle"),
                };
                if (place.id == selectedTicket.id_place)
                {
                    btn.Resources["ButtonBorderBrush"] = clr;
                    btn.Background = new SolidColorBrush(Colors.White);
                }

                CanvasHall.Children.Add(btn);
                Canvas.SetLeft(btn, place.X * zc - deltaW * scrX);
                Canvas.SetTop(btn, place.Y * zc - deltaH * scrY);
            }

            foreach (ElementHall element in elements)
            {
                if (element.text == "")
                {
                    Rectangle rect = new Rectangle()
                    {
                        Width = element.width * zc,
                        Height = element.height * zc,
                        Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom(element.color))
                    };

                    CanvasHall.Children.Add(rect);
                    Canvas.SetLeft(rect, element.X * zc - deltaW * scrX);
                    Canvas.SetTop(rect, element.Y * zc - deltaH * scrY);
                }
                else
                {
                    TextBlock txtBlock = new TextBlock()
                    {
                        Width = element.width * zc,
                        Height = element.height * zc,
                        FontSize = 13 + (zoom - 100) / 10,
                        Text = element.text,
                    };

                    CanvasHall.Children.Add(txtBlock);
                    Canvas.SetLeft(txtBlock, element.X * zc - deltaW * scrX);
                    Canvas.SetTop(txtBlock, element.Y * zc - deltaH * scrY);
                }
            }
        }

        private void GeneratePriceCategoryTypesSP()
        {
            PriceCategoryTypesSP.Children.Clear();
            foreach (PriceCategory pc in priceCategories)
            {
                Button btn = new Button()
                {
                    Width = 150,
                    Height = 45,
                    BorderBrush = new SolidColorBrush(Colors.Transparent),
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Content = pc.name,
                    FontSize = 20,
                    Foreground = new SolidColorBrush(Colors.White),
                    Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(pc.color)),
                    Margin = new Thickness(0, 0, 15, 0)
                };

                PriceCategoryTypesSP.Children.Add(btn);
            }
        }

        private void GetHallElement()
        {
            using (MovieEntities context = new MovieEntities())
            {
                Session curSession = context.Session.Where(s => s.id == idSession).FirstOrDefault();

                elements = context.ElementHall.Where(e => e.id_hall == curSession.id_hall).ToList();
                places = context.Place.Where(e => e.id_hall == curSession.id_hall).ToList();
                priceCategories = context.PriceCategory.ToList();
            }
        }

        private void PrintMovieData()
        {
            using (MovieEntities context = new MovieEntities())
            {
                Session curSession = context.Session.Where(s => s.id == idSession).FirstOrDefault();

                DurationMovie.Text = curSession.Movie.duration.ToString("hh\\:mm");
                TitleMovie.Text = curSession.Movie.name;

                CultureInfo cultureInfo = new CultureInfo("ru-RU");
                string customFormat = "dddd, d MMMM, HH:mm";
                SelectSession.Text = curSession.date.ToString(customFormat, cultureInfo);

                SelectHall.Text = curSession.Hall.name;
            }
        }

        /*
        private void ViewPlace(object sender, MouseButtonEventArgs e)
        {
            idMovie = 1;
            CreateRowsAndColumns();
            LoadSeatLayout();
        }

        private void GenerateBarcodeImage()
        {
            var ticketNumber = "00000001"; // Замените на свой номер билета
            var barcodeBitmap = GenerateBarcodeBitmap(ticketNumber);

            if (barcodeBitmap != null)
            {
                barcodeImage.Source = ConvertBitmapToImageSource(barcodeBitmap);
            }
        }

        private BitmapSource ConvertBitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            var hBitmap = bitmap.GetHbitmap();
            var source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(hBitmap); // Освободим ресурсы

            return source;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private System.Drawing.Bitmap GenerateBarcodeBitmap(string content)
        {
            var barcode = new ZXing.BarcodeWriter();
            barcode.Format = ZXing.BarcodeFormat.CODE_128;
            barcode.Options = new ZXing.Common.EncodingOptions
            {
                Width = 200, // Устанавливает ширину изображения
                Height = 50, // Устанавливает высоту изображения
                Margin = 0, // Устанавливает поля вокруг штрих-кода
            };
            barcode.Options.PureBarcode = true;
            barcode.Options.NoPadding = true;
            try
            {
                var bitmap = barcode.Write(content);
                return bitmap;
            }
            catch (Exception ex)
            {
                // Обработайте исключение, если что-то пошло не так
                MessageBox.Show("Ошибка при создании штрих-кода: " + ex.Message);
                return null;
            }
        }
        */
    }
}

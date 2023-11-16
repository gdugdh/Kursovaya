using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
using System.Xml.Linq;
using Microsoft.AspNetCore.Components;
using static System.Collections.Specialized.BitVector32;

namespace Kursovaya
{
    /// <summary>
    /// Логика взаимодействия для SelectTicket.xaml
    /// </summary>
    /// 

    public partial class SelectTicket : Page
    {
        int idSession;
        int idMovie;

        int allPrice = 0;
        List<int> reservedPlaces = new List<int>();
        List<int> selectedPlaces = new List<int>();

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
        public SelectTicket()
        {
            InitializeComponent();
            idSession = MainWindow.idSelectSession;
            idMovie = MainWindow.idSelectMovie;
            PrintMovieData();
            //CreatePlace();
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

                double coefDeltaX = deltaX / deltaW;
                double coefDeltaY = deltaY / deltaH;

                scrX += coefDeltaX;
                if (scrX > 1) scrX = 1;
                if (scrX < 0) scrX = 0;


                scrY += coefDeltaY;
                if (scrY > 1) scrY = 1;
                if (scrY < 0) scrY = 0;

                foreach (UIElement child in CanvasHall.Children)
                {
                    if (!((scrX == 1 && coefDeltaX > 0) || (scrX == 0 && coefDeltaX < 0))) Canvas.SetLeft(child, Canvas.GetLeft(child) - deltaX);
                    if (!((scrY == 1 && coefDeltaY > 0) || (scrY == 0 && coefDeltaY < 0))) Canvas.SetTop(child, Canvas.GetTop(child) - deltaY);
                }
                //RenderingHall();

                previousMousePosition = currentMousePosition;
            } else
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

            int placeIndex = 0;
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
                    Tag = placeIndex.ToString()
                };
                btn.Resources["ButtonBorderBrush"] = Brushes.White;

                if (selectedPlaces.Contains(placeIndex))
                {
                    btn.Resources["ButtonBorderBrush"] = clr;
                    btn.Background = new SolidColorBrush(Colors.White);
                } else if (reservedPlaces.Contains(place.id))
                {
                    btn.Background = Brushes.Gray;
                }

                btn.Click += SeatButton_Click;

                CanvasHall.Children.Add(btn);
                Canvas.SetLeft(btn, place.X * zc - deltaW * scrX);
                Canvas.SetTop(btn, place.Y * zc - deltaH * scrY);

                placeIndex++;
            }

            foreach (ElementHall element in elements)
            {
                if (element.text == "") {
                    Rectangle rect = new Rectangle()
                    {
                        Width = element.width * zc,
                        Height = element.height * zc,
                        Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom(element.color))
                    };

                    CanvasHall.Children.Add(rect);
                    Canvas.SetLeft(rect, element.X * zc - deltaW * scrX);
                    Canvas.SetTop(rect, element.Y * zc - deltaH * scrY);
                } else
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
                foreach (ElementHall el in context.ElementHall.Where(e => e.id_hall == curSession.id_hall).ToList())
                {
                    elements.Add(el);
                }

                foreach (Place p in context.Place.Where(e => e.id_hall == curSession.id_hall))
                {
                    places.Add(p);
                }

                foreach (PriceCategory pc in context.PriceCategory.ToList())
                {
                    priceCategories.Add(pc);
                }

                foreach (Ticket t in context.Ticket.Where(tckt => tckt.Place.id_hall == curSession.id_hall && tckt.id_user == MainWindow.sessionUser.id))
                {
                    reservedPlaces.Add(t.id_place);
                }
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
                // formattedDateTime содержит "Среда, 15 ноября, 18:00"

                SelectHall.Text = curSession.Hall.name;

                ChangeReservedPlaces();
                ChangePrice();
            }
        }

        private void SeatButton_Click(object sender, RoutedEventArgs e)
        {
            // Обработка клика на месте
            Button clickedSeat = (Button)sender;
            if (clickedSeat.Background == Brushes.Gray)
            {
                return;
            }
            int indexPlace = Convert.ToInt32(clickedSeat.Tag);
            if (clickedSeat.Resources["ButtonBorderBrush"] == Brushes.White)
            {
                clickedSeat.Resources["ButtonBorderBrush"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(priceCategories[places[indexPlace].id_price_category - 1].color));
                clickedSeat.Background = new SolidColorBrush(Colors.White);
                selectedPlaces.Add(indexPlace);
                allPrice += Decimal.ToInt32(priceCategories[places[indexPlace].id_price_category - 1].price);
            } else
            {
                clickedSeat.Resources["ButtonBorderBrush"] = Brushes.White;
                clickedSeat.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(priceCategories[places[indexPlace].id_price_category - 1].color));
                selectedPlaces.Remove(indexPlace);
                allPrice -= Decimal.ToInt32(priceCategories[places[indexPlace].id_price_category - 1].price);
            }
            ChangeReservedPlaces();
            ChangePrice();
        }

        private void ChangePrice()
        {
            FinalPrice.Text = $"{allPrice} руб";
        }

        private void ChangeReservedPlaces()
        {
            if (selectedPlaces.Count > 0)
            {
                string selectPlacesText = "";
                foreach (int indexPlace in selectedPlaces)
                {
                    selectPlacesText += $"Ряд {places[indexPlace].row} Место {places[indexPlace].column},";
                }
                SelectPlaces.Text = selectPlacesText;
            } else
            {
                SelectPlaces.Text = "Вы их не выбрали(";
            }
        }

        private void BuyTickets(object sender, RoutedEventArgs e)
        {
            if (selectedPlaces.Count == 0) return;
            using (MovieEntities context = new MovieEntities())
            {
                foreach (var indexPlace in selectedPlaces)
                {
                    Ticket newTicket = new Ticket
                    {
                        id_session = idSession,
                        id_user = MainWindow.sessionUser.id,
                        id_place = places[indexPlace].id,
                        date = DateTime.Now
                    };
                    context.Ticket.Add(newTicket);
                }

                context.SaveChanges();
            }

            Window parentWindow = Window.GetWindow(this);

            Frame frame = LogicalTreeHelper.FindLogicalNode(parentWindow, "MainFrame") as Frame;
            frame.Navigate(new MyTickets());
        }


        private void CreatePlace()
        {
            using (MovieEntities context = new MovieEntities())
            {
                int offsetRow = 0;
                for (int row = 0; row < 20; row++)
                {
                    if (row == 7)
                    {
                        offsetRow += 25;
                    }
                    ElementHall elementHall = new ElementHall()
                    {
                        id_hall = 1,
                        width = 20,
                        height = 20,
                        X = 60,
                        Y = 45 + 25 * row + offsetRow,
                        color = "",
                        text = (row + 1).ToString()
                    };
                    context.ElementHall.Add(elementHall);

                    int offsetSeat = 55;
                    if (row == 19)
                    {
                        for (int seat = 0; seat < 8; seat++)
                        {
                            if (seat == 2 || seat == 6)
                            {
                                offsetSeat += 75;
                            }

                            Place newPlace = new Place()
                            {
                                id_hall = 1,
                                id_price_category = 4,
                                row = row + 1,
                                column = seat + 1,
                                X = 25 + 50 * seat + offsetSeat,
                                Y = 45 + 25 * row + offsetRow,
                                width = 45,
                                height = 20
                            };

                            context.Place.Add(newPlace);
                        }
                    } else { 
                        for (int seat = 0; seat < 20; seat++)
                        {
                            if (seat == 5 || seat == 15)
                            {
                                offsetSeat += 25;
                            }
                            int typeOfPlace = 1;
                            if (offsetSeat == 80 && row < 7)
                            {
                                typeOfPlace = 3;
                            } else if (offsetSeat == 80 && row < 13)
                            {
                                typeOfPlace = 2;
                            }

                            Place newPlace = new Place()
                            {
                                id_hall = 1,
                                id_price_category = typeOfPlace,
                                row = row + 1,
                                column = seat + 1,
                                X = 25 * (seat + 1) + offsetSeat,
                                Y = 45 + 25 * row + offsetRow,
                                width = 20,
                                height = 20
                            };
                            context.Place.Add(newPlace);
                        }
                    }
                }
                context.SaveChanges();
            }
        }

        /*
        private void LoadSeatLayout()
        {
            using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
            {

                List<Sessions> sessions = context.Sessions.ToList();
                Sessions session = sessions.Where(p => p.Script.FirstOrDefault().ID == idMovie).FirstOrDefault();
                TitleMovie.Text = Convert.ToString(session.Script.FirstOrDefault().Name);

                var query = context.Place.Where(p => p.HallsID == session.HallsID);
                var results = query.ToList();
                var reservedSeats = context.Tickets.Select(p => p.Place.ID).ToList();
                //var results = CreatePlace();
        
                List<List<int>> colorPlac = new List<List<int>>
{
                    new List<int> {198, 65, 144},
                    new List<int> {133, 68, 152},
                    new List<int> {26, 110, 193},
                    new List<int> {74, 178, 169 }
                };

                foreach (var item in results)
                {
                    Button seatButton = new Button();
                    seatButton.Style = (Style)FindResource("RoundedButtonStyle");
                    seatButton.Content = $"{item.Column}";
                    seatButton.Tag = $"{item.Row};{item.Column};{Convert.ToInt32(item.Sectors.PriceCategory.Price)};{item.SectorID};{item.ID}";
                    if (reservedSeats.Contains(item.ID))
                    {                       
                        seatButton.Background = new SolidColorBrush(Color.FromRgb(112, 112, 112));
                    }
                    else
                    {
                        seatButton.Background = new SolidColorBrush(Color.FromRgb((byte)colorPlac[item.Sectors.PriceCategoryID - 1][0], (byte)colorPlac[item.Sectors.PriceCategoryID - 1][1], (byte)colorPlac[item.Sectors.PriceCategoryID - 1][2]));
                        seatButton.Click += SeatButton_Click;
                    }
                    
                    gridList[item.SectorID - 1].Children.Add(seatButton);
                    var CountRowColumn = context.Place.Where(p => p.SectorID == item.SectorID).Where(p => p.Row == item.Row).Count();

                    switch (item.Sectors.Position)
                    {
                        case 0:                           
                            Grid.SetColumn(seatButton, item.Column - 1);
                            break;
                        case 1:
                            int coll1 = (gridList[item.SectorID - 1].ColumnDefinitions.Count - CountRowColumn) / 2 + item.Column - 1;
                            Grid.SetColumn(seatButton, coll1);
                            break;
                        case 2:
                            int coll2 = (gridList[item.SectorID - 1].ColumnDefinitions.Count - CountRowColumn) + item.Column - 1;
                            Grid.SetColumn(seatButton, coll2);                            
                            break;
                    }
                    Grid.SetRow(seatButton, item.Row);

                }
            }
        }

        private void CreateTicketsEl(string seatInfo, Brush color)
        {
            string[] seatData = seatInfo.Split(';');

            StackPanel innerStackPanel = new StackPanel();
            innerStackPanel.Orientation = Orientation.Horizontal;
            innerStackPanel.VerticalAlignment = VerticalAlignment.Center;
            innerStackPanel.Margin = new Thickness(15, 10, 15, 10);
            innerStackPanel.Tag = seatInfo;

            Border border = new Border();
            border.Margin = new Thickness(0, 0, 15, 0);
            border.Background = color;
            border.CornerRadius = new CornerRadius(10);


            // Создаем первый StackPanel
            StackPanel stackPanel1 = new StackPanel();
            TextBlock textBlock1 = new TextBlock();
            textBlock1.Text = $"Ряд {seatData[0]}, Место {seatData[1]}";
            TextBlock textBlock2 = new TextBlock();
            textBlock2.Text = $"{seatData[2]} руб";
            stackPanel1.Children.Add(textBlock1);
            stackPanel1.Children.Add(textBlock2);

            // Создаем второй StackPanel
            StackPanel stackPanel2 = new StackPanel();
            stackPanel2.VerticalAlignment = VerticalAlignment.Center;
            stackPanel2.HorizontalAlignment = HorizontalAlignment.Left;
            stackPanel2.Margin = new Thickness(10, 0, 0, 0);
            Button deleteBtn = new Button();
            deleteBtn.Content = "X";
            deleteBtn.Background = Brushes.Transparent;
            deleteBtn.BorderBrush = Brushes.Transparent;
            deleteBtn.Click += DeleteTicketClick;
            stackPanel2.Children.Add(deleteBtn);

            // Добавляем stackPanel1 и stackPanel2 в innerStackPanel
            innerStackPanel.Children.Add(stackPanel1);
            innerStackPanel.Children.Add(stackPanel2);


            border.Child = innerStackPanel;
            TicketsGrid.Children.Add(border);

            // Добавляем mainStackPanel в окно
            countTickets += 1;
            // int number = 0;

            int price = Convert.ToInt32(seatData[2]);
            //int.TryParse(seatData[2], out number);
            allPrice += price;
            ChangePrice();

            idReservedPlaces.Add(Convert.ToInt32(seatData[4]));

        }

        private void DeleteTicketClick(object sender, RoutedEventArgs e)
        {
            UIElement elementToRemove = (UIElement)sender; // Замените этот код на получение вашего элемента

            DependencyObject parent = VisualTreeHelper.GetParent(elementToRemove);
            StackPanel ticketPanel = (StackPanel)VisualTreeHelper.GetParent(parent);
            Border ticketPanelBorder = (Border)VisualTreeHelper.GetParent(ticketPanel);
            
            string ticketTag = (string)ticketPanel.Tag;
            TicketsGrid.Children.Remove(ticketPanelBorder);
            countTickets -= 1;
            string[] tagInfo = ((string)(ticketTag)).Split(';');

            int price = Convert.ToInt32(tagInfo[2]);
            //int price = (int)(((string)TicketsGrid.Tag).Split(';')[2]);
            allPrice -= price;
            ChangePrice();

            Grid gridElem = gridList[Convert.ToInt32(tagInfo[3]) - 1];
            Console.WriteLine(tagInfo[3]);
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(gridElem); i++) {
            
                Button btnPlace = (Button)VisualTreeHelper.GetChild(gridElem, i);

                if (btnPlace.Tag == ticketTag)
                {
                    btnPlace.Resources["ButtonBorderBrush"] = Brushes.Black;
                    break;
                }
            }

            idReservedPlaces.Remove(Convert.ToInt32(tagInfo[4]));
        }
        
        */
    }
}

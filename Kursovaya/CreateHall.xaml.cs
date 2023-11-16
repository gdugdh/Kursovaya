using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Instrumentation;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
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
using System.Xml;
using System.Xml.Linq;
using static ZXing.QrCode.Internal.Mode;

namespace Kursovaya
{
    /// <summary>
    /// Логика взаимодействия для CreateHall.xaml
    /// </summary>

    abstract class ElementCanvas
    {
        static public int uniqueNumber = 0;
        static public List<string> systemProperties = new List<string>() { "obj", "canvas", "_posX", "_posY", "_width", "_height", "id", "hasChanges" };

        public ElementCanvas( Canvas canvas, int id = 0)  //int width, int height, double posX, double posY,
        {
            this.id = id;
            this.canvas = canvas;
            hasChanges = false;
            ElementCanvas.uniqueNumber += 1;
        }

        public bool hasChanges { get; set; }
        public int id { get; set; }

        private int _width { get; set; }
        public int width
        {
            get { return this._width; }
            set
            {
                if (this._width != value)
                {
                    this._width = value;
                    obj.Width = this._width;
                }
            }
        }

        private int _height { get; set; }
        public int height
        {
            get { return this._height; }
            set
            {
                if (this._height != value)
                {
                    this._height = value;
                    obj.Height = this._height;
                }
            }
        }

        private double _posX { get; set; }
        public double posX
        {
            get { return this._posX; }
            set
            {
                if (this._posX != value)
                {
                    this._posX = value;
                    Canvas.SetLeft(this.obj, this._posX);
                }
            }
        }

        private double _posY { get; set; }
        public double posY
        {
            get { return _posY; }
            set
            {
                if (this._posY != value)
                {
                    this._posY = value;
                    Canvas.SetTop(this.obj, this._posY);
                }
            }
        }

        public FrameworkElement obj { get; set; }
        public Canvas canvas { get; set; }

        abstract public void saveInDB(MovieEntities context, int hall_id);
        abstract public void deleteDB(MovieEntities context);
    }

    class ElementCanvasRectangle : ElementCanvas
    {
        static public new List<string> systemProperties = ElementCanvas.systemProperties.Concat(new List<string>() { "_color" }).ToList();

        public ElementCanvasRectangle(Canvas canvas, int id = 0) : base(canvas, id)
        {
            this.obj = new Rectangle() { Tag = ElementCanvas.uniqueNumber.ToString() };
            this.width = 100;
            this.height = 50;
            this.color = "#999999";
        }

        private string _color { get; set; }
        public string color {
            get { return this._color; }
            set {
                if (this._color != value)
                {
                    this._color = value;
                    ((Rectangle)obj).Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom(this._color));
                }
            }
        }

        public override void saveInDB(MovieEntities context, int hall_id)
        {
            if (this.id != 0 && !this.hasChanges) return;

            if (this.id != 0)
            {
                ElementHall elementHall = context.ElementHall.Where(eh => eh.id == this.id).FirstOrDefault();
                elementHall.color = this.color;
                elementHall.height = this.height;
                elementHall.width = this.width;
                elementHall.Y = Convert.ToInt32(this.posY);
                elementHall.X = Convert.ToInt32(this.posX);
            }
            else
            {
                ElementHall elementHall = new ElementHall()
                {
                    id_hall = hall_id,
                    X = Convert.ToInt32(this.posX),
                    Y = Convert.ToInt32(this.posY),
                    width = this.width,
                    height = this.height,
                    color = this.color,
                    text = ""
                };
                context.ElementHall.Add(elementHall);
            }
        }
        public override void deleteDB(MovieEntities context)
        {
            if (this.id != 0)
            {
                var elem = context.ElementHall.Where(eh => eh.id == this.id).FirstOrDefault();
                context.ElementHall.Remove(elem);
            }
            this.canvas.Children.Remove(this.obj);
        }
    }

    class ElementCanvasPlace : ElementCanvas
    {
        static public new List<string> systemProperties = ElementCanvas.systemProperties.Concat(new List<string>() { "_price_category" }).ToList();

        public ElementCanvasPlace(Canvas canvas, int id = 0) : base(canvas, id)
        {
            this.obj = new Button() {
                Tag = ElementCanvas.uniqueNumber.ToString(),
                //Style = (Style)Application.Current.MainWindow.FindResource("RoundedButtonStyle")
            };
            this.width = 20;
            this.height = 20;
            this.row = 1;
            this.column = 1;
        }

        public int row { get; set; }

        private int _column { get; set; }
        public int column
        {
            get { return this._column; }
            set
            {
                if (this._column != value)
                {
                    this._column = value;
                    ((Button)this.obj).Content = this._column.ToString();
                }
            }
        }

        private string _price_category { get; set; }
        public string price_category
        {
            get { return this._price_category; }
            set
            {
                if (this._price_category != value)
                {
                    Dictionary<string, string> priceCategories = new Dictionary<string, string>()
                    {
                        { "Standart", "#1a6ec1" },
                        { "Premium", "#854498" },
                        { "VIP", "#c64190" },
                        { "Royal", "#4ab2a9" },
                    };
                    
                    SolidColorBrush clr = (SolidColorBrush)(new BrushConverter().ConvertFrom(priceCategories[value]));
                    Button btn = (Button)this.obj;
                    btn.Resources["ButtonBorderBrush"] = clr;
                    btn.Background = new SolidColorBrush(Colors.White);
                    btn.Foreground = clr;
                    this._price_category = value;
                }
            }
        }

        public override void saveInDB(MovieEntities context, int hall_id)
        {

            if (this.id != 0 && !this.hasChanges) return;

            if (this.id != 0)
            {
                Place place = context.Place.Where(eh => eh.id == this.id).FirstOrDefault();
                place.height = this.height;
                place.width = this.width;
                place.Y = Convert.ToInt32(this.posY);
                place.X = Convert.ToInt32(this.posX);
                place.row = this.row;
                place.column = this.column;
                place.id_price_category = context.PriceCategory.Where(pc => pc.name == this.price_category).FirstOrDefault().id;
            }
            else
            {
                Place place = new Place()
                {
                    id_hall = hall_id,
                    X = Convert.ToInt32(this.posX),
                    Y = Convert.ToInt32(this.posY),
                    width = this.width,
                    height = this.height,
                    row = this.row,
                    column = this.column,
                    id_price_category = context.PriceCategory.Where(pc => pc.name == this.price_category).FirstOrDefault().id
                };
                context.Place.Add(place);
            }
        }
        public override void deleteDB(MovieEntities context)
        {
            if (this.id != 0)
            {
                var elem = context.Place.Where(eh => eh.id == this.id).FirstOrDefault();
                Console.WriteLine(elem.X.ToString());
                context.Place.Remove(elem);
            }
            this.canvas.Children.Remove(this.obj);
        }
    }

    class ElementCanvasText : ElementCanvas
    {
        static public new List<string> systemProperties = ElementCanvas.systemProperties.Concat(new List<string>() { "_text" }).ToList();
        public ElementCanvasText(Canvas canvas, int id = 0) : base(canvas, id)
        {
            this.obj = new TextBlock() { Tag = ElementCanvas.uniqueNumber.ToString()};
            this.width = 100;
            this.height = 20;
            this.text = "Text";
        }

        private string _text { get; set; }
        public string text
        {
            get { return this._text; }
            set
            {
                if (this._text != value)
                {
                    this._text = value;
                    TextBlock tb = this.obj as TextBlock;
                    tb.Text = this._text;
                }
            }
        }

        public override void saveInDB(MovieEntities context, int hall_id)
        {
            if (this.id != 0 && !this.hasChanges) return;
            
            if (this.id != 0)
            {
                ElementHall elementHall = context.ElementHall.Where(eh => eh.id == this.id).FirstOrDefault();
                elementHall.text = this.text;
                elementHall.height = this.height;
                elementHall.width = this.width;
                elementHall.Y = Convert.ToInt32(this.posY);
                elementHall.X = Convert.ToInt32(this.posX);
            } else
            {
                ElementHall elementHall = new ElementHall()
                {
                    id_hall = hall_id,
                    X = Convert.ToInt32(this.posX),
                    Y = Convert.ToInt32(this.posY),
                    width = this.width,
                    height = this.height,
                    color = "",
                    text = this.text
                };
                context.ElementHall.Add(elementHall);
            }
        }
        public override void deleteDB(MovieEntities context)
        {
            if (this.id != 0)
            {
                var elem = context.ElementHall.Where(eh => eh.id == this.id).FirstOrDefault();
                context.ElementHall.Remove(elem);
            }
            this.canvas.Children.Remove(this.obj);
        }
    }

    class PropertyItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public partial class CreateHall : Page
    {
        List<ElementCanvas> allElements = new List<ElementCanvas>();
        ElementCanvas selectedElement = null;
        Dictionary<string, Type> dragElementCanvasTags = new Dictionary<string, Type>();
        public CreateHall()
        {
            InitializeComponent();
            GeneratedragElementCanvasTags();
            GenerateElement();
            btn.Style = (Style)FindResource("RoundedButtonStyle");
        }

        private void GeneratedragElementCanvasTags ()
        {
            dragElementCanvasTags["Rectangle"] = typeof(ElementCanvasRectangle);
            dragElementCanvasTags["Place"] = typeof(ElementCanvasPlace);
            dragElementCanvasTags["Text"] = typeof(ElementCanvasText);
        }

        private void GenerateElement()
        {
            if (MainWindow.idHall == 0) return;
            using (MovieEntities context = new MovieEntities())
            {
                foreach (ElementHall elem in context.ElementHall.Where(eh => eh.id_hall == MainWindow.idHall).ToList())
                {
                    ElementCanvas el;
                    if (elem.text != "")
                    {
                        el = new ElementCanvasText(HallCanvas, elem.id) { text = elem.text, width=elem.width, height = elem.height};
                        
                    } else
                    {
                        el = new ElementCanvasRectangle(HallCanvas, elem.id) { color = elem.color, width = elem.width, height = elem.height };
                    }
                    HallCanvas.Children.Add(el.obj);
                    el.posX = elem.X;
                    el.posY = elem.Y;

                    el.obj.MouseLeftButtonDown += ClickSelectElement;
                    el.obj.MouseMove += DragAndDropMouseMove;
                    allElements.Add(el);
                }

                foreach (Place place in context.Place.Where(p => p.id_hall == MainWindow.idHall).ToList())
                {
                    ElementCanvasPlace p = new ElementCanvasPlace(HallCanvas, place.id) { row=place.row, column=place.column, width=place.width, height=place.height, price_category=place.PriceCategory.name };

                    HallCanvas.Children.Add(p.obj);
                    p.posX = place.X;
                    p.posY = place.Y;

                    p.obj.MouseLeftButtonDown += ClickSelectElement;
                    p.obj.MouseMove += DragAndDropMouseMove;

                    allElements.Add(p);
                    p.obj.Style = (Style)FindResource("RoundedButtonStyle");
                    ((Button)p.obj).Click += ClickSelectPlace;
                }
            }
        }

        private void GenerateProperties()
        {
            List<PropertyItem> items = new List<PropertyItem> { };
            Type type = selectedElement.GetType();
            Console.WriteLine(type.Name);
            List<string> systemProperties = (List<string>)type.GetField("systemProperties", BindingFlags.Public | BindingFlags.Static).GetValue(null);

            foreach (PropertyInfo property in type.GetProperties())
            {
                string propertyName = property.Name;
                string propertyValue = property.GetValue(selectedElement).ToString();

                if (systemProperties.Contains(propertyName)) continue;

                items.Add(new PropertyItem() { Name = propertyName, Value = propertyValue });
            }
            dataGrid.ItemsSource = items;
        }

        private void PropertyCellEditEnding(object sender, DataGridCellEditEndingEventArgs e) //
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                PropertyItem editedItem = (PropertyItem)e.Row.Item;
                TextBox t = (TextBox)e.EditingElement;
                string curFieldValue = t.Text;

                PropertyInfo prop = selectedElement.GetType().GetProperty(editedItem.Name);
                if (prop.PropertyType == typeof(int)) {
                    prop.SetValue(selectedElement, Convert.ToInt32(curFieldValue), null);
                }
                else if (prop.PropertyType == typeof(double)) {
                    prop.SetValue(selectedElement, Convert.ToDouble(curFieldValue), null);
                }
                else {
                    prop.SetValue(selectedElement, curFieldValue, null);
                }
                selectedElement.hasChanges = true;
            }
        }

        private void MouseLeftButtonDownAddNewElement(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement elem = (FrameworkElement)sender;

            DataObject data = new DataObject("Tag", elem.Tag.ToString());
            DragDrop.DoDragDrop(elem, data, DragDropEffects.Move);
        }

        private void DragAndDropMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                FrameworkElement elem = (FrameworkElement)sender;
                DataObject data = new DataObject("Tag", elem.Tag.ToString());
                DragDrop.DoDragDrop(elem, data, DragDropEffects.Move);
            }
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            Point dropPosition = e.GetPosition(HallCanvas);

            string tag = (string)e.Data.GetData("Tag");

            int trash;
            bool isNumber = int.TryParse(tag, out trash);
            if (isNumber)
            {
                selectedElement = GetElementCanvasByTag(tag);
                selectedElement.posX = dropPosition.X;
                selectedElement.posY = dropPosition.Y;
                GenerateProperties();
                return;
            }
            selectedElement = Activator.CreateInstance(dragElementCanvasTags[tag], HallCanvas, 0) as ElementCanvas;

            HallCanvas.Children.Add(selectedElement.obj);
            selectedElement.posX = dropPosition.X;
            selectedElement.posY = dropPosition.Y;

            selectedElement.obj.MouseLeftButtonDown += ClickSelectElement;
            selectedElement.obj.MouseMove += DragAndDropMouseMove;
            allElements.Add(selectedElement);

            if (tag == "Place")
            {
                selectedElement.obj.Style = (Style)FindResource("RoundedButtonStyle");
                ((ElementCanvasPlace)selectedElement).price_category = "Standart";
                ((Button)selectedElement.obj).Click += ClickSelectPlace;
            }
            deleteButton.Visibility = Visibility.Visible;
            GenerateProperties();
        }

        private void ClickSelectPlace(object sender, RoutedEventArgs e)
        {
            FrameworkElement clickedElem = (FrameworkElement)sender;
            selectedElement = GetElementCanvasByTag((string)clickedElem.Tag);
            GenerateProperties();
            deleteButton.Visibility = Visibility.Visible;
            e.Handled = true;
        }

        private void ClickSelectElement(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement clickedElem = (FrameworkElement)sender;
            selectedElement = GetElementCanvasByTag((string)clickedElem.Tag);
            GenerateProperties();
            deleteButton.Visibility = Visibility.Visible;
            e.Handled = true;
        }

        private ElementCanvas GetElementCanvasByTag(string Tag)
        {
            foreach (ElementCanvas elem in allElements)
            {
                if (elem.obj.Tag == Tag)
                {
                    return elem;
                }
            }
            return null;
        }

        private void HallCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedElement = null;
            dataGrid.ItemsSource = new List<PropertyItem> { };
            deleteButton.Visibility = Visibility.Hidden;
        }

        private void saveInDB(object sender, RoutedEventArgs e)
        {
            using (MovieEntities context = new MovieEntities())
            {
                Hall hall;
                if (MainWindow.idHall == 0) {
                    hall = new Hall()
                    {
                        name = hallName.Text,
                        rows = 10,
                        columns = 10
                    };

                    context.Hall.Add(hall);
                    context.SaveChanges();
                } else {
                    hall = context.Hall.Where(h => h.id == MainWindow.idHall).FirstOrDefault();
                }

                foreach (ElementCanvas element in allElements)
                {
                    element.saveInDB(context, hall.id);
                }
                context.SaveChanges();
            }

            Window parentWindow = Window.GetWindow(this);

            Frame frame = LogicalTreeHelper.FindLogicalNode(parentWindow, "MainFrame") as Frame;
            frame.Navigate(new SelectHallChange());
        }

        private void DeleteElement(object sender, RoutedEventArgs e)
        {
            using (MovieEntities context = new MovieEntities())
            {
                allElements.Remove(selectedElement);

                selectedElement.deleteDB(context);
                context.SaveChanges();

                selectedElement = null;
                dataGrid.ItemsSource = new List<PropertyItem> { };
                deleteButton.Visibility = Visibility.Hidden;
            }
        }

        /*
        private void grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MyTable path = dataGrid.SelectedItem as MyTable;
            MessageBox.Show(" ID: " + path.Id + "\n Исполнитель: " + path.Vocalist + "\n Альбом: " + path.Album
                + "\n Год: " + path.Year);
        }

        private void ButtonPropertyColor(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void ButtonPriceCategory_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void DragAndDropMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(DragAndDropRectangle, DragAndDropRectangle, DragDropEffects.Move);
            }
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            Point dropPosition = e.GetPosition(HallCanvas);

            Rectangle rect = new Rectangle()
            {
                Width = 100,
                Height = 50,
                Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#999999")),
            };

            rect.MouseLeftButtonDown += ClickSelectElement;

            HallCanvas.Children.Add(rect);
            Canvas.SetLeft(rect, dropPosition.X);
            Canvas.SetTop(rect, dropPosition.Y);

            selectedElement = rect;
            GenerateUI();
        }

        private void ClickSelectElement(object sender, MouseButtonEventArgs e)
        {
            selectedElement = (UIElement)sender;
            GenerateUI();
            e.Handled = true;
        }

        private void HallCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedElement = null;
            GenerateUI();
        }

        private void ReadProperty()
        {
            if (selectedElement is Rectangle)
            {
                Rectangle rect = (Rectangle)selectedElement;
                foreach (TextBox prop in propertiesInput)
                {
                    switch (prop.Tag)
                    {
                        case "pos X":
                            prop.Text = Canvas.GetLeft(rect).ToString();
                            break;
                        case "pos Y":
                            prop.Text = Canvas.GetTop(rect).ToString();
                            break;
                        case "Width":
                            prop.Text = rect.Width.ToString();
                            break;
                        case "Height":
                            prop.Text = rect.Height.ToString();
                            break;
                    }
                }
            }
            else if (selectedElement is TextBlock)
            {
                TextBlock textBlock = (TextBlock)selectedElement;
                foreach (TextBox prop in propertiesInput)
                {
                    switch (prop.Tag)
                    {
                        case "pos X":
                            prop.Text = Canvas.GetLeft(textBlock).ToString();
                            break;
                        case "pos Y":
                            prop.Text = Canvas.GetTop(textBlock).ToString();
                            break;
                        case "Width":
                            prop.Text = textBlock.Width.ToString();
                            break;
                        case "Height":
                            prop.Text = textBlock.Height.ToString();
                            break;
                    }
                }
            }
            else if (selectedElement is Button)
            {
                Button btn = (Button)selectedElement;
                foreach (TextBox prop in propertiesInput)
                {
                    switch (prop.Tag)
                    {
                        case "pos X":
                            prop.Text = Canvas.GetLeft(btn).ToString();
                            break;
                        case "pos Y":
                            prop.Text = Canvas.GetTop(btn).ToString();
                            break;
                        case "Width":
                            prop.Text = btn.Width.ToString();
                            break;
                        case "Height":
                            prop.Text = btn.Height.ToString();
                            break;
                    }
                }
            }

        }

        private void GenerateUI()
        {
            propertiesSelectedElement.Children.Clear();
            GenerateTextInput("Width");
            GenerateTextInput("Height");
            GenerateTextInput("pos X");
            GenerateTextInput("pos Y");
            if (selectedElement is Rectangle)
            {
                Console.WriteLine();
                GenerateColorButtons();
            }
            else if (selectedElement is TextBlock)
            {
                GenerateTextInput("Text");
            }
            else if (selectedElement is Button)
            {
                GeneratePriceCategory();
            }
            else
            {
                propertiesSelectedElement.Children.Clear();
                return;
            }
            ReadProperty();
        }

        private void GenerateRectangleUI()
        {
            propertiesSelectedElement.Children.Clear();
            // Генерация StackPanel для каждого элемента
            GenerateColorButtons();
        }

        private void GenerateTextBlockUI()
        {
            propertiesSelectedElement.Children.Clear();
            // Генерация StackPanel для каждого элемента
            GenerateTextInput("Text");
        }

        private void GeneratePlaceUI()
        {
            propertiesSelectedElement.Children.Clear();
            // Генерация StackPanel для каждого элемента
            GeneratePriceCategory();
        }

        private void GenerateTextInput(string labelText)
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Margin = new Thickness(0, 0, 0, 5);

            TextBlock label = new TextBlock();
            label.FontSize = 13;
            label.Text = labelText + ":";
            label.Margin = new Thickness(0, 0, 10, 0);

            TextBox textBox = new TextBox();
            textBox.Width = 100;
            textBox.Tag = labelText;
            propertiesInput.Add(textBox);

            stackPanel.Children.Add(label);
            stackPanel.Children.Add(textBox);

            propertiesSelectedElement.Children.Add(stackPanel);
        }

        private void GeneratePriceCategory()
        {
            StackPanel priceCategoryStackPanel = new StackPanel();

            TextBlock textBlock = new TextBlock();
            textBlock.FontSize = 13;
            textBlock.Text = "Price Category:";

            StackPanel buttonStackPanel1 = new StackPanel();
            buttonStackPanel1.Orientation = Orientation.Horizontal;

            StackPanel buttonStackPanel2 = new StackPanel();
            buttonStackPanel2.Orientation = Orientation.Horizontal;

            StackPanel[] buttonsStackPanel = { buttonStackPanel1, buttonStackPanel2 };

            using (MovieEntities context = new MovieEntities())
            {
                foreach (PriceCategory category in context.PriceCategory.ToList())
                {
                    Button button = new Button();
                    button.BorderBrush = Brushes.Transparent;
                    button.Width = 80;
                    button.Height = 20;
                    button.Foreground = Brushes.White;
                    button.Tag = category.id.ToString();
                    button.Content = category.name;
                    button.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(category.color); // Установите соответствующий цвет фона
                    button.Margin = new Thickness(0, 0, 5, 5);
                    button.Click += ButtonPriceCategory_Click; // Подключите обработчик события Click

                    buttonsStackPanel[category.id < 3 ? 0: 1].Children.Add(button);
                }
            }

            priceCategoryStackPanel.Children.Add(textBlock);
            priceCategoryStackPanel.Children.Add(buttonStackPanel1);
            priceCategoryStackPanel.Children.Add(buttonStackPanel2);

            propertiesSelectedElement.Children.Add(priceCategoryStackPanel);
        }

        private void GenerateColorButtons()
        {
            string[] colorValues = new string[] { "#1a6ec1", "#c64190", "#854498", "#4ab2a9", "#242424", "#999999" };
            StackPanel parentStackPanel = new StackPanel();

            TextBlock textBlock = new TextBlock();
            textBlock.FontSize = 13;
            textBlock.Text = "Color:";

            StackPanel buttonStackPanel1 = new StackPanel();
            buttonStackPanel1.Orientation = Orientation.Horizontal;

            StackPanel buttonStackPanel2 = new StackPanel();
            buttonStackPanel2.Orientation = Orientation.Horizontal;
            StackPanel[] buttonsStackPanel = { buttonStackPanel1, buttonStackPanel2 };

            int index = 0;
            foreach (string colorValue in colorValues)
            {
                Button button = new Button();
                button.BorderBrush = Brushes.Transparent;
                button.Width = 50;
                button.Height = 20;
                button.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(colorValue);
                button.Margin = new Thickness(0, 0, 5, 5);

                buttonsStackPanel[index < 3 ? 0 : 1].Children.Add(button);
                index++;
            }

            parentStackPanel.Children.Add(textBlock);
            parentStackPanel.Children.Add(buttonStackPanel1);
            parentStackPanel.Children.Add(buttonStackPanel2);
            propertiesSelectedElement.Children.Add(parentStackPanel);
        }
        */
    }
}

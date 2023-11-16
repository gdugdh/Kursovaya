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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kursovaya
{
    /// <summary>
    /// Выполните шаги 1a или 1b, а затем 2, чтобы использовать этот пользовательский элемент управления в файле XAML.
    ///
    /// Шаг 1a. Использование пользовательского элемента управления в файле XAML, существующем в текущем проекте.
    /// Добавьте атрибут XmlNamespace в корневой элемент файла разметки, где он 
    /// будет использоваться:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Kursovaya"
    ///
    ///
    /// Шаг 1б. Использование пользовательского элемента управления в файле XAML, существующем в другом проекте.
    /// Добавьте атрибут XmlNamespace в корневой элемент файла разметки, где он 
    /// будет использоваться:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Kursovaya;assembly=Kursovaya"
    ///
    /// Потребуется также добавить ссылку из проекта, в котором находится файл XAML,
    /// на данный проект и пересобрать во избежание ошибок компиляции:
    ///
    ///     Щелкните правой кнопкой мыши нужный проект в обозревателе решений и выберите
    ///     "Добавить ссылку"->"Проекты"->[Поиск и выбор проекта]
    ///
    ///
    /// Шаг 2)
    /// Теперь можно использовать элемент управления в файле XAML.
    ///
    ///     <MyNamespace:Carousel/>
    ///
    /// </summary>
    [TemplatePart(Name = "PART_Animation", Type = typeof(Storyboard))]
    public class Carousel : Control
    {
        static Carousel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Carousel),
            new FrameworkPropertyMetadata(typeof(Carousel)));
        }
        Storyboard animation; // текущая анимация
        bool isAnimationRunning = false;

        #region dp object Content, on change OnContentChanged
        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
                "Content", typeof(object), typeof(Carousel),
                new PropertyMetadata(OnContentChangedStatic));

        static void OnContentChangedStatic(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (Carousel)d;
            self.OnContentChanged(e.OldValue, e.NewValue);
        }
        #endregion

        #region dp object PreviousContent
        public object PreviousContent
        {
            get { return (object)GetValue(PreviousContentProperty); }
            set { SetValue(PreviousContentProperty, value); }
        }

        public static readonly DependencyProperty PreviousContentProperty =
            DependencyProperty.Register(
                "PreviousContent", typeof(object), typeof(Carousel));
        #endregion

        // когда Content поменяется...
        void OnContentChanged(object oldContent, object newContent)
        {
            if (isAnimationRunning)
                animation?.Stop();

            // ... запомним старый Content в PreviousContent
            PreviousContent = oldContent;

            // и перезапустим анимацию
            if (animation != null)
            {
                animation.Begin();
                isAnimationRunning = true;
            }
        }

        // при появлении шаблона, вычитаем из него анимацию
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (animation != null)
                animation.Completed -= OnAnimationCompleted;

            if (isAnimationRunning)
            {
                // TODO: начать новую анимацию там, где предыдущая завершилась?
                animation?.Stop();
            }

            animation = (Storyboard)Template.FindName("PART_Animation", this);

            if (animation != null) // подпишемся на завершение анимации
                animation.Completed += OnAnimationCompleted;
        }

        // по окончанию анимации...
        private void OnAnimationCompleted(object sender, EventArgs e)
        {
            // выбросим старый контент
            PreviousContent = null;
            // сбросим эффект анимации
            animation.Remove();
            isAnimationRunning = false;
        }
    }
}

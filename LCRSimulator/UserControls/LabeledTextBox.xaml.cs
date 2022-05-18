using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LCRSimulator.Helpers;

namespace LCRSimulator.UserControls
{
    /// <summary>
    /// Interaction logic for LabeledTextBox.xaml
    /// </summary>
    public partial class LabeledTextBox : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", 
            typeof(string), typeof(LabeledTextBox), new PropertyMetadata(string.Empty));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label",
            typeof(string), typeof(LabeledTextBox), new PropertyMetadata(string.Empty));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static readonly DependencyProperty LabelHeightProperty = DependencyProperty.Register("LabelHeigth",
            typeof(int), typeof(LabeledTextBox), new PropertyMetadata(default(int)));

        public int LabelHeight
        {
            get => (int)GetValue(LabelHeightProperty);
            set => SetValue(LabelHeightProperty, value);
        }

        public static readonly DependencyProperty TextBoxHeightProperty = DependencyProperty.Register("TextBoxHeigth",
            typeof(int), typeof(LabeledTextBox), new PropertyMetadata(default(int)));

        public int TextBoxHeight
        {
            get => (int)GetValue(TextBoxHeightProperty);
            set => SetValue(TextBoxHeightProperty, value);
        }

        public static readonly DependencyProperty LostFocusCommandProperty = DependencyProperty.Register("LostFocusCommand",
            typeof(ICommand), typeof(LabeledTextBox), new PropertyMetadata(null));

        public ICommand LostFocusCommand
        {
            get => (ICommand)GetValue(LostFocusCommandProperty);
            set => SetValue(LostFocusCommandProperty, value);
        }

        public LabeledTextBox()
        {
            LabelHeight = DefaultDimensions.LabelHeight;
            TextBoxHeight = DefaultDimensions.TextBoxHeight;

            InitializeComponent();
        }
    }
}

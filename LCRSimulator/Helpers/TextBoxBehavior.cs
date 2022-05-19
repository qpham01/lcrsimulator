using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LCRSimulator.Helpers;

public class TextBoxBehavior
{
    public static DependencyProperty OnLostFocusProperty = DependencyProperty.RegisterAttached(
        "OnLostFocus",
        typeof(ICommand),
        typeof(TextBoxBehavior),
        new UIPropertyMetadata(TextBoxBehavior.OnLostFocus));

    public static void SetOnLostFocus(DependencyObject target, ICommand value)
    {
        target.SetValue(TextBoxBehavior.OnLostFocusProperty, value);
    }

    private static void OnLostFocus(DependencyObject target, DependencyPropertyChangedEventArgs e)
    {
        if (!(target is TextBox element))
        {
            throw new InvalidOperationException("This behavior can be attached to a TextBox item only.");
        }

        if ((e.NewValue != null) && (e.OldValue == null))
        {
            element.LostFocus += OnPreviewLostFocus;
        }
        else if ((e.NewValue == null) && (e.OldValue != null))
        {
            element.LostFocus -= OnPreviewLostFocus;
        }
    }

    private static void OnPreviewLostFocus(object sender, RoutedEventArgs e)
    {
        var element = (UIElement)sender;
        var command = (ICommand)element.GetValue(TextBoxBehavior.OnLostFocusProperty);
        command?.Execute(e);
    }
}

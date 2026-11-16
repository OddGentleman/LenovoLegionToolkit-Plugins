using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using UiCardControl = Wpf.Ui.Controls.CardControl;

namespace LenovoLegionToolkit.Plugin.CustomFanCurve
{
    // Clicking anywhere on a card that holds a single toggle switch flips the switch, as in Windows Settings. The same handler
    // ships in the Legion Toolkit fork; this copy covers hosts without it. It is a WPF class handler, so once registered it
    // applies to every card in the app. The AppDomain flag makes sure only one copy is registered.
    internal static class CardClickToggle
    {
        private const string RegisteredKey = "LLT.CardClickToggle.Registered";

        // ButtonBase.OnClick is what a mouse click on the switch runs: it flips the state, raises Click and executes the command,
        // so every existing Click handler and binding sees the change exactly as if the switch itself had been clicked.
        private static readonly MethodInfo? OnClickMethod =
            typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null);

        internal static void Register()
        {
            if (OnClickMethod is null || AppDomain.CurrentDomain.GetData(RegisteredKey) is true)
                return;

            AppDomain.CurrentDomain.SetData(RegisteredKey, true);
            EventManager.RegisterClassHandler(typeof(UiCardControl), ButtonBase.ClickEvent, new RoutedEventHandler(OnCardClick));
        }

        private static void OnCardClick(object sender, RoutedEventArgs e)
        {
            // Only clicks on the card itself; clicks on the switch or any other control inside it bubble up with their own source.
            if (e.Handled || sender is not UiCardControl card || !ReferenceEquals(e.OriginalSource, card))
                return;

            if (FindSingleToggle(card) is not { } toggle)
                return;

            OnClickMethod!.Invoke(toggle, null);
            e.Handled = true;
        }

        // The card's only visible input control, when that is an enabled toggle. Cards with a combo box, number box, button,
        // slider or several toggles are left alone, since a click on them has no single obvious meaning.
        private static ToggleButton? FindSingleToggle(UiCardControl card)
        {
            ToggleButton? found = null;
            var ambiguous = false;

            void Visit(DependencyObject parent)
            {
                var count = VisualTreeHelper.GetChildrenCount(parent);
                for (var i = 0; i < count && !ambiguous; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    if (child is UIElement { IsVisible: false })
                        continue;

                    switch (child)
                    {
                        case ToggleButton toggle:
                            if (found is not null)
                                ambiguous = true;
                            found = toggle;
                            continue;
                        case ButtonBase or Selector or TextBoxBase or PasswordBox or RangeBase:
                            ambiguous = true;
                            continue;
                    }

                    Visit(child);
                }
            }

            Visit(card);
            return !ambiguous && found is { IsEnabled: true } ? found : null;
        }
    }
}

using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace MeetingNotes.MAUI.Views.Components;

public partial class StatusBadgeView : ContentView
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(StatusBadgeView),
        string.Empty);

    public static readonly BindableProperty BadgeBackgroundColorProperty = BindableProperty.Create(
        nameof(BadgeBackgroundColor),
        typeof(Color),
        typeof(StatusBadgeView),
        Colors.Transparent);

    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor),
        typeof(Color),
        typeof(StatusBadgeView),
        Colors.Black);

    public static readonly BindableProperty DotColorProperty = BindableProperty.Create(
        nameof(DotColor),
        typeof(Color),
        typeof(StatusBadgeView),
        Colors.Black);

    public static readonly BindableProperty IsBadgeVisibleProperty = BindableProperty.Create(
        nameof(IsBadgeVisible),
        typeof(bool),
        typeof(StatusBadgeView),
        true);

    public StatusBadgeView()
    {
        InitializeComponent();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Color BadgeBackgroundColor
    {
        get => (Color)GetValue(BadgeBackgroundColorProperty);
        set => SetValue(BadgeBackgroundColorProperty, value);
    }

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public Color DotColor
    {
        get => (Color)GetValue(DotColorProperty);
        set => SetValue(DotColorProperty, value);
    }

    public bool IsBadgeVisible
    {
        get => (bool)GetValue(IsBadgeVisibleProperty);
        set => SetValue(IsBadgeVisibleProperty, value);
    }
}

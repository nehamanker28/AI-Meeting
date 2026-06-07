using System.Windows.Input;
using MeetingNotes.MAUI.Models;
using MeetingNotes.MAUI.ViewModels.Meetings;
using Microsoft.Maui.Controls;

namespace MeetingNotes.MAUI.Views.Components;

public partial class MeetingCardView : ContentView
{
    public static readonly BindableProperty ItemProperty = BindableProperty.Create(
        nameof(Item),
        typeof(MeetingCardUiModel),
        typeof(MeetingCardView));

    public static readonly BindableProperty OpenMeetingCommandProperty = BindableProperty.Create(
        nameof(OpenMeetingCommand),
        typeof(ICommand),
        typeof(MeetingCardView));

    public static readonly BindableProperty RetryCommandProperty = BindableProperty.Create(
        nameof(RetryCommand),
        typeof(ICommand),
        typeof(MeetingCardView));

    public MeetingCardView()
    {
        InitializeComponent();
    }

    public MeetingCardUiModel? Item
    {
        get => (MeetingCardUiModel?)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public ICommand? OpenMeetingCommand
    {
        get => (ICommand?)GetValue(OpenMeetingCommandProperty);
        set => SetValue(OpenMeetingCommandProperty, value);
    }

    public ICommand? RetryCommand
    {
        get => (ICommand?)GetValue(RetryCommandProperty);
        set => SetValue(RetryCommandProperty, value);
    }
}

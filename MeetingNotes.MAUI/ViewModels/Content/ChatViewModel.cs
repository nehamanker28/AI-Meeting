using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingNotes.MAUI.Models;
using MeetingNotes.MAUI.Services.Interfaces;
using MeetingNotes.MAUI.ViewModels.Base;

namespace MeetingNotes.MAUI.ViewModels.Content;

public partial class ChatViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IChatService _chatService;
    private Guid _meetingId;

    [ObservableProperty]
    private ObservableCollection<ChatMessageDto> _messages = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
    private string _inputText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
    private bool _isTyping;

    public ChatViewModel(IChatService chatService)
    {
        Title = "Chat";
        _chatService = chatService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj) && idObj != null)
        {
            _meetingId = Guid.Parse(idObj.ToString()!);
            _ = LoadHistoryAsync();
        }
    }

    private async Task LoadHistoryAsync()
    {
        IsBusy = true;
        ClearError();
        try
        {
            var history = await _chatService.GetHistoryAsync(_meetingId);
            Messages = new ObservableCollection<ChatMessageDto>(history);
        }
        catch (Exception ex)
        {
            SetError("Failed to load chat history.");
            System.Diagnostics.Debug.WriteLine($"LoadHistory error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendMessageAsync()
    {
        var question = InputText.Trim();
        InputText = string.Empty;
        
        var userMsg = new ChatMessageDto
        {
            Id = Guid.NewGuid(),
            MeetingId = _meetingId,
            Role = "user",
            Content = question,
            CreatedAt = DateTime.UtcNow
        };
        Messages.Add(userMsg);
        
        IsTyping = true;
        ClearError();

        try
        {
            var contextHistory = Messages.TakeLast(6).ToList();
            var responseMsg = await _chatService.SendMessageAsync(_meetingId, question, contextHistory);
            
            Messages.Add(responseMsg);
        }
        catch (Exception ex)
        {
            SetError("Could not get a response. Please check your connection.");
            System.Diagnostics.Debug.WriteLine($"SendMessage error: {ex.Message}");
        }
        finally
        {
            IsTyping = false;
        }
    }

    private bool CanSend() => !string.IsNullOrWhiteSpace(InputText) && !IsTyping;
}

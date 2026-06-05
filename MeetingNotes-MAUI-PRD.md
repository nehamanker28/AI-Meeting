# Product Requirements Document (PRD)
## AI Meeting Notes — .NET MAUI Mobile Application
**Version:** 1.0.0 | **Status:** Draft | **Owner:** Product Team | **Date:** 2025

---

## Table of Contents

1. [Overview](#1-overview)
2. [Goals & Success Metrics](#2-goals--success-metrics)
3. [User Personas](#3-user-personas)
4. [User Stories & Acceptance Criteria](#4-user-stories--acceptance-criteria)
5. [Feature Specifications](#5-feature-specifications)
6. [Screen Inventory & Navigation](#6-screen-inventory--navigation)
7. [MAUI Folder Structure](#7-maui-folder-structure)
8. [MVVM Architecture](#8-mvvm-architecture)
9. [Service Layer](#9-service-layer)
10. [Non-Functional Requirements](#10-non-functional-requirements)
11. [Out of Scope](#11-out-of-scope)

---

## 1. Overview

### 1.1 Product Summary

AI Meeting Notes is a cross-platform mobile application (iOS and Android) built with .NET MAUI. It allows users to record or upload meeting audio, automatically transcribe speech to text via OpenAI Whisper, and generate AI-powered summaries, action items, and decisions using GPT-4o. Users can also chat with any past meeting transcript using RAG (Retrieval-Augmented Generation) and export notes in multiple formats.

### 1.2 Problem Statement

Professionals spend significant time manually writing meeting notes, tracking action items, and searching for past decisions. Existing tools are either too manual or too enterprise-heavy. There is no lightweight mobile-first app that combines automatic transcription, AI summarisation, and conversational retrieval.

### 1.3 Platform Targets

| Platform | Min Version | Notes |
|---|---|---|
| iOS | iOS 16+ | TestFlight for beta |
| Android | API 33 (Android 13)+ | Play Store for beta |

---

## 2. Goals & Success Metrics

### 2.1 Business Goals

- Demonstrate a production-quality AI-powered portfolio application
- Showcase .NET MAUI + ASP.NET Core + OpenAI integration skills
- Publish to TestFlight and Google Play beta

### 2.2 Success Metrics

| Metric | Target |
|---|---|
| Meeting → Summary time | < 2 minutes for a 30-min meeting |
| Transcription accuracy | > 90% word accuracy (English) |
| App crash rate | < 0.5% of sessions |
| Action item extraction precision | > 85% relevant items |
| Chat answer relevance | > 90% grounded in transcript |

---

## 3. User Personas

### Persona 1 — Arjun, Engineering Manager
- **Age:** 34 | **Device:** iPhone 15
- **Goal:** Track action items across 10+ meetings/week automatically
- **Frustration:** Manually writing follow-up emails after every meeting
- **Key Feature:** Action item extraction with owner + due date

### Persona 2 — Priya, Product Manager
- **Age:** 29 | **Device:** Android Pixel 8
- **Goal:** Share instant meeting summaries with stakeholders
- **Frustration:** Spending 2 hours/week writing summaries
- **Key Feature:** One-tap executive summary + PDF export

### Persona 3 — David, Consultant
- **Age:** 41 | **Device:** iPad + iPhone
- **Goal:** Retrieve client decisions made months ago
- **Frustration:** Ctrl+F through hundreds of text files
- **Key Feature:** Semantic chat — "What did we decide about the contract?"

---

## 4. User Stories & Acceptance Criteria

### Authentication

**US-001: Register**
> As a new user, I can create an account with my name, email, and password.

Acceptance Criteria:
- Email must be valid format, password minimum 8 characters
- Duplicate email shows inline error "This email is already registered"
- On success, user is navigated to MeetingsListPage
- Passwords are never stored locally

**US-002: Login**
> As a returning user, I can log in with email and password.

Acceptance Criteria:
- Invalid credentials shows "Incorrect email or password"
- JWT access token stored in SecureStorage (Keychain/Keystore)
- Refresh token stored separately in SecureStorage
- "Remember me" keeps user logged in across app restarts

**US-003: Auto Token Refresh**
> As a logged-in user, my session refreshes automatically without interruption.

Acceptance Criteria:
- AuthHttpHandler intercepts 401 responses
- Silently exchanges refresh token for new access token
- If refresh fails, redirects to LoginPage
- User never sees a 401 error in the UI

---

### Meeting Management

**US-010: Create Meeting**
> As a user, I can create a meeting with a title and date.

Acceptance Criteria:
- Title is required (min 1 char, max 200 chars)
- Date defaults to today, can be changed via DatePicker
- On success, navigates to the new meeting's detail page

**US-011: Record Audio**
> As a user, I can record audio directly from my phone during a meeting.

Acceptance Criteria:
- App requests microphone permission on first use
- Displays a live timer (MM:SS) while recording
- Pause / Resume / Stop buttons all functional
- Stop automatically uploads the audio file
- Recording saved as M4A (iOS) or AAC (Android)

**US-012: Upload Audio**
> As a user, I can upload an existing audio file from my device.

Acceptance Criteria:
- FilePicker supports MP3, WAV, M4A, AAC, OGG
- File size limit: 100MB (shown as error if exceeded)
- Upload progress bar displayed during upload
- On completion, processing status shown

**US-013: View Meeting List**
> As a user, I can see all my past meetings in reverse chronological order.

Acceptance Criteria:
- Shows meeting title, date, and status badge
- Status badges: Pending (grey), Processing (amber), Ready (green), Failed (red)
- Pull-to-refresh updates the list
- Empty state shows "No meetings yet. Tap + to create one."

**US-014: Delete Meeting**
> As a user, I can delete a meeting and all its data.

Acceptance Criteria:
- Swipe-to-delete on list row, with confirmation alert
- Confirmation: "Delete this meeting? This cannot be undone."
- On confirm, removes from list immediately (optimistic UI)
- Deletes all associated transcript, summary, chat history

---

### AI Features

**US-020: View Transcript**
> As a user, I can read the full transcript of a meeting.

Acceptance Criteria:
- Transcript displayed in a scrollable view
- Shows estimated word count and duration
- Loading skeleton shown while transcript is being generated
- "Transcript not yet available" if processing is still running

**US-021: View Summary**
> As a user, I can view an AI-generated executive summary of the meeting.

Acceptance Criteria:
- Executive Summary section (max 150 words)
- Key Highlights as a bullet list (3–5 items)
- All sections shown in the Summary tab of MeetingDetailPage

**US-022: View Action Items**
> As a user, I can see extracted action items with owner, priority, and due date.

Acceptance Criteria:
- Each item shows: task description, owner name (or "Unassigned"), priority badge, due date (or "No date")
- Priority badges: High (red), Medium (amber), Low (green)
- Tapping an item allows marking it complete (local state only in MVP)

**US-023: View Decisions**
> As a user, I can see key decisions extracted from the meeting.

Acceptance Criteria:
- Each decision shows the decision text and "Made by" if available
- Minimum 1 decision shown if any were made
- Empty state: "No decisions were identified in this meeting"

**US-024: Chat With Meeting**
> As a user, I can ask natural language questions about any meeting.

Acceptance Criteria:
- Chat input at the bottom, message bubbles above
- User messages right-aligned (purple), assistant left-aligned (grey)
- Typing indicator shown while waiting for response
- "Chat unavailable — meeting is still processing" if embeddings not ready
- Chat history persists across app sessions

---

### Search & Export

**US-030: Search Meetings**
> As a user, I can search across all my meetings by keyword.

Acceptance Criteria:
- Search bar on MeetingsListPage filters as user types (debounce 300ms)
- Searches across: meeting title, transcript content, summary content
- Results show matching meeting with keyword highlighted
- Empty results: "No meetings match your search"

**US-031: Export Meeting**
> As a user, I can export a meeting's notes in multiple formats.

Acceptance Criteria:
- Export formats: PDF, Markdown, Plain Text
- PDF includes: title, date, summary, action items table, decisions, full transcript
- Native share sheet opens after export (share to Mail, Notes, Slack, etc.)
- Export button visible in MeetingDetailPage toolbar

---

## 5. Feature Specifications

### 5.1 Audio Recording

```
Component:     AudioRecordingService (platform-specific via DependencyService)
iOS impl:      AVAudioRecorder (AVFoundation)
Android impl:  MediaRecorder
Output format: M4A (iOS), AAC (Android)
Max duration:  4 hours
Permissions:   NSMicrophoneUsageDescription (iOS), RECORD_AUDIO (Android)
```

### 5.2 Processing Status Flow

```
pending → transcribing → summarising → embedding → completed
                                                  ↘ failed
```

Polling strategy: GET /meetings/{id}/status every 3 seconds.
Stop polling on: `completed` or `failed`.
Max poll attempts: 120 (6 minutes timeout).

### 5.3 Offline Behaviour

| Action | Online | Offline |
|---|---|---|
| View meeting list | Fetch from API | Serve from SQLite cache |
| View summary | Fetch from API | Serve from SQLite cache |
| Create meeting | Post to API | Queue, sync on reconnect |
| Upload audio | Upload to API | Queue, sync on reconnect |
| Chat with meeting | Post to API | Show "Chat requires connection" |

### 5.4 Token Management

```
Access token:   15 min expiry, stored in SecureStorage
Refresh token:  30 day expiry, stored in SecureStorage
Rotation:       New pair issued on every refresh call
Reuse detect:   If revoked token used → clear all local tokens → force login
```

---

## 6. Screen Inventory & Navigation

### 6.1 Screen List

| Screen | File | ViewModel |
|---|---|---|
| SplashPage | SplashPage.xaml | SplashViewModel |
| LoginPage | LoginPage.xaml | LoginViewModel |
| RegisterPage | RegisterPage.xaml | RegisterViewModel |
| ForgotPasswordPage | ForgotPasswordPage.xaml | ForgotPasswordViewModel |
| MeetingsListPage | MeetingsListPage.xaml | MeetingsListViewModel |
| CreateMeetingPage | CreateMeetingPage.xaml | CreateMeetingViewModel |
| RecordMeetingPage | RecordMeetingPage.xaml | RecordingViewModel |
| UploadAudioPage | UploadAudioPage.xaml | UploadAudioViewModel |
| MeetingDetailPage | MeetingDetailPage.xaml | MeetingDetailViewModel |
| SummaryPage | SummaryPage.xaml | SummaryViewModel |
| TranscriptPage | TranscriptPage.xaml | TranscriptViewModel |
| ChatPage | ChatPage.xaml | ChatViewModel |
| ExportPage | ExportPage.xaml | ExportViewModel |
| SearchPage | SearchPage.xaml | SearchViewModel |
| ProfilePage | ProfilePage.xaml | ProfileViewModel |
| SettingsPage | SettingsPage.xaml | SettingsViewModel |

### 6.2 Navigation Flow

```
App Launch
    │
    ├── No token / expired ──────────────────────────────────┐
    │                                                        ▼
    │                                               LoginPage
    │                                                 │     │
    │                                       Register ◄     ► ForgotPassword
    │
    └── Token valid ──► AppShell (Shell routing)
                              │
                    ┌─────────┴──────────┐
                    │                    │
             Tab: Meetings          Tab: Profile
                    │
            MeetingsListPage  [SearchPage via toolbar icon]
                    │
           ┌────────┴──────────┐
           │                   │
        [FAB +]           [Meeting row tap]
           │                   │
    CreateMeetingPage    MeetingDetailPage
           │               ┌───┴────────────────┐
    ┌──────┴──────┐      [Tab]               [Tab]
    │             │     Summary           Transcript
  Record        Upload      │
  Meeting       Audio    [Tab]
                          Chat
                        [Toolbar]
                          Export ──► ExportPage
```

---

## 7. MAUI Folder Structure

```
MeetingNotes.MAUI/
│
├── MauiProgram.cs                          # App entry, DI registration, fonts
├── AppShell.xaml                           # Shell routes definition
├── AppShell.xaml.cs
│
├── Core/
│   ├── Constants/
│   │   ├── AppConstants.cs                 # API base URL, timeout, max file size
│   │   └── NavigationRoutes.cs             # Route name constants (avoid magic strings)
│   ├── Extensions/
│   │   ├── ServiceCollectionExtensions.cs  # Register all services + ViewModels
│   │   └── StringExtensions.cs             # Truncate, ToTitleCase helpers
│   └── Helpers/
│       ├── DateTimeHelper.cs               # "2 days ago" relative formatting
│       └── FileSizeHelper.cs               # Bytes → "2.4 MB"
│
├── Models/                                 # Local DTOs (mirror API responses)
│   ├── MeetingDto.cs
│   ├── MeetingDetailDto.cs
│   ├── SummaryDto.cs
│   ├── ActionItemDto.cs
│   ├── DecisionDto.cs
│   ├── TranscriptDto.cs
│   ├── ChatMessageDto.cs
│   └── UserDto.cs
│
├── Services/
│   ├── Interfaces/
│   │   ├── IMeetingService.cs
│   │   ├── IAuthService.cs
│   │   ├── IChatService.cs
│   │   ├── IExportService.cs
│   │   ├── IAudioRecordingService.cs
│   │   └── ILocalCacheService.cs
│   ├── Api/
│   │   ├── AuthService.cs                  # Calls /auth/* endpoints
│   │   ├── MeetingService.cs               # Calls /meetings/* endpoints
│   │   └── ChatService.cs                  # Calls /meetings/{id}/chat endpoints
│   ├── Local/
│   │   ├── LocalCacheService.cs            # SQLite-net-pcl offline cache
│   │   └── SecureTokenService.cs           # SecureStorage read/write/clear
│   ├── Platform/
│   │   └── AudioRecordingService.cs        # Calls platform implementations
│   └── Export/
│       └── ExportService.cs                # Download + share file
│
├── Platforms/
│   ├── iOS/
│   │   ├── AudioRecording/
│   │   │   └── iOSAudioRecordingService.cs # AVAudioRecorder implementation
│   │   ├── Info.plist                      # NSMicrophoneUsageDescription
│   │   └── AppDelegate.cs
│   └── Android/
│       ├── AudioRecording/
│       │   └── AndroidAudioRecordingService.cs # MediaRecorder implementation
│       ├── AndroidManifest.xml             # RECORD_AUDIO permission
│       └── MainActivity.cs
│
├── Http/
│   ├── AuthHttpHandler.cs                  # DelegatingHandler: inject JWT + auto-refresh
│   └── ApiClient.cs                        # HttpClient factory wrapper
│
├── ViewModels/
│   ├── Base/
│   │   ├── BaseViewModel.cs                # IsBusy, Title, OnNavigatedTo
│   │   └── BaseDetailViewModel.cs          # Guid Id query attribute parsing
│   ├── Auth/
│   │   ├── LoginViewModel.cs
│   │   ├── RegisterViewModel.cs
│   │   └── ForgotPasswordViewModel.cs
│   ├── Meetings/
│   │   ├── MeetingsListViewModel.cs        # Load, search, delete, navigate
│   │   ├── CreateMeetingViewModel.cs       # Create + navigate to record/upload
│   │   ├── RecordingViewModel.cs           # Start, pause, resume, stop + upload
│   │   ├── UploadAudioViewModel.cs         # FilePicker, upload with progress
│   │   └── MeetingDetailViewModel.cs       # Load detail, tab state
│   ├── Content/
│   │   ├── SummaryViewModel.cs             # Load summary, action items, decisions
│   │   ├── TranscriptViewModel.cs          # Load transcript text
│   │   └── ChatViewModel.cs               # Send message, load history, RAG chat
│   ├── Search/
│   │   └── SearchViewModel.cs             # Debounced search, navigate to result
│   ├── Export/
│   │   └── ExportViewModel.cs             # Format selection, download, share
│   └── Profile/
│       ├── ProfileViewModel.cs
│       └── SettingsViewModel.cs
│
├── Views/
│   ├── Auth/
│   │   ├── LoginPage.xaml
│   │   ├── LoginPage.xaml.cs
│   │   ├── RegisterPage.xaml
│   │   ├── RegisterPage.xaml.cs
│   │   ├── ForgotPasswordPage.xaml
│   │   └── ForgotPasswordPage.xaml.cs
│   ├── Meetings/
│   │   ├── MeetingsListPage.xaml
│   │   ├── MeetingsListPage.xaml.cs
│   │   ├── CreateMeetingPage.xaml
│   │   ├── CreateMeetingPage.xaml.cs
│   │   ├── RecordMeetingPage.xaml
│   │   ├── RecordMeetingPage.xaml.cs
│   │   ├── UploadAudioPage.xaml
│   │   ├── UploadAudioPage.xaml.cs
│   │   ├── MeetingDetailPage.xaml          # TabbedPage host
│   │   └── MeetingDetailPage.xaml.cs
│   ├── Content/
│   │   ├── SummaryPage.xaml
│   │   ├── SummaryPage.xaml.cs
│   │   ├── TranscriptPage.xaml
│   │   ├── TranscriptPage.xaml.cs
│   │   ├── ChatPage.xaml
│   │   └── ChatPage.xaml.cs
│   ├── Search/
│   │   ├── SearchPage.xaml
│   │   └── SearchPage.xaml.cs
│   ├── Export/
│   │   ├── ExportPage.xaml
│   │   └── ExportPage.xaml.cs
│   └── Profile/
│       ├── ProfilePage.xaml
│       ├── ProfilePage.xaml.cs
│       ├── SettingsPage.xaml
│       └── SettingsPage.xaml.cs
│
├── Controls/                               # Reusable custom controls
│   ├── StatusBadge.xaml                    # Pending/Processing/Ready/Failed badge
│   ├── StatusBadge.xaml.cs
│   ├── ActionItemCard.xaml                 # Single action item display
│   ├── ActionItemCard.xaml.cs
│   ├── ChatBubble.xaml                     # User/assistant message bubble
│   ├── ChatBubble.xaml.cs
│   ├── SkeletonView.xaml                   # Loading skeleton placeholder
│   └── SkeletonView.xaml.cs
│
├── Converters/                             # IValueConverter implementations
│   ├── StatusToColorConverter.cs           # MeetingStatus → Color
│   ├── PriorityToBadgeColorConverter.cs    # "high" → Red
│   ├── BoolToVisibilityConverter.cs
│   ├── StringToVisibilityConverter.cs      # null/empty → Collapsed
│   └── DateTimeToRelativeConverter.cs      # DateTime → "3 days ago"
│
├── Resources/
│   ├── Styles/
│   │   ├── Colors.xaml                     # App colour palette
│   │   ├── Styles.xaml                     # Global styles (Button, Label, Entry)
│   │   └── Templates.xaml                  # DataTemplates (MeetingCell, etc.)
│   ├── Fonts/
│   │   └── (Inter or custom font files)
│   └── Images/
│       ├── appicon.svg
│       ├── splash.svg
│       └── (other assets)
│
└── MeetingNotes.MAUI.csproj
```

---

## 8. MVVM Architecture

### 8.1 BaseViewModel

```csharp
// ViewModels/Base/BaseViewModel.cs
public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public bool IsNotBusy => !IsBusy;

    protected void SetError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    protected void ClearError()
    {
        ErrorMessage = string.Empty;
        HasError = false;
    }

    public virtual Task OnNavigatedToAsync() => Task.CompletedTask;
    public virtual Task OnNavigatedFromAsync() => Task.CompletedTask;
}
```

### 8.2 MeetingsListViewModel

```csharp
// ViewModels/Meetings/MeetingsListViewModel.cs
public partial class MeetingsListViewModel : BaseViewModel
{
    private readonly IMeetingService _meetingService;

    [ObservableProperty]
    private ObservableCollection<MeetingDto> _meetings = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _isEmpty;

    public MeetingsListViewModel(IMeetingService meetingService)
        => _meetingService = meetingService;

    partial void OnSearchQueryChanged(string value)
        => SearchCommand.ExecuteAsync(null);

    [RelayCommand]
    private async Task LoadMeetingsAsync()
    {
        IsBusy = true;
        ClearError();
        try
        {
            var result = await _meetingService.GetMeetingsAsync();
            Meetings = new ObservableCollection<MeetingDto>(result);
            IsEmpty = !Meetings.Any();
        }
        catch (Exception ex)
        {
            SetError("Failed to load meetings. Pull to refresh.");
        }
        finally { IsBusy = false; IsRefreshing = false; }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            await LoadMeetingsAsync();
            return;
        }
        var result = await _meetingService.GetMeetingsAsync(search: SearchQuery);
        Meetings = new ObservableCollection<MeetingDto>(result);
        IsEmpty = !Meetings.Any();
    }

    [RelayCommand]
    private async Task NavigateToMeetingAsync(MeetingDto meeting)
        => await Shell.Current.GoToAsync(
               $"{NavigationRoutes.MeetingDetail}?id={meeting.Id}");

    [RelayCommand]
    private async Task DeleteMeetingAsync(MeetingDto meeting)
    {
        bool confirmed = await Shell.Current.DisplayAlert(
            "Delete Meeting",
            "This cannot be undone. Are you sure?",
            "Delete", "Cancel");
        if (!confirmed) return;
        Meetings.Remove(meeting);
        await _meetingService.DeleteMeetingAsync(meeting.Id);
    }

    [RelayCommand]
    private async Task NavigateToCreateAsync()
        => await Shell.Current.GoToAsync(NavigationRoutes.CreateMeeting);
}
```

### 8.3 RecordingViewModel

```csharp
// ViewModels/Meetings/RecordingViewModel.cs
public partial class RecordingViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IAudioRecordingService _recorder;
    private readonly IMeetingService _meetingService;
    private Guid _meetingId;
    private CancellationTokenSource? _timerCts;

    [ObservableProperty] private TimeSpan _elapsed;
    [ObservableProperty] private bool _isRecording;
    [ObservableProperty] private bool _isPaused;
    [ObservableProperty] private bool _isUploading;
    [ObservableProperty] private double _uploadProgress;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _meetingId = Guid.Parse(query["id"].ToString()!);

    [RelayCommand]
    private async Task StartRecordingAsync()
    {
        var path = Path.Combine(FileSystem.CacheDirectory, $"{_meetingId}.m4a");
        await _recorder.StartAsync(path);
        IsRecording = true;
        _ = RunTimerAsync();
    }

    [RelayCommand]
    private async Task PauseResumeAsync()
    {
        if (IsPaused) { await _recorder.ResumeAsync(); IsPaused = false; }
        else { await _recorder.PauseAsync(); IsPaused = true; }
    }

    [RelayCommand]
    private async Task StopAndUploadAsync()
    {
        _timerCts?.Cancel();
        var filePath = await _recorder.StopAsync();
        IsRecording = false;
        IsUploading = true;
        using var stream = File.OpenRead(filePath);
        await _meetingService.UploadAudioAsync(_meetingId, stream,
            Path.GetFileName(filePath),
            progress => UploadProgress = progress);
        IsUploading = false;
        await Shell.Current.GoToAsync(
            $"{NavigationRoutes.MeetingDetail}?id={_meetingId}");
    }

    private async Task RunTimerAsync()
    {
        _timerCts = new CancellationTokenSource();
        while (!_timerCts.Token.IsCancellationRequested)
        {
            await Task.Delay(1000, _timerCts.Token);
            if (!IsPaused) Elapsed = Elapsed.Add(TimeSpan.FromSeconds(1));
        }
    }
}
```

### 8.4 ChatViewModel

```csharp
// ViewModels/Content/ChatViewModel.cs
public partial class ChatViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IChatService _chatService;
    private Guid _meetingId;

    [ObservableProperty]
    private ObservableCollection<ChatMessageDto> _messages = new();

    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private bool _isTyping;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _meetingId = Guid.Parse(query["id"].ToString()!);
        _ = LoadHistoryAsync();
    }

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendMessageAsync()
    {
        var question = InputText.Trim();
        InputText = string.Empty;
        Messages.Add(new ChatMessageDto { Role = "user", Content = question });
        IsTyping = true;
        try
        {
            var history = Messages.TakeLast(6)
                .Select(m => new ChatHistoryItem { Role = m.Role, Content = m.Content })
                .ToList();
            var response = await _chatService.SendMessageAsync(
                _meetingId, question, history);
            Messages.Add(new ChatMessageDto
                { Role = "assistant", Content = response.Answer });
        }
        catch { SetError("Could not get a response. Please try again."); }
        finally { IsTyping = false; }
    }

    private bool CanSend() => !string.IsNullOrWhiteSpace(InputText) && !IsTyping;

    private async Task LoadHistoryAsync()
    {
        var history = await _chatService.GetHistoryAsync(_meetingId);
        Messages = new ObservableCollection<ChatMessageDto>(history);
    }
}
```

---

## 9. Service Layer

### 9.1 IMeetingService

```csharp
public interface IMeetingService
{
    Task<List<MeetingDto>> GetMeetingsAsync(string? search = null, int page = 1);
    Task<MeetingDetailDto> GetMeetingAsync(Guid id);
    Task<Guid> CreateMeetingAsync(string title, DateTime meetingDate, string? description);
    Task UploadAudioAsync(Guid meetingId, Stream stream, string fileName,
        Action<double>? onProgress = null);
    Task<MeetingStatusDto> GetStatusAsync(Guid meetingId);
    Task<SummaryDto> GetSummaryAsync(Guid meetingId);
    Task<TranscriptDto> GetTranscriptAsync(Guid meetingId);
    Task DeleteMeetingAsync(Guid meetingId);
    Task<byte[]> ExportMeetingAsync(Guid meetingId, ExportFormat format);
}
```

### 9.2 AuthHttpHandler

```csharp
// Http/AuthHttpHandler.cs
public class AuthHttpHandler : DelegatingHandler
{
    private readonly ISecureTokenService _tokenService;
    private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var token = await _tokenService.GetAccessTokenAsync();
        if (token != null)
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _refreshSemaphore.WaitAsync(ct);
            try
            {
                var newToken = await TryRefreshAsync(ct);
                if (newToken == null)
                {
                    await _tokenService.ClearAllTokensAsync();
                    WeakReferenceMessenger.Default.Send(new SessionExpiredMessage());
                    return response;
                }
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", newToken);
                response = await base.SendAsync(request, ct);
            }
            finally { _refreshSemaphore.Release(); }
        }
        return response;
    }

    private async Task<string?> TryRefreshAsync(CancellationToken ct)
    {
        var refreshToken = await _tokenService.GetRefreshTokenAsync();
        if (refreshToken == null) return null;
        // Call refresh endpoint, store new tokens, return new access token
        // ...
        return null;
    }
}
```

### 9.3 DI Registration (MauiProgram.cs)

```csharp
// MauiProgram.cs
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder.UseMauiApp<App>().ConfigureFonts(fonts =>
    {
        fonts.AddFont("Inter-Regular.ttf", "InterRegular");
        fonts.AddFont("Inter-Medium.ttf", "InterMedium");
    });

    // HTTP
    builder.Services.AddTransient<AuthHttpHandler>();
    builder.Services.AddHttpClient<IMeetingService, MeetingService>(client =>
    {
        client.BaseAddress = new Uri(AppConstants.ApiBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    }).AddHttpMessageHandler<AuthHttpHandler>();

    // Services
    builder.Services.AddSingleton<ISecureTokenService, SecureTokenService>();
    builder.Services.AddSingleton<ILocalCacheService, LocalCacheService>();
    builder.Services.AddSingleton<IAudioRecordingService, AudioRecordingService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IChatService, ChatService>();
    builder.Services.AddScoped<IExportService, ExportService>();

    // ViewModels
    builder.Services.AddTransient<LoginViewModel>();
    builder.Services.AddTransient<RegisterViewModel>();
    builder.Services.AddTransient<MeetingsListViewModel>();
    builder.Services.AddTransient<CreateMeetingViewModel>();
    builder.Services.AddTransient<RecordingViewModel>();
    builder.Services.AddTransient<UploadAudioViewModel>();
    builder.Services.AddTransient<MeetingDetailViewModel>();
    builder.Services.AddTransient<SummaryViewModel>();
    builder.Services.AddTransient<TranscriptViewModel>();
    builder.Services.AddTransient<ChatViewModel>();
    builder.Services.AddTransient<SearchViewModel>();
    builder.Services.AddTransient<ExportViewModel>();
    builder.Services.AddTransient<ProfileViewModel>();

    return builder.Build();
}
```

---

## 10. Non-Functional Requirements

| Category | Requirement |
|---|---|
| Performance | List loads in < 1 second on 4G |
| Performance | Smooth 60fps scroll on CollectionView |
| Offline | Meeting list and summaries available without internet |
| Security | All tokens in SecureStorage, never in Preferences |
| Security | No sensitive data in app logs |
| Accessibility | All controls have SemanticProperties.Description |
| Accessibility | Dynamic font size supported |
| Compatibility | iOS 16+, Android API 33+ |
| Error handling | All API calls wrapped in try/catch with user-visible error |
| Logging | Microsoft.Extensions.Logging to Application Insights |

---

## 11. Out of Scope (MVP)

- Speaker diarisation (who said what)
- Real-time live transcription (streaming)
- Team sharing / multi-user workspaces
- Calendar integration (auto-create meetings from calendar invites)
- Jira / Trello action item sync
- iPad-optimised layout
- Dark mode (Phase 2)
- Localisation / multi-language (Phase 2)

---

*End of Document — AI Meeting Notes MAUI PRD v1.0.0*

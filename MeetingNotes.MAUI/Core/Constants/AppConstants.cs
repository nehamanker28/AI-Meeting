namespace MeetingNotes.MAUI.Core.Constants;

public static class AppConstants
{
    // API config
    // 10.0.2.2 is the special IP address pointing to host machine's localhost in Android Emulator.
    // On iOS/Windows, localhost is used.
    public static readonly string ApiBaseUrl = DeviceInfo.Platform == DevicePlatform.Android 
        ? "http://10.0.2.2:8000" 
        : "http://localhost:8000";

    public const int ApiTimeoutSeconds = 30;
    
    // Limits
    public const long MaxAudioFileSizeInBytes = 100 * 1024 * 1024; // 100MB
    public const int MaxPollAttempts = 120; // 6 minutes
    public const int PollIntervalSeconds = 3;
}

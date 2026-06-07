using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MeetingNotes.MAUI.Models;

namespace MeetingNotes.MAUI.Services.LocalApi;

public sealed class LocalApiHttpMessageHandler : HttpMessageHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly List<MeetingDto> _meetings;
    private UserDto _user;

    public LocalApiHttpMessageHandler()
    {
        var now = DateTime.UtcNow;
        _meetings =
        [
            new MeetingDto
            {
                Id = Guid.NewGuid(),
                Title = "Product Strategy Alignment",
                MeetingDate = now.AddMinutes(-15),
                Status = "processing",
                CreatedAt = now.AddMinutes(-15)
            },
            new MeetingDto
            {
                Id = Guid.NewGuid(),
                Title = "Quarterly Growth Review",
                MeetingDate = now.AddHours(-2),
                Status = "ready",
                CreatedAt = now.AddHours(-2)
            },
            new MeetingDto
            {
                Id = Guid.NewGuid(),
                Title = "Engineering Sync: Architecture Update",
                MeetingDate = now.AddDays(-1),
                Status = "completed",
                CreatedAt = now.AddDays(-1)
            },
            new MeetingDto
            {
                Id = Guid.NewGuid(),
                Title = "Client Discovery: Project Zenith",
                MeetingDate = now.AddDays(-2),
                Status = "failed",
                ErrorMessage = "Transcription service unavailable",
                CreatedAt = now.AddDays(-2)
            },
            new MeetingDto
            {
                Id = Guid.NewGuid(),
                Title = "Internal Kickoff - Mobile App v2",
                MeetingDate = new DateTime(2023, 10, 12, 10, 0, 0, DateTimeKind.Utc),
                Status = "ready",
                CreatedAt = new DateTime(2023, 10, 12, 10, 0, 0, DateTimeKind.Utc)
            }
        ];

        _user = new UserDto
        {
            Id = Guid.NewGuid(),
            FullName = "AI Notes User",
            Email = "user@ainotes.app",
            CreatedAt = now
        };
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await Task.Delay(180, cancellationToken);

        var path = request.RequestUri?.AbsolutePath.TrimEnd('/') ?? string.Empty;
        var method = request.Method.Method.ToUpperInvariant();

        if (path.Equals("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase) && method == "POST")
        {
            return JsonResponse(new { accessToken = "local-access-token", refreshToken = "local-refresh-token" });
        }

        if (path.Equals("/api/v1/auth/register", StringComparison.OrdinalIgnoreCase) && method == "POST")
        {
            return JsonResponse(new { success = true });
        }

        if (path.Equals("/api/v1/auth/forgot-password", StringComparison.OrdinalIgnoreCase) && method == "POST")
        {
            return JsonResponse(new { success = true });
        }

        if (path.Equals("/api/v1/auth/profile", StringComparison.OrdinalIgnoreCase) && method == "GET")
        {
            return JsonResponse(_user);
        }

        if (path.Equals("/api/v1/auth/profile", StringComparison.OrdinalIgnoreCase) && method == "PUT")
        {
            var payload = await ReadBodyAsync<ProfileUpdateRequest>(request, cancellationToken);
            if (!string.IsNullOrWhiteSpace(payload?.FullName))
            {
                _user.FullName = payload.FullName;
            }

            _user.AvatarUrl = payload?.AvatarUrl;
            return JsonResponse(_user);
        }

        if (path.Equals("/api/v1/auth/logout", StringComparison.OrdinalIgnoreCase) && method == "POST")
        {
            return JsonResponse(new { success = true });
        }

        if (path.Equals("/api/v1/meetings", StringComparison.OrdinalIgnoreCase) && method == "GET")
        {
            var search = GetQueryParam(request.RequestUri, "search");
            var list = string.IsNullOrWhiteSpace(search)
                ? _meetings
                : _meetings.Where(m => m.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            var result = new
            {
                items = list.OrderByDescending(m => m.MeetingDate).ToList(),
                total = list.Count,
                page = 1,
                pageSize = 20
            };
            return JsonResponse(result);
        }

        if (path.Equals("/api/v1/meetings", StringComparison.OrdinalIgnoreCase) && method == "POST")
        {
            var payload = await ReadBodyAsync<CreateMeetingRequest>(request, cancellationToken);
            var now = DateTime.UtcNow;
            var created = new MeetingDto
            {
                Id = Guid.NewGuid(),
                Title = payload?.Title ?? "Untitled Meeting",
                Description = payload?.Description,
                MeetingDate = payload?.MeetingDate ?? now,
                Status = "processing",
                CreatedAt = now
            };
            _meetings.Insert(0, created);
            return JsonResponse(created);
        }

        if (TryExtractMeetingId(path, out var meetingId))
        {
            var meeting = _meetings.FirstOrDefault(m => m.Id == meetingId);
            if (meeting == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            if (path.EndsWith("/audio", StringComparison.OrdinalIgnoreCase) && method == "POST")
            {
                meeting.Status = "processing";
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            if (path.EndsWith("/status", StringComparison.OrdinalIgnoreCase) && method == "GET")
            {
                return JsonResponse(new MeetingStatusDto
                {
                    Status = meeting.Status,
                    ErrorMessage = meeting.ErrorMessage
                });
            }

            if (path.EndsWith("/summary", StringComparison.OrdinalIgnoreCase) && method == "GET")
            {
                return JsonResponse(new SummaryDto
                {
                    MeetingId = meetingId,
                    ExecutiveSummary = "Local summary generated from mock API.",
                    DetailedSummary = "This is a local in-memory API response to support UI development without backend login.",
                    Highlights = new List<string>
                    {
                        "Dashboard is available without login.",
                        "Meeting cards are loaded from local API simulation.",
                        "Profile updates are persisted in-memory."
                    },
                    ActionItems = new List<ActionItemDto>
                    {
                        new ActionItemDto
                        {
                            Id = Guid.NewGuid(),
                            Task = "Connect real backend endpoints",
                            Owner = "Engineering",
                            Priority = "high"
                        }
                    }
                });
            }

            if (path.EndsWith("/transcript", StringComparison.OrdinalIgnoreCase) && method == "GET")
            {
                return JsonResponse(new TranscriptDto
                {
                    Id = Guid.NewGuid(),
                    MeetingId = meetingId,
                    RawText = "Local transcript text for development mode.",
                    Language = "en",
                    WordCount = 7
                });
            }

            if (path.EndsWith("/chat", StringComparison.OrdinalIgnoreCase) && method == "GET")
            {
                var history = new List<ChatMessageDto>
                {
                    new ChatMessageDto
                    {
                        Id = Guid.NewGuid(),
                        MeetingId = meetingId,
                        Role = "assistant",
                        Content = "Ask me anything about this meeting. I am running in local API mode.",
                        CreatedAt = DateTime.UtcNow
                    }
                };
                return JsonResponse(history);
            }

            if (path.EndsWith("/chat", StringComparison.OrdinalIgnoreCase) && method == "POST")
            {
                var payload = await ReadBodyAsync<ChatRequest>(request, cancellationToken);
                var answer = string.IsNullOrWhiteSpace(payload?.Question)
                    ? "Please enter a question."
                    : $"Local API response: I received '{payload.Question}'.";

                return JsonResponse(new { answer });
            }

            if (path.EndsWith("/export", StringComparison.OrdinalIgnoreCase) && method == "GET")
            {
                var bytes = Encoding.UTF8.GetBytes($"Export for meeting '{meeting.Title}' from local API mode.");
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(bytes)
                };
            }

            if (path.Equals($"/api/v1/meetings/{meetingId}", StringComparison.OrdinalIgnoreCase) && method == "DELETE")
            {
                _meetings.Remove(meeting);
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            if (path.Equals($"/api/v1/meetings/{meetingId}", StringComparison.OrdinalIgnoreCase) && method == "GET")
            {
                var detail = new MeetingDetailDto
                {
                    Id = meeting.Id,
                    Title = meeting.Title,
                    Description = meeting.Description,
                    MeetingDate = meeting.MeetingDate,
                    Status = meeting.Status,
                    CreatedAt = meeting.CreatedAt,
                    Transcript = new TranscriptDto
                    {
                        Id = Guid.NewGuid(),
                        MeetingId = meeting.Id,
                        RawText = "Meeting transcript from local in-memory API.",
                        Language = "en",
                        WordCount = 6
                    },
                    Summary = new SummaryDto
                    {
                        MeetingId = meeting.Id,
                        ExecutiveSummary = "Local API detail summary",
                        DetailedSummary = "Detailed summary generated by local in-memory API for development.",
                        Highlights = new List<string> { "No backend required", "Login bypassed" }
                    }
                };
                return JsonResponse(detail);
            }
        }

        return new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent($"No local route for {method} {path}")
        };
    }

    private static HttpResponseMessage JsonResponse<T>(T value)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private static string? GetQueryParam(Uri? uri, string key)
    {
        if (uri == null || string.IsNullOrWhiteSpace(uri.Query))
        {
            return null;
        }

        var query = uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in query)
        {
            var pieces = part.Split('=', 2);
            if (pieces.Length == 2 && pieces[0].Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(pieces[1]);
            }
        }

        return null;
    }

    private static bool TryExtractMeetingId(string path, out Guid meetingId)
    {
        meetingId = Guid.Empty;
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 4)
        {
            return false;
        }

        return segments[0].Equals("api", StringComparison.OrdinalIgnoreCase)
               && segments[1].Equals("v1", StringComparison.OrdinalIgnoreCase)
               && segments[2].Equals("meetings", StringComparison.OrdinalIgnoreCase)
               && Guid.TryParse(segments[3], out meetingId);
    }

    private static async Task<T?> ReadBodyAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content == null)
        {
            return default;
        }

        await using var stream = await request.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
    }

    private sealed class ProfileUpdateRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }

    private sealed class CreateMeetingRequest
    {
        public string? Title { get; set; }
        public DateTime? MeetingDate { get; set; }
        public string? Description { get; set; }
    }

    private sealed class ChatRequest
    {
        public string? Question { get; set; }
    }
}
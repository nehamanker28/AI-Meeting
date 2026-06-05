using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MeetingNotes.MAUI.Models;
using MeetingNotes.MAUI.Services.Interfaces;

namespace MeetingNotes.MAUI.Services.Api;

public class ChatService : IChatService
{
    private readonly HttpClient _httpClient;

    public ChatService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ChatMessageDto>> GetHistoryAsync(Guid meetingId)
    {
        var response = await _httpClient.GetAsync($"/api/v1/meetings/{meetingId}/chat");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<ChatMessageDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? new List<ChatMessageDto>();
    }

    public async Task<ChatMessageDto> SendMessageAsync(Guid meetingId, string question, List<ChatMessageDto> history)
    {
        var historyPayload = new List<object>();
        foreach (var msg in history)
        {
            historyPayload.Add(new { role = msg.Role, content = msg.Content });
        }

        var payload = new
        {
            question,
            history = historyPayload
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"/api/v1/meetings/{meetingId}/chat", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var chatResponse = JsonSerializer.Deserialize<ChatResponsePayload>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (chatResponse == null)
        {
            throw new Exception("Failed to deserialize chat response.");
        }

        return new ChatMessageDto
        {
            Id = Guid.NewGuid(),
            MeetingId = meetingId,
            Role = "assistant",
            Content = chatResponse.Answer,
            CreatedAt = DateTime.UtcNow
        };
    }

    private class ChatResponsePayload
    {
        public string Answer { get; set; } = string.Empty;
    }
}

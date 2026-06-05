using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MeetingNotes.MAUI.Models;
using MeetingNotes.MAUI.Services.Interfaces;

namespace MeetingNotes.MAUI.Services.Api;

public class MeetingService : IMeetingService
{
    private readonly HttpClient _httpClient;

    public MeetingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<MeetingDto>> GetMeetingsAsync(string? search = null, int page = 1)
    {
        var url = $"/api/v1/meetings?page={page}&page_size=20";
        if (!string.IsNullOrEmpty(search))
        {
            url += $"&search={Uri.EscapeDataString(search)}";
        }

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        try
        {
            var listResponse = JsonSerializer.Deserialize<MeetingListResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (listResponse?.Items != null)
            {
                return listResponse.Items;
            }
        }
        catch
        {
            var list = JsonSerializer.Deserialize<List<MeetingDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (list != null) return list;
        }

        return new List<MeetingDto>();
    }

    public async Task<MeetingDetailDto> GetMeetingAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"/api/v1/meetings/{id}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<MeetingDetailDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? throw new Exception("Failed to deserialize meeting details.");
    }

    public async Task<Guid> CreateMeetingAsync(string title, DateTime meetingDate, string? description)
    {
        var payload = new
        {
            title,
            meeting_date = meetingDate,
            description
        };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/v1/meetings", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var meeting = JsonSerializer.Deserialize<MeetingDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return meeting?.Id ?? throw new Exception("Failed to create meeting.");
    }

    public async Task UploadAudioAsync(Guid meetingId, Stream stream, string fileName, Action<double>? onProgress = null)
    {
        var totalLength = stream.Length;
        var progressStream = new ProgressStream(stream, totalLength, onProgress);
        
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(progressStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");
        content.Add(streamContent, "file", fileName);

        var response = await _httpClient.PostAsync($"/api/v1/meetings/{meetingId}/audio", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task<MeetingStatusDto> GetStatusAsync(Guid meetingId)
    {
        var response = await _httpClient.GetAsync($"/api/v1/meetings/{meetingId}/status");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<MeetingStatusDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? throw new Exception("Failed to deserialize meeting status.");
    }

    public async Task<SummaryDto> GetSummaryAsync(Guid meetingId)
    {
        var response = await _httpClient.GetAsync($"/api/v1/meetings/{meetingId}/summary");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SummaryDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? throw new Exception("Failed to deserialize summary.");
    }

    public async Task<TranscriptDto> GetTranscriptAsync(Guid meetingId)
    {
        var response = await _httpClient.GetAsync($"/api/v1/meetings/{meetingId}/transcript");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TranscriptDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? throw new Exception("Failed to deserialize transcript.");
    }

    public async Task DeleteMeetingAsync(Guid meetingId)
    {
        var response = await _httpClient.DeleteAsync($"/api/v1/meetings/{meetingId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<byte[]> ExportMeetingAsync(Guid meetingId, string format)
    {
        var response = await _httpClient.GetAsync($"/api/v1/meetings/{meetingId}/export?format={format.ToLower()}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync();
    }

    private class MeetingListResponse
    {
        public List<MeetingDto>? Items { get; set; }
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    private class ProgressStream : Stream
    {
        private readonly Stream _innerStream;
        private readonly long _totalBytes;
        private readonly Action<double>? _onProgress;
        private long _bytesWritten = 0;

        public ProgressStream(Stream innerStream, long totalBytes, Action<double>? onProgress)
        {
            _innerStream = innerStream;
            _totalBytes = totalBytes;
            _onProgress = onProgress;
        }

        public override bool CanRead => _innerStream.CanRead;
        public override bool CanSeek => _innerStream.CanSeek;
        public override bool CanWrite => _innerStream.CanWrite;
        public override long Length => _innerStream.Length;
        public override long Position { get => _innerStream.Position; set => _innerStream.Position = value; }

        public override void Flush() => _innerStream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = _innerStream.Read(buffer, offset, count);
            _bytesWritten += bytesRead;
            if (_totalBytes > 0 && _onProgress != null)
            {
                _onProgress((double)_bytesWritten / _totalBytes);
            }
            return bytesRead;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int bytesRead = await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
            _bytesWritten += bytesRead;
            if (_totalBytes > 0 && _onProgress != null)
            {
                _onProgress((double)_bytesWritten / _totalBytes);
            }
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin) => _innerStream.Seek(offset, origin);
        public override void SetLength(long value) => _innerStream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _innerStream.Write(buffer, offset, count);
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using MeetingNotes.MAUI.Models;
using MeetingNotes.MAUI.Services.Interfaces;
using System.Text.Json;

namespace MeetingNotes.MAUI.Services.Local;

public class LocalCacheService : ILocalCacheService
{
    private SQLiteAsyncConnection? _database;

    private async Task InitAsync()
    {
        if (_database != null)
            return;

        var databasePath = Path.Combine(Microsoft.Maui.Storage.FileSystem.AppDataDirectory, "meeting_notes_cache.db3");
        _database = new SQLiteAsyncConnection(databasePath);

        await _database.CreateTableAsync<CachedMeeting>();
        await _database.CreateTableAsync<CachedMeetingDetail>();
    }

    public async Task SaveMeetingsAsync(List<MeetingDto> meetings)
    {
        await InitAsync();
        
        await _database!.DeleteAllAsync<CachedMeeting>();
        
        var cached = new List<CachedMeeting>();
        foreach (var m in meetings)
        {
            cached.Add(new CachedMeeting
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                MeetingDate = m.MeetingDate,
                DurationSecs = m.DurationSecs,
                Status = m.Status,
                ErrorMessage = m.ErrorMessage,
                CreatedAt = m.CreatedAt
            });
        }
        await _database.InsertAllAsync(cached);
    }

    public async Task<List<MeetingDto>> GetMeetingsAsync(string? search = null)
    {
        await InitAsync();

        var query = _database!.Table<CachedMeeting>();
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(m => m.Title.Contains(search) || (m.Description != null && m.Description.Contains(search)));
        }

        var list = await query.OrderByDescending(m => m.MeetingDate).ToListAsync();
        var result = new List<MeetingDto>();
        foreach (var item in list)
        {
            result.Add(new MeetingDto
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                MeetingDate = item.MeetingDate,
                DurationSecs = item.DurationSecs,
                Status = item.Status,
                ErrorMessage = item.ErrorMessage,
                CreatedAt = item.CreatedAt
            });
        }
        return result;
    }

    public async Task SaveMeetingDetailAsync(MeetingDetailDto detail)
    {
        await InitAsync();

        var json = JsonSerializer.Serialize(detail);
        var cached = new CachedMeetingDetail
        {
            Id = detail.Id,
            JsonData = json,
            LastUpdatedAt = DateTime.UtcNow
        };

        await _database!.InsertOrReplaceAsync(cached);
    }

    public async Task<MeetingDetailDto?> GetMeetingDetailAsync(Guid id)
    {
        await InitAsync();

        var cached = await _database!.Table<CachedMeetingDetail>()
            .Where(m => m.Id == id)
            .FirstOrDefaultAsync();

        if (cached == null || string.IsNullOrEmpty(cached.JsonData))
            return null;

        try
        {
            return JsonSerializer.Deserialize<MeetingDetailDto>(cached.JsonData);
        }
        catch
        {
            return null;
        }
    }

    public async Task ClearCacheAsync()
    {
        await InitAsync();
        await _database!.DeleteAllAsync<CachedMeeting>();
        await _database!.DeleteAllAsync<CachedMeetingDetail>();
    }
}

public class CachedMeeting
{
    [PrimaryKey]
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime MeetingDate { get; set; }
    public int? DurationSecs { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CachedMeetingDetail
{
    [PrimaryKey]
    public Guid Id { get; set; }
    public string JsonData { get; set; } = string.Empty;
    public DateTime LastUpdatedAt { get; set; }
}

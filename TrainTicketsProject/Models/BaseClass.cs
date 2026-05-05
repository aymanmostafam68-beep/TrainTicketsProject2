public class BaseClass
{
    public DateTime CreatedAt { get; set; } =
        TimeZoneInfo.ConvertTime(DateTime.UtcNow,
        TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"));

    public string CreatedUserId { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } =
        TimeZoneInfo.ConvertTime(DateTime.UtcNow,
        TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"));

    public string UpdatedUserId { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
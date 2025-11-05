namespace Infrastructure.FirebaseDatabase;

public class FirebaseResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? RawJson { get; set; }
}
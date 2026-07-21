namespace GymTracker.Domain.Entities;

/// <summary>Entitate generica de media (imagine/video/pdf/gif), reutilizabila oriunde e nevoie
/// (Exercise, Machine si, in viitor, orice alta entitate) fara sa modificam schema existenta.</summary>
public class Media
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Url { get; set; } = null!;
    public string MimeType { get; set; } = null!; // video/mp4, image/png, application/pdf...
    public string Storage { get; set; } = null!;  // "AzureBlob" | "S3" | "Local"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
}

public class ExerciseMedia
{
    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public Guid MediaId { get; set; }
    public Media Media { get; set; } = null!;
}

public class MachineMedia
{
    public Guid MachineId { get; set; }
    public Machine Machine { get; set; } = null!;
    public Guid MediaId { get; set; }
    public Media Media { get; set; } = null!;
}

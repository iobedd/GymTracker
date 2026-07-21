using System.ComponentModel.DataAnnotations;

namespace GymTracker.Application.DTOs.Gyms;

public class GymDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
}

public class CreateGymRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = null!;

    [MaxLength(250)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [Phone, MaxLength(30)]
    public string? Phone { get; set; }
}

public class UpdateGymRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = null!;

    [MaxLength(250)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [Phone, MaxLength(30)]
    public string? Phone { get; set; }
}

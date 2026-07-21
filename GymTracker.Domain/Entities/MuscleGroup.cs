namespace GymTracker.Domain.Entities;

public class MuscleGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = null!; // Chest, Back, Shoulders, Arms, Legs, Glutes, Core
}

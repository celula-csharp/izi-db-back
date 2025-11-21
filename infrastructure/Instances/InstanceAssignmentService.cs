using Application.Instances.Services;
using IziDbBack.Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Instances;

public class InstanceAssignmentService : IInstanceAssignmentService
{
    private readonly SystemDbContext _context;

    public InstanceAssignmentService(SystemDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AssignInstanceToUserAsync(int userId, int databaseInstanceId)
    {
        // 1. Verificar si el usuario ya tiene instancia
        var existingInstance = await _context.UserInstances
            .FirstOrDefaultAsync(ui => ui.UserId == userId);

        if (existingInstance != null)
        {
            // Ya tiene instancia → NO asignar otra
            return false;
        }

        // (Opcional) verificar que el usuario exista y sea Student
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        if (user.Role == null || !string.Equals(user.Role.Name, "Student", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only users with role 'Student' can receive instances.");
        }

        // (Opcional) verificar que la instancia exista
        var dbInstance = await _context.DatabaseInstances
            .FirstOrDefaultAsync(di => di.Id == databaseInstanceId);

        if (dbInstance == null)
        {
            throw new InvalidOperationException("Database instance not found.");
        }

        // 2. Crear la nueva asignación
        var userInstance = new UserInstance
        {
            UserId = userId,
            DatabaseInstanceId = databaseInstanceId,
            AssignedAt = DateTime.UtcNow
        };

        _context.UserInstances.Add(userInstance);
        await _context.SaveChangesAsync();

        return true;
    }
}

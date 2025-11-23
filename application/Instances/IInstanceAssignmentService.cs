namespace Application.Instances.Services;

public interface IInstanceAssignmentService
{
    /// <summary>
    /// Asigna una instancia de base de datos a un usuario Student.
    /// Debe validar la regla "1 estudiante = 1 instancia".
    /// </summary>
    /// <param name="userId">Id del usuario (Student).</param>
    /// <param name="databaseInstanceId">Id de la instancia a asignar.</param>
    /// <returns>True si se asignó correctamente, false si ya tenía instancia.</returns>
    Task<bool> AssignInstanceToUserAsync(int userId, int databaseInstanceId);
}
using Backend.Core.Entities.Supplements;

namespace Backend.Core.Interfaces.IServices;

public interface ISmartSchedulingService
{
    /// <summary>
    /// Assigns each supplement to an intake slot based on chronobiology rules.
    /// </summary>
    IReadOnlyList<ScheduledIntake> BuildSchedule(IEnumerable<SupplementProduct> supplementProducts);
}

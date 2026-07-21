using Backend.Core.Entities.Supplements;
using Backend.Core.Enums;

namespace Backend.Core.Interfaces.IServices;

public interface ISchedulingService
{
    ScheduleTimeBlock RecommendTimeBlock(SupplementProduct supplementProduct);
}

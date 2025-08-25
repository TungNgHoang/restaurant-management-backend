using RestaurantManagement.Service.Dtos.AttendanceDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Interfaces
{
    public interface IAssignmentService
    {
        Task CreateAssignmentAsync(AssignmentDto dto);
        Task UpdateAssignmentAsync(AssignmentDto dto);
        Task<List<AssignmentGroupDto>> GetAssignmentsGroupedByDateAsync();
        Task<AssignmentGroupDto?> GetAssignmentsByDateAsync(DateOnly workDate);
    }
}

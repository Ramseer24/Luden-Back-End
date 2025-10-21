using Application.DTOs.UserDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions.Interfaces.Services
{
    public interface IProfileService
    {
        Task<UserProfileDto> GetUserProfileAsync(int userId);
        Task SetUserProfileAsync(UserProfileDto dto);
    }
}

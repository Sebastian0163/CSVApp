using CSVApp.Dal;
using CSVApp.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSVApp.Service.Manager
{
    public  interface IUserService
    {
        Task<List<UserViewModel>> GetAllUsersAsync();
        Task<UserViewModel> GetUserProfileByUsernameAsync(string username);
        Task<Dictionary<string, List<string>>> AddUserAsync(User user);

        Task<bool> DeleteUserAsync(long id);

        Task<bool> UpdateProfileAsync(string username, IFormCollection formData);
    }
}

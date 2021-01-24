using CSVApp.Dal;
using CSVApp.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CSVApp.Service.Manager
{
    public class UserService:IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _db;
        [Obsolete]
        private readonly IHostingEnvironment _env;

        [Obsolete]
        public UserService(UserManager<User> userManager, ApplicationDbContext db, IHostingEnvironment env)
        {
            _userManager = userManager;
            _db = db;
            _env = env;
        }


        public async Task<List<UserViewModel>> GetAllUsersAsync()
        {
            List<UserViewModel> usersList = new List<UserViewModel>();

            try
            {
                usersList = new List<UserViewModel>();


                var result = await _db.Users.ToListAsync();
                if (result != null)
                {
                    usersList.AddRange(result.Select(item => new UserViewModel
                    {
                        Name = item.Name,
                       Birthday = item.Birthday,
                        Married = item.Married,
                        Phone = item.Phone,
                        Csv_file = item.Csv_file,
                        Salary = item.Salary
                    }));
                }

            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);

            }
            return usersList;
        }

        public async Task<Dictionary<string, List<string>>> AddUserAsync(User csv)
        {
            IdentityResult result = new IdentityResult();
            var errorList = new List<string>();
            try
            {
                result = await _userManager.CreateAsync(csv);
                if (result.Succeeded)
                {
                    // Update the CVS table CvsId Column
                    return CreateResponse(result.Succeeded, errorList);
                }

                errorList.AddRange(result.Errors.Select(error => error.Description));

            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return CreateResponse(result.Succeeded, errorList);
        }

        public async Task<bool> DeleteUserAsync(long id)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(x => x.CsvId == id);

                if (user != null)
                {
                    await using var dbContextTransaction = await _db.Database.BeginTransactionAsync();
                    try
                    {
                        user.IsActive = false;
                        _db.Entry(user).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        await dbContextTransaction.CommitAsync();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                            ex.Message, ex.StackTrace, ex.InnerException, ex.Source);

                        await dbContextTransaction.RollbackAsync();
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return false;
        }

        [Obsolete]
        public async Task<bool> UpdateProfileAsync(string username, IFormCollection formData)
        {

            try
            {
                var user = await _userManager.FindByNameAsync(username);

                if (user == null) return false;

                await UpdateProfilePicAsync(formData, user);
                user.Name = formData["Name"];
                user.Phone = formData["Phone"];
                user.Married = Convert.ToBoolean(Convert.ToInt32(formData["Is.Married"]));
                user.Birthday = formData["Birthday"];
                user.Salary = formData["Salary"];
                return true;

            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return false;
        }



        /// <summary>
        /// //helper  CreateResponse
        /// </summary>
        /// <param name="response"></param>
        /// <param name="errorList"></param>
        /// <returns></returns>
        private static Dictionary<string, List<string>> CreateResponse(bool response, List<string> errorList)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            if (response)
            {
                result.Add("Success", new List<string>());
                return result;
            }

            result.Add("Failed", errorList);
            return result;
        }

        [Obsolete]
        private async Task<User> UpdateProfilePicAsync(IFormCollection formData, User csv)
        {
            // First we create an empty array to store old file info
            var oldcsvfile = new string[1];
            // we will store path of old file to delete in an empty array.
            oldcsvfile[0] = Path.Combine(_env.WebRootPath + csv.Csv_file);

            // Create the Profile csvfile Path
            var ProfilecsvfilePath = _env.WebRootPath + $"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}csv{Path.DirectorySeparatorChar}profile{Path.DirectorySeparatorChar}";

            // If we have received any files for update, then we update the file path after saving to server
            // else we return the csv without any changes
            if (formData.Files.Count <= 0) return csv;

            var extension = Path.GetExtension(formData.Files[0].FileName);
            var filename = DateTime.Now.ToString("yymmssfff");
            var path = Path.Combine(ProfilecsvfilePath, filename) + extension;
            var dbImagePath = Path.Combine($"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}csv{Path.DirectorySeparatorChar}profile{Path.DirectorySeparatorChar}", filename) + extension;

            csv.Csv_file = dbImagePath;

            // Copying New Files to the Server - profile Folder
            await using (var stream = new FileStream(path, FileMode.Create))
            {
                await formData.Files[0].CopyToAsync(stream);
            }

            // Delete old file after successful update
            if (!System.IO.File.Exists(oldcsvfile[0])) return csv;

            System.IO.File.SetAttributes(oldcsvfile[0], FileAttributes.Normal);
            System.IO.File.Delete(oldcsvfile[0]);

            return csv;
        }

        public async Task<UserViewModel> GetUserProfileByUsernameAsync(string username)
        {
            var userModel = new UserViewModel();

            try
            {

                var user = await _userManager.FindByNameAsync(username);

                if (user == null) return null;

                userModel = new UserViewModel
                {
                    Name = user.Name,
                    Birthday = user.Birthday,
                    Married = user.Married,
                    Csv_file = user.Csv_file,
                    Salary = user.Salary
                };

            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return userModel;
        }
    }

}
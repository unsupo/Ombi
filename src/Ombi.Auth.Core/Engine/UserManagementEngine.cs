using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Auth.Core.Models;
using Ombi.Auth.Policy;
using Ombi.Core.Authentication;
using Ombi.Core.Engine;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using ClaimCheckboxes = Ombi.Core.Models.UI.ClaimCheckboxes;
using UserViewModel = Ombi.Core.Models.UI.UserViewModel;

namespace Ombi.Auth.Core.Engine
{
    public class UserManagementEngine
    {
        private readonly OmbiUserManager _userManager;
        private readonly OmbiRoleManager _roleManager;
        private readonly ILogger _logger;
        private readonly IMovieRequestEngine _movieRequestEngine;
        private readonly ITvRequestEngine _tvRequestEngine;
        private readonly IMusicRequestEngine _musicEngine;
        private readonly IRepository<UserQualityProfiles> _userProfiles;
        private readonly IOmbiAuthorizationService _authorizationService;

        public UserManagementEngine(OmbiUserManager user, OmbiRoleManager role, ILogger<UserManagementEngine> log,
            IMovieRequestEngine movieRequestEngine, ITvRequestEngine tvRequestEngine, IMusicRequestEngine musicEngine, IRepository<UserQualityProfiles> userProfiles,
            IOmbiAuthorizationService authorizationService)
        {
            _userManager = user;
            _roleManager = role;
            _logger = log;
            _movieRequestEngine = movieRequestEngine;
            _tvRequestEngine = tvRequestEngine;
            _musicEngine = musicEngine;
            _userProfiles = userProfiles;
            _authorizationService = authorizationService;
        }

        public async Task<OmbiIdentityResult> CreateLocalUser(UserViewModel user)
        {
            if (!EmailValidator.IsValidEmail(user.EmailAddress))
            {
                return Error($"The email address {user.EmailAddress} is not a valid format");
            }
            if (!await _authorizationService.HasPermissionAsync(PermissionAction.Create + Permissions.User))
            {
                return Error("You do not have the correct permissions to create this user");
            }
            var ombiUser = new OmbiUser
            {
                Alias = user.Alias,
                Email = user.EmailAddress,
                UserName = user.UserName,
                UserType = UserType.LocalUser,
                MovieRequestLimit = user.MovieRequestLimit,
                EpisodeRequestLimit = user.EpisodeRequestLimit,
                MusicRequestLimit = user.MusicRequestLimit,
                UserAccessToken = Guid.NewGuid().ToString("N"),
            };
            var userResult = await _userManager.CreateAsync(ombiUser, user.Password);

            if (!userResult.Succeeded)
            {
                // We did not create the user
                return new OmbiIdentityResult
                {
                    Errors = userResult.Errors.Select(x => x.Description).ToList()
                };
            }

            var roleResult = await AddRoles(user.Claims, ombiUser);

            if (roleResult.Any(x => !x.Succeeded))
            {
                var messages = new List<string>();
                foreach (var errors in roleResult.Where(x => !x.Succeeded))
                {
                    messages.AddRange(errors.Errors.Select(x => x.Description).ToList());
                }

                return new OmbiIdentityResult
                {
                    Errors = messages
                };
            }

            // Add the quality profiles
            if (user.UserQualityProfiles != null)
            {
                user.UserQualityProfiles.UserId = ombiUser.Id;
                await _userProfiles.Add(user.UserQualityProfiles);
            }
            else
            {
                user.UserQualityProfiles = new UserQualityProfiles
                {
                    UserId = ombiUser.Id
                };
            }

            return new OmbiIdentityResult
            {
                Successful = true
            };
        }

        private async Task<List<IdentityResult>> AddRoles(IEnumerable<ClaimCheckboxes> roles, OmbiUser ombiUser)
        {
            var roleResult = new List<IdentityResult>();
            foreach (var role in roles)
            {
                if (role.Enabled)
                {
                    roleResult.Add(await _userManager.AddToRoleAsync(ombiUser, role.Value));
                }
            }
            return roleResult;
        }

        private async Task<UserViewModel> GetUserWithRoles(OmbiUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var vm = new UserViewModel
            {
                Alias = user.Alias,
                UserName = user.UserName,
                Id = user.Id,
                EmailAddress = user.Email,
                UserType = (Ombi.Core.Models.UserType)(int)user.UserType,
                Claims = new List<ClaimCheckboxes>(),
                LastLoggedIn = user.LastLoggedIn,
                HasLoggedIn = user.LastLoggedIn.HasValue,
                EpisodeRequestLimit = user.EpisodeRequestLimit ?? 0,
                MovieRequestLimit = user.MovieRequestLimit ?? 0,
                MusicRequestLimit = user.MusicRequestLimit ?? 0,
            };

            foreach (var role in userRoles)
            {
                vm.Claims.Add(new ClaimCheckboxes
                {
                    Value = role,
                    Enabled = true
                });
            }

            // Add the missing claims
            var allRoles = await _roleManager.Roles.ToListAsync();
            var missing = allRoles.Select(x => x.Name).Except(userRoles);
            foreach (var role in missing)
            {
                vm.Claims.Add(new ClaimCheckboxes
                {
                    Value = role,
                    Enabled = false
                });
            }

            if (vm.EpisodeRequestLimit > 0)
            {
                vm.EpisodeRequestQuota = await _tvRequestEngine.GetRemainingRequests(user);
            }

            if (vm.MovieRequestLimit > 0)
            {
                vm.MovieRequestQuota = await _movieRequestEngine.GetRemainingRequests(user);
            }

            if (vm.MusicRequestLimit > 0)
            {
                vm.MusicRequestQuota = await _musicEngine.GetRemainingRequests(user);
            }

            // Get the quality profiles
            vm.UserQualityProfiles = await _userProfiles.GetAll().FirstOrDefaultAsync(x => x.UserId == user.Id) ?? new UserQualityProfiles
            {
                UserId = user.Id
            };

            return vm;
        }

        private OmbiIdentityResult Error(string message)
        {
            return new OmbiIdentityResult
            {
                Errors = new List<string> { message }
            };
        }
    }
}
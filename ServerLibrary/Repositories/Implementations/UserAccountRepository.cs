using BaseLibrary.DTOs;
using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServiceLibrary.Data;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories.Contracts;

namespace ServiceLibrary.Repositories.Implementations;

public class UserAccountRepository : IUserAccount
{
    private readonly AppDbContext _appDbContext;
    private readonly JwtSection _jwtSection;

    public UserAccountRepository(AppDbContext appDbContext, IOptions<JwtSection> jwtSection)
    {
        _appDbContext = appDbContext;
        _jwtSection = jwtSection.Value;
    }
    
    public async Task<GeneralResponse> CreateAsync(Register user)
    {
        if (user is null)
        {
            return new GeneralResponse("15", "Model is empty", null);
        }

        // Check if the user already exists by email
        var existingUser = await FindUserByEmail(user.Email!);
        if (existingUser != null)
        {
            return new GeneralResponse("15", "User with this email already exists", null);
        }

        // Hash the password and create new user
        var newUser = new ApplicationUser
        {
            Fullname = user.Fullname,
            Email = user.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(user.Password),
        };
        await AddToDatabase(newUser);

        // Ensure Admin role exists and assign Admin role if this is the first user
        var adminRole = await _appDbContext.SystemRoles.FirstOrDefaultAsync(r => r.Name == Constants.Admin);
        if (adminRole is null)
        {
            adminRole = await AddToDatabase(new SystemRole
            {
                Name = Constants.Admin
            });

            // Assign Admin role to the new user
            await AddToDatabase(new UserRole
            {
                RoleId = adminRole.Id,
                UserId = newUser.Id,
            });

            return new GeneralResponse("00", "Admin account created successfully!", newUser);
        }

        // Ensure User role exists
        var userRole = await _appDbContext.SystemRoles.FirstOrDefaultAsync(r => r.Name == Constants.User);
        if (userRole is null)
        {
            userRole = await AddToDatabase(new SystemRole
            {
                Name = Constants.User
            });
        }

        // Assign User role to the new user
        await AddToDatabase(new UserRole
        {
            RoleId = userRole.Id,
            UserId = newUser.Id,
        });

        return new GeneralResponse("00", "User account created successfully!", newUser);
    }

    public Task<LoginResponse> SignInAsync(Login user)
    {
        throw new NotImplementedException();
    }

    private async Task<ApplicationUser> FindUserByEmail(string email)
    {
        return await _appDbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower()); 
    }

    private async Task<T> AddToDatabase<T>(T model)
    {
        var result = _appDbContext.Add(model!);
        await _appDbContext.SaveChangesAsync();
        return (T)result.Entity;
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BaseLibrary.DTOs;
using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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

    public async Task<GeneralResponse> SignInAsync(Login user)
    {
        if (user is null)
        {
            return new GeneralResponse("15", "Model is empty", null);
        }
        
        var applicationUser = await FindUserByEmail(user.Email!);
        if (applicationUser is null)
        {
            return new GeneralResponse("15", "User with this email not found", null);
        }
        
        // verify password
        if (!BCrypt.Net.BCrypt.Verify(user.Password, applicationUser.Password))
        {
            return new GeneralResponse("15", " Email or Password not valid", null);
        }
        
        var getUserRole = await _appDbContext.UserRoles.FirstOrDefaultAsync(r => r.UserId == applicationUser.Id);
        if (getUserRole is null)
        {
            return new GeneralResponse("15", "User role not found", null);
        }
        var getRoleName = await _appDbContext.SystemRoles.FirstOrDefaultAsync(r => r.Id == getUserRole.RoleId);
        if (getUserRole is null)
        {
            return new GeneralResponse("15", "Role name not found", null);
        }

        string jwtToken = GenerateToken(applicationUser, getRoleName!.Name!);
        string refreshToken = GenerateRefreshToken();
        var loginResponse = new LoginResponse(jwtToken, refreshToken);
        
        return new GeneralResponse("00", "Login Successfully", loginResponse);
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
    
    // generateToken
    private string GenerateToken(ApplicationUser applicationUser, string roleName)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSection.Key!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var userClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, applicationUser.Id.ToString()),
            new Claim(ClaimTypes.Name, applicationUser.Fullname!),
            new Claim(ClaimTypes.Email, applicationUser.Email!),
            new Claim(ClaimTypes.Role, roleName!),
        };
        var token = new JwtSecurityToken(
            issuer: _jwtSection.Issuer,
            audience: _jwtSection.Audience,
            claims: userClaims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    // genereate refreshToken
    private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
using BaseLibrary.DTOs;
using BaseLibrary.Responses;

namespace ServiceLibrary.Repositories.Contracts;

public interface IUserAccount
{
    Task<GeneralResponse> CreateAsync(Register user);
    Task<GeneralResponse> SignInAsync(Login user);
}
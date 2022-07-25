using Demo.AllFeatures.Data.Dtos;
using Demo.AllFeatures.Models;

namespace Demo.AllFeatures.Services;

public interface IUserService
{
    public Task<UserCreated> CreateUserAsync(CreateUser body);
    public Task<FindUser?> FindUserViaIdAsync(string id);

}
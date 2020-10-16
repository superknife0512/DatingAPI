using DatingAPI.Models;

namespace DatingAPI.Common.Interfaces
{
    interface ITokenService
    {
        string CreateToken(UserModel user);
    }
}

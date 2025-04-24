using System.Threading.Tasks;
using KLTN.Models;

namespace KLTN.Repositories
{
    public interface IAccountRepository
    {
        Task<Account?> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(Account account);
        Task<bool> IsUserNameExistAsync(string username);
        Task<bool> IsPhoneExistAsync(string phone);
        Task<bool> IsEmailExistAsync(string email);
        Task<IEnumerable<Account>> GetAllAccountsAsync();
        Task<Account> GetAccountByIdAsync(int id);
        Task UpdateAccountAsync(Account account); // Cập nhật tài khoản
        Task DeleteAccountAsync(int id);
        Task<Account?> GetAccountByEmailAsync(string email);
        Task<int?> GetUserIdByUsername(string username);
    }
}

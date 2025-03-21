using System.Linq;
using System.Threading.Tasks;
using KLTN.Models;
using Microsoft.EntityFrameworkCore;

namespace KLTN.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly KLTNContext _context;

        public AccountRepository(KLTNContext context)
        {
            _context = context;
        }

        public async Task<Account?> LoginAsync(string username, string password)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a =>
                a.UserName == username && a.Password == password
            );
        }

        public async Task<bool> RegisterAsync(Account account)
        {
            if (await IsUserNameExistAsync(account.UserName))
            {
                return false;
            }

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsUserNameExistAsync(string username)
        {
            return await _context.Accounts.AnyAsync(a => a.UserName == username);
        }

        public async Task<IEnumerable<Account>> GetAllAccountsAsync()
        {
            return await _context.Accounts.ToListAsync();
        }

        public async Task<Account> GetAccountByIdAsync(int id)
        {
            return await _context.Accounts.FindAsync(id);
        }

        // public async Task UpdateAccountAsync(Account account)
        // {
        //     _context.Accounts.Update(account);
        //     await _context.SaveChangesAsync();
        // }

        public async Task UpdateAccountAsync(Account account)
        {
            var existingAccount = await _context.Accounts.FindAsync(account.IdUser);
            if (existingAccount != null)
            {
                _context.Entry(existingAccount).State = EntityState.Detached; // Tách thực thể cũ
            }

            _context.Entry(account).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // Xóa tài khoản
        public async Task DeleteAccountAsync(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Account?> GetAccountByEmailAsync(string email)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task<int?> GetUserIdByUsername(string username)
        {
            var user = await _context
                .Accounts.Where(u => u.UserName == username)
                .Select(u => u.IdUser)
                .FirstOrDefaultAsync();

            return user != 0 ? user : (int?)null;
        }
    }
}

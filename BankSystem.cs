using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
namespace BankingSystemCApp
{
    public class BankSystem
    {
        private List<User> users = new List<User>();
        //private List<User> GetUsers;
        private UserDataManager userDataManager;

        private List<Account> accounts = new List<Account>();
        private List<Transaction> transactions = new List<Transaction>();
        private List<ExchangeRate> exchangeRates = new List<ExchangeRate>();

        public BankSystem()
        {
            userDataManager = new UserDataManager();
            users = userDataManager.LoadUserData();
        }

        public User GetUserByEmail(string email)
        {
            return users.Find(u => u.Email == email);
        }

        public void RegisterUser(string name, string email, string password)
        {
            if (IsEmailUnique(email))
            {
                // Generate a salt for password hashing
                byte[] salt = GenerateSalt();

                // Hash the password with the salt
                byte[] passwordHash = HashPassword(password, salt);

                // Create a new user
                User newUser = new User
                {
                    UserId = users.Count + 1,
                    Name = name,
                    Email = email,
                    PasswordHash = Convert.ToBase64String(passwordHash),
                };

                // Add the user to the list
                users.Add(newUser);
                Console.WriteLine($"User registered - Email: {email}, Password: {password}");
            }
            else
            {
                Console.WriteLine("Email address is already in use.");
            }
            userDataManager.SaveUserData(users);
        }

        public bool Login(string email, string password)
        {
            User user = users.Find(u => u.Email == email);

            if (user != null)
            {
                byte[] passwordHash = Convert.FromBase64String(user.PasswordHash);
                byte[] salt = GenerateSalt();

                byte[] inputHash = HashPassword(password, salt);

                //Console.WriteLine($"Input Password Hash: {Convert.ToBase64String(inputHash)}");
                //Console.WriteLine($"Stored Password Hash: {user.PasswordHash}");

                // Compare the stored password hash with the input hash
                if (CompareByteArrays(passwordHash, inputHash))
                {
                    Console.WriteLine($"Welcome, {user.Name}!");
                    return true;
                }
            }
            else
            {
                Console.WriteLine("Invalid email or password.");
            }
            return true;
        }

        public void CreateAccount(int userId)
        {
            Console.Write("Enter account holder name: ");
            string accountHolderName = Console.ReadLine();

            Console.Write("Enter initial balance: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal initialBalance) || initialBalance < 0)
            {
                Console.WriteLine("Invalid initial balance. Please enter a valid non-negative number.");
                return;
            }

            // Check if the account number is already in use
            if (accounts.Exists(acc => acc.AccountNumber == accounts.Count + 1))
            {
                Console.WriteLine("An account with this account number already exists.");
                return;
            }

            // Create a new account for the user
            Account newAccount = new Account
            {
                AccountNumber = accounts.Count + 1,
                UserId = userId,
                Balance = initialBalance, // Set the initial balance
            };

            accounts.Add(newAccount);
            Console.WriteLine($"Account created successfully. Account Number: {newAccount.AccountNumber}");
        }

        public void Deposit(int accountNumber, decimal amount)
        {
            Account account = accounts.Find(a => a.AccountNumber == accountNumber);

            if (account != null)
            {
                account.Balance += amount;

                // Record the transaction
                Transaction transaction = new Transaction
                {
                    TransactionId = transactions.Count + 1,
                    AccountNumber = accountNumber,
                    Amount = amount,
                    Timestamp = DateTime.Now,
                };

                transactions.Add(transaction);
                Console.WriteLine($"Deposited {amount:C} into account {accountNumber}. New balance: {account.Balance:C}");
            }
            else
            {
                Console.WriteLine("Account not found.");
            }
        }

        public void Withdraw(int accountNumber, decimal amount)
        {
            Account account = accounts.Find(a => a.AccountNumber == accountNumber);

            if (account != null)
            {
                if (account.Balance >= amount)
                {
                    account.Balance -= amount;

                    // Record the transaction
                    Transaction transaction = new Transaction
                    {
                        TransactionId = transactions.Count + 1,
                        AccountNumber = accountNumber,
                        Amount = -amount,
                        Timestamp = DateTime.Now,
                    };

                    transactions.Add(transaction);
                    Console.WriteLine($"Withdrawn {amount:C} from account {accountNumber}. New balance: {account.Balance:C}");
                }
                else
                {
                    Console.WriteLine("Insufficient balance.");
                }
            }
            else
            {
                Console.WriteLine("Account not found.");
            }
        }

        public void TransferMoney(int senderAccountNumber, int receiverAccountNumber, decimal amount)
        {
            Account senderAccount = accounts.Find(a => a.AccountNumber == senderAccountNumber);
            Account receiverAccount = accounts.Find(a => a.AccountNumber == receiverAccountNumber);

            if (senderAccount != null && receiverAccount != null)
            {
                if (senderAccount.Balance >= amount)
                {
                    senderAccount.Balance -= amount;
                    receiverAccount.Balance += amount;

                    // Record the sender's transaction
                    Transaction senderTransaction = new Transaction
                    {
                        TransactionId = transactions.Count + 1,
                        AccountNumber = senderAccountNumber,
                        Amount = -amount,
                        Timestamp = DateTime.Now,
                    };

                    // Record the receiver's transaction
                    Transaction receiverTransaction = new Transaction
                    {
                        TransactionId = transactions.Count + 1,
                        AccountNumber = receiverAccountNumber,
                        Amount = amount,
                        Timestamp = DateTime.Now,
                    };

                    transactions.Add(senderTransaction);
                    transactions.Add(receiverTransaction);

                    Console.WriteLine($"Transferred {amount:C} from account {senderAccountNumber} to account {receiverAccountNumber}.");
                    Console.WriteLine($"Sender's new balance: {senderAccount.Balance:C}");
                    Console.WriteLine($"Receiver's new balance: {receiverAccount.Balance:C}");
                }
                else
                {
                    Console.WriteLine("Insufficient balance.");
                }
            }
            else
            {
                Console.WriteLine("One or both accounts not found.");
            }
        }

        public void AddExchangeRate(string currencyFrom, string currencyTo, decimal rate)
        {
            ExchangeRate exchangeRate = new ExchangeRate
            {
                CurrencyFrom = currencyFrom,
                CurrencyTo = currencyTo,
                Rate = rate,
            };

            exchangeRates.Add(exchangeRate);
            Console.WriteLine($"Exchange rate added: {currencyFrom}/{currencyTo} = {rate}");
        }

        public decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
        {
            ExchangeRate exchangeRate = exchangeRates.Find(rate =>
                rate.CurrencyFrom == fromCurrency && rate.CurrencyTo == toCurrency);

            if (exchangeRate != null)
            {
                return amount * exchangeRate.Rate;
            }
            else
            {
                Console.WriteLine("Exchange rate not found.");
                return 0;
            }
        }

        private bool IsEmailUnique(string email)
        {
            return users.Find(u => u.Email == email) == null;
        }

        private byte[] GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[16];
                rng.GetBytes(salt);
                return salt;
            }
        }

        private byte[] HashPassword(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                return pbkdf2.GetBytes(20); // 20-byte hash
            }
        }

        private bool CompareByteArrays(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                    return false;
            }

            return true;
        }
        public decimal GetAccountBalance(int accountNumber)
        {
            Account account = accounts.Find(a => a.AccountNumber == accountNumber);
            return account != null ? account.Balance : 0;
        }

        public bool CanWithdraw(int accountNumber, decimal amount)
        {
            decimal balance = GetAccountBalance(accountNumber);
            return balance >= amount;
        }

        public bool Transfer(int senderAccountNumber, int receiverAccountNumber, decimal amount)
        {
            if (!CanWithdraw(senderAccountNumber, amount))
            {
                Console.WriteLine("Insufficient balance for the transfer.");
                return false;
            }

            Account senderAccount = accounts.Find(a => a.AccountNumber == senderAccountNumber);
            Account receiverAccount = accounts.Find(a => a.AccountNumber == receiverAccountNumber);

            if (senderAccount != null && receiverAccount != null)
            {
                senderAccount.Balance -= amount;
                receiverAccount.Balance += amount;

                // Record the sender's transaction
                Transaction senderTransaction = new Transaction
                {
                    TransactionId = transactions.Count + 1,
                    AccountNumber = senderAccountNumber,
                    Amount = -amount,
                    Timestamp = DateTime.Now,
                };

                // Record the receiver's transaction
                Transaction receiverTransaction = new Transaction
                {
                    TransactionId = transactions.Count + 1,
                    AccountNumber = receiverAccountNumber,
                    Amount = amount,
                    Timestamp = DateTime.Now,
                };

                transactions.Add(senderTransaction);
                transactions.Add(receiverTransaction);

                Console.WriteLine($"Transferred {amount:C} from account {senderAccountNumber} to account {receiverAccountNumber}.");
                Console.WriteLine($"Sender's new balance: {senderAccount.Balance:C}");
                Console.WriteLine($"Receiver's new balance: {receiverAccount.Balance:C}");
                return true;
            }
            else
            {
                Console.WriteLine("One or both accounts not found.");
                return false;
            }
        }

    }
}

public class User
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
}

public class Account
{
    public int AccountNumber { get; set; }
    public int UserId { get; set; }
    public decimal Balance { get; set; }
}

public class Transaction
{
    public int TransactionId { get; set; }
    public int AccountNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ExchangeRate
{
    public string CurrencyFrom { get; set; }
    public string CurrencyTo { get; set; }
    public decimal Rate { get; set; }
}

//public void CreateAccount(int userId)
//{
//    Console.Write("Enter account holder name: ");
//    string accountHolderName = Console.ReadLine();

//    Console.Write("Enter initial balance: ");
//    if (!decimal.TryParse(Console.ReadLine(), out decimal initialBalance))
//    {
//        Console.WriteLine("Invalid initial balance. Please enter a valid number.");
//        return;
//    }

//    // Check if the account number is already in use
//    if (user.Accounts.Exists(acc => acc.AccountHolderName == accountHolderName))
//    {
//        Console.WriteLine("An account with this account holder name already exists.");
//        return;
//    }
//    // Create a new account for the user
//    Account newAccount = new Account
//    {
//        AccountNumber = accounts.Count + 1,
//        UserId = userId,
//        Balance = 0,
//    };

//    // Add the account to the list
//    accounts.Add(newAccount);
//    Console.WriteLine("Account created successfully.");
//}
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
namespace BankingSystemCApp;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello and welcome to the bank of money!");

        BankSystem bankSystem = new BankSystem();
        User loggedInUser = null;  // Store the logged-in user

        while (true)
        {
            Console.Clear(); // Clear the console to display the menu cleanly

            if (loggedInUser == null)
            {
                Console.WriteLine("\nTo serve you better, choose from the option:");
                Console.WriteLine("\n1. Register");
                Console.WriteLine("2. Login");
                Console.WriteLine("3. Exit");
                Console.Write("\nEnter your choice: ");
                int choice = int.Parse(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        Console.Write("Name: ");
                        string name = Console.ReadLine();
                        Console.Write("Email: ");
                        string email = Console.ReadLine();
                        Console.Write("Password: ");
                        string password = Console.ReadLine();
                        bankSystem.RegisterUser(name, email, password);
                        break;

                    case 2:
                        Console.Write("Email: ");
                        string loginEmail = Console.ReadLine();
                        Console.Write("Password: ");
                        string loginPassword = Console.ReadLine();
                        bool isLoggedIn = bankSystem.Login(loginEmail, loginPassword);
                        if (isLoggedIn)
                        {
                            // Retrieve the user information after successful login
                            loggedInUser = bankSystem.GetUserByEmail(loginEmail);
                            Console.WriteLine($"\nLogged in as {loggedInUser.Name} ({loggedInUser.Email}).");
                        }
                        break;

                    case 3:
                        Console.WriteLine("Are you sure you want to exit? (Y/N)");
                        string exitChoice = Console.ReadLine().Trim().ToLower();

                        if (exitChoice == "y" || exitChoice == "yes")
                        {
                            Console.WriteLine("Thank you for using the system.");
                            // Set a timer to exit after 3 seconds
                            Thread.Sleep(3000);
                            Environment.Exit(0);
                        }
                        break;

                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
            else
            // If a user is logged in, allow them to perform operations
            {
                Console.WriteLine("\nChoose an option:");
                Console.WriteLine("\n1. Create Account");
                Console.WriteLine("2. Deposit");
                Console.WriteLine("3. Withdraw");
                Console.WriteLine("4. Transfer Money");
                Console.WriteLine("5. Account Information");
                Console.WriteLine("6. Logout");
                Console.Write("\nEnter your choice: ");

                int userChoice = int.Parse(Console.ReadLine());

                switch (userChoice)
                {
                    case 1:
                        bankSystem.CreateAccount(loggedInUser.UserId);
                        break;
                    case 2:
                        Console.Write("Enter account number: ");
                        int depositAccountNumber = int.Parse(Console.ReadLine());
                        Console.Write("Enter deposit amount: ");
                        decimal depositAmount = decimal.Parse(Console.ReadLine());
                        bankSystem.Deposit(depositAccountNumber, depositAmount);
                        break;
                    case 3:
                        Console.Write("Enter account number: ");
                        int withdrawAccountNumber = int.Parse(Console.ReadLine());
                        Console.Write("Enter withdrawal amount: ");
                        decimal withdrawAmount = decimal.Parse(Console.ReadLine());
                        if (bankSystem.CanWithdraw(withdrawAccountNumber, withdrawAmount))
                        {
                            bankSystem.Withdraw(withdrawAccountNumber, withdrawAmount);
                        }
                        else
                        {
                            Console.WriteLine("Insufficient balance for withdrawal.");
                        }
                        break;
                    case 4:
                        Console.Write("Enter sender account number: ");
                        int senderAccountNumber = int.Parse(Console.ReadLine());
                        Console.Write("Enter receiver account number: ");
                        int receiverAccountNumber = int.Parse(Console.ReadLine());
                        Console.Write("Enter transfer amount: ");
                        decimal transferAmount = decimal.Parse(Console.ReadLine());
                        bankSystem.Transfer(senderAccountNumber, receiverAccountNumber, transferAmount);
                        break;
                    case 5:
                        break;
                    case 6:
                        loggedInUser = null; // Logout
                        Console.WriteLine("Logged out.");
                        Console.WriteLine("Press Enter to continue...");
                        Console.ReadLine(); // Wait for user input before proceeding
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }
    }
}
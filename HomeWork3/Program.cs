using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;

namespace HomeWork3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //-----------------------------------------код SQL----------------------------------------

            //            CREATE DATABASE AccountsDatabase;
            //            USE AccountsDatabase;

            //            CREATE TABLE Accounts(
            //                AccountID INT PRIMARY KEY,
            //                AccountNumber VARCHAR(20) NOT NULL,
            //                Balance DECIMAL(18, 2) NOT NULL
            //            );

            //            INSERT INTO Accounts(AccountID, AccountNumber, Balance)
            //            VALUES(1, 'account1', 1000.00),
            //                  (2, 'account2', 500.00);

            //            BEGIN TRY
            //            BEGIN TRANSACTION;

            //            DECLARE @Amount DECIMAL(18, 2) = 200.00;
            //            DECLARE @FromAccountID INT = 1;
            //            DECLARE @ToAccountID INT = 2;

            //            IF(SELECT Balance FROM Accounts WHERE AccountID = @FromAccountID) >= @Amount
            //            BEGIN
            //            UPDATE Accounts SET Balance = Balance - @Amount WHERE AccountID = @FromAccountID;
            //            UPDATE Accounts SET Balance = Balance + @Amount WHERE AccountID = @ToAccountID;
            //            COMMIT;
            //            PRINT 'Transaction complete.';
            //            END
            //            ELSE
            //            BEGIN
            //            PRINT 'Error!';
            //            END
            //            END TRY
            //            BEGIN CATCH
            //            ROLLBACK;
            //            PRINT 'Error: ' + ERROR_MESSAGE();
            //            END CATCH;

            //-----------------------------------------код C#----------------------------------------

            string connectionString = "Server=localhost;Database=AccountsDatabase;Trusted_Connection=True;TrustServerCertificate=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlTransaction transaction = connection.BeginTransaction("MoneyTransfer");

                try
                {
                    TransferMoney(connection, transaction, 1, 2, 200);
                    transaction.Commit();
                    Console.WriteLine("Transsaction complete.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    transaction.Rollback();
                }
            }
        }

        static void TransferMoney(SqlConnection connection, SqlTransaction transaction, int fromAccountID, int toAccountID, decimal amount)
        {
            try
            {
                decimal fromAccountBalance = GetAccountBalance(connection, transaction, fromAccountID);

                if (fromAccountBalance >= amount)
                {
                    UpdateAccountBalance(connection, transaction, fromAccountID, fromAccountBalance - amount);
                    UpdateAccountBalance(connection, transaction, toAccountID, GetAccountBalance(connection, transaction, toAccountID) + amount);
                }
                else
                {
                    throw new Exception("insufficient funds");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static decimal GetAccountBalance(SqlConnection connection, SqlTransaction transaction, int accountID)
        {
            using (SqlCommand command = new SqlCommand($"SELECT Balance FROM Accounts WHERE AccountID = {accountID}", connection, transaction))
            {
                return (decimal)command.ExecuteScalar();
            }
        }

        static void UpdateAccountBalance(SqlConnection connection, SqlTransaction transaction, int accountID, decimal newBalance)
        {
            using (SqlCommand command = new SqlCommand($"UPDATE Accounts SET Balance = {newBalance} WHERE AccountID = {accountID}", connection, transaction))
            {
                command.ExecuteNonQuery();
            }
        }
    }

}

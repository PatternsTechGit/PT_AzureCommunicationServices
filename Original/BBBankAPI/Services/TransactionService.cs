using Entities;
using Entities.Request;
using Entities.Responses;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class TransactionService : ITransactionService
    {
        private readonly BBBankContext _bbBankContext;
        private readonly INotificationService _notificationService;
        
        public TransactionService(BBBankContext BBBankContext, INotificationService NotificationService)
        {
            _bbBankContext = BBBankContext;
            _notificationService = NotificationService;
        }

        public async Task<LineGraphData> GetLast12MonthBalances(string? userId)
        {
            var lineGraphData = new LineGraphData();

            var allTransactions = new List<Transaction>();
            if (userId == null)
            {
                allTransactions = _bbBankContext.Transactions.ToList();
            }
            else
            {
                allTransactions = _bbBankContext.Transactions.Where(x => x.Account.User.Id == userId).ToList();
            }
            if (allTransactions.Count() > 0)
            {
                var totalBalance = allTransactions.Sum(x => x.TransactionAmount);
                lineGraphData.TotalBalance = totalBalance;
                decimal lastMonthTotal = 0;
                for (int i = 12; i > 0; i--)
                {
                    var runningTotal = allTransactions.Where(x => x.TransactionDate >= DateTime.Now.AddMonths(-i) &&
                       x.TransactionDate < DateTime.Now.AddMonths(-i + 1)).Sum(y => y.TransactionAmount) + lastMonthTotal;
                    lineGraphData.Labels.Add(DateTime.Now.AddMonths(-i + 1).ToString("MMM yyyy"));
                    lineGraphData.Figures.Add(runningTotal);
                    lastMonthTotal = runningTotal;
                }
            }
            return lineGraphData;
        }

        public async Task<int> DepositFunds(DepositRequest depositRequest)
        {
            var account = _bbBankContext.Accounts.Where(x => x.AccountNumber == depositRequest.AccountId).FirstOrDefault();
            if (account == null)
                return -1;
            else
            {
                var transaction = new Transaction()
                {
                    Id = Guid.NewGuid().ToString(),
                    TransactionAmount = depositRequest.Amount,
                    TransactionDate = DateTime.UtcNow,
                    TransactionType = TransactionType.Deposit
                };
                if (account.Transactions != null)
                    account.Transactions.Add(transaction);
                
                SendEmailNotification(account.User.Email, depositRequest);
                return 1;
            }
        }

        private async void SendEmailNotification(string toAddress, DepositRequest depositRequest)
        {
            EmailMessage emailMessage = new EmailMessage();
            emailMessage.FromAddress = "Notification@PatternsTech.com";
            emailMessage.ToAddress = toAddress;
            emailMessage.Subject = "Amount Deposit Notification";
            emailMessage.Body = depositRequest.Amount+"$ has been deposited successfully.";          
         
            await _notificationService.SendEmail(emailMessage);
        }
    }
}

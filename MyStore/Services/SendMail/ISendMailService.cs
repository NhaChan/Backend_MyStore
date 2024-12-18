﻿namespace MyStore.Services.SendMail
{
    public interface ISendMailService
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        string GetPathOrderConfirm { get; }
        string GetPathProductList { get; }
    }
}

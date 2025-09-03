using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Enums
{
    public enum OtpPurpose
    {
        ChangePassword = 1,
        ForgotPassword = 2,
        RegisterNewAccount = 3,
        EmailVerification = 4,
        PhoneVerification = 5,
        TwoFactorAuthentication = 6,
        ResetPin = 7,
        TransactionApproval = 8,
        DeviceVerification = 9,
        UpdateSensitiveInfo = 10, // e.g., changing email, phone, or address
        UnblockAccount = 11,
        SubscriptionConfirmation = 12,
        DeleteAccountConfirmation = 13,
        PaymentVerification = 14
    }


}


using Identity.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Imp
{
    public static class SharedFunctions
    {
        
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        public static string NormalizeEmail(string email)
        {
            var parts = email.Split('@');
            if (parts.Length != 2)
                return email;

            var local = parts[0];
            var domain = parts[1].ToLower();

            if (domain == "gmail.com" || domain == "googlemail.com")
            {
                // Remove everything after +
                var plusIndex = local.IndexOf('+');
                if (plusIndex >= 0)
                    local = local.Substring(0, plusIndex);

                // Remove dots (Gmail ignores dots in username)
                local = local.Replace(".", "");
            }

            return $"{local}@{domain}";
        }
        public  static bool CanSendMail(AppUser user)
        {
            var now = DateTime.UtcNow;

            // Reset counter if it's a new day
            if (user.LastVerificationSentAt == null || user.LastVerificationSentAt.Value.Date < now.Date)
            {
                user.VerificationAttempts = 0;
                user.LastVerificationSentAt = now;  // ✅ set new date when resetting
            }

            // Limit: max 5 per day
            if (user.VerificationAttempts >= 5)
            {
                return false;
            }
            return true;
        }
    }
}

    

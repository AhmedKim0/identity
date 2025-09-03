using Identity.Domain.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.DTO.OTP
{
    public class VerifyOtpDto
    {
        public string Email { get; set; }
        public string Otp { get; set; }
        public OtpPurpose otpPurpose { get; set; }
    }
}

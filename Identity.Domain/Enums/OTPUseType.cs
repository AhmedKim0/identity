using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Enums
{
    public enum OTPUseType
    {
       EmailConfirm=0,
        PhoneConfirm=1,
        ChangePassword=2,
        ChangeEmail=3,
        ChangePhoneNumber=4,

    }
}

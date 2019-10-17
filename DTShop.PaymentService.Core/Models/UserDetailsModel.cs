using DTShop.PaymentService.Core.Enums;

namespace DTShop.PaymentService.Core.Models
{
    public class UserDetailsModel
    {
        public string UserName { get; set; }
        public CardAuthorizationInfo CardAuthorizationInfo { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace ChatMeService.Models.AccountViewModels
{
    public class LoginByPhoneViewModel
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string VerificationCode { get; set; }
    }
}
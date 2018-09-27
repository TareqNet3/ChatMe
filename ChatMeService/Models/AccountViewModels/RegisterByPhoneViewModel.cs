using System;
using System.ComponentModel.DataAnnotations;

namespace ChatMeService.Models.AccountViewModels
{
    public class RegisterByPhoneViewModel
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
    }
}
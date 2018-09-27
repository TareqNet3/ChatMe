using System;
using System.ComponentModel.DataAnnotations;

namespace ChatMeService.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace ChatMeService.Models.DTO
{
    public class User
    {
        public string ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? Birthdate { get; set; }
        public DateTime AddDateTime { get; set; }
        public DateTime LastLoginDateTime { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }

        public string Name { get { return $"{FirstName} {LastName}"; } }
    }
}
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatMeService.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using ChatMeService.Models.Entities;

namespace ChatMeService.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get { return $"{FirstName} {LastName}"; } }
        public DateTime? Birthdate { get; set; }
        public DateTime AddDateTime { get; set; }
        public DateTime LastLoginDateTime { get; set; }

        public virtual List<ApplicationUser> Friends { get; set; }
        public virtual List<Conversation> Conversations { get; set; }
        public virtual List<Message> Messages { get; set; }

        public virtual List<ConversationUser> ConversationUsers { get; set; }
        public virtual List<ConversationAdmin> ConversationAdmins { get; set; }

        public static ApplicationUser Get(ApplicationDbContext db, ClaimsPrincipal User)
        {
            var username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            return db.Users.FirstOrDefault(u => u.UserName == username);
        }
    }
}
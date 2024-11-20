using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPNServer
{
    public class UserManager
    {
        // In-memory store for usernames and their hashed passwords
        private Dictionary<string, string> userCredentials = new Dictionary<string, string>();

        public UserManager()
        {
            // Adding a default user (admin) for demonstration purposes
            // Note: In a real-world scenario, this should be stored in a database, not hardcoded
            AddUser("admin", "password123");
        }

        // Add a user with hashed password
        public void AddUser(string username, string password)
        {
            string hashedPassword = PasswordHasher.HashPassword(password);
            userCredentials[username] = hashedPassword;
        }

        // Validate user credentials
        public bool ValidateUser(string username, string password)
        {
            if (userCredentials.ContainsKey(username))
            {
                string storedHashedPassword = userCredentials[username];
                return PasswordHasher.VerifyPassword(password, storedHashedPassword);
            }
            return false;
        }
    }
}

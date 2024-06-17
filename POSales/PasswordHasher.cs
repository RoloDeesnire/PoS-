using System;
using System.Security.Cryptography;
using System.Text;

public static class PasswordHasher
{
    public static string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = sha256.ComputeHash(passwordBytes);
            string hashedPassword = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            // Ensure the hashed password is no longer than 50 characters
            if (hashedPassword.Length > 50)
            {
                hashedPassword = hashedPassword.Substring(0, 50);
            }

            return hashedPassword;
        }
    }
}
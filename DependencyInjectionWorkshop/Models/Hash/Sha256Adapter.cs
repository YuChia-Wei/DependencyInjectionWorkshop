using System.Text;

namespace DependencyInjectionWorkshop.Models.Hash
{
    public class Sha256Adapter : IHash
    {
        public Sha256Adapter()
        {
        }

        public string Hash(string psw)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(psw));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            return hash.ToString();
        }
    }
}
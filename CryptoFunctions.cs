using System.Linq;

namespace WebsiteBackend{
    class CryptoFunctions{
        public static int CreateSalt(){
            return System.Security.Cryptography.RandomNumberGenerator.GetInt32(int.MinValue,int.MinValue);
        }

        public static string CreatePassword(string plaintextEntered, string salt = ""){
            return System.Convert.ToHexString(System.Security.Cryptography.SHA512.HashData(System.Text.Encoding.UTF8.GetBytes(string.Concat(plaintextEntered,salt))));
        }

        public static bool ComparePassword(string enteredData, string savedData){
            return enteredData.Equals(savedData);
        }
    }
}
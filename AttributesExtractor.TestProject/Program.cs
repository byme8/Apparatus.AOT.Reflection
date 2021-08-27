using System.ComponentModel.DataAnnotations;
using AttributesExtractor;

namespace AttributesExtractor.TestProject
{
    public class User
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName {  get; set; }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var user = new User();
        }

        static void DontCall()
        {
            var user = new User();
            var attributes = user.GetAttributes();
        }
    }
}

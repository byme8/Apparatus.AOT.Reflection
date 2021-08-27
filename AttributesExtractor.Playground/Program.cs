using System;
using System.ComponentModel.DataAnnotations;

namespace AttributesExtractor.Playground
{
    public class User
    {
        [Required]
        // place to replace 2
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
    }


    // place to replace 0

    class Program
    {
        static void Main(string[] args)
        {
            var user = new User();
            // place to replace 1
        }

        static void DontCall()
        {
            var user = new User();
            var attributes = user.GetAttributes();
        }
    }
}
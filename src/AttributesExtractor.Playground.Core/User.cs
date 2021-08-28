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
}
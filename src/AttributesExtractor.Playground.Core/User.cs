using System.ComponentModel.DataAnnotations;

namespace AttributesExtractor.Playground
{
    public class User
    {
        [Required]
        // place to replace 2
        public string FirstName { get; set; }

        [Required]
        public virtual string LastName { get; set; }
    }

    public class Admin : User
    {
        public new string FirstName { get; set; }
        public override string LastName { get; set; }
    }
}
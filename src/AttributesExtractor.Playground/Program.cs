using System;
using System.ComponentModel.DataAnnotations;

namespace AttributesExtractor.Playground
{

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
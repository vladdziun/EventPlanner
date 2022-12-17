using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace LoginReg.Models
{
     public interface IUser
    {
         string Email { get; set; }

         string Password { get; set; }
    }
}

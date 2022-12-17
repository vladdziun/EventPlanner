using LoginReg.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class LoginUser: IUser
{
    [Required]
    public string Email {get; set;}

    [DataType(DataType.Password)]
    [Required]
    public string Password { get; set; }
}
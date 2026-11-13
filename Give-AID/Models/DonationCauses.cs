using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;


public class DonationCauses
{
    public int DonationCausesID {get;  set;}
    [ StringLength(50)]
     public required string CauseName{get;  set;}
   
    [StringLength(100)]       
    public required string CauseDescription{get; set;}
    public string? imageURL{get; set;}

    public bool isActive{get; set;} = true;
}
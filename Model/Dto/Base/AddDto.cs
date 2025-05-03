using System.ComponentModel.DataAnnotations;

namespace APIWithAuth.Model.Dto.Base;

public class AddDto
{
    [Required]
    public string Body { get; set; }
}
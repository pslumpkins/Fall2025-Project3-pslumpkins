using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace Fall2025_Project3_pslumpkins.Models;
public class ActorMovie
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select an actor.")]
    public int ActorId { get; set; }

    [ValidateNever]                 // <-- don't validate this on POST
    public Actor? Actor { get; set; }  // <-- make nullable

    [Range(1, int.MaxValue, ErrorMessage = "Please select a movie.")]
    public int MovieId { get; set; }

    [ValidateNever]                 // <-- don't validate this on POST
    public Movie? Movie { get; set; }  // <-- make nullable
}
